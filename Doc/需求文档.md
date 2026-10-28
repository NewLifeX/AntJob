# AntJob 蚂蚁调度 — 需求文档

> 版本：v5.0 | 日期：2026-10-18 | 从代码逆向整理

本文档描述 AntJob 分布式任务调度系统的愿景、核心目标和功能方向。完成状态在[功能清单](功能清单.md)中追踪，详细设计在[架构设计](架构设计.md)中展开。

---

## 1. 背景与愿景

### 1.1 系统定位

AntJob 是一个面向 .NET 生态的**分布式任务调度与大数据实时计算平台**。以"蚂蚁搬家"思想把任意大任务切分成可并行的小片段，适用于海量数据持续抽取、按时间窗口滚动汇总、数据补算/回溯修复等场景。

| 产品/工程 | 定位 |
|-----------|------|
| `AntJob` (NuGet: NewLife.AntJob) | 客户端 SDK，提供调度器、处理器基类、作业提供者接口 |
| `AntJob.Extensions` (NuGet) | 扩展调度模式：SQL 驱动、消息驱动、C# 动态脚本 |
| `AntJob.Data` | XCode ORM 数据层，作业/任务/消息/错误等 6 张核心表 |
| `AntJob.Server` | 调度中心服务端，基于 Remoting TCP RPC（兼容通道），负责任务分片与分配 |
| `AntJob.Web` | Cube MVC 可视化管理后台，同时提供 HTTP API 与 WebSocket 下行通知 |
| `AntJob.Agent` | 独立执行代理，接收调度中心下发的 C#/SQL 任务 |

### 1.2 愿景

企业只需编写一个 Handler 类，即可获得分布式调度、弹性伸缩、失败重试、数据切片、可视化监控等全部能力——把调度复杂度交给框架，业务开发者只关心"处理一条数据"。

---

## 2. 核心目标

| 编号 | 目标 | 所属层级 | 一句话描述 |
|------|------|----------|------------|
| SYS | 核心调度引擎 | 基础层 | 提供多模式统一调度能力（定时/数据窗口/消息驱动），管理 Handler 生命周期与任务分发 |
| DATA | 数据持久化 | 基础层 | XCode ORM 实体层，持久化作业配置、任务实例、错误记录、消息队列 |
| EXT | 扩展调度 | 核心层 | 提供 SQL 数据驱动、消息驱动 SQL、C# 动态脚本等高级调度模式 |
| SRV | 调度中心服务 | 核心层 | RPC 服务端，负责任务切片生成、分配、重试策略、集群弹性伸缩 |
| WEB | 可视化管理 | 表现层 | Cube MVC 后台，应用/作业/任务/消息/错误的 CRUD 管理与监控看板 |
| AGT | 蚂蚁代理 | 工具层 | 独立可执行代理程序，接收并执行调度中心下发的 C#/SQL 任务 |
| TST | 自动化测试 | 质量层 | xUnit 单元测试，覆盖核心调度逻辑、时间表达式、模板引擎、SQL 解析 |

### 2.1 非功能目标

| 目标 | 说明 |
|------|------|
| 原生 AOT | 核心 SDK、扩展库、数据层、服务端与代理常驻 AOT/裁剪分析器并保持 0 告警，可在 NativeAOT 发布的应用中直接引用；Web 宿主受 ASP.NET Core MVC/Razor 上游限制不参与 AOT |
| 运行时多语言 | 五套 `Locales` 词表各覆盖 11 种语言，英文为源文与兜底，缺译只回退不报错；NuGet 包元数据中英双语 |
| 跨目标框架 | 核心 SDK 覆盖 net45 至 net8.0 共 5 个目标框架，入口程序统一 net10.0 |

> 编码说明：采用字母前缀方案。存量项目可继续使用 Mx- 编码。

---

## 3. 功能需求

### 3.1 SYS — 核心调度引擎

