using System;
using System.IO;
using XCode;
using XCode.DataAccessLayer;

namespace AntTest.Web;

/// <summary>集成测试共享数据库。XCode 的实体工厂会缓存 DAL，同一进程内同名连接必须始终指向同一个库</summary>
internal static class TestDb
{
    private static readonly Object _lock = new();
    private static Boolean _inited;

    /// <summary>数据目录。按进程隔离，避免历史数据影响唯一约束</summary>
    public static String DataPath { get; } = Path.Combine(AppContext.BaseDirectory, "TestData", Environment.ProcessId.ToString());

    /// <summary>初始化连接。重复调用幂等</summary>
    public static void Init()
    {
        lock (_lock)
        {
            if (_inited) return;

            Directory.CreateDirectory(DataPath);
            foreach (var name in new[] { "Membership", "Log", "Cube", "Ant" })
            {
                var file = Path.Combine(DataPath, name + ".db");
                DAL.AddConnStr(name, $"Data Source={file};Journal Mode=WAL", null, "SQLite");
            }

            EntityFactory.InitConnection("Ant");

            _inited = true;
        }
    }
}
