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

# 蚂蚁调度AntJob-分布式任务调度系统

分布式任务调度系统，纯NET打造的重量级大数据实时计算平台，万亿级调度经验积累！面向中小企业大数据分析场景。  

开源地址：[https://github.com/NewLifeX/AntJob](https://github.com/NewLifeX/AntJob)  
使用教程：[https://newlifex.com/blood/antjob](https://newlifex.com/blood/antjob)  
体验地址：[http://ant.newlifex.com](http://ant.newlifex.com/)  

# v4架构升级
v4版本是对v3版本的重构，主要是为了解决v3版本的一些问题，以及提供更多的功能。  
v4版本亮点：
1. []新增Http接入，由AntWeb提供调度服务，无需部署AntServer，满足轻量级项目需要。（进行中，等NewLife.Remoting提供WebsocketClient）
2. [x]增强定时调度，支持指定Cron表达式，逐步替代Start+Step的恒定间隔定时调度
3. []提前生成任务，提前下发给执行器，到时间后马上执行，提高任务执行时间精度。（待重新评估）
4. []支持任务主动延迟，任务在执行中发现数据条件未满足时，可以向调度中心请求延迟一段时间后再执行，增加执行次数但不增加错误次数
5. []扩充调度模式，常态化部署AntAgent，正式把Sql调度和C#调度加入主线，将来增加数据抽取和数据推送等多种调度模式

# 功能特点
AntJob的核心是**蚂蚁算法**：**把任意大数据拆分成为小块，采用蚂蚁搬家策略计算每一块！**  
（蚂蚁搬家，一个馒头掉在地上，众多小蚂蚁会把馒头掰成小块小块往家里般！）  

该算法设计于2008年，最开始用于处理基金公司的短信/邮件/传真群发（每批两百万）和电话话费分析（上百种国际长途计费规则），数据量不算大，但是有一定复杂度，并且要求支持持续处理（实时计算）以及出错重试。  

2016年在中通快递某产品项目中使用该算法进行大数据实时计算，成功挑战每日1200万的订单。并进一步发展衍生成为重量级实时计算平台，集分布式计算、集群调度、配置中心、负载均衡、故障转移、跨机房冗余、作业监控告警、百亿级数据清洗、超大Redis缓存（>2T）于一身，于2019年达到每年万亿级计算量（2019年双十一日订单量破亿）。  

AntJob是开源简化版，仅提供分布式计算和集中调度能力，支持百亿级调度。  

AntJob主要功能点：
1. 作业处理器。每一个最小业务模块实现一个处理器类，用于处理这一类作业。例如同步数据表时，每张表写一个处理器类，并在调度中心注册一个作业，调度中心按照作业时间切片得到任务，然后把任务（主要包含时间区间）分派给各个计算节点上的处理器类执行。又如，每天汇总计算是一个作业，而每月汇总计算又是另一个作业；
1. 任务上下文。作业处理器类实例化以后，将反复向调度中心申请任务来执行，每个任务的上下文核心数据是时间区间（数据调度）、时间点（定时调度）、消息体（消息调度）。调度中心记录任务处理结果；
1. 数据切片。支持按照时间区间（如5秒）把大数据切分为小片，也即是数据调度，处理过最大单表60亿行；
1. 定时调度。支持定时执行（秒级）指定业务逻辑，每个执行时间点得到一个任务；
1. 任务重试。每个任务完整记录处理结果，失败任务在延迟一段时间后将会自动重新分派（可能由原节点或其它节点执行）；
1. 任务重置。支持批量重置已执行完成的任务，让其再次执行处理；
1. 作业面板。在Web控制台上可查看每个应用所有作业的运行状态，或修改参数；
1. 作业重置。调整作业参数，让其再次处理某段时间的任务数据，例如重算过去一个月的数据；


# 定时调度
以下源码位于 [https://github.com/NewLifeX/AntJob/tree/master/Samples/HisAgent](https://github.com/NewLifeX/AntJob/tree/master/Samples/HisAgent) 

## 新建项目
新建.net core 3.1项目，从nuget引用 AntJob。实例化一个调度器Scheduler，配置网络提供者。  
```csharp
using System;
using AntJob;
using AntJob.Providers;
using NewLife.Log;

namespace HisAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            XTrace.UseConsole();

            var set = AntSetting.Current;

            // 实例化调度器
            var sc = new Scheduler();

            // 使用分布式调度引擎替换默认的本地文件调度
            sc.Provider = new NetworkJobProvider
            {
                Server = set.Server,
                AppID = set.AppID,
                Secret = set.Secret,
            };

            // 添加作业处理器
            sc.Handlers.Add(new HelloJob());

            // 启动调度引擎，调度器内部多线程处理
            sc.Start();

            Console.WriteLine("OK!");
            Console.ReadKey();
        }
    }
}
```
然后添加第一个定时调度的作业处理器  
```csharp
using System;
using AntJob;

namespace HisAgent
{
    internal class HelloJob : Handler
    {
        public HelloJob()
        {
            // 今天零点开始，每10秒一次
            var job = Job;
            job.Time = DateTime.Today;
            job.Step = 10;
        }

        protected override Int32 Execute(JobContext ctx)
        {
            // 当前任务时间
            var time = ctx.Task.Time;
            WriteLog("新生命蚂蚁调度系统！当前任务时间：{0}", time);

            // 成功处理数据量
            return 1;
        }
    }
}
```
作业处理器必须继承自Handler，并且重写Execute实现业务逻辑。  
我们这里的业务逻辑就是输出一行日志，其中的ctx.Task就是切分得到的任务上下文，Start是时间点。  
构造函数中设定的开始时间和步进Step，仅用于首次注册作业到调度中心，后面就没有用处了。  

为了编译观察，修改项目输出目录，在项目文件上点右键选“编辑项目文件”  
```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>netcoreapp3.1</TargetFramework>
  <AssemblyVersion>1.0.*</AssemblyVersion>
  <Deterministic>false</Deterministic>
  <OutputPath>..\..\Bin\HisAgent</OutputPath>
  <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
</PropertyGroup>
```

## 编译执行
代码能编译通过，先跑起来看看  
![image.png](https://cdn.nlark.com/yuque/0/2020/png/1144030/1586439204543-396a9f07-3340-4bd0-80c2-782c628cba0d.png#align=left&display=inline&height=192&name=image.png&originHeight=384&originWidth=1468&size=93962&status=done&style=none&width=734)  
可以看到，调度器首先连接 tcp://127.0.0.1:9999，其次 tcp://ant.newlifex.com:9999 ，而上面代码中并没有提及这两个地址。其实这就是调度中心地址，默认本地用于调试，如果链接失败再连接公开版调度中心，位于配置文件中：  

```csharp
/// <summary>蚂蚁配置。主要用于网络型调度系统</summary>
[Config("Ant")]
public class AntSetting : Config<AntSetting>
{
    #region 属性
    /// <summary>调试开关。默认false</summary>
    [Description("调试开关。默认false")]
    public Boolean Debug { get; set; }

    /// <summary>调度中心。逗号分隔多地址，主备架构</summary>
    [Description("调度中心。逗号分隔多地址，主备架构")]
    public String Server { get; set; } = "tcp://127.0.0.1:9999,tcp://ant.newlifex.com:9999";

    /// <summary>应用标识。调度中心以此隔离应用，默认当前应用</summary>
    [Description("应用标识。调度中心以此隔离应用，默认当前应用")]
    public String AppID { get; set; }

    /// <summary>应用密钥。</summary>
    [Description("应用密钥。")]
    public String Secret { get; set; }
    #endregion

    #region 方法
    /// <summary>重载</summary>
    protected override void OnLoaded()
    {
        if (AppID.IsNullOrEmpty())
        {
            var asm = Assembly.GetEntryAssembly();
            if (asm != null) AppID = asm.GetName().Name;
        }

        base.OnLoaded();
    }
    #endregion
}
```

其实上面Main函数中已经看到从配置文件里面读取Server+AppID+Secret，该配置类读取的配置文件在这：  
![image.png](https://cdn.nlark.com/yuque/0/2020/png/1144030/1586439584880-ea4aa91c-460d-4893-b849-8ac8b0995311.png#align=left&display=inline&height=340&name=image.png&originHeight=680&originWidth=1075&size=127959&status=done&style=none&width=537.5)  
AppID默认取本应用名，Secret由调度中心生成并下发。  
调度中心默认打开自动注册AutoRegistry，任意应用登录时自动注册，省去人工配置应用账号的麻烦。  
企业内部正式场景使用时，为安全起见，建议关闭自动注册。  

再来看看前面跑起来的日志  
```sql
21:33:08.470  1 N - 启动任务调度引擎[AntJob.Providers.NetworkJobProvider]，作业[1]项，定时5秒
21:33:08.471  1 N - HelloJob 开始工作 False 区间（2020-04-09 00:00:00, 0001-01-01 00:00:00） Offset=15 Step=10 MaxTask=8
21:33:08.587  5 Y Job HelloJob 停止工作
21:33:09.467  7 Y T [180.174.185.180:53926]上线！X3
```
启动了调度引擎，带有一个作业；  
作业HelloJob，就是我们通过 `sc.Handlers.Add(new HelloJob())`` `添加进去的作业处理器实例；  
HelloJob状态False，处于停止工作状态，那是因为作业注册后，默认都是停止状态，需要去web控制台配置参数后手工开启；  
最后一个xxx上线，这是蚂蚁调度的Peers功能，可以探测得到当前应用下所有已连接节点的状态。当HisAgent部署于多个服务器时，每个进程都可以通过Peers得知其它节点的存在；  

## 作业管理
不用关闭HistAgent客户端窗口，我们去线上web控制台看看 [http://ant.newlifex.com/](http://ant.newlifex.com/)  
![image.png](https://cdn.nlark.com/yuque/0/2020/png/1144030/1586440482188-a895c1b9-8c03-4966-ade8-f15d39d4bb68.png#align=left&display=inline&height=245&name=image.png&originHeight=491&originWidth=1359&size=93355&status=done&style=none&width=679.5)  
![image.png](https://cdn.nlark.com/yuque/0/2020/png/1144030/1586440526419-b2edc421-9ecb-4d4c-9879-fdaea412908f.png#align=left&display=inline&height=171&name=image.png&originHeight=342&originWidth=1616&size=82675&status=done&style=none&width=808)  
可以看到应用节点在线，点击应用名进去作业面板  
![image.png](https://cdn.nlark.com/yuque/0/2020/png/1144030/1586440600271-416571eb-8604-4a3c-9907-3fa5ebe1f6d6.png#align=left&display=inline&height=99&name=image.png&originHeight=197&originWidth=1631&size=55947&status=done&style=none&width=815.5)  
这就是我们的HelloJob作业，对应HisAgent中的HelloJob作业处理器。  
它处于停用状态，下一次执行时间是 00:00:00 ，也就是今天零点，加上10秒步进，也远小于当前时间，因此，只要启用该作业，调度中心将会马上开始切分任务，并分派给客户都执行。  
我们来点击红色叉叉，让它改变为启用状态  
![image.png](https://cdn.nlark.com/yuque/0/2020/png/1144030/1586440804204-a7a232d5-e640-4de7-b108-1155045edbb6.png#align=left&display=inline&height=394&name=image.png&originHeight=788&originWidth=1769&size=344111&status=done&style=none&width=884.5)  
几秒后，客户端HisAgent欢快地跑起来！它正在以10秒间隔不断切分并执行任务。  

刷新作业面板，可以看到，开始时间已经变为当前附近的时间，右边也有了执行次数。  
![image.png](https://cdn.nlark.com/yuque/0/2020/png/1144030/1586441093138-8b1c59fb-4277-4e45-a928-0c99465dba70.png#align=left&display=inline&height=95&name=image.png&originHeight=189&originWidth=1642&size=59847&status=done&style=none&width=821)  

点击作业名HelloJob，进去查看任务明细  
![image.png](https://cdn.nlark.com/yuque/0/2020/png/1144030/1586441195402-c2005a03-8298-4300-a6be-598d34ca1469.png#align=left&display=inline&height=602&name=image.png&originHeight=1204&originWidth=2156&size=353044&status=done&style=none&width=1078)  
任务切分后，插入作业任务表，此时状态为“就绪”，等待分发给客户端执行。  
客户端执行后，向调度中心报告执行结果，可能“完成”，可能“错误”。  
错误的任务，会在1分钟后，重新执行，最多连续错误10次。  

## 双跑，沸腾吧，分布式计算
再开两个HisAgent进程，查看应用在线表，可以看到有三个节点在线。  
![image.png](https://cdn.nlark.com/yuque/0/2020/png/1144030/1586442033394-770585b2-9fcd-41d4-a27d-afcfd32dfff9.png#align=left&display=inline&height=446&name=image.png&originHeight=892&originWidth=2053&size=482186&status=done&style=none&width=1026.5)  
HisAgent控制台中，可以看到各自都有机会分配了任务，每个任务有且仅有一个节点执行。  

刷新作业HelloJob的任务列表，可以看到不同客户端执行了不同的任务。  
![image.png](https://cdn.nlark.com/yuque/0/2020/png/1144030/1586442181551-b4cd0691-0e74-45ae-bf5f-d005a6ebcf2c.png#align=left&display=inline&height=249&name=image.png&originHeight=497&originWidth=1799&size=140083&status=done&style=none&width=899.5)  

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
计算应用->调度中心: 登录
调度中心->数据库: 连接
计算应用-->>调度中心2: 故障转移
调度中心2->数据库: 连接

计算应用2->调度中心: 登录
计算应用3->调度中心: 登录
计算应用4->调度中心: 登录

Web控制台-->调度中心: 监控
```

## 新生命项目矩阵
各项目默认支持net7.0/netstandard2.1/netstandard2.0/net4.61，旧版（2022.1225）支持net4.5/net4.0/net2.0  

|                               项目                               | 年份  | 说明                                                                                   |
| :--------------------------------------------------------------: | :---: | -------------------------------------------------------------------------------------- |
|                             基础组件                             |       | 支撑其它中间件以及产品项目                                                             |
|          [NewLife.Core](https://github.com/NewLifeX/X)           | 2002  | 核心库，日志、配置、缓存、网络、序列化、APM性能追踪                                    |
|    [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode)    | 2005  | 大数据中间件，单表百亿级，MySql/SQLite/SqlServer/Oracle/TDengine/达梦，自动分表        |
|      [NewLife.Net](https://github.com/NewLifeX/NewLife.Net)      | 2005  | 网络库，单机千万级吞吐率（2266万tps），单机百万级连接（400万Tcp）                      |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | 2011  | RPC通信框架，内网高吞吐或物联网硬件设备场景                                            |
|     [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube)     | 2010  | 魔方快速开发平台，集成了用户权限、SSO登录、OAuth服务端等，单表100亿级项目验证          |
|    [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent)    | 2008  | 服务管理组件，把应用安装成为操作系统守护进程，Windows服务、Linux的Systemd              |
|     [NewLife.Zero](https://github.com/NewLifeX/NewLife.Zero)     | 2020  | Zero零代脚手架，基于NewLife组件生态的项目模板，Web、WebApi、Service                    |
|                              中间件                              |       | 对接知名中间件平台                                                                     |
|    [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis)    | 2017  | Redis客户端，微秒级延迟，百万级吞吐，丰富的消息队列，百亿级数据量项目验证              |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | 2018  | RocketMQ纯托管客户端，支持Apache RocketMQ和阿里云消息队列，十亿级项目验                |
|     [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT)     | 2019  | 物联网消息协议，MqttClient/MqttServer，客户端支持阿里云物联网                          |
|      [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT)      | 2022  | IoT标准库，定义物联网领域的各种通信协议标准规范                                        |
|   [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus)   | 2022  | ModbusTcp/ModbusRTU/ModbusASCII，基于IoT标准库实现，支持IoT平台和IoTEdge               |
|  [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens)  | 2022  | 西门子PLC协议，基于IoT标准库实现，支持IoT平台和IoTEdge                                 |
|      [NewLife.Map](https://github.com/NewLifeX/NewLife.Map)      | 2022  | 地图组件库，封装百度地图、高德地图和腾讯地图                                           |
|       [NewLife.IP](https://github.com/NewLifeX/NewLife.IP)       | 2022  | IP地址库，IP地址转物理地址                                                             |
|                             产品平台                             |       | 产品平台级，编译部署即用，个性化自定义                                                 |
|           [AntJob](https://github.com/NewLifeX/AntJob)           | 2019  | 蚂蚁调度，分布式大数据计算平台（实时/离线），蚂蚁搬家分片思想，万亿级数据量项目验证    |
|         [Stardust](https://github.com/NewLifeX/Stardust)         | 2018  | 星尘，分布式服务平台，节点管理、APM监控中心、配置中心、注册中心、发布中心              |
|      [NewLife.ERP](https://github.com/NewLifeX/NewLife.ERP)      | 2021  | 企业ERP，产品管理、客户管理、销售管理、供应商管理                                      |
|         [CrazyCoder](https://github.com/NewLifeX/XCoder)         | 2006  | 码神工具，众多开发者工具，网络、串口、加解密、正则表达式、Modbus                       |
|           [XProxy](https://github.com/NewLifeX/XProxy)           | 2005  | 产品级反向代理，NAT代理、Http代理                                                      |
|        [HttpMeter](https://github.com/NewLifeX/HttpMeter)        | 2022  | Http压力测试工具                                                                       |
|         [GitCandy](https://github.com/NewLifeX/GitCandy)         | 2015  | Git源代码管理系统                                                                      |
|          [SmartOS](https://github.com/NewLifeX/SmartOS)          | 2014  | 嵌入式操作系统，完全独立自主，支持ARM Cortex-M芯片架构                                 |
|          [SmartA2](https://github.com/NewLifeX/SmartA2)          | 2019  | 嵌入式工业计算机，物联网边缘网关，高性能.NET6主机，应用于工业、农业、交通、医疗        |
|                           菲凡物联FIoT                           | 2020  | 物联网整体解决方案，建筑、环保、农业，软硬件及大数据分析一体化，单机十万级点位项目验证 |
|                           NewLife.UWB                            | 2020  | 厘米级（10~20cm）高精度室内定位，软硬件一体化，与其它系统联动，大型展厅项目验证        |

## 新生命开发团队
![XCode](https://newlifex.com/logo.png)  

新生命团队（NewLife）成立于2002年，是新时代物联网行业解决方案提供者，致力于提供软硬件应用方案咨询、系统架构规划与开发服务。  
团队主导的开源NewLife系列组件已被广泛应用于各行业，Nuget累计下载量高达60余万次。  
团队开发的大数据核心组件NewLife.XCode、蚂蚁调度计算平台AntJob、星尘分布式平台Stardust、缓存队列组件NewLife.Redis以及物联网平台NewLife.IoT，均成功应用于电力、高校、互联网、电信、交通、物流、工控、医疗、文博等行业，为客户提供了大量先进、可靠、安全、高质量、易扩展的产品和系统集成服务。  

我们将不断通过服务的持续改进，成为客户长期信赖的合作伙伴，通过不断的创新和发展，成为国内优秀的IT服务供应商。  

`新生命团队始于2002年，部分开源项目具有20年以上漫长历史，源码库保留有2010年以来所有修改记录`  
网站：https://newlifex.com  
开源：https://github.com/newlifex  
QQ群：1600800/1600838  
微信公众号：  
![智能大石头](https://newlifex.com/stone.jpg)  
