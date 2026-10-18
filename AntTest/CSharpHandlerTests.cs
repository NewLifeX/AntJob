using System;
using AntJob;
using AntJob.Data;
using AntJob.Handlers;
using Xunit;

namespace AntTest;

/// <summary>C#脚本处理器测试（Roslyn 脚本）</summary>
public class CSharpHandlerTests
{
    private static JobContext CreateContext(String code) => new()
    {
        Data = code,
        Task = new TaskModel { Data = code },
    };

    [Fact]
    public void Mode_Is_Time()
    {
        var handler = new CSharpHandler();
        Assert.Equal(JobModes.Time, handler.Mode);
        Assert.Equal(8, handler.Job.BatchSize);
    }

    [Fact]
    public void Execute_Null_Context_Returns_MinusOne()
    {
        var handler = new CSharpHandler();
        Assert.Equal(-1, handler.Execute(null));
    }

    [Fact]
    public void Execute_Empty_Code_Returns_MinusOne()
    {
        var handler = new CSharpHandler();

        Assert.Equal(-1, handler.Execute(new JobContext()));
        Assert.Equal(-1, handler.Execute(CreateContext("")));
        Assert.Equal(-1, handler.Execute(CreateContext("   ")));
    }

    [Fact]
    public void Execute_Returns_Script_Value()
    {
        var handler = new CSharpHandler();

        Assert.Equal(3, handler.Execute(CreateContext("1 + 2")));
    }

    [Fact]
    public void Execute_Without_Return_Counts_One()
    {
        var handler = new CSharpHandler();

        Assert.Equal(1, handler.Execute(CreateContext("var x = 1;")));
    }

    [Fact]
    public void Execute_Can_Access_Context()
    {
        var handler = new CSharpHandler();
        var ctx = CreateContext("ctx.Task == null ? 0 : 7");

        Assert.Equal(7, handler.Execute(ctx));
    }

    [Fact]
    public void Execute_Can_Use_Linq()
    {
        var handler = new CSharpHandler();

        Assert.Equal(6, handler.Execute(CreateContext("new[] { 1, 2, 3 }.Sum()")));
    }

    [Fact]
    public void Execute_From_Task_Data()
    {
        var handler = new CSharpHandler();
        var ctx = new JobContext { Task = new TaskModel { Data = "2 + 3" } };

        Assert.Equal(5, handler.Execute(ctx));
    }

    [Fact]
    public void Execute_Invalid_Code_Throws()
    {
        var handler = new CSharpHandler();

        Assert.ThrowsAny<Exception>(() => handler.Execute(CreateContext("this is not csharp")));
    }

    [Fact]
    public void Execute_NonNumeric_Return_Counts_One()
    {
        var handler = new CSharpHandler();

        Assert.Equal(1, handler.Execute(CreateContext("\"done\"")));
    }
}

