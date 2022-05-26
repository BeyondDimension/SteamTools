using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace System;

[SetUpFixture]
public class SetupFixture2
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // TODO: Add code here that is run before
        //  all tests in the assembly are run
        OneTimeSetUpCore();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // TODO: Add code here that is run after
        //  all tests in the assembly have been run
        OneTimeTearDownCore();
    }

    static void OneTimeSetUpCore()
    {
        global::System.Application.SetupFixture.OneTimeSetUpCore(ConfigureServices);
    }

    static void ConfigureServices(IServiceCollection services)
    {
        global::System.SetupFixture.ConfigureServices(services);
        global::System.Application.SetupFixture.ConfigureServices(services);
    }

    static void OneTimeTearDownCore()
    {
        global::System.Application.SetupFixture.OneTimeTearDownCore();
    }
}
