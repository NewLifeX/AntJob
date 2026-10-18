using System;
using AntJob;
using AntJob.Data;
using NewLife;
using NewLife.Model;
using NewLife.Remoting.Clients;
using Xunit;

namespace AntTest;

/// <summary>蚂蚁配置测试</summary>
[Collection("Integration")]
public class AntSettingTests
{
    [Fact]
    public void Default_Server()
    {
        var set = new AntSetting();
        Assert.Equal("tcp://127.0.0.1:9999,tcp://ant.newlifex.com:9999", set.Server);
    }

    [Fact]
    public void Default_Debug_False()
    {
        var set = new AntSetting();
        Assert.False(set.Debug);
    }

    [Fact]
    public void Default_AppId_Null()
    {
        var set = new AntSetting();
        Assert.Null(set.AppId);
    }

    [Fact]
    public void Default_Secret_Null()
    {
        var set = new AntSetting();
        Assert.Null(set.Secret);
    }

    [Fact]
    public void Default_UseRegistry_True()
    {
        var set = new AntSetting();
        Assert.True(set.UseRegistry);
    }

    [Fact]
    public void Set_Properties()
    {
        var set = new AntSetting
        {
            Server = "tcp://localhost:1234",
            AppId = "MyApp",
            Secret = "mysecret",
            Debug = true,
            UseRegistry = false,
        };

        Assert.Equal("tcp://localhost:1234", set.Server);
        Assert.Equal("MyApp", set.AppId);
        Assert.Equal("mysecret", set.Secret);
        Assert.True(set.Debug);
        Assert.False(set.UseRegistry);
    }

    [Fact]
    public void IClientSetting_Code_Maps_To_AppId()
    {
        IClientSetting set = new AntSetting();
        set.Code = "TestApp";
        Assert.Equal("TestApp", (set as AntSetting).AppId);
        Assert.Equal("TestApp", set.Code);
    }
}

/// <summary>邻居伙伴模型测试</summary>
[Collection("Integration")]
public class PeerModelTests
{
    [Fact]
    public void Default_Properties()
    {
        var peer = new PeerModel();
        Assert.Null(peer.Instance);
        Assert.Null(peer.Client);
        Assert.Null(peer.Machine);
        Assert.Null(peer.Version);
        Assert.Equal(default, peer.CreateTime);
        Assert.Equal(default, peer.UpdateTime);
    }

    [Fact]
    public void Set_Properties()
    {
        var now = DateTime.Now;
        var peer = new PeerModel
        {
            Instance = "192.168.1.1:9999",
            Client = "192.168.1.1:12345",
            Machine = "Server01",
            Version = "4.4",
            CreateTime = now,
            UpdateTime = now,
        };

        Assert.Equal("192.168.1.1:9999", peer.Instance);
        Assert.Equal("192.168.1.1:12345", peer.Client);
        Assert.Equal("Server01", peer.Machine);
        Assert.Equal("4.4", peer.Version);
        Assert.Equal(now, peer.CreateTime);
        Assert.Equal(now, peer.UpdateTime);
    }
}

/// <summary>DI扩展测试</summary>
[Collection("Integration")]
public class AntJobExtensionsTests
{
    [Fact]
    public void AddAntJob_Returns_Scheduler()
    {
        var ioc = ObjectContainer.Current;
        var scheduler = ioc.AddAntJob();
        Assert.NotNull(scheduler);
    }

    [Fact]
    public void AddAntJob_WithServer_Sets_Server()
    {
        var ioc = ObjectContainer.Current;
        var scheduler = ioc.AddAntJob("tcp://myserver:9999", "MyApp", "secret");

        Assert.NotNull(scheduler);
        // AddAntJob 设置 AntSetting.Current，而非 scheduler.Setting
        // AntSetting 由 AntJobWorker 在后台服务启动时注入 scheduler
        Assert.Equal("tcp://myserver:9999", AntSetting.Current.Server);
        Assert.Equal("MyApp", AntSetting.Current.AppId);
        Assert.Equal("secret", AntSetting.Current.Secret);
    }

    [Fact]
    public void AddAntJob_Registers_Scheduler_InContainer()
    {
        var ioc = ObjectContainer.Current;
        var scheduler = ioc.AddAntJob();

        var resolved = ioc.GetService<Scheduler>();
        Assert.Same(scheduler, resolved);
    }
}
