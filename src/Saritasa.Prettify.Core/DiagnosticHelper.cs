// Copyright (c) Saritasa, LLC

using System.Threading;
using Microsoft.CodeAnalysis;

namespace Saritasa.Prettify.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Diagnostics;

    public class DiagnosticHelper
    {
        public static ImmutableArray<DiagnosticAnalyzer> GetAnalyzersFromAssemblies(Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            if (assemblies.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(assemblies));
            }

            var analyzers = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
            var diagnosticAnalyzerType = typeof(DiagnosticAnalyzer);

            return assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsSubclassOf(diagnosticAnalyzerType) && !x.IsAbstract)
                .Aggregate(analyzers, (seed, item) =>
                {
                    var diagnosticAnalyzer = Activator.CreateInstance(item) as DiagnosticAnalyzer;
                    seed.Add(diagnosticAnalyzer);
                    return seed;
                }).ToImmutable();
        }

        public static async Task<ImmutableDictionary<ProjectId, ImmutableArray<Diagnostic>>> GetAnalyzerDiagnosticsAsync(Solution solution,
            ImmutableArray<DiagnosticAnalyzer> analyzers, bool force, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<KeyValuePair<ProjectId, Task<ImmutableArray<Diagnostic>>>> projectDiagnosticTasks = new List<KeyValuePair<ProjectId, Task<ImmutableArray<Diagnostic>>>>();

            // Make sure we analyze the projects in parallel
            foreach (var project in solution.Projects)
            {
                if (project.Language != LanguageNames.CSharp)
                {
                    continue;
                }

                projectDiagnosticTasks.Add(new KeyValuePair<ProjectId, Task<ImmutableArray<Diagnostic>>>(project.Id, ProjectHelper.GetProjectAnalyzerDiagnosticsAsync(analyzers, project, force)));
            }

            ImmutableDictionary<ProjectId, ImmutableArray<Diagnostic>>.Builder projectDiagnosticBuilder = ImmutableDictionary.CreateBuilder<ProjectId, ImmutableArray<Diagnostic>>();
            foreach (var task in projectDiagnosticTasks)
            {
                projectDiagnosticBuilder.Add(task.Key, await task.Value.ConfigureAwait(false));
            }

            return projectDiagnosticBuilder.ToImmutable();
        }
    }
}
