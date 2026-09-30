using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Stage1.Basic;

internal class ClassInfo
{
    public INamedTypeSymbol Symbol { get; }
    public ClassDeclarationSyntax Declaration { get; }
    public AttributeData AttributeData { get; }

    public ClassInfo(INamedTypeSymbol symbol, ClassDeclarationSyntax declaration, AttributeData attributeData)
    {
        Symbol = symbol;
        Declaration = declaration;
        AttributeData = attributeData;
    }
}