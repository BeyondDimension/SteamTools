[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:Using directives should be placed correctly", Justification = "<挂起>")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "<挂起>")]

var rootCommand = new RootCommand($"{AssemblyInfo.Product} Publish Tools");
var interfaceType = typeof(ICommand);
var addMethod = interfaceType.GetMethod(nameof(ICommand.AddCommand), BindingFlags.Static | BindingFlags.Public);
var commands = interfaceType.Assembly.GetTypes().
    Where(x => x != interfaceType && x.IsInterface && interfaceType.IsAssignableFrom(x)).
    Select(x => addMethod!.MakeGenericMethod(x)).
    ToArray();
Array.ForEach(commands, m => m.Invoke(null, new object?[] { rootCommand, }));
return await rootCommand.InvokeAsync(args);