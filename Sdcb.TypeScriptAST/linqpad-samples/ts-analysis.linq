<Query Kind="Statements">
  <NuGetReference>Sdcb.TypeScriptAST</NuGetReference>
  <Namespace>Sdcb.TypeScript</Namespace>
  <Namespace>Sdcb.TypeScript.TsTypes</Namespace>
</Query>

// ShootR code is forked from: https://github.com/NTaylorMullen/ShootR
Directory.EnumerateFiles(path: @"C:\Users\flysha.zhou\source\repos\ShootR\ShootR\ShootR\Client\Ships", "*.ts")
    .Select(x => new TypeScriptAST(File.ReadAllText(x), x))
    .SelectMany(x => x.OfKind(SyntaxKind.ClassDeclaration))
    .Select(x => new
    {
        Name = x.OfKind(SyntaxKind.Identifier).FirstOrDefault().GetText(),
        Properties = x.OfKind(SyntaxKind.PropertyDeclaration)
            .Select(x => new
            {
                Name = x.IdentifierStr,
                IsPublic = x.First.Kind != SyntaxKind.PrivateKeyword,
                Type = GetType(x),
            }),
        Methods = x.OfKind(SyntaxKind.Constructor).Concat(x.OfKind(SyntaxKind.MethodDeclaration))
            .Select(x => new
            {
                Name = x is ConstructorDeclaration ctor ? ".ctor" : x.IdentifierStr,
                IsPublic = x.First.Kind != SyntaxKind.PrivateKeyword,
                Args = ((ISignatureDeclaration)x).Parameters.Select(x => new
                {
                    Name = x.Children.OfKind(SyntaxKind.Identifier).FirstOrDefault().GetText(), 
                    Type = x.Children.OfKind(SyntaxKind.TypeReference).FirstOrDefault()?.GetText(), 
                }), 
            }), 
    })
    .Dump();

string GetType(Node x)
{
    var typeReference = x.Children.OfType<TypeReferenceNode>().FirstOrDefault();
    if (typeReference != null)
        return typeReference.IdentifierStr;

    if (x.Last is LiteralExpression literal)
        return literal.Kind.ToString()[..^7].ToLower();

    return Regex.Match(x.Last.GetText(), "<(.+?)>").Groups[1].Value;
}