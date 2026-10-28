using System;
using System.ComponentModel;
using System.Globalization;
using NewLife;
using Xunit;

namespace AntTest;

[DisplayName("多语言词表")]
public class LocalizationTests
{
    [Fact]
    [DisplayName("核心库词表按界面语言翻译，缺失键回退英文源文")]
    public void CoreMessageTranslate()
    {
        const String source = "No handler is available.";
        var original = Localization.Culture;
        try
        {
            Localization.Culture = new CultureInfo("zh-Hans");
            Assert.Equal("没有可用处理器", Localization.Translate(source));

            Localization.Culture = new CultureInfo("en-US");
            Assert.Equal(source, Localization.Translate(source));

            // 词表缺失的键原样返回，不影响调用方
            Assert.Equal("message that has no translation", Localization.Translate("message that has no translation"));
        }
        finally
        {
            Localization.Culture = original;
        }
    }

    [Fact]
    [DisplayName("扩展库词表按界面语言翻译并保留占位符")]
    public void ExtensionMessageTranslate()
    {
        var original = Localization.Culture;
        try
        {
            // 触发扩展库加载，模块初始化器才会注册它的词表
            _ = new AntJob.Extensions.SqlHandler();

            Localization.Culture = new CultureInfo("zh-Hans");

            Assert.Equal("未设置数据抽取配置。", Localization.Translate("Data extraction is not configured."));
            Assert.Equal("未为[select 1]指定连接名。", Localization.Translate("Connection name is not specified for [{0}].", "select 1"));
            Assert.Equal("无法抽取[System.String]类型的字段数据。", Localization.Translate("Field data of type [{0}] cannot be extracted.", typeof(String).FullName));

            Localization.Culture = new CultureInfo("en-US");
            Assert.Equal("Data extraction is not configured.", Localization.Translate("Data extraction is not configured."));
        }
        finally
        {
            Localization.Culture = original;
        }
    }

    [Fact]
    [DisplayName("数据层词表按界面语言翻译")]
    public void DataMessageTranslate()
    {
        var original = Localization.Culture;
        try
        {
            // 触发数据层加载，模块初始化器才会注册它的词表
            _ = new AntJob.Data.Entity.App();

            Localization.Culture = new CultureInfo("zh-Hans");
            Assert.Equal("名称不能为空。", Localization.Translate("Name cannot be empty."));

            Localization.Culture = new CultureInfo("en-US");
            Assert.Equal("Name cannot be empty.", Localization.Translate("Name cannot be empty."));
        }
        finally
        {
            Localization.Culture = original;
        }
    }
}
