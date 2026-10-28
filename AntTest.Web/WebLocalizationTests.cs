extern alias web;

using System;
using System.ComponentModel;
using System.Globalization;
using NewLife;
using Xunit;

namespace AntTest.Web;

[DisplayName("Web 宿主多语言词表")]
public class WebLocalizationTests
{
    [Fact]
    [DisplayName("Web 宿主词表按界面语言翻译并保留占位符")]
    public void WebMessageTranslate()
    {
        var original = Localization.Culture;
        try
        {
            // 触发 AntWeb 程序集加载，模块初始化器才会注册它的词表
            _ = new web::AntJob.Web.Startup();

            Localization.Culture = new CultureInfo("zh-Hans");
            Assert.Equal("认证失败。", Localization.Translate("Authentication failed."));
            Assert.Equal("不支持的 grant_type password。", Localization.Translate("Unsupported grant_type {0}.", "password"));
            Assert.Equal("无效操作，TaskID=0。", Localization.Translate("Invalid operation, TaskID={0}.", 0));

            Localization.Culture = new CultureInfo("en-US");
            Assert.Equal("Authentication failed.", Localization.Translate("Authentication failed."));
        }
        finally
        {
            Localization.Culture = original;
        }
    }
}
