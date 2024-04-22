namespace AntJob.Data;

/// <summary>邻居伙伴</summary>
public interface IPeer
{
    /// <summary>实例。IP加端口，唯一</summary>
    String Instance { get; set; }

    /// <summary>客户端。IP加进程</summary>
    String Client { get; set; }

    /// <summary>名称。机器名称</summary>
    String Machine { get; set; }

    /// <summary>版本。客户端</summary>
    String Version { get; set; }

    /// <summary>创建时间</summary>
    DateTime CreateTime { get; set; }

    /// <summary>更新时间</summary>
    DateTime UpdateTime { get; set; }
}

public partial class PeerModel : IPeer { }