# AntJob 版本更新历史

## v4.5.2026.1018 (2026-10-18)

### 工程升级
- **解决方案迁移**：`蚂蚁.sln` 迁移为 `.slnx` 新格式
- **依赖升级**：NewLife.* 全部升级到最新 beta（Core 12.0、XCode 12.2、Remoting 3.9、Stardust 3.10、Cube.Core 6.15、Redis 6.6、MySql 1.4）
- **确定性构建**：版本号在确定性编译下改用固定程序集版本，兼容 BenchmarkDotNet 等构建器

### 功能完善
- **SYS-18 C# 脚本处理器**：基于 Roslyn 执行任务携带的 C# 代码，脚本可访问作业上下文，返回值为成功处理数；net45 目标不支持
- **SYS-10 HttpJobProvider**：修正令牌端点为 `/AntJob/Token`、AddJobs 请求体与 Produce 路径；Web 端补齐 `SetJob` 接口、令牌动作路由与特性路由映射（`MapControllers`）
- **并发自动注册**：多客户端同时注册同名应用时，唯一约束冲突后自动重新加载，避免登录失败
- **模板替换优化**：`TemplateHelper` 占位符查找改为 Ordinal，模板替换提升 37~61 倍

### 测试与文档
- **新增 Web 集成测试项目**：`AntTest.Web`；补齐 RPC 客户端、服务端、C# 处理器、代理组装与 Web API/页面路由测试，本地共 195 项测试通过
- **性能基准**：新增 `Benchmark/AntJob.Benchmarks`，报告见 `Doc/Benchmark/核心组件性能测试.md`
- **文档补齐**：架构设计补充消息调度时序、`topic_` 消息生产、主备切换与星尘集成说明；功能清单完成率 100%

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
