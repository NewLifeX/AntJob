namespace AntJob.Data;

/// <summary>邻居伙伴</summary>
public partial class PeerModel
{
    /// <summary>实例。IP加端口</summary>
    public String Instance { get; set; }

    /// <summary>客户端。IP加进程</summary>
    public String Client { get; set; }

    /// <summary>名称。机器名称</summary>
    public String Machine { get; set; }

    /// <summary>版本。客户端</summary>
    public String Version { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreateTime { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdateTime { get; set; }
}