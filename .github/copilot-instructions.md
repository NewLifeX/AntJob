# 项目概述

本项目是蚂蚁调度(AntJob)，一个分布式任务调度系统的MIT开源项目，属于NewLife框架的一员，支持net45/net461/netstandard2.0/netstandard2.1/net8.0等主流.Net版本。

蚂蚁调度是纯NET打造的重量级大数据实时计算平台，基于**蚂蚁算法**核心理念：**把任意大数据拆分成为小块，采用蚂蚁搬家策略计算每一块！** 该算法设计于2008年，在中通快递等项目中成功挑战每日1200万订单，并在2019年达到万亿级调度经验积累。

## 项目地址

- 源码地址：https://github.com/NewLifeX/AntJob
- 文档地址：https://newlifex.com/blood/antjob
- 体验地址：http://ant.newlifex.com
- NuGet包：NewLife.AntJob、NewLife.AntJob.Extensions

## 文件夹结构

- `/AntJob`：核心SDK类库，包含调度器、处理器、作业提供者等核心组件
- `/AntJob.Server`：调度中心服务端，负责任务分发和状态管理  
- `/AntJob.Agent`：蚂蚁代理，支持C#或Sql代码的动态执行
- `/AntJob.Web`：Web管理界面，基于NewLife.Cube构建
- `/AntJob.Data`：数据层，包含作业、任务等实体模型
- `/AntJob.Extensions`：扩展库，提供依赖注入等功能
- `/Samples`：示例项目，包括HisAgent、HisData、HisWeb等
- `/Test`和`/AntTest`：测试项目
- `/Doc`：项目文档、图标和签名证书newlife.snk

## 技术栈和框架特性

### 支持的.NET版本
- **.NET Framework**: 4.5, 4.6.1
- **.NET Standard**: 2.0, 2.1  
- **.NET**: 8.0

### 核心功能模块
1. **调度器(Scheduler)**: 作业调度的核心引擎，管理处理器生命周期和任务分发
2. **处理器(Handler)**: 作业处理器基类，每个业务模块继承实现Execute方法
3. **作业提供者(IJobProvider)**: 任务来源提供者，支持网络调度、本地文件等
4. **任务上下文(JobContext)**: 封装任务执行环境和状态信息
5. **调度模式**: 支持定时调度(Timer)、数据调度(Data)、消息调度(Message)、Cron表达式
6. **分布式通信**: 基于NewLife.Remoting实现调度中心与执行节点的通信

### 核心概念
- **作业(Job)**: 业务处理单元的抽象，包含调度参数和执行配置
- **任务(Task)**: 作业按时间或数据切片后的执行单元
- **处理器(Handler)**: 具体业务逻辑的实现类，继承Handler基类
- **调度中心**: 集中管理作业配置、任务分发和执行监控
- **执行节点**: 部署处理器的客户端，接收并执行任务

### 技术要求
- **C#版本**: 最新版(latest)，启用nullable和implicit usings
- **编译**: 支持Visual Studio和dotnet CLI
- **强命名**: 使用newlife.snk证书进行强命名签名
- **文档**: 生成XML文档文件用于IntelliSense

## 编码规范

### 基本规范
- **基础类型**: 使用.Net类型名而不是C#关键字（如String而不是string，Int32而不是int，Boolean而不是bool）
- **语法**: 使用最新版C#语法来简化代码，例如自动属性、模式匹配、表达式主体成员、record类型等
- **命名**: 遵循Pascal命名法用于类型和公共成员，camelCase用于参数，camelCase加下划线前缀用于私有字段
- **换行**: if语句后只有一行代码时不使用大括号，并且跟if在同一行；多行代码时使用大括号并另起一行

### 文档注释要求
- 所有公开的类或成员都需要编写XML文档注释
- summary标签头尾放在同一行，内容简洁明了
- 如果注释内容过长则增加remarks标签来补充详细说明
- 使用param和returns标签描述参数和返回值
- 示例格式：
```csharp
/// <summary>获取或设置作业名称</summary>
/// <remarks>用于标识不同的作业实例，支持多作业并存</remarks>
public String Name { get; set; }

/// <summary>异步申请任务</summary>
/// <param name="count">要申请的任务个数</param>
/// <returns>任务数组</returns>
public virtual Task<ITask[]> Acquire(Int32 count)
```

