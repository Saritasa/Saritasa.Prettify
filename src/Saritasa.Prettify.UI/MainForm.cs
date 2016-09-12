using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using Saritasa.Prettify.Core;

namespace Saritasa.Prettify.UI
{
    public partial class MainForm : Form
    {
        private string solutionFile;
        private Assembly styleCopAnalyzerAssembly;
        private Assembly styleCopFixersAssembly;
        private string selectedDiagnosticId = string.Empty;
        private ImmutableArray<DiagnosticAnalyzer> analyzers;

        public MainForm()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            styleCopAnalyzerAssembly = AssembliesHelper.GetAnalyzersAssembly();
            styleCopFixersAssembly = AssembliesHelper.GetCodeFixAssembly();
            SeedCheckList();
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
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
                        // TODO: logging
                    }

                    return ++seed;
                });

            return assembly;
        }

        private void selectSolutionButton_Click(object sender, EventArgs e)
        {
            var dialog = this.openSolutionDialog.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                solutionFile = this.openSolutionDialog.FileName;
                if (Path.GetExtension(solutionFile) != ".sln")
                {
                    MessageBox.Show(this.Owner, "Please choice solution file.", "File select error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    this.selectedSolutionTextBox.Text = solutionFile;
                    this.currentStatusLabel.Text = $"Selected solution file {this.openSolutionDialog.SafeFileName}";
                    this.runButton.Enabled = true;
                }
            }
        }

        private void SeedCheckList()
        {
            analyzers = DiagnosticHelper.GetAnalyzersFromAssemblies(new[] { styleCopAnalyzerAssembly, styleCopFixersAssembly });
            foreach (var analyzer in analyzers)
            {
                foreach (var diagnostic in analyzer.SupportedDiagnostics)
                {
                    this.issuesChecked.Items.Add($"{diagnostic.Id}: {diagnostic.Title}");
                }
            }
        }

        private void selectAllCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < this.issuesChecked.Items.Count; i++)
            {
                this.issuesChecked.SetItemChecked(i, this.selectAllCheckBox.Checked);
            }
        }

        private string RetrieveId(string input)
        {
            var regex = new Regex("(?<id>S(A|X)[0-9]+([_p]+)?)", RegexOptions.Singleline | RegexOptions.Compiled);
            var match = regex.Match(input);
            if (match.Groups["id"].Success)
            {
                var id = match.Groups["id"];
                return id.Value;
            }

            return string.Empty;
        }

        private async void runButton_Click(object sender, EventArgs e)
        {
            this.runButton.Enabled = false;
            var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(this.solutionFile);
            var rules = this.issuesChecked.CheckedItems.OfType<string>()
                .Select(x => RetrieveId(x))
                .ToList();

            foreach (var solutionProject in solution.Projects)
            {
                var diagnostics = await ProjectHelper.GetProjectAnalyzerDiagnosticsAsync(analyzers, solutionProject, true);
                var projectAnalyzers = diagnostics.Where(x => !rules.Any() || rules.Contains(x.Id))
                    .ToList();

                outputTextBox.AppendText($"<======= Project: {solutionProject.Name} =======>");

                if (!diagnostics.Any())
                {
                    outputTextBox.AppendText($@"Can't find any diagnostic issues for {string.Join(",", this.issuesChecked.CheckedItems.OfType<string>())}");
                }
                else
                {
                    foreach (var projectAnalyzer in projectAnalyzers)
                    {
                        this.outputTextBox.AppendText(string.Format("DiagnosticId {0} - {1} in file {2}({3},{4})",
                            projectAnalyzer.Id,
                            projectAnalyzer.GetMessage(),
                            GetFormattedFileName(projectAnalyzer.Location.GetLineSpan().Path),
                            projectAnalyzer.Location.GetLineSpan().StartLinePosition.Line,
                            projectAnalyzer.Location.GetLineSpan().StartLinePosition.Character) + "\r\n");
                    }

                    if (this.fixIssues.Checked)
                    {
                        var codeFixes =
                        CodeFixProviderHelper.GetFixProviders(new[] { styleCopAnalyzerAssembly, styleCopFixersAssembly })
                        .Where(x => !rules.Any() || rules.Contains(x.Key))
                            .ToList();

                        var diagnistics = await DiagnosticHelper.GetAnalyzerDiagnosticsAsync(solution, analyzers, true);
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
                                this.outputTextBox.AppendText("[Warning] Allowed only one equivalence group for fix");
                                continue;
                            }

                            var operations = await fix.GetOperationsAsync();

                            if(operations.Length == 0)
                            {
                                this.outputTextBox.AppendText("[Information] No changes was found for this fixer");
                                continue;
                            }

                            operations[0].Apply(workspace, default(CancellationToken));

                            this.outputTextBox.AppendText($"[Information] Fixer with DiagnosticId {keyValuePair.Key} was applied");
                        }
                    }
                }
            }

            this.runButton.Enabled = true;
        }

        private void clearOutputButton_Click(object sender, EventArgs e)
        {
            this.outputTextBox.Clear();
        }

        private static string GetFormattedFileName(string input)
            => !string.IsNullOrWhiteSpace(input) ? "..\\" + Path.GetFileName(input) : string.Empty;

        private void issuesChecked_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.viewDescriptionButton.Enabled = true;
            this.openHelpUrlButton.Enabled = true;
            var selectedIndex = (sender as CheckedListBox).SelectedIndex;
            if(selectedIndex != default(int))
            {
                var checkBox = sender as CheckedListBox;
                var selectedValue = checkBox.Items[selectedIndex];
                var id = RetrieveId(selectedValue.ToString());
                selectedDiagnosticId = id;
            }
        }

        private void viewDescriptionButton_Click(object sender, EventArgs e)
        {
            var diagnostic = analyzers.SelectMany(x => x.SupportedDiagnostics)
                    .FirstOrDefault(x => x.Id == selectedDiagnosticId);

            MessageBox.Show(diagnostic.Id + ": " + diagnostic.Description.ToString() + Environment.NewLine + diagnostic.HelpLinkUri, 
                diagnostic.Title.ToString(), 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
        }

        private void openHelpUrlButton_Click(object sender, EventArgs e)
        {
            var diagnostic = analyzers.SelectMany(x => x.SupportedDiagnostics)
                    .FirstOrDefault(x => x.Id == selectedDiagnosticId);
            Process.Start(diagnostic.HelpLinkUri);
        }
    }
}
