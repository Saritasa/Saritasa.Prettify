// Copyright (c) Saritasa, LLC

namespace Saritasa.Prettify.ConsoleApp
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
    }
}
