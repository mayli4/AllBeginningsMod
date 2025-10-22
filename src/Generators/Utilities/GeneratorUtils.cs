using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Generators.Utilities;

public static class GeneratorUtils {
    public static string GetRootNamespaceOrRaiseDiagnostic(SourceProductionContext ctx, AnalyzerConfigOptions options) {
        if(options.TryGetValue("build_property.rootnamespace", out string rootNamespace)) {
            return rootNamespace;
        }

        ctx.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "SG0001",
                    "Failed to get root namespace",
                    "Property 'build_property.rootnamespace' wasn't found",
                    "CodeAnalysis",
                    DiagnosticSeverity.Error,
                    true
                ),
                null
            )
        );
        return null;
    }
}