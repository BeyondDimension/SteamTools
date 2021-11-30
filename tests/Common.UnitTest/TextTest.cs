using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    [TestFixture]
    public class TextTest
    {
        [Test]
        public void Console()
        {
            var maxLine = 5;
            IConsoleBuilder builder = new ConsoleBuilder() { MaxLine = maxLine, };
            Enumerable.Range(0, 10).ToList().ForEach(x => builder.AppendLine(x.ToString() + "_" + Random2.GenerateRandomString(Random2.Next(1, x + 2))));
            var str = builder.ToString();
            var array = str.Split(Environment.NewLine);
            TestContext.Write(str);
            Assert.IsTrue(maxLine == array.Length);
        }

        [Test]
        public void ConsoleException()
        {
            var maxLine = 7;
            IConsoleBuilder builder = new ConsoleBuilder() { MaxLine = maxLine, };
            builder.AppendLine(new Exception("aaaaa").ToStringThrow());
            builder.AppendLine(new AccessViolationException("bbbb").ToStringThrow());
            builder.AppendLine(new AggregateException("ddd").ToStringThrow());
            builder.AppendLine(new IndexOutOfRangeException("ffff").ToStringThrow());
            builder.AppendLine(new ArgumentOutOfRangeException("ggg").ToStringThrow());
            builder.AppendLine(new BadImageFormatException("zzzz").ToStringThrow());
            builder.AppendLine(new DllNotFoundException("qqqqqqq").ToStringThrow());
            var str = builder.ToString();
            var array = str.Split(Environment.NewLine);
            TestContext.Write(str);
            Assert.IsTrue(maxLine == array.Length);
        }
    }

    public static partial class TestExtensions
    {
        public static string ToStringThrow(this Exception exception)
        {
            try
            {
                throw exception;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
