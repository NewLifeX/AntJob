# AntJob 版本更新历史

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
