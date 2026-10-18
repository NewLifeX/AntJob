using System.Linq;
using AntJob;
using AntJob.Data;
using AntJob.Extensions;
using XCode.DataAccessLayer;
using XCode.Membership;
using Xunit;

namespace AntTest;

[Collection("Integration")]
public class SqlHandlerTests
{
    [Fact]
    public void ParseAllRoles()
    {
        var tt = """
            /*use membership*/
            select * from role

            /*use membership_bak*/
            delete from role;

            /*use membership_bak*/
            insert role;

            """;
        var count = Role.Meta.Count;
        var table = Role.Meta.Session.Dal.Tables.FirstOrDefault(e => e.Name == "Role");
        DAL.Create("membership").SetTables(table);
        DAL.Create("membership_bak").SetTables(table);

        var handler = new SqlHandler();
        var task = new TaskModel
        {
            Data = tt
        };

        //handler.Process(task);

        var ctx = new JobContext
        {
            Task = task,
        };

        //var method = handler.GetType().GetMethodEx("OnProcess", typeof(JobContext));
        //method.Invoke(handler, new Object[] { ctx });
        var rs = handler.Execute(ctx);

        Assert.Equal(count, ctx.Total);
        //Assert.Equal(4, ctx.Success);
        Assert.Equal(count * 2, rs);
        //Assert.True(ctx.Success > 0);
    }
}

