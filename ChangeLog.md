# AntJob 版本更新历史

## v5.1.2026.1028 (2026-10-28)

### 通信与调度
- **HTTP 通道完整化**：客户端按地址协议自动选择 HTTP(S) 或 TCP RPC，HTTP 通道补齐登录令牌、心跳保活、UTC 换算与增量 SetJob；直接对接 AntWeb 即可，无需独立 AntServer
- **WebSocket 下行通知**：新增 `/AntJob/Notify` 长连接，作业变更与消息到达即时下发；断线重连后自动补偿拉取，通知丢失由轮询兜底
- **注册中心开关**：新增 `UseRegistry`，可关闭星尘注册中心解析、直接使用配置地址；经注册中心解析时优先 AntWeb
- **消息型作业空闲周期可配置**：新增 `MessageIdlePeriod`（默认 15 秒），HTTP 通道收到通知即时唤醒，无需等待轮询

### AOT 与多语言
- **NativeAOT 就绪**：AntJob、AntJob.Extensions、AntJob.Data、AntJob.Server、AntJob.Agent 全量开启 AOT/裁剪分析器并做到 0 告警；新增 `Samples/AotSmoke` 并在 CI 增加 NativeAOT 发布冒烟，验证词表、调度器、扩展处理器与时间表达式在原生编译后仍可用
- **依赖注入可裁剪**：`Scheduler.AddHandler<T>` 与 `EntityBase<TEntity>` 补齐 `DynamicallyAccessedMembers` 标注，泛型注册不再被裁剪
- **全产品词表**：`AntJob`、`AntJob.Extensions`、`AntJob.Data`、`AntJob.Server`、`AntJob.Web` 五套词表统一覆盖 11 种语言，英文为源文与兜底；扩展库 4 条、数据层 1 条、Web 宿主 4 条（认证、grant_type、TaskID、WebSocket 通知）运行时消息改为英文源文
- **包元数据多语言**：`AntJob` 与 `AntJob.Extensions` 的 `AssemblyTitle`/`Description` 改用英文，`PackageDescription` 中英双语，`PackageTags` 补 `AOT`/`NativeAOT`/`本地化`，与 NewLife.Core 一致

### 可靠性与并发
- **上报幂等**：忽略任务终结后的重复与乱序上报，同一作业的上报串行更新统计，避免统计漂移
- **锁与调度**：作业锁超时不再继续分配任务，调度轮次禁止重入，单轮调度异常不再中断定时器
- **在线记录**：统一会话标识构建并限制长度，清理改为条件删除，并发登录唯一约束冲突后回读，RPC 调用不再逐次刷新在线记录
- **停机**：停止调度器时并行停止处理器并取消启动重试，连接失败时释放客户端与提供者，Web 停机释放后台清理定时器

### 性能优化
- **调度循环**：多作业并行申请任务，调度循环与动态处理器按名称建索引，处理器集合枚举与下行通知并发安全
- **通知链路**：同应用通知并发下发，通知会话按应用建立索引，转发不再重复反序列化，作业新增仅在启用时发送通知
- **消息型作业**：按通知定向唤醒并空闲退避，降低空转轮询开销
- **模板替换**：`TemplateHelper` 占位符查找改为 Ordinal，模板替换提升 37~61 倍

### 功能完善
- **SYS-18 C# 脚本处理器**：基于 Roslyn 执行任务携带的 C# 代码，脚本可访问作业上下文，返回值为成功处理数；net45 目标不支持
- **SYS-10 HttpJobProvider**：修正令牌端点为 `/AntJob/Token`、AddJobs 请求体与 Produce 路径；Web 端补齐 `SetJob` 接口、令牌动作路由与特性路由映射（`MapControllers`）
- **并发自动注册**：多客户端同时注册同名应用时，唯一约束冲突后自动重新加载，避免登录失败

### 依赖升级与工程
- **依赖升级**：NewLife.Core 12.0.2026.1024、NewLife.Remoting 4.0.2026.1024、NewLife.Stardust 3.10.2026.1024、NewLife.XCode 12.3.2026.1024、NewLife.Redis 7.0.2026.1024、NewLife.MySql 1.5.2026.1024 均为最新正式版；NewLife.Cube.Core 从 6.15 beta 切到 6.16.2026.1027 正式版
- **工具链**：Microsoft.NET.Test.Sdk 18.11.0、xunit.runner.visualstudio 4.0.1、coverlet.collector 10.1.0、Microsoft.SourceLink.GitHub 10.0.403
- **数据层目标框架**：`AntJob.Data` 增加 `net8.0` 目标使 AOT 分析器真正生效，`netstandard2.1` 保留兼容
- **解决方案迁移**：`蚂蚁.sln` 迁移为 `.slnx` 新格式
- **确定性构建**：版本号在确定性编译下改用固定程序集版本，兼容 BenchmarkDotNet 等构建器
- **打包质量**：`AntJob.Extensions` 与 `AntWeb` 补齐 `PackageReadmeFile` 与 `PackageIcon`，三个包打包告警清零

### 测试与文档
- **CI**：新增 win-x64 / linux-arm64 测试矩阵、riscv64 / s390x 指令集冒烟与 NativeAOT 发布冒烟；本地化词表（含同程序集多词表键冲突）、README 语言切换与结构一致性纳入门禁
- **发布流程**：publish 工作流同时推送 GitHub Packages 与 NuGet.org 并创建 GitHub Release；`publish-beta.yml` 触发路径纳入 `AntJob.Web/**`
- **README**：重新排版，顶部只保留标题、徽章与语言切换；英文为默认落地页，其余 10 种语言为同构完整翻译；新增 `check-readme-structure.ps1` 防止单语种漏改
- **测试与基准**：新增 `AntTest.Web` 集成测试项目与 `Benchmark/AntJob.Benchmarks`；架构设计补充消息调度时序、消息生产、主备切换与星尘集成说明，功能清单完成率 100%
- **发布基线**：这是 NuGet.org 上首个 5.x 正式包，`v4.5.2026.901` 之后的改动完整收录

