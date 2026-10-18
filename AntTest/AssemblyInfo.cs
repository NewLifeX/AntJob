using Xunit;

// 集成测试共享 XCode 全局连接配置（DAL.AddConnStr），必须串行执行
[assembly: CollectionBehavior(DisableTestParallelization = true)]
