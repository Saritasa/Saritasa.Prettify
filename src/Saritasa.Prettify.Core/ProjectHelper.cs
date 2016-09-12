// Copyright (c) Saritasa, LLC

namespace Saritasa.Prettify.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Helper for "project" unit of workspace in terms of visual studio
    /// </summary>
    public class ProjectHelper
    {
        public static async Task<ImmutableArray<Diagnostic>> GetProjectAnalyzerDiagnosticsAsync(ImmutableArray<DiagnosticAnalyzer> analyzers, Project project, bool force, CancellationToken cancellationToken = default(CancellationToken))
        {
            var supportedDiagnosticsSpecificOptions = new Dictionary<string, ReportDiagnostic>();
            if (force)
            {
                foreach (var analyzer in analyzers)
                {
                    foreach (var diagnostic in analyzer.SupportedDiagnostics)
                    {
                        // make sure the analyzers we are testing are enabled
                        supportedDiagnosticsSpecificOptions[diagnostic.Id] = ReportDiagnostic.Default;
                    }
                }
            }

            // Report exceptions during the analysis process as errors
            supportedDiagnosticsSpecificOptions.Add("AD0001", ReportDiagnostic.Error);

            // update the project compilation options
            var modifiedSpecificDiagnosticOptions = supportedDiagnosticsSpecificOptions.ToImmutableDictionary().SetItems(project.CompilationOptions.SpecificDiagnosticOptions);
            var modifiedCompilationOptions = project.CompilationOptions.WithSpecificDiagnosticOptions(modifiedSpecificDiagnosticOptions);
            var processedProject = project.WithCompilationOptions(modifiedCompilationOptions);

            Compilation compilation = await processedProject.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(analyzers, new CompilationWithAnalyzersOptions(new AnalyzerOptions(ImmutableArray.Create<AdditionalText>()), null, true, false));

            var diagnostics = await GetAllDiagnosticsAsync(compilation, compilationWithAnalyzers, analyzers, project.Documents, true, cancellationToken).ConfigureAwait(false);
            return diagnostics;
        }

        private static async Task<ImmutableArray<Diagnostic>> GetAllDiagnosticsAsync(Compilation compilation, CompilationWithAnalyzers compilationWithAnalyzers, ImmutableArray<DiagnosticAnalyzer> analyzers, IEnumerable<Document> documents, bool includeCompilerDiagnostics, CancellationToken cancellationToken)
        {
            return await compilationWithAnalyzers.GetAllDiagnosticsAsync().ConfigureAwait(false);
        }
    }
}