- **SYS-1 调度器 Scheduler**：核心调度引擎，管理 Handler 集合，从 DI 容器发现处理器，连接调度中心并启停调度。
- **SYS-2 处理器基类 Handler**：作业处理器抽象基类，提供 Init→Start→Acquire→Process→Execute→OnFinish 完整生命周期，支持同步/异步双模。
- **SYS-3 任务上下文 JobContext**：封装单次任务执行的全部状态（参数、结果、数据、异常、耗时、速度）。
- **SYS-4 调度模式 JobModes**：三种模式——Data(数据窗口, 值=1)、Time(定时/Cron, 值=2)、Message(消息驱动, 值=3)。
- **SYS-5 任务状态 JobStatus**：七态状态机——就绪→抽取中→处理中→完成/错误/取消/延迟重试。
- **SYS-6 作业与任务模型**：IJob/JobModel/ITask/TaskModel，含 DataTime/End/Step/Offset/BatchSize/MaxTask 等调度参数。
- **SYS-7 作业提供者接口**：IJobProvider 定义 Acquire/Produce/Report/Finish/GetJobs/SetJob 六个核心操作。
- **SYS-8 网络提供者**：基于 NewLife.Remoting TCP RPC 连接调度中心，自动上报作业、申请任务、报告进度。
- **SYS-9 文件提供者**：XML 文件持久化作业配置，适合离线/单机调试场景。
- **SYS-10 Http 提供者**：HTTP 调度接入通道，与 NetworkJobProvider 功能等价；对接 Web 端 `/AntJob/*` 令牌与业务接口，适用于无 TCP 端口的环境。
- **SYS-11 蚂蚁客户端 AntClient**：RPC 客户端，封装 Login/GetJobs/AddJobs/Acquire/Report/Finish 等远程调用。
- **SYS-12 客户端配置 AntSetting**：Server(多地址主备)/AppID/Secret/Debug 配置。
- **SYS-13 DI 集成**：AddAntJob() 扩展方法，注册 Scheduler/AntJobWorker/AntSetting 到 DI 容器。
- **SYS-14 后台服务**：BackgroundService 封装，自动初始化并启动调度器。
- **SYS-15 模板助手**：SQL 模板变量替换 {dt}/{End}/{Message}，支持自定义格式化字符串。
- **SYS-16 时间表达式**：解析 {dt+1M+5d:yyyyMMdd} 等时间偏移表达式，支持 y/M/d/H/m/s/w 单位。
- **SYS-17 消息处理器**：消息驱动调度基类，Topic 订阅 + JSON 消息解码 + 逐条 ProcessItem。
- **SYS-18 C# 脚本处理器**：基于 Roslyn 执行任务携带的 C# 代码，脚本内可访问作业上下文，返回值作为成功处理数；不做沙箱隔离，仅用于可信代码；net45 目标不支持。
- **SYS-19 消息选项**：消息投递选项——延迟执行/消息去重/跨应用投递。
- **SYS-20 HTTP 通道完整化**：HTTP 提供者与 RPC 提供者共用同一套客户端与作业流程，补齐登录令牌、心跳保活、UTC 时间换算、增量 SetJob 与消息选项；`Scheduler` 按地址协议自动选择通道。
- **SYS-21 WebSocket 下行通知**：客户端登录后经 HTTP 心跳建立 WebSocket 长连接，接收作业变更与消息到达信号；信号只用于降低延迟，客户端收到后仍通过 HTTP 拉取权威数据，通知丢失由兜底轮询修复。消息型作业在无消息时进入空闲退避，收到对应主题通知后立即复位。
- **SYS-22 统一远程提供者**：NetworkJobProvider 与 HttpJobProvider 共用基础实现与 AntClient，避免两套通信代码在时间、参数语义上漂移。

### 3.2 DATA — 数据持久化

- **DATA-1 应用系统实体 App**：多租户应用隔离，Name/Secret/Enable/Version/JobCount。
- **DATA-2 应用在线实体 AppOnline**：在线实例追踪（心跳更新），Instance/Client/统计(Tasks/Total/Success/Error/Cost/Speed)。
- **DATA-3 作业实体 Job**：作业完整定义，Mode/Cron/Step/Offset/MaxTask/控制参数(MaxError/MaxRetry/MaxTime)/统计/QuietTime免打扰。
- **DATA-4 作业任务实体 JobTask**：任务实例，DataTime/End/Status/Client/统计(Speed/Cost/FullCost)/Key/Data/TraceId。
- **DATA-5 作业错误实体 JobError**：错误追踪记录，关联 Job/Task，记录异常信息和执行上下文。
- **DATA-6 应用历史实体 AppHistory**：操作审计日志（雪花Id分表），Action/Success/TraceId。
- **DATA-7 应用消息实体 AppMessage**：消息队列存储（雪花Id分表），Topic/Data/DelayTime，支持延迟消费。
- **DATA-8 Model.xml 数据模型**：XCode ORM 完整表结构定义，含 DataScale 分表、Map 外键、Index 索引。

### 3.3 EXT — 扩展调度

- **EXT-1 数据处理作业 DataHandler**：数据窗口调度基类，按时间字段分批 Fetch→Execute，自动识别雪花Id/主时间字段，支持 KeepFirstPage 模式。
- **EXT-2 SQL 语句处理器 SqlHandler**：定时执行 SQL 语句集，支持 Query/Execute/Insert 三种操作，自动事务管理（提交/回滚）。
- **EXT-3 SQL 消息处理器 SqlMessage**：消息驱动 SQL 执行，解析 SELECT 结果中 topic_ 前缀列自动生产消息。
- **EXT-4 SQL 片段解析 SqlSection**：多 SQL 语句解析器，/*use connName*/ 指定连接名，双换行分隔片段，自动识别操作类型。