### Bug 修复
- **[fix]** `TimeExpression.Build(String, DateTime)` 静态重载此前直接返回 `null`，现按模板首对花括号解析时间偏移并保留前后缀，无花括号时原样返回
- **[fix]** 手工实例化调度器（未经过依赖注入）启动不再空引用，并回落全局日志，控制台接入不再全程静默
- **[fix]** 作业未注册或未启用时输出明确提示，首次接入不再"看起来什么都没发生"
- **[fix]** 新建 Cron 定时作业以当前时间为基准计算下一次执行，避免启用后按客户端上报的当天零点补跑数小时时间片
- **[fix]** 示例预热数据层后再启动作业，消除首次执行与建表竞争导致的 `no such table`
- **[fix]** 示例默认关闭 XCode SQL 日志，避免日志被 SQL 淹没；示例程序集补齐标题、公司与版本信息
- **[fix]** `Samples/HelloWork` 迁移到当前接口（`Scheduler.Setting` + `StartAsync`），恢复可编译可运行
- **[fix]** 申请未知作业不再空引用，并限制单次申请数
- **[fix]** 应用消息数与登录写入改用应用编号并串行
- **[fix]** 会话超时配置真正生效
- **[fix]** 动态处理器按类名搜索已加载程序集，检查不再因单个节流跳过后续作业
- **[fix]** 注册作业后刷新应用作业数
- **[fix]** 消息消费后清理正确的去重键，生产消息时同步落库消息数
- **[fix]** 执行与上报异常不再抛到线程池
- **[fix]** 上报时记录链路标识并改进越权提示
- **[fix]** WebSocket 通知会话按客户端标识复用，旧会话销毁不再误删同编码的新会话
- **[fix]** `AntJob.Web` 打包内嵌 `AntJob.Data` 的路径改为实际产物所在的目标框架目录，避免干净工作区打包报 NU5026

---
## v4.5.2026.0902 (2026-09-02)

### 网络层优化
- **HttpJobProvider 全异步化**：重构为全异步接口并补全 `SetJob` 方法，避免阻塞线程；该提供者基于 HTTP REST API 与调度中心通信，适用于无 TCP 端口的场景，与 `NetworkJobProvider`（TCP RPC）功能等价

### 测试与质量
- **全量测试覆盖**：分批为数据层、处理器、SqlHandler、DataHandler 等核心模块补充 SQLite 集成测试与单元测试，显著提升测试覆盖率与可靠性

### 文档与协作
- **新增设计文档**：新增需求文档、功能清单与架构设计三份设计文档，并在 README 中增加文档索引
- **注释优化**：精简跨行 XML 注释为单行格式，提升 AI 与开发者阅读效率

### 工程优化
- **版本号生成修正**：修正版本号生成逻辑，规范程序集与打包输出
- **修复 TrimEnd 误用**：将多处 `TrimEnd` 修正为 `TrimSuffix`，确保正确处理区域名称；同时禁用 SourceLink
- **依赖更新**：升级各子项目 NuGet 依赖包至 2026 年 7 月最新版

---

## v4.4.2026.0501 (2026-05-01)

### 打包与工程优化
- **AntJob.Web 独立发包**：打包 AntJob.Web 时内嵌 AntJob.Data，无需单独发布两个 NuGet 包，简化用户依赖
- **CI 构建增强**：增强 AntJob.Web 多目标框架打包及 CI 输出路径灵活性，支持按环境变量控制目标框架
- **依赖更新**：更新各子项目 NuGet 依赖包版本；添加 `.csproj.lscache` 到 `.gitignore`

---

## v4.4.2026.0201 (2026-02-01)

### 主要变更
- 升级XCode拦截器架构，支持更灵活的数据访问拦截
- 升级.NET10.0支持，保持与最新.NET版本同步
- 实体类增强，完善作业、作业任务、作业错误等实体的字段和注释
- 更新基础组件到最新版本（NewLife.Core、NewLife.XCode、NewLife.Remoting、NewLife.Stardust）
- 优化GitHub Actions工作流配置

### 详细变更
- **XCode拦截器架构** (2026-02-01)
  - 升级XCode拦截器架构，增强数据访问层的扩展能力
  - 新增XCode专用指令文档 `.github/instructions/xcode.instructions.md`
  - 完善实体类注释和字段定义
  - 优化作业、作业任务、作业错误等实体的业务逻辑

- **框架升级** (2026-01-13)
  - 升级.NET10.0支持，所有Web项目和测试项目均支持最新.NET版本
  - 更新目标框架：AntJob.Agent、AntJob.Server、AntJob.Web、AntTest、HisAgent、HisWeb、Test

- **依赖更新**
  - 2026-01-24: 更新Copilot协作指令文档
  - 2026-01-14: 升级NuGet包依赖，更新基础组件
  - 2025-12-14: 优化GitHub Actions工作流
  - 2025-12-09: 更新基础组件版本

### 包依赖版本
- NewLife.Core: 11.11.2026.0201
- NewLife.XCode: 11.24.2026.0201
- NewLife.Remoting: 3.7.2026.0201
- NewLife.Stardust: 3.7.2026.0201

---

## v4.4.2025.0902 (2025-09-02)

### 主要变更
- 处理器Handler新增异步支持
- 增加僵死任务检测，特别解决调用C++非托管代码阻塞问题
- 调度通信全部使用异步处理

---

历史更早版本的变更记录请参考 Git 提交历史。
