using Microsoft.CodeAnalysis;

namespace BD.WTTS.Generators;

[Generator]
public class AttributeGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Build up the source code
        string source = $@"// <auto-generated/>
namespace BD.WTTS.Settings;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class {SettingsGenerationReceiver.AttributeName} : Attribute
{{
}}
        ";
        context.AddSource($"{SettingsGenerationReceiver.AttributeName}.g.cs", source);
    }

    public void Initialize(GeneratorInitializationContext context)
    {

    }
}
