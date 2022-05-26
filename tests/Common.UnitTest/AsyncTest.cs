using NUnit.Framework;

namespace System;

[TestFixture]
public partial class AsyncTest
{
    [Test]
    public async Task Test1()
    {
        var task = TestInside();

        TestContext.WriteLine($"status: {task.Status}, {nameof(AsyncTest)}");
        await Task.Delay(600);

        var num = await task;
        TestContext.WriteLine($"status: {task.Status}, {num}");
    }

    [Test]
    public async Task Test2()
    {
        var num = await TestInside();

        TestContext.WriteLine($"status: {TaskStatus.RanToCompletion}, {nameof(AsyncTest)}");
        await Task.Delay(600);

        TestContext.WriteLine($"status: {TaskStatus.RanToCompletion}, {num}");
    }

    async Task<int> TestInside()
    {
        await Task.Delay(500);
        TestContext.WriteLine(nameof(TestInside));
        return 100;
    }
}