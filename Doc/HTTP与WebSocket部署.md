# HTTP 与 WebSocket 部署指南

> 适用版本：AntJob 5.0 起 | 更新日期：2026-10-18

客户端把 `AntSetting.Server` 配置为 `http://` 或 `https://` 地址后，自动使用 HTTP REST + WebSocket 通知，无需独立 AntServer。

---

## 1. 通道分工

| 动作 | 通道 |
|------|------|
| 登录、心跳、注销 | HTTP |
| 作业注册、查询、控制参数修改 | HTTP |
| 申请任务、报告进度、生产消息 | HTTP |
| 作业变更、消息到达通知 | WebSocket |

WebSocket 只下发信号，客户端收到后再通过 HTTP 拉取权威数据。通知丢失、连接中断都不影响正确性，客户端仍按轮询周期工作。

> Redis 消费组创建完成前的短暂窗口内，通知可能被跳过；这属于“尽力而为”的范畴，由客户端兜底轮询修复。

---

## 2. 服务端要求

1. 部署 AntJob.Web（IIS / Kestrel / Docker），并确保站点支持 WebSocket。
2. 配置 `AntJobSetting.TokenSecret`（如 `HS256:请替换为足够长的随机密钥`），用于签发 JWT。未配置时服务端会自动生成一个随机密钥，仅适合单实例开发环境；**多实例必须显式配置同一个密钥**，否则各实例签发的令牌互不认可。
3. 多实例部署时配置 Redis。`ICacheProvider` 会创建 Redis Stream 事件总线，每个实例以自身 `Runtime.ClientId` 作为消费组，各自收到一份通知。连接串配置在 `RedisCache`，例如：`server=127.0.0.1:6379;db=0`。
4. 未配置 Redis 时只能单实例推送，功能由轮询兜底，不会报错。
5. 若同时运行 AntJob.Server（RPC 兼容通道）与 AntJob.Web，并希望 Server 侧产生的消息通知到连接在 Web 上的客户端，必须配置 Redis 事件总线；否则跨进程通知无法送达。

---

## 3. Nginx 反向代理

```nginx
location /AntJob/Notify {
    proxy_pass         http://antjob_web;
    proxy_http_version 1.1;
    proxy_set_header   Upgrade $http_upgrade;
    proxy_set_header   Connection "upgrade";
    proxy_set_header   Host $host;
    proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_read_timeout 3600s;
    proxy_send_timeout 3600s;
    proxy_buffering    off;
}

location /AntJob/ {
    proxy_pass       http://antjob_web;
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
}
```

要点：

- `Upgrade`/`Connection` 头必须透传，否则握手失败。
- `proxy_read_timeout` 必须大于客户端心跳周期（60 秒），建议 3600 秒。
- 关闭响应缓冲，避免通知被缓冲。
- 健康检查不要复用 `/AntJob/Notify`。

---

## 4. 云负载均衡

- 确认支持 WebSocket 与长连接；空闲超时建议 ≥ 300 秒。
- 无需会话粘性：通知经 Redis 广播到持有连接的实例，重连可落到任意实例。
- 若 LB 不支持 WebSocket，客户端会保持轮询，调度功能不受影响，只是通知延迟回到轮询周期。

---

## 5. 客户端配置

```json
{
  "Ant": {
    "Server": "https://ant.example.com",
    "AppId": "MyJobApp",
    "Secret": ""
  }
}
```

- 首次登录且 `Secret` 为空时，服务端自动注册并下发密钥，客户端保存到配置文件。
- 多个地址用逗号分隔，客户端自动主备切换。
- 继续使用 `tcp://` 地址时走 RPC 兼容通道，行为不变。
- 客户端经网关直连时，建议配置 `UseRegistry=false`，避免星尘注册中心用内网地址覆盖网关地址；默认 `true` 保持原有服务发现行为。
- 消息型作业在 HTTP 通道下默认空闲 15 秒申请一次（`MessageIdlePeriod`），收到消息通知后立即唤醒；对消息延迟敏感的场景可调小该值。

---

## 6. 迁移步骤

1. 升级客户端 SDK 与 AntJob.Web 到 5.0。`SetJob` 的时间字段在 5.0 按 UTC 传输，客户端与服务端需同时升级；旧版客户端不带 `__utc` 标记，服务端保持原有本地时间语义。
2. 确认 Web 站点已启用 WebSocket，配置好 `TokenSecret`。
3. 将客户端 `Server` 从 `tcp://host:9999` 改为 `https://host`。
4. 观察日志：登录成功、`GetJobs` 正常、WebSocket 已连接。
5. 停用独立 AntServer（可选，RPC 通道仍保留）。

---

## 7. 排查

| 现象 | 排查方向 |
|------|----------|
| 登录 401/403 | `TokenSecret` 是否一致；AppId/Secret 是否正确 |
| 作业变更未即时生效 | WebSocket 是否握手成功；网关 Upgrade 头是否透传；Redis 是否连通 |
| 连接频繁断开 | 网关空闲超时是否小于心跳周期；LB 是否支持长连接 |
| 多实例只有部分客户端收到通知 | 是否配置 Redis；各实例 `Runtime.ClientId` 是否唯一 |

---

（完）
