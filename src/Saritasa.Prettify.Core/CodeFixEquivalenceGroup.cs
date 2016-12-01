

namespace Saritasa.Prettify.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;

    /// <summary>
    /// Contains data for comparing groups, we cannot fix same groups for same time
    /// </summary>
    public class CodeFixEquivalenceGroup
    {
        private CodeFixEquivalenceGroup(
            string equivalenceKey,
            Solution solution,
            FixAllProvider fixAllProvider,
            CodeFixProvider codeFixProvider,
            ImmutableDictionary<ProjectId, ImmutableDictionary<string, ImmutableArray<Diagnostic>>> documentDiagnosticsToFix,
            ImmutableDictionary<ProjectId, ImmutableArray<Diagnostic>> projectDiagnosticsToFix)
        {
            CodeFixEquivalenceKey = equivalenceKey;
            Solution = solution;
            FixAllProvider = fixAllProvider;
            CodeFixProvider = codeFixProvider;
            DocumentDiagnosticsToFix = documentDiagnosticsToFix;
            ProjectDiagnosticsToFix = projectDiagnosticsToFix;
            NumberOfDiagnostics = documentDiagnosticsToFix.SelectMany(x => x.Value.Select(y => y.Value).SelectMany(y => y)).Count()
                + projectDiagnosticsToFix.SelectMany(x => x.Value).Count();
        }

        internal string CodeFixEquivalenceKey { get; }

        internal Solution Solution { get; }

        internal FixAllProvider FixAllProvider { get; }

        internal CodeFixProvider CodeFixProvider { get; }

        internal ImmutableDictionary<ProjectId, ImmutableDictionary<string, ImmutableArray<Diagnostic>>> DocumentDiagnosticsToFix { get; }

        internal ImmutableDictionary<ProjectId, ImmutableArray<Diagnostic>> ProjectDiagnosticsToFix { get; }

        internal int NumberOfDiagnostics { get; }

        public static async Task<ImmutableArray<CodeFixEquivalenceGroup>> CreateAsync(CodeFixProvider codeFixProvider, ImmutableDictionary<ProjectId, ImmutableArray<Diagnostic>> allDiagnostics,
            Solution solution, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fixAllProvider = codeFixProvider.GetFixAllProvider();
            if (fixAllProvider == null)
            {
                return ImmutableArray.Create<CodeFixEquivalenceGroup>();
            }

            var relevantDocumentDiagnostics =
                new Dictionary<ProjectId, Dictionary<string, List<Diagnostic>>>();
            var relevantProjectDiagnostics =
                new Dictionary<ProjectId, List<Diagnostic>>();

            foreach (var projectDiagnostics in allDiagnostics)
            {
                foreach (var diagnostic in projectDiagnostics.Value)
                {
                    if (!codeFixProvider.FixableDiagnosticIds.Contains(diagnostic.Id))
                    {
                        continue;
                    }

                    if (diagnostic.Location.IsInSource)
                    {
                        var sourcePath = diagnostic.Location.GetLineSpan().Path;

                        Dictionary<string, List<Diagnostic>> projectDocumentDiagnostics;
                        if (!relevantDocumentDiagnostics.TryGetValue(projectDiagnostics.Key, out projectDocumentDiagnostics))
                        {
                            projectDocumentDiagnostics = new Dictionary<string, List<Diagnostic>>();
                            relevantDocumentDiagnostics.Add(projectDiagnostics.Key, projectDocumentDiagnostics);
                        }

                        List<Diagnostic> diagnosticsInFile;
                        if (!projectDocumentDiagnostics.TryGetValue(sourcePath, out diagnosticsInFile))
                        {
                            diagnosticsInFile = new List<Diagnostic>();
                            projectDocumentDiagnostics.Add(sourcePath, diagnosticsInFile);
                        }

                        diagnosticsInFile.Add(diagnostic);
                    }
                    else
                    {
                        List<Diagnostic> diagnosticsInProject;
                        if (!relevantProjectDiagnostics.TryGetValue(projectDiagnostics.Key, out diagnosticsInProject))
                        {
                            diagnosticsInProject = new List<Diagnostic>();
                            relevantProjectDiagnostics.Add(projectDiagnostics.Key, diagnosticsInProject);
                        }

                        diagnosticsInProject.Add(diagnostic);
                    }
                }
            }

            var documentDiagnosticsToFix =
                relevantDocumentDiagnostics.ToImmutableDictionary(i => i.Key, i => i.Value.ToImmutableDictionary(j => j.Key, j => j.Value.ToImmutableArray(), StringComparer.OrdinalIgnoreCase));
            var projectDiagnosticsToFix =
                relevantProjectDiagnostics.ToImmutableDictionary(i => i.Key, i => i.Value.ToImmutableArray());

            var equivalenceKeys = new HashSet<string>();
            foreach (var diagnostic in relevantDocumentDiagnostics.Values.SelectMany(i => i.Values).SelectMany(i => i).Concat(relevantProjectDiagnostics.Values.SelectMany(i => i)))
            {
                foreach (var codeAction in await GetFixesAsync(solution, codeFixProvider, diagnostic, cancellationToken).ConfigureAwait(false))
                {
                    equivalenceKeys.Add(codeAction.EquivalenceKey);
                }
            }

            var groups = new List<CodeFixEquivalenceGroup>();
            foreach (var equivalenceKey in equivalenceKeys)
            {
                groups.Add(new CodeFixEquivalenceGroup(equivalenceKey, solution, fixAllProvider, codeFixProvider, documentDiagnosticsToFix, projectDiagnosticsToFix));
            }

            return groups.ToImmutableArray();
        }

        public async Task<ImmutableArray<CodeActionOperation>> GetOperationsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var diagnostic = DocumentDiagnosticsToFix.Values.SelectMany(i => i.Values).Concat(ProjectDiagnosticsToFix.Values).First().First();
            var document = Solution.GetDocument(diagnostic.Location.SourceTree);
            var diagnosticIds = new HashSet<string>(DocumentDiagnosticsToFix.Values.SelectMany(i => i.Values).Concat(ProjectDiagnosticsToFix.Values).SelectMany(i => i.Select(j => j.Id)));

            var diagnosticsProvider = new FixDiagnosticProvider(DocumentDiagnosticsToFix, ProjectDiagnosticsToFix);

            var context = new FixAllContext(document, CodeFixProvider, FixAllScope.Solution, CodeFixEquivalenceKey, diagnosticIds, diagnosticsProvider, cancellationToken);

            var action = await FixAllProvider.GetFixAsync(context).ConfigureAwait(false);
            if (action == null)
            {
                return Enumerable.Empty<CodeActionOperation>()
                    .ToImmutableArray();
            }

            return await action.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
        }

        private static async Task<IEnumerable<CodeAction>> GetFixesAsync(Solution solution, CodeFixProvider codeFixProvider, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var codeActions = new List<CodeAction>();

            await codeFixProvider.RegisterCodeFixesAsync(new CodeFixContext(solution.GetDocument(diagnostic.Location.SourceTree), diagnostic, (a, d) => codeActions.Add(a), cancellationToken)).ConfigureAwait(false);

            return codeActions;
        }
    }
}
