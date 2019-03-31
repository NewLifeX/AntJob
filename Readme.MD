#星尘
AntJob  蚂蚁调度，轻量级调度系统！  

### 设计概要  
计算型应用（实现IJob）  

```sequence{theme="simple"}
计算应用->调度中心: app登录
note over 调度中心: app/secret
计算应用-->>调度中心: 注册作业

Web控制台->调度中心: 设置参数
Web控制台->调度中心: 启动作业

计算应用->调度中心: 申请作业分片
调度中心->计算应用: 返回分片
note over 计算应用: 多线程处理任务

计算应用-->调度中心: 上报局部状态
note over Web控制台: 作业状态看板
计算应用->调度中心: 处理成功
计算应用-->调度中心: 处理失败
```

### 系统架构  
调度中心主从架构  

```sequence{theme="simple"}
计算应用->调度中心: app登录
note over 调度中心: app/secret
计算应用-->>调度中心2: 故障转移

计算应用2->调度中心: app登录
计算应用3->调度中心: app登录
计算应用4->调度中心: app登录

Web控制台->调度中心: 设置参数
Web控制台->调度中心: 启动作业

计算应用->调度中心: 申请作业分片
调度中心->计算应用: 返回分片
note over 计算应用: 多线程处理任务

计算应用-->调度中心: 上报局部状态
note over Web控制台: 作业状态看板
计算应用->调度中心: 处理成功
计算应用-->调度中心: 处理失败
```

## 新生命开源项目矩阵

|                               项目                               | 年份  |  状态  | .NET Core | 说明                                               |
| :--------------------------------------------------------------: | :---: | :----: | :-------: | -------------------------------------------------- |
|          [NewLife.Core](https://github.com/NewLifeX/X)           | 2002  | 维护中 |     √     | 日志、网络、RPC、序列化、缓存、多线程 |
|              [XCode](https://github.com/NewLifeX/X)              | 2005  | 维护中 |     √     | 数据中间件，MySQL、SQLite、SqlServer、Oracle       |
|     [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube)     | 2010  | 维护中 |     √     | Web魔方，权限基础框架，集成OAuth                   |
|    [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis)    | 2017  | 维护中 |     √     | Redis客户端，微秒级延迟，百亿级项目验证            |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | 2018  | 维护中 |     √     | 支持Apache RocketMQ和阿里云消息队列                |
|       [NewLife.MQ](https://github.com/NewLifeX/NewLife.MQ)       | 2016  | 维护中 |     √     | 轻量级消息队列                                     |
|         [Stardust](https://github.com/NewLifeX/Stardust)         | 2018  | 开发中 |     √     | 星尘，微服务平台，学习Dubbo                        |
|           [XAgent](https://github.com/NewLifeX/XAgent)           | 2008  | 重构中 |     √     | 集群节点分布式计算平台，学习Hadoop                 |
|           [AntJob](https://github.com/NewLifeX/AntJob)           | 2019  | 开发中 |     √     | 蚂蚁调度系统，大数据实时计算平台                 |
|             [NoDb](https://github.com/NewLifeX/NoDb)             | 2017  | 开发中 |     √     | NoSQL数据库，百万级读写性能，学习Redis+MongoDB     |
|    [NewLife.Cache](https://github.com/NewLifeX/NewLife.Cache)    | 2018  | 维护中 |     √     | 自定义缓存服务器                                   |
|            [XLink](https://github.com/NewLifeX/XLink)            | 2016  | 维护中 |     √     | 物联网云平台                                       |
|      [NewLife.Net](https://github.com/NewLifeX/NewLife.Net)      | 2005  | 维护中 |     √     | 网络库，千万级吞吐率，学习gRPC、Thrift             |
|           [XCoder](https://github.com/NewLifeX/XCoder)           | 2006  | 维护中 |     ×     | 码神工具，开发者必备                               |
|           [XProxy](https://github.com/NewLifeX/XProxy)           | 2005  | 维护中 |     √     | 产品级反向代理                                     |
|          [XScript](https://github.com/NewLifeX/XScript)          | 2010  | 维护中 |     ×     | C#脚本引擎                                         |
|        [XTemplate](https://github.com/NewLifeX/XTemplate)        | 2008  | 维护中 |     ×     | 模版引擎，学习TT                                   |
|      [NewLife.DNS](https://github.com/NewLifeX/NewLife.DNS)      | 2011  | 维护中 |     ×     | DNS代理服务器                                      |
|      [NewLife.CMX](https://github.com/NewLifeX/NewLife.CMX)      | 2013  | 维护中 |     ×     | 内容管理系统                                       |
|          [SmartOS](https://github.com/NewLifeX/SmartOS)          | 2014  | 保密中 |   C++11   | 嵌入式操作系统，面向ARM Cortex-M芯片架构           |
|         [GitCandy](https://github.com/NewLifeX/GitCandy)         | 2015  | 维护中 |     ×     | Git管理系统                                        |
|    [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql)    | 2018  | 开发中 |     √     | MySql驱动                                          |
|       [X组件 .NET2.0](https://github.com/NewLifeX/X_NET20)       | 2002  | 存档中 |  .NET2.0  | 日志、网络、RPC、序列化、缓存、Windows服务、多线程 |
|       [X组件 .NET4.0](https://github.com/NewLifeX/X_NET40)       | 2002  | 存档中 |  .NET4.0  | 日志、网络、RPC、序列化、缓存、Windows服务、多线程 |

#### 新生命开发团队
`新生命团队始于2002年，部分开源项目具有15年以上漫长历史，源码库保留有2010年以来所有修改记录，并一直保持更新，请确保获取得到最新版本源代码`
网站：http://www.NewLifeX.com  
国内：http://git.NewLifeX.com  
国外：https://github.com/NewLifeX  
博客：https://nnhy.cnblogs.com  
QQ群：1600800/1600838  
