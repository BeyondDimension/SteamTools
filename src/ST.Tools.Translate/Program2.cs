using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using static System.Constants;

try
{
    DI.Init(s =>
    {
        s.AddHttpClient();
    });

    var rootCommand = new RootCommand(Title);

    var commands = Assembly.GetExecutingAssembly().GetTypes()
        .Where(x => x.Namespace == "System.Commands" && x.IsClass && !x.IsGenericType && x.IsAbstract && x.IsSealed);
    foreach (var item in commands)
    {
        var method = item.GetMethod("Add", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(RootCommand) }, null);
        method?.Invoke(null, new[] { rootCommand });
    }

    return rootCommand.InvokeAsync(args).Result;
}
catch (Exception? e)
{
    while (e != null)
    {
        Console.WriteLine(e.ToString());
        e = e.InnerException;
    }
    return 500;
}