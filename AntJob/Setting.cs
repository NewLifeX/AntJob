using System;
using System.ComponentModel;
using NewLife.Xml;

namespace Stardust
{
    /// <summary>配置</summary>
    [XmlConfigFile("Config/Stardust.config", 15000)]
    public class Setting : XmlConfig<Setting>
    {
        #region 属性
        /// <summary>调试开关。默认true</summary>
        [Description("调试开关。默认true")]
        public Boolean Debug { get; set; } = true;

        /// <summary>服务端口。默认6666</summary>
        [Description("服务端口。默认6666")]
        public Int32 Port { get; set; } = 6666;
        #endregion

        #region 构造
        /// <summary>实例化</summary>
        public Setting()
        {
        }
        #endregion
    }
}
