using AllBeginningsGeneration.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace AllBeginningsGeneration.Generators;

[Generator(LanguageNames.CSharp)]
public class GlobalUsingsGenerator : IIncrementalGenerator {
    void IIncrementalGenerator.Initialize(
        IncrementalGeneratorInitializationContext context) {
        context.RegisterSourceOutput(context.AnalyzerConfigOptionsProvider, (x, options) => {
                if(GeneratorUtilities.GetRootNamespaceOrRaiseDiagnostic(x, options.GlobalOptions) is not { } rootNamespace)
                    return;

                x.AddSource(
                    "_Usings.g.cs",
                    SourceText.From(GenerateUsings(rootNamespace), Encoding.UTF8)
                );
            }
        );

        string GenerateUsings(string rootNamespace) {
            var sb = new StringBuilder();
            sb.Append("global using AllBeginningsMod.Assets;");
            return sb.ToString();
        }
    }
}