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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using Saritasa.Prettify.Core;
using Saritasa.Prettify.UI.Utilities;

namespace Saritasa.Prettify.UI
{
    public partial class MainForm : Form
    {
        private const int TVIF_STATE = 0x8;
        private const int TVIS_STATEIMAGEMASK = 0xF000;
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETITEM = TV_FIRST + 63;

        private string solutionFile;
        private Assembly styleCopAnalyzerAssembly;
        private Assembly styleCopFixersAssembly;
        private string selectedDiagnosticId = string.Empty;
        private ImmutableArray<DiagnosticAnalyzer> analyzers;
        private HashSet<string> checkedItems = new HashSet<string>();

#pragma warning disable SA1307
        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
        private struct TVITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public IntPtr lParam;
        }
#pragma warning restore SA1307

#pragma warning disable SA1313
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam,
                                                 ref TVITEM lParam);
#pragma warning restore SA1313

        /// <summary>
        /// Hides the checkbox for the specified node on a TreeView control.
        /// </summary>
        private void HideCheckBox(TreeView tvw, TreeNode node)
        {
            var tvi = default(TVITEM);
            tvi.hItem = node.Handle;
            tvi.mask = TVIF_STATE;
            tvi.stateMask = TVIS_STATEIMAGEMASK;
            tvi.state = 0;
            SendMessage(tvw.Handle, TVM_SETITEM, IntPtr.Zero, ref tvi);
        }

        public MainForm()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            styleCopAnalyzerAssembly = AssembliesHelper.GetAnalyzersAssembly();
            styleCopFixersAssembly = AssembliesHelper.GetCodeFixAssembly();
            SeedCheckList();
            var autoCompleteSource = new AutoCompleteStringCollection();
            autoCompleteSource.AddRange(analyzers.SelectMany(x => x.SupportedDiagnostics).Select(x => $"{x.Id}: {x.Title}")
                .ToArray());
            this.autoCompleteTextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.autoCompleteTextBox.AutoCompleteCustomSource = autoCompleteSource;
            this.autoCompleteTextBox.TextChanged += AutoCompleteTextBox_TextChanged;
            this.Text += $" {ApplicationVersionUtility.GetVersion()}";
            this.DoubleBuffered = true;
        }

        private void AutoCompleteTextBox_TextChanged(object sender, EventArgs e)
        {
            this.issuesTreeView.Nodes.Clear();
            SeedCheckList(this.autoCompleteTextBox.Text);

            this.selectAllCheckBox.Checked = false;
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
                    catch (Exception)
                    {
                        // TODO: logging
                    }

                    return ++seed;
                });

            return assembly;
        }

        private void SelectSolutionButton_Click(object sender, EventArgs e)
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
                    this.fixButton.Enabled = true;
                    this.analyzeButton.Enabled = true;
                }
            }
        }

        private void SeedCheckList(string filterText = "")
        {
            analyzers = DiagnosticHelper.GetAnalyzersFromAssemblies(new[] { styleCopAnalyzerAssembly, styleCopFixersAssembly });
            foreach (var analyzer in analyzers)
            {
                foreach (var diagnostic in analyzer.SupportedDiagnostics)
                {
                    var node = this.issuesTreeView.Nodes[diagnostic.Category];
                    var text = $"{diagnostic.Id}: {diagnostic.Title}";
                    if (!string.IsNullOrWhiteSpace(filterText) && !text.Contains(filterText) && !checkedItems.Contains(text))
                    {
                        continue;
                    }
                    if (node == null)
                    {
                        node = new TreeNode
                        {
                            Text = diagnostic.Category,
                            Checked = false,
                            Name = diagnostic.Category
                        };
                        this.issuesTreeView.Nodes.Add(node);
                    }

                    var nodeFont = new Font(Font, FontStyle.Bold);
                    var childNode = new TreeNode
                    {
                        Text = text,
                        Checked = checkedItems.Contains(text)
                    };

                    if (RulesUtility.GetImportantRules().Contains(RetrieveId(childNode.Text)))
                    {
                        childNode.NodeFont = nodeFont;
                    }

                    node.Nodes.Add(childNode);
                }
            }

            if (this.issuesTreeView.Nodes.Count > 0)
            {
                HideCheckBox(this.issuesTreeView, this.issuesTreeView.Nodes[0]);
            }

            foreach (var node in this.issuesTreeView.Nodes.OfType<TreeNode>())
            {
                HideCheckBox(this.issuesTreeView, node);
            }

            this.issuesTreeView.Refresh();
        }

        private void SelectAllCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var senderTyped = sender as CheckBox;
            foreach (TreeNodeCollection nodes in issuesTreeView.Nodes.OfType<TreeNode>().Select(x => x.Nodes))
            {
                foreach (var node in nodes.OfType<TreeNode>())
                {
                    node.Checked = senderTyped.Checked;
                    if (node.Checked)
                    {
                        checkedItems.Add(node.Text);
                    }
                    else
                    {
                        checkedItems.Remove(node.Text);
                    }
                }
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

        private async Task Run(bool fixIssues = false)
        {
            var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(this.solutionFile);
            var rules = this.checkedItems
                .Select(x => RetrieveId(x))
                .ToList();

            var filesToBeFixed = new HashSet<string>();
            var diagnosticsToBeFixes = new HashSet<string>();

            foreach (var solutionProject in solution.Projects)
            {
                var diagnostics = await ProjectHelper.GetProjectAnalyzerDiagnosticsAsync(analyzers, solutionProject, true);
                var projectAnalyzers = diagnostics.Where(x => !rules.Any() || rules.Contains(x.Descriptor.Id))
                    .ToList();

                outputTextBox.AppendText($"<======= Project: {solutionProject.Name} =======>\r\n");

                if (!projectAnalyzers.Any())
                {
                    outputTextBox.AppendText($"Can't find any diagnostic issues for {string.Join(",", rules)}\r\n");
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

                        filesToBeFixed.Add(projectAnalyzer.Location.GetLineSpan().Path);
                        diagnosticsToBeFixes.Add(projectAnalyzer.Id);
                    }

                    if (fixIssues)
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
                                this.outputTextBox.AppendText("[Warning] Allowed only one equivalence group for fix\r\n");
                                continue;
                            }

                            var operations = await fix.GetOperationsAsync();

                            if (operations.Length == 0)
                            {
                                this.outputTextBox.AppendText("[Information] No changes was found for this fixer\r\n");
                                continue;
                            }

                            operations[0].Apply(workspace, default(CancellationToken));

                            this.outputTextBox.AppendText($"[Information] Fixer with DiagnosticId {keyValuePair.Key} was applied\r\n");
                        }
                    }
                }
            }

            this.outputTextBox.AppendText($"Done, found {diagnosticsToBeFixes.Count} issues in {filesToBeFixed.Count} files.\r\n");
        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
            this.fixButton.Enabled = false;
            this.analyzeButton.Enabled = false;

            await Run(true);

            this.analyzeButton.Enabled = true;
            this.fixButton.Enabled = true;
        }

        private void ClearOutputButton_Click(object sender, EventArgs e)
        {
            this.outputTextBox.Clear();
        }

        private static string GetFormattedFileName(string input)
            => !string.IsNullOrWhiteSpace(input) ? "..\\" + Path.GetFileName(input) : string.Empty;

        private void IssuesChecked_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.openHelpUrlButton.Enabled = true;
            var selectedIndex = (sender as CheckedListBox).SelectedIndex;
            if (selectedIndex != default(int))
            {
                var checkBox = sender as CheckedListBox;
                var selectedValue = checkBox.Items[selectedIndex];
                var id = RetrieveId(selectedValue.ToString());
                selectedDiagnosticId = id;
            }
        }

        private void ViewDescriptionButton_Click(object sender, EventArgs e)
        {
            var diagnostic = analyzers.SelectMany(x => x.SupportedDiagnostics)
                    .FirstOrDefault(x => x.Id == selectedDiagnosticId);

            MessageBox.Show(diagnostic.Id + ": " + diagnostic.Description.ToString() + Environment.NewLine + diagnostic.HelpLinkUri,
                diagnostic.Title.ToString(),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OpenHelpUrlButton_Click(object sender, EventArgs e)
        {
            var diagnostic = analyzers.SelectMany(x => x.SupportedDiagnostics)
                    .FirstOrDefault(x => x.Id == selectedDiagnosticId);
            Process.Start(diagnostic.HelpLinkUri);
        }

        private void IssuesTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Checked)
            {
                checkedItems.Add(e.Node.Text);
            }
            else
            {
                checkedItems.Remove(e.Node.Text);
            }
        }

        private void IssuesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var id = RetrieveId(e.Node.Text);
            if (string.IsNullOrWhiteSpace(id))
            {
                this.openHelpUrlButton.Enabled = false;
                return;
            }

            selectedDiagnosticId = id;
            this.openHelpUrlButton.Enabled = true;
        }

        private async void AnalyzeButton_Click(object sender, EventArgs e)
        {
            this.fixButton.Enabled = false;
            this.analyzeButton.Enabled = false;

            await Run();

            this.analyzeButton.Enabled = true;
            this.fixButton.Enabled = true;
        }
    }
}