### 异步编程规范
- 优先使用async/await模式，避免直接使用.Result或.GetAwaiter().GetResult()
- 异步方法名以Async结尾
- 使用ConfigureAwait(false)避免死锁，除非需要回到原始上下文
- 提供同步和异步版本的重载方法时，明确区分使用场景

### 错误处理
- 使用具体的异常类型而不是通用Exception
- 需要抛出带有错误码的异常时，优先使用ApiException，并优先考虑ApiCode枚举中符合需要的错误码
- 在日志中记录异常信息，使用XTrace.WriteException
- 对于性能敏感的代码，考虑使用try-parse模式而不是异常
- 私有方法内部不需要捕获异常，也不需要校验输入参数
- 修改代码时，不必增加try-catch，除非有明确的异常处理需求

### 性能优化原则
- 使用对象池减少GC压力
- 优先使用Span<T>和Memory<T>处理大数据
- 避免不必要的字符串拼接，使用StringBuilder或字符串插值
- 频繁创建StringBuiler或MemoryStream时，使用对象池Pool中的StringBuilder和MemoryStream池
- 合理使用缓存避免重复计算

## 测试规范

### 单元测试
- 使用xUnit测试框架
- 测试类放在AntTest项目中
- 测试方法使用[Fact]或[Theory]特性
- 测试方法名称要清晰表达测试意图
- 使用DisplayName提供中文描述

### 测试组织
- 按功能模块组织测试目录结构
- 每个类对应一个测试类，命名为{ClassName}Tests
- 复杂功能提供多个测试方法覆盖不同场景
- 使用临时文件进行文件操作测试，确保测试清理

## 项目特定注意事项

### AntJob核心概念理解
- **Handler处理器**: 每个作业一个处理器类，继承Handler基类实现Execute方法
- **Scheduler调度器**: 管理多个处理器，负责任务分发和生命周期管理
- **JobContext上下文**: 包含任务参数、执行状态、处理结果等完整信息
- **IJobProvider提供者**: 抽象任务来源，支持NetworkJobProvider、FileJobProvider等实现

### 调度模式设计
- **定时调度(JobModes.Time)**: 基于时间点或Cron表达式的定时执行
- **数据调度(JobModes.Data)**: 基于数据时间区间的批量处理
- **消息调度(JobModes.Message)**: 基于消息队列的事件驱动处理
- **混合调度**: 支持多种模式组合使用

### 兼容性考虑
- 代码需要同时支持.NET Framework 4.5到.NET 8.0
- 使用条件编译指令处理平台差异(如#if NET5_0_OR_GREATER)
- 注意不同目标框架的API差异

### 依赖管理
- 核心依赖：NewLife.Core、NewLife.Remoting、NewLife.Stardust
- 尽量减少外部依赖，保持核心库的轻量级
- 不同目标框架使用不同的包引用策略

### 分布式架构设计
- 调度中心采用主从架构，支持多地址故障转移
- 执行节点支持水平扩展，自动负载均衡
- 任务状态统一管理，支持失败重试和延迟执行
- 网络通信基于NewLife.Remoting，支持TCP/HTTP协议

### 安全和性能
- 启用强命名程序集确保安全性
- 支持应用隔离和密钥验证
- 关注内存使用和GC性能，特别是大数据处理场景
- 网络操作考虑并发性能和连接池管理

### 日志和诊断
- 使用ILog接口进行日志记录，调度器和处理器都有Log属性
- 支持APM性能追踪，使用ITracer进行链路追踪
- 提供详细的错误信息用于问题诊断
- 考虑不同日志级别的性能影响

### Web管理界面
- 基于NewLife.Cube框架构建
- 支持作业配置、任务监控、状态查看
- 遵循Cube的MVC模式和页面布局规范
- 支持应用级别的数据隔离

### 扩展开发指南
- 继承Handler类实现自定义业务处理器
- 实现IJobProvider接口扩展任务来源
- 使用依赖注入容器管理处理器生命周期
- 遵循单一职责原则，每个Handler处理一类业务
