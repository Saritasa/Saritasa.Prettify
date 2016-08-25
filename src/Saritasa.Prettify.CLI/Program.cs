// Copyright (c) Saritasa, LLC

namespace Saritasa.Prettify.ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.MSBuild;
    using Serilog;

    public class AssembliesHelper
    {
        public const string StyleCopAssemblyFolderKey = "StyleCopAssemblyFolder";
        public const string StyleCopCodeFixesDllNameKey = "StyleCopCodeFixesDllName";
        public const string StyleCopAnalyzersDllNameKey = "StyleCopAnalyzersDllName";


        public static string GetFolderPath()
        {
            var folderValue = ConfigurationManager.AppSettings[StyleCopAssemblyFolderKey];
            if (string.IsNullOrWhiteSpace(folderValue))
            {
                throw new ArgumentException($"Please specify folder in application settings with key {StyleCopAssemblyFolderKey}", nameof(folderValue));
            }
            if (!Directory.Exists(folderValue))
            {
                throw new DirectoryNotFoundException("Provided directory path for assemblies not found");
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderValue);
        }

        public static Assembly GetCodeFixAssembly()
        {
            var folder = GetFolderPath();

            var codeFixAssemblyName = ConfigurationManager.AppSettings[StyleCopCodeFixesDllNameKey];
            if (string.IsNullOrWhiteSpace(codeFixAssemblyName))
            {
                throw new ArgumentException($"Please specify dll name for codefixes in application settings with key {StyleCopCodeFixesDllNameKey}", nameof(codeFixAssemblyName));
            }

            var pathToAssembly = Path.Combine(folder, codeFixAssemblyName);
            if (!File.Exists(pathToAssembly))
            {
                throw new FileNotFoundException($"Can't find file with provided name at path - {pathToAssembly}");
            }

            return Assembly.LoadFile(pathToAssembly);
        }

        public static Assembly GetAnalyzersAssembly()
        {
            var folder = GetFolderPath();

            var assemblyName = ConfigurationManager.AppSettings[StyleCopAnalyzersDllNameKey];
            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                throw new ArgumentException($"Please specify dll name for analyzers in application settings with key {StyleCopAnalyzersDllNameKey}", nameof(assemblyName));
            }

            var pathToAssembly = Path.Combine(folder, assemblyName);
            if (!File.Exists(pathToAssembly))
            {
                throw new FileNotFoundException($"Can't find file with provided name at path - {pathToAssembly}");
            }

            return Assembly.LoadFile(pathToAssembly);
        }

        public static Assembly GetMicrosoftCodeAnalysisAssembly()
        {
            var folder = GetFolderPath();

            var pathToAssembly = Path.Combine(folder, "ManagedSourceCodeAnalysis.dll");

            if (!File.Exists(pathToAssembly))
            {
                throw new FileNotFoundException($"Can't find file with provided name at path - ManagedSourceCodeAnalysis.dll");
            }

            return Assembly.LoadFile(pathToAssembly);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            if (args.Length == 0)
            {
                Console.WriteLine(Args.Usage);
                return;
            }

            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

                var options = new Args(args);
                var styleCopAnalyzerAssembly = AssembliesHelper.GetAnalyzersAssembly();
                var styleCopFixersAssembly = AssembliesHelper.GetCodeFixAssembly();
                var microsoftCodeAnalysisAssembly = AssembliesHelper.GetMicrosoftCodeAnalysisAssembly();

                var analyzers = DiagnosticHelper.GetAnalyzersFromAssemblies(new[] { styleCopAnalyzerAssembly, styleCopFixersAssembly, microsoftCodeAnalysisAssembly });
                var codeFixes =
                    CodeFixProviderHelper.GetFixProviders(new[] { styleCopAnalyzerAssembly, styleCopFixersAssembly, microsoftCodeAnalysisAssembly })
                    .Where(x => options.Rules == null || options.Rules.Contains(x.Key))
                            .ToList();

                var workspace = MSBuildWorkspace.Create();
                var solution = workspace.OpenSolutionAsync(options.SolutionPath).Result;

                foreach (var solutionProject in solution.Projects)
                {
                    var diagnostics = ProjectHelper.GetProjectAnalyzerDiagnosticsAsync(analyzers, solutionProject, true).Result
                        .Where(x => options.Rules == null || options.Rules.Contains(x.Id))
                        .ToList();

                    Log.Information("<======= Project: {project} =======>", solutionProject.Name);
                    if (!diagnostics.Any())
                    {
                        Log.Information("Can't find any diagnostic issues for {@rules}", options.Rules);
                        continue;
                    }

                    foreach (var projectAnalyzer in diagnostics)
                    {
                        Log.Information("DiagnosticId {@id} - {message} in file {path}({row},{column})", projectAnalyzer.Id, projectAnalyzer.GetMessage(), GetFormattedFileName(projectAnalyzer.Location.GetLineSpan().Path),
                            projectAnalyzer.Location.GetLineSpan().StartLinePosition.Line, projectAnalyzer.Location.GetLineSpan().StartLinePosition.Character);
                    }

                    if (options.Rules == null && options.Mode == Args.RunningMode.Fix)
                    {
                        Log.Warning("Please specify rules for fix");
                        break;
                    }


                    if (options.Mode == Args.RunningMode.Fix)
                    {
                        var diagnistics = GetAnalyzerDiagnosticsAsync(solution, analyzers, true).Result;


                        foreach (var keyValuePair in codeFixes)
                        {
                            var equivalenceGroups = new List<CodeFixEquivalenceGroup>();

                            keyValuePair.Value.ForEach(x =>
                            {
                                equivalenceGroups.AddRange(CodeFixEquivalenceGroup.CreateAsync(x, diagnistics, solution).Result);
                            });

                            if (!equivalenceGroups.Any())
                            {
                                continue;
                            }

                            var fix = equivalenceGroups[0];

                            if (equivalenceGroups.Count() > 1)
                            {
                                Log.Warning("Allowed only one equivalence group for fix");
                            }

                            var operations = fix.GetOperationsAsync().Result;
                            if (operations.Length == 0)
                            {
                                Log.Information("No changes was found for this fixer");
                            }
                            else
                            {
                                operations[0].Apply(workspace, default(CancellationToken));
                                Log.Information("Fixer with DiagnosticId {@id} was applied ", keyValuePair.Key);
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Log.Fatal(ex, string.Empty);
            }
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = (Assembly)null;

            Directory.EnumerateFiles(AssembliesHelper.GetFolderPath(), "*.dll")
                .Aggregate(0, (seed, file) =>
                {
                    try
                    {
                        var asm = Assembly.LoadFile(file);
                        if (asm.FullName == args.Name)
                        {
                            assembly = asm;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex, string.Empty);
                    }

                    return ++seed;
                });

            return assembly;
        }

        private static async Task<ImmutableDictionary<ProjectId, ImmutableArray<Diagnostic>>> GetAnalyzerDiagnosticsAsync(Solution solution,
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

        private static string GetFormattedFileName(string input)
            => !string.IsNullOrWhiteSpace(input) ? "..\\" + Path.GetFileName(input) : string.Empty;
    }

    class Args
    {
        public const string Stats = "--stats";
        public const string Fix = "--fix";
        public const string SolutionFileExtension = ".sln";
        public Regex rules = new Regex("^--((?i)rules=)(?<rules>[A-z0-9,_]+)", RegexOptions.Compiled | RegexOptions.Singleline);

        public Args(string[] args)
        {
            if (args.Length < 2)
            {
                throw new InvalidOperationException("Please specify arguments.");
            }

            Enumerable.Range(0, args.Length)
                .Aggregate(0, (i, seed) =>
                {
                    if (Regex.IsMatch(args[i], $"(?i){Stats}"))
                    {
                        Mode = RunningMode.Stats;
                    }
                    if (Regex.IsMatch(args[i], $"(?i){Fix}"))
                    {
                        Mode = RunningMode.Fix;
                    }
                    if (rules.IsMatch(args[i]))
                    {
                        rules.Matches(args[i])
                            .OfType<Match>()
                            .Aggregate(0, (seed1, match) =>
                            {
                                if (match.Groups["rules"].Success)
                                {
                                    var splittedRules = match.Groups["rules"].Value.Split(new[] { ',' },
                                        StringSplitOptions.RemoveEmptyEntries);
                                    Rules = splittedRules;
                                }

                                return ++seed1;
                            });
                    }
                    if (i == 0)
                    {
                        if (!File.Exists(args[i]))
                        {
                            throw new FileNotFoundException($"Can't find file with provided path - {args[i]}");
                        }
                        if (Path.GetExtension(args[i]) != SolutionFileExtension)
                        {
                            throw new InvalidOperationException("Specified file must have .sln extension");
                        }

                        SolutionPath = args[i];
                    }

                    return ++i;
                });
        }

        public enum RunningMode
        {
            None,
            Fix,
            Stats
        }

        public static string Usage => @"Usage: " + Environment.NewLine +
                                    "saritasa.prettify solution.sln [Options]" + Environment.NewLine +
                                    "Options: " + Environment.NewLine +
                                    "--rules=[ruleName(s)] - name(s) of rule separated by comma" + Environment.NewLine +
                                    "--stats - key indicates to print statistics" + Environment.NewLine +
                                    "--fix - key indicates to use code fix providers" + Environment.NewLine +
                                    "Possible for usage only one key." + Environment.NewLine +
                                    "Example: saritasa.prettify solution.sln --rules=SA1633,SAXXXX --stats";

        public string SolutionPath { get; set; }

        public string[] Rules { get; set; }

        public RunningMode Mode { get; set; }
    }
}
