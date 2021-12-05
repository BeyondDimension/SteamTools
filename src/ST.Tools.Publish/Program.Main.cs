using System;
using System.CommandLine;
using System.Linq;
using System.Reflection;

static partial class Program
{
    static int _(string[] args,
        string description = "",
        Assembly? assembly = null,
        string @namespace = "System.Application.Steps",
        string methodName = "Add",
        Func<int?>? init = null,
        Action<RootCommand>? action = null)
    {
        try
        {
            var result = init?.Invoke();
            if (result.HasValue) return result.Value;

            var rootCommand = new RootCommand(description);

            action?.Invoke(rootCommand);

            var steps = (assembly ?? Assembly.GetCallingAssembly()).GetTypes()
               .Where(x => x.Namespace == @namespace && x.IsClass && !x.IsGenericType && x.IsAbstract && x.IsSealed);
            foreach (var step in steps)
            {
                var method = step.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(RootCommand) }, null);
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
    }
}