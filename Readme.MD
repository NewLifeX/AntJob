# NewLife.AntJob - 蚂蚁调度

![GitHub top language](https://img.shields.io/github/languages/top/newlifex/antjob?logo=github)
![GitHub License](https://img.shields.io/github/license/newlifex/antjob?logo=github)
![Nuget Downloads](https://img.shields.io/nuget/dt/newlife.antjob?logo=nuget)
![Nuget](https://img.shields.io/nuget/v/newlife.antjob?logo=nuget)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/newlife.antjob?label=dev%20nuget&logo=nuget)

![Nuget Downloads](https://img.shields.io/nuget/dt/newlife.antjob.extensions?logo=nuget)
![Nuget](https://img.shields.io/nuget/v/newlife.antjob.extensions?logo=nuget)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/newlife.antjob.extensions?label=dev%20nuget&logo=nuget)

![Ant](Doc/ant.png)  

> EN: AntJob is a high-throughput distributed job scheduling & near real-time computing platform for .NET, focusing on massive (10^11+) task slicing, reliable retry and elastic horizontal scaling. Chinese documentation below is the canonical version.

# 蚂蚁调度 AntJob - 分布式任务/数据调度系统

分布式任务调度系统，纯 .NET 打造的重量级大数据实时计算平台，万亿级调度经验积累！面向中小企业乃至大型集团大数据分析与实时 / 离线混合计算场景。  

开源地址：[https://github.com/NewLifeX/AntJob](https://github.com/NewLifeX/AntJob)  
使用教程：[https://newlifex.com/blood/antjob](https://newlifex.com/blood/antjob)  
体验地址（托管演示环境）：[http://ant.newlifex.com](http://ant.newlifex.com/)  

---
## 目录 (Table of Contents)
1. 背景与定位
2. 核心特性速览
3. 架构与组成组件
4. 调度模式详解
5. 快速开始（模板 / 最小示例 / Docker / Web）
6. 典型部署拓扑
7. 目录结构说明
8. 配置说明 & 参数要点
9. 扩展机制（Handler / Provider / 动态脚本）
10. 可靠性与一致性保证
11. 性能与容量规划
12. 与常见调度系统对比
13. 常见问题 (FAQ)
14. Roadmap（除 v4 升级外的后续规划）
15. 核心概念回顾
16. 设计示例（定时调度 Sample）
17. 新生命项目矩阵（保持原样）
18. 开发团队

---
## 1. 背景与定位
AntJob 以 “蚂蚁搬家” 思想把任意大任务切分成可并行的小片段，天然适合：
- 海量日志 / 订单 / 交易 / 设备数据持续抽取与实时计算
- 按时间窗口（秒 / 分 / 小时 / 天 / 自定义 Cron）滚动汇总
- 数据补算 / 重算 / 回溯修复
- 分布式跨多机房、弹性节点数的高并发批处理

`蚂蚁搬家` 思想诞生于 2008 年金融危机期间的上海陆家嘴，回应金融机构对低成本、高效率数据分析能力的紧急诉求。  

对标类：XxlJob、Quartz、Hangfire、Airflow（部分场景）、自研定时脚本集群等；但 AntJob 更强调“数据/时间切片+重试+多模式统一”和对企业内已有 .NET 生态的无侵入集成。

---
## 2. 核心特性速览
- 多调度模式：定时 (Time)、数据窗口 (Data)、消息 (Message)、Cron、SQL 语句、C# 动态脚本
- 作业时间/数据切片：精细到秒级，支持追赶、回溯补齐、批量重置
- 任务可靠投递：失败自动延迟重试（容忍暂时性依赖失败）
- 集群弹性：天然无状态执行节点，可随时扩缩容
- 通信协议：基于 NewLife.Remoting (TCP) + 规划中的 WebSocket/Http 轻量接入
- 监控可视化：Web 控制台实时查看作业曲线、任务状态、节点在线
- 可插拔提供者 (IJobProvider)：网络、文件、本地自定义源
- 轻依赖：核心依赖 NewLife 自家生态（日志、配置、追踪），减少外部复杂度
- 跨平台 & 多 TFM：支持从 .NET Framework 4.5 到 .NET 8/9
- 强命名 & 长期兼容：方便企业私有化与二次封装

---
## 3. 架构与组成组件
```
+----------------------+            +-------------------------+
|   执行节点 (Agent)   |  RPC/TCP   |   调度中心 (Server)     |
|  - Scheduler         | <--------> | - JobService / Task逻辑 |
|  - Handlers(业务)    |            | - 状态 & 元数据存储      |
+----------+-----------+            +-----------+-------------+
           |  同步日志/追踪 (可选对接 Stardust)  |
           v                                          
    +-------------+      Web UI / API      +------------------+
    | 数据库/存储 |  <------------------>  |  Web 控制台 AntWeb |
    +-------------+                         +------------------+
```
组件说明：
- AntJob.Server：调度中心服务端逻辑（Job & Task 元数据、分片、分配、重试策略）
- AntJob.Web：基于 NewLife.Cube 的可视化管理平台（多租户 App 隔离、权限、监控）
- AntJob（库）：客户端 SDK（Scheduler + Handler 抽象、网络通信、上下文封装）
- AntJob.Extensions：扩展调度类型（SqlMessage、DataHandler、MessageHandler、CSharpHandler 等）
- Samples：示例（HisAgent、HisData、HisWeb 等）

---
## 4. 调度模式详解
| 模式 | 适用场景 | 触发粒度 | 关键字段 | 说明 |
|------|----------|----------|----------|------|
| Time / Cron | 通用周期性任务 | 秒 ~ Cron | Job.Time / Cron | 支持 Cron 表达式、替代 Start+Step 固定间隔 |
| Data (时间窗口) | 按时间分片处理新增数据 | 秒 ~ 任意窗口 | Start / End | 自动补齐窗口，适合流水表、日志表滚动计算 |
| Message | 被动消息驱动 | 实时 | Body | 外部推送/队列消息转成任务 |
| SQL / 数据驱动 | 扫描数据表按主键/时间切片 | 批 | 自定义 | 扩展组件，常用于历史补算或批量迁移 |
| C# 动态脚本 | 快速试验、小逻辑热更 | 同上 | 脚本 | 运行时编译执行（受权限/沙箱限制） |

---
## 5. 快速开始
### 5.1 模板方式（推荐）
```
dotnet new install NewLife.Templates
# 创建一个带 AntJob 示例的工程
dotnet new antjob --name TestJob
cd TestJob
dotnet run
```
### 5.2 手工最小示例
参见下文 “设计示例” 段落。

### 5.3 仅本地文件调度（离线 / 单机测试）
不配置 NetworkJobProvider 时使用默认本地 Provider，适合调试。

### 5.4 引入 Web 控制台
部署 AntJob.Web（IIS / Kestrel / Docker）后修改客户端配置 Server 指向其服务地址即可。

### 5.5 Docker（示意）
> 官方暂未提供镜像，可在企业内封装：
```
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY ./publish/ .
ENTRYPOINT ["dotnet","AntJob.Web.dll"]
```

---
## 6. 典型部署拓扑
1. 单实例（开发机）：Web + Server + JobApp 同机部署，适合开发测试
2. 双中心高可用：两个调度中心地址逗号分隔（主备切换），客户端自动 Failover
3. 多执行集群：若干执行节点（按业务或负载分组），统一连接调度中心
4. 混合：生产使用双中心 + 多执行节点；测试/预发独立集群

---
## 7. 目录结构说明（摘取关键）
```
AntJob/                 客户端核心库
AntJob.Extensions/      扩展处理器/消息/SQL等
AntJob.Server/          调度中心核心服务逻辑 (JobService 等)
AntJob.Web/             Razor Pages + Cube 管理后台
AntJob.Data/            基础数据模型 (实体层)
Samples/                示例项目（HisAgent/HisData/HisWeb）
AntTest/                单元/集成测试
Doc/                    文档与资源
```

---
## 8. 配置说明
核心客户端配置类：`AntSetting`
- Server：调度中心地址，支持多地址 `tcp://a:9999,tcp://b:9999`
- AppID / Secret：应用隔离凭据（可自动注册或手动分发）
- Debug：调试开关，输出更详细日志

Web/Server 侧：
- 数据库连接通过 XCode 自动反向工程创建表
- 可对接 Stardust 获取注册中心、APM、配置中心能力

---
## 9. 扩展机制
1. 编写自定义 Handler：继承 `Handler`，重写 `Execute(JobContext ctx)` 返回成功处理数量
2. 自定义数据来源 Provider：实现 `IJobProvider`（如：Kafka / Redis Stream / REST）
3. 消息调度：`MessageHandler` 推送任意消息体形成任务
4. SQL/数据驱动：`SqlMessage` + `DataHandler` 扫描数据并切片
5. 动态脚本：`CSharpHandler` 允许以脚本方式快速试错（生产需限制权限）
6. 插件化：由于强命名与多 TFM 支持，可在独立程序集封装处理器后热插拔

---
## 10. 可靠性 & 一致性
- 任务原子分配：同一任务 ID 只会被一个节点获取
- 状态机：就绪 → 处理中 → 完成 / 错误 / 取消
- 自动重试：失败后按延迟（默认 1 分钟）重试，最多 N 次（可配置）
- 重置机制：批量重置历史区间重新计算（补算 / 校正）
- 幂等性：业务层建议基于主键/时间窗口实现幂等写入
- 心跳与在线表：实时发现节点上下线，支持弹性扩容

---
## 11. 性能与容量规划
- 单作业切片规模：可达百亿级任务历史记录（视数据库能力）
- 单节点吞吐：依据 Handler 业务逻辑；框架调度开销极低（内存级调度控制）
- 低 GC 压力：内部使用对象池/缓存（依赖 NewLife.Core 优化）
- 调优建议：
  - 适度控制切片粒度（过细增加调度元数据开销）
  - 使用批量 IO 或缓存减少数据库压力
  - 大计算窗口优先 Data 模式 + 子窗口循环

---
## 12. 对比（概述）
| 系统 | 特点 | AntJob 差异点 |
|------|------|---------------|
| Quartz.NET | 纯定时/Cron | AntJob 追加数据/消息切片、重算、集群分片简单 |
| Hangfire | 持久化后台任务 | AntJob 专注“时间/数据”连续切片而非一次性任务队列 |
| XXL-JOB | Java 生态 | AntJob .NET 原生 + 多模式统一 + 动态脚本 |
| Airflow | DAG 编排 | AntJob 更轻量，聚焦高频窗口处理；可与 Airflow 互补 |

---
## 13. 常见问题 (FAQ)
Q: 作业注册后为什么默认停止?  
A: 防止未配置参数即大量启动；需在 Web 控制台手动启用。  

Q: 如何回溯重算一个月数据?  
A: 在作业面板修改开始时间/重置指定时间段或批量选择任务重置。  

Q: 任务失败是否会阻塞后续?  
A: 不会；失败任务延迟重试，其它窗口继续调度。  

Q: 能否只用 Http/WebSocket 接入?  
A: v4 规划中（待 Remoting WebSocketClient），届时轻量场景无需独立 AntServer。  

Q: 如何限制脚本风险?  
A: 关闭未使用的 CSharpHandler；或隔离运行进程 + 最小权限配置。  

---
## 14. Roadmap（补充 v4 外）
- WebSocket/Http Provider （无 TCP 端口环境支持）
- 任务主动延迟 API（执行中评估延时再执行而不计错）
- 可插拔告警渠道（邮件 / Webhook / 企业微信等）
- 多维度监控指标导出（Prometheus/OpenTelemetry）
- DAG / 依赖编排的可选扩展层（在核心稳定后）

---
## 15. 核心概念回顾
- Job：作业配置（时间/步进/Cron/模式/并发限制等）
- Task：作业实例化后的最小执行单元（带时间点或区间、消息体）
- Handler：业务逻辑载体；生命周期由 Scheduler 管理
- JobContext：一次任务执行上下文（含 Task、统计、日志辅助）
- IJobProvider：任务来源提供者（网络/本地/自定义）

---
## 16. 设计示例（定时调度）
以下源码位于 [Samples/HisAgent](https://github.com/NewLifeX/AntJob/tree/master/Samples/HisAgent) 

### 新建项目
新建 .NET Core 3.1+ 项目，从 NuGet 引用 AntJob。实例化一个调度器 Scheduler，配置网络提供者。  
```csharp
using AntJob;
using HisAgent;
using NewLife.Log;
using NewLife.Model;
using Stardust;

// 启用控制台日志，拦截所有异常
XTrace.UseConsole();

var services = ObjectContainer.Current;
services.AddStardust();

services.AddAntJob()
    .AddHandler<HelloJob>();

// 友好退出
var host = services.BuildHost();
await host.RunAsync();
```
添加第一个定时调度作业处理器：
```csharp
using System;
using AntJob;

internal class HelloJob : Handler
{
    public HelloJob() => Job.Cron = "7/30 * * * * ?";

    public override Int32 Execute(JobContext ctx)
    {
        // 当前任务时间
        var time = ctx.Task.Time;
        WriteLog("新生命蚂蚁调度系统！当前任务时间：{0}", time);
        return 1; // 成功处理数据量
    }
}
```
作业处理器必须继承 `Handler` 并重写 `Execute` 实现业务逻辑。构造函数中设定的 Time、Step 仅首次注册使用，后续修改在控制台完成。  

或者使用异步处理的定时调度作业处理器：
```csharp
using System;
using AntJob;

internal class HelloJob : Handler
{
    public HelloJob() => Job.Cron = "7/30 * * * * ?";

    public override async Task<Int32> ExecuteAsync(JobContext ctx)
    {
        using var span = Tracer?.NewSpan("HelloJob", ctx.Task.DataTime);

        // 当前任务时间
        var time = ctx.Task.DataTime;
        WriteLog("新生命蚂蚁调度系统！当前任务时间：{0}", time);
        if (!ctx.Task.Data.IsNullOrEmpty()) WriteLog("数据：{0}", ctx.Task.Data);

        var state = Rand.NextString(16);
        var http = new HttpClient { BaseAddress = new Uri("https://newlifex.com") };
        http.SetUserAgent();
        var rs = await http.GetAsync<IDictionary<String, Object>>("/cube/info", new { state });

        if (rs.TryGetValue("state", out var value) && value is String str)
        {
            Trace.Assert(state == str, "返回状态不一致");
        }

        // 成功处理数据量
        return 1;
    }
}
```

### 编译 & 运行
首次运行会自动尝试连接 `tcp://127.0.0.1:9999` 与 `tcp://ant.newlifex.com:9999`（主备机制），来源于 `AntSetting` 配置。

日志示例：
```sql
21:33:08.470  1 N - 启动任务调度引擎[AntJob.Providers.NetworkJobProvider]，作业[1]项，定时5秒
21:33:08.471  1 N - HelloJob 开始工作 False 区间（2020-04-09 00:00:00, 0001-01-01 00:00:00） Offset=15 Step=10 MaxTask=8
```
### Web 作业管理与重置
在 Web 控制台启用作业后，调度中心将按时间点切分任务并分发。可查看任务明细、错误重试、批量重置以回溯计算。

### 分布式并行
多开若干 Agent，任务由调度中心分派，任一任务只会被一个节点执行；失败将按策略重试，提升整体吞吐与稳定性。

---
## 17. 核心功能模块（简表）
1. 调度器 (Scheduler)：作业调度核心引擎，管理处理器生命周期和任务分发
2. 处理器 (Handler)：作业处理器基类，每个业务模块继承实现 Execute/ExecuteAsync 方法
3. 作业提供者 (IJobProvider)：任务来源，支持网络、本地等
4. 任务上下文 (JobContext)：封装任务执行环境与状态
5. 调度模式：定时 (Timer)、数据 (Data)、消息 (Message)、Cron 表达式、SQL/C# 扩展
6. 分布式通信：基于 NewLife.Remoting 实现调度中心与执行节点通信

---
## 18. v4 架构升级 (进展)
v4 亮点：
1. [] 新增 Http 接入（AntWeb 提供调度服务，无需 AntServer）
2. [x] 增强定时调度：支持 Cron 表达式，逐步替代 Start+Step 固定步进
3. [] 提前生成任务：到点即执行（精度提升，待评估）
4. [x] 任务主动延迟：执行中请求延迟但不计失败，提高鲁棒性
5. [] 扩充调度模式：常态化 AntAgent，纳入 Sql 调度 / C# 调度，后续增加抽取、推送等模式

---
## 19. 功能特点（算法与能力）
蚂蚁算法：把任意大数据拆分成小块并行计算（类似“馒头被蚂蚁搬家”）。2008 年诞生，历经基金短信群发、快递 1200 万/日订单处理、百亿级数据清洗、万亿级年度调度实践打磨。聚焦分布式切片与集中调度能力，已验证百亿级调度规模。

主要功能点（回顾）：
- 作业处理器：最小业务模块单元
- 任务上下文：时间区间 / 时间点 / 消息体
- 数据切片：按区间（秒/分）切分（单表验证 60 亿行）
- 定时调度：秒级 / Cron 粒度
- 任务重试：延迟重派，跨节点兜底
- 任务/作业重置：批量回放 / 补算指定时间段
- 作业面板：参数动态调整 + 运行态统计

---
## 20. 设计概要 & 系统架构（原示意）
计算型应用（实现 IJob）

```sequence
计算应用->调度中心: app登录
note over 调度中心: app/secret
计算应用-->>调度中心: 注册作业

Web控制台->调度中心: 设置参数
Web控制台->调度中心: 启动作业

计算应用->调度中心: 申请作业任务
调度中心->计算应用: 返回任务
note over 计算应用: 多线程处理任务

计算应用-->调度中心: 上报局部状态
note over Web控制台: 作业状态看板
计算应用->调度中心: 处理成功
计算应用-->调度中心: 处理失败
计算应用-->调度中心: 延迟处理
```

调度中心主从架构

```sequence
计算应用->调度中心: 登录
调度中心->数据库: 连接
计算应用-->>调度中心2: 故障转移
调度中心2->数据库: 连接

计算应用2->调度中心: 登录
计算应用3->调度中心: 登录
计算应用4->调度中心: 登录

Web控制台-->调度中心: 监控
```

---
## 21. 新生命项目矩阵
各项目默认支持net9.0/netstandard2.1/netstandard2.0/net4.62/net4.5，旧版（2024.0801）支持net4.0/net2.0  

|                               项目                               | 年份  | 说明                                                                                        |
| :--------------------------------------------------------------: | :---: | ------------------------------------------------------------------------------------------- |
|                             基础组件                             |       | 支撑其它中间件以及产品项目                                                                  |
|          [NewLife.Core](https://github.com/NewLifeX/X)           | 2002  | 核心库，日志、配置、缓存、网络、序列化、APM性能追踪                                         |
|    [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode)    | 2005  | 大数据中间件，单表百亿级，MySql/SQLite/SqlServer/Oracle/PostgreSql/达梦，自动分表，读写分离 |
|      [NewLife.Net](https://github.com/NewLifeX/NewLife.Net)      | 2005  | 网络库，单机千万级吞吐率（2266万tps），单机百万级连接（400万Tcp长连接）                     |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | 2011  | 协议通信库，提供CS应用通信框架，支持Http/RPC通信框架，高吞吐，物联网设备低开销易接入        |
|     [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube)     | 2010  | 魔方快速开发平台，集成了用户权限、SSO登录、OAuth服务端等，单表100亿级项目验证               |
|    [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent)    | 2008  | 服务管理组件，把应用安装成为操作系统守护进程，Windows服务、Linux的Systemd                   |
|     [NewLife.Zero](https://github.com/NewLifeX/NewLife.Zero)     | 2020  | Zero零代脚手架，基于NewLife组件生态的项目模板NewLife.Templates，Web、WebApi、Service        |
|                              中间件                              |       | 对接知名中间件平台                                                                          |
|    [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis)    | 2017  | Redis客户端，微秒级延迟，百万级吞吐，丰富的消息队列，百亿级数据量项目验证                   |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | 2018  | RocketMQ纯托管客户端，支持Apache RocketMQ和阿里云消息队列，十亿级项目验                     |
|     [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT)     | 2019  | 物联网消息协议，MqttClient/MqttServer，客户端支持阿里云物联网                               |
|      [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT)      | 2022  | IoT标准库，定义物联网领域的各种通信协议标准规范                                             |
|   [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus)   | 2022  | ModbusTcp/ModbusRTU/ModbusASCII，基于IoT标准库实现，支持ZeroIoT平台和IoTEdge网关            |
|  [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens)  | 2022  | 西门子PLC协议，基于IoT标准库实现，支持IoT平台和IoTEdge                                      |
|      [NewLife.Map](https://github.com/NewLifeX/NewLife.Map)      | 2022  | 地图组件库，封装百度地图、高德地图、腾讯地图、天地图                                        |
|    [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio)    | 2023  | 音频编解码库，PCM/ADPCMA/G711A/G722U/WAV/AAC                                                |
|                             产品平台                             |       | 产品平台级，编译部署即用，个性化自定义                                                      |
|         [Stardust](https://github.com/NewLifeX/Stardust)         | 2018  | 星尘，分布式服务平台，节点管理、APM监控中心、配置中心、注册中心、发布中心                   |
|           [AntJob](https://github.com/NewLifeX/AntJob)           | 2019  | 蚂蚁调度，分布式大数据计算平台（实时/离线），蚂蚁搬家分片思想，万亿级数据量项目验证         |
|      [NewLife.ERP](https://github.com/NewLifeX/NewLife.ERP)      | 2021  | 企业ERP，产品管理、客户管理、销售管理、供应商管理                                           |
|         [CrazyCoder](https://github.com/NewLifeX/XCoder)         | 2006  | 码神工具，众多开发者工具，网络、串口、加解密、正则表达式、Modbus、MQTT                      |
|           [EasyIO](https://github.com/NewLifeX/EasyIO)           | 2023  | 简易文件存储，支持分布式系统中文件集中存储。                                                |
|           [XProxy](https://github.com/NewLifeX/XProxy)           | 2005  | 产品级反向代理，NAT代理、Http代理                                                           |
|        [HttpMeter](https://github.com/NewLifeX/HttpMeter)        | 2022  | Http压力测试工具                                                                            |
|         [GitCandy](https://github.com/NewLifeX/GitCandy)         | 2015  | Git源代码管理系统                                                                           |
|          [SmartOS](https://github.com/NewLifeX/SmartOS)          | 2014  | 嵌入式操作系统，完全独立自主，支持ARM Cortex-M芯片架构                                      |
|          [SmartA2](https://github.com/NewLifeX/SmartA2)          | 2019  | 嵌入式工业计算机，物联网边缘网关，高性能.NET8主机，应用于工业、农业、交通、医疗             |
|                          FIoT物联网平台                          | 2020  | 物联网整体解决方案，建筑、环保、农业，软硬件及大数据分析一体化，单机十万级点位项目验证      |
|                        UWB高精度室内定位                         | 2020  | 厘米级（10~20cm）高精度室内定位，软硬件一体化，与其它系统联动，大型展厅项目验证             |



## 新生命开发团队
![XCode](https://newlifex.com/logo.png)  

新生命团队（NewLife）成立于2002年，是新时代物联网行业解决方案提供者，致力于提供软硬件应用方案咨询、系统架构规划与开发服务。  
团队主导的80多个开源项目已被广泛应用于各行业，Nuget累计下载量高达400余万次。  
团队开发的大数据中间件NewLife.XCode、蚂蚁调度计算平台AntJob、星尘分布式平台Stardust、缓存队列组件NewLife.Redis以及物联网平台FIoT，均成功应用于电力、高校、互联网、电信、交通、物流、工控、医疗、文博等行业，为客户提供了大量先进、可靠、安全、高质量、易扩展的产品和系统集成服务。  

我们将不断通过服务的持续改进，成为客户长期信赖的合作伙伴，通过不断的创新和发展，成为国内优秀的IoT服务供应商。  

`新生命团队始于2002年，部分开源项目具有20年以上漫长历史，源码库保留有2010年以来所有修改记录`  
网站：https://newlifex.com  
开源：https://github.com/newlifex  
QQ群：1600800/1600838  
微信公众号：  
![智能大石头](https://newlifex.com/stone.jpg)  
