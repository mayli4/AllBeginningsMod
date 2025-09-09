using Generators.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Generators;

[Generator(LanguageNames.CSharp)]
public class GlobalUsingsGenerator : IIncrementalGenerator {
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterSourceOutput(context.AnalyzerConfigOptionsProvider, (x, options) =>
        {
            if(GeneratorUtils.GetRootNamespaceOrRaiseDiagnostic(x, options.GlobalOptions) is not { } rootNamespace) return;
            x.AddSource(
                "_Usings.g.cs",
                SourceText.From(GenerateUsings(rootNamespace), Encoding.UTF8)
            );
        });

        static string GenerateUsings(string rootNamespace) {
            var sb = new StringBuilder();
            sb.Append("global using static AllBeginningsMod.Assets.Assets;");
            sb.Append("global using AllBeginningsMod.Localization;");
            sb.Append("global using Terraria.ModLoader;");
            sb.Append("global using Terraria;");
            sb.Append("global using Microsoft.Xna.Framework;");
            sb.Append("global using Microsoft.Xna.Framework.Graphics;");
            sb.Append("");
            sb.Append("namespace AllBeginningsMod;");
            return sb.ToString();
        }
    }
}