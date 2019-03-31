using System;
using System.Threading;
using NewLife.Log;
using NewLife.Remoting;

namespace Test
{
    class Program
    {
        static void Main(String[] args)
        {
            XTrace.UseConsole();

            try
            {
                Test1();
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }

            Thread.Sleep(-1);
        }

        static void Test1()
        {

        }
    }
}