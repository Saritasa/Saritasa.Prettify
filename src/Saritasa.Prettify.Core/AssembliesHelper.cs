using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Prettify.Core
{
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
    }
}
