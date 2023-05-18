using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace BD.WTTS.Generators;

public sealed class SettingsGenerationReceiver : ISyntaxReceiver
{
    public const string AttributeName = "SettingsGenerationAttribute";

    public List<TypeDeclarationSyntax> Candidates { get; } = new List<TypeDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
        {
            foreach (var attributeList in
            typeDeclarationSyntax.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (attribute.Name.ToString() == "SettingsGeneration" ||
                        attribute.Name.ToString() == AttributeName)
                    {
                        this.Candidates.Add(typeDeclarationSyntax);
                    }
                }
            }
        }
    }
}