### 3.4 SRV — 调度中心服务

- **SRV-1 蚂蚁 RPC 服务 AntService**：IApi 服务层，IActionFilter 鉴权拦截，暴露 Login/GetJobs/AddJobs/SetJob/Acquire/Report/Finish。
- **SRV-2 应用认证服务 AppService**：应用登录认证（SaltPasswordProvider md5）、自动注册、Ping 心跳、在线实例管理。
- **SRV-3 作业调度服务 JobService**：核心调度逻辑——作业 CRUD、任务分配（延迟重试→错误重试→时间切片→消息消费）、全局 Redis 锁防并发。
- **SRV-4 Worker 后台服务**：ApiServer 启动(端口9999)、定时清理在线过期/过期任务、星尘注册中心注册。
- **SRV-5 服务端配置 AntJobSetting**：Port/TokenSecret/TokenExpire/SessionTimeout/AutoRegistry。
- **SRV-6 Program 入口**：DI 注册（Stardust/Cache/AppService/JobService/密码提供者/EntityFactory 预热）。
- **SRV-7 作业通知器**：订阅作业调度字段变更与消息生产事件，通过事件总线向在线客户端发布通知；单机使用内存队列，集群使用 Redis Stream 广播。

### 3.5 WEB — 可视化管理后台

- **WEB-1a AntJob HTTP API**：REST API 控制器，JWT 鉴权，提供 Login/GetJobs/AddJobs/Acquire 等 HTTP 端点。
- **WEB-2a 蚂蚁区域注册**：Cube 区域"蚂蚁调度"，AntEntityController\<T\> 基类支持 appId 导航栏。
- **WEB-3a 应用系统管理接口**：应用 CRUD 数据操作。
- **WEB-3b 应用系统管理页面**：应用列表页，含在线/作业/任务/消息/错误/历史快捷链接。
- **WEB-4a 作业管理接口**：作业 CRUD 数据操作。
- **WEB-4b 作业管理页面**：作业列表页，模式彩色标记，Cron/下一次时间显示，Success/Error 彩色数字。
- **WEB-5a 任务管理接口**：任务查询与批量重置操作。
- **WEB-5b 任务管理页面**：任务列表页，按模式彩色渲染时间，状态颜色，支持批量重置。
- **WEB-6a 在线实例接口**：在线节点 CRUD 数据操作。
- **WEB-6b 在线实例页面**：在线实例列表，查看各节点统计信息。
- **WEB-7a 操作历史接口**：审计日志查询。
- **WEB-7b 操作历史页面**：应用审计日志列表查看。
- **WEB-8a 消息管理接口**：消息队列 CRUD 数据操作。
- **WEB-8b 消息管理页面**：消息队列查看管理。
- **WEB-9a 错误管理接口**：错误记录查询。
- **WEB-9b 错误管理页面**：作业错误列表查看。
- **WEB-10 Startup 启动配置**：AddCube/AddStardust/EntityFactory 预热 4 个连接（Membership/Log/Cube/Ant）。
- **WEB-1b WebSocket 通知端点**：`/AntJob/Notify` 建立长连接，按应用前缀向本机所有实例会话分发通知；与 HTTP API 共用令牌鉴权。

### 3.6 AGT — 蚂蚁代理

- **AGT-1 Agent 入口程序**：独立可执行代理，引用 AntJob + AntJob.Extensions，接收调度中心下发的 C#/SQL 任务并执行。

### 3.7 TST — 自动化测试

- **TST-1 作业逻辑测试**：CheckQuiet 免打扰时段计算（跨天/多时段/边界）。
- **TST-2 时间表达式测试**：ParseItem/Execute/TestDefault。
- **TST-3 模板助手测试**：Build 时间变量/消息变量替换。
- **TST-4 SQL 处理器测试**：Execute 多语句执行。
- **TST-5 SQL 片段解析测试**：ParseQuery/Insert/Delete 解析验证。

---

## 4. 🧊 暂缓清单

| 功能 | 暂缓理由 | 解冻前提 |
|------|----------|----------|
| C# 脚本执行沙箱 | CSharpHandler.Execute 主体逻辑为空（仅抛出 NotImplementedException），安全沙箱需设计 | 确定脚本安全策略与运行时隔离方案 |
| SSE 下行通道 | 企业网关对 SSE 支持弱，且已有 WebSocket 与兜底轮询 | 出现明确无法使用 WebSocket 的部署场景 |

---

## 5. 不做什么

- 不提供 DAG 工作流编排（核心稳定后可作为可选扩展层）
- 不内置告警通知渠道（邮件/Webhook 等为 Roadmap 规划）
- 不替代消息队列（MQ），仅做任务调度层面的消息驱动
