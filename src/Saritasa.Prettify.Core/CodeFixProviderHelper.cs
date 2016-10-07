

namespace Saritasa.Prettify.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis.CodeFixes;

    /// <summary>
    /// Helper for <see cref="CodeFixProvider"/>
    /// </summary>
    public class CodeFixProviderHelper
    {
        public static ImmutableDictionary<string, ImmutableList<CodeFixProvider>> GetFixProviders(Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            if (assemblies.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(assemblies));
            }

            var codeFixProviderType = typeof(CodeFixProvider);

            var providers = new Dictionary<string, ImmutableList<CodeFixProvider>>();

            return assemblies.SelectMany(x => x.GetTypes())
                 .Where(x => x.IsSubclassOf(codeFixProviderType))
                 .Aggregate(providers, (seed, type) =>
                 {
                     var codeFixProvider = (CodeFixProvider)Activator.CreateInstance(type);

                     foreach (var diagnosticId in codeFixProvider.FixableDiagnosticIds)
                     {
                         seed.AddToInnerList(diagnosticId, codeFixProvider);
                     }

                     return seed;
                 })
                 .ToImmutableDictionary();
        }
    }
}
