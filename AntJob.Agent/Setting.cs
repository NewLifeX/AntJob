using System;
using System.ComponentModel;
using NewLife.Xml;

namespace AntJob.Agent
{
    /// <summary>配置</summary>
    [XmlConfigFile("Config/AntAgent.config", 15000)]
    public class Setting : XmlConfig<Setting>
    {
        #region 属性
        /// <summary>调试开关。默认true</summary>
        [Description("调试开关。默认true")]
        public Boolean Debug { get; set; } = true;

        /// <summary>调度中心。逗号分隔多地址，主备架构</summary>
        [Description("调度中心。逗号分隔多地址，主备架构")]
        public String Server { get; set; } = "tcp://127.0.0.1:9999,tcp://ant.newlifex.com:9999";

        /// <summary>应用标识。调度中心以此隔离应用，默认AntAgent</summary>
        [Description("应用标识。调度中心以此隔离应用，默认AntAgent")]
        public String AppID { get; set; } = "AntAgent";

        /// <summary>应用密钥。</summary>
        [Description("应用密钥。")]
        public String Secret { get; set; }
        #endregion
    }
}