# AntJob 版本更新历史

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
