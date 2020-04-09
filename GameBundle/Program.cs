using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using CommandLine;

namespace GameBundle {
    internal static class Program {

        private static int Main(string[] args) {
            return Parser.Default.ParseArguments<Options>(args).MapResult(Run, _ => -1);
        }

        private static int Run(Options options) {
            if (RunProcess(options, "dotnet", "tool install nulastudio.ncbeauty -g") < 0)
                return -1;

            var proj = new FileInfo(options.SourceFile);
            if (!proj.Exists) {
                Console.WriteLine("Project file not found at " + proj.FullName);
                return -1;
            }
            if (options.Verbose)
                Console.WriteLine("Found project file at " + proj.FullName);

            var bundleDir = new DirectoryInfo(Path.Combine(options.OutputDirectory, "Bundled"));
            if (!bundleDir.Exists)
                bundleDir.Create();

            if (options.BundleWindows)
                Publish(options, proj, $"{bundleDir}/win", options.Publish32Bit ? "win-x86" : "win-x64");
            if (options.BundleLinux)
                Publish(options, proj, $"{bundleDir}/linux", "linux-x64");
            if (options.BundleMac)
                Publish(options, proj, $"{bundleDir}/mac", "osx-x64");
            
            return 0;
        }

        private static void Publish(Options options, FileInfo proj, string path, string rid) {
            RunProcess(options, "dotnet", $"publish {proj.FullName} -o {path} -r {rid} /p:PublishTrimmed={options.Trim}");

            // Run beauty
            var excludes = string.Empty;
            if (options.ExcludedFiles.Length > 0)
                excludes = "excludes=" + string.Join(";", options.ExcludedFiles);
            var log = options.Verbose ? "Detail" : "Error";
            RunProcess(options, "ncbeauty", $"--loglevel={log} --force=True {path} Lib {excludes}");

            // Remove the beauty file since it's just a marker
            var beautyFile = new FileInfo(Path.Combine(path, "NetCoreBeauty"));
            if (beautyFile.Exists)
                beautyFile.Delete();
        }

        private static int RunProcess(Options options, string program, string args) {
            if (options.Verbose)
                Console.WriteLine($"$ {program} {args}");
            var process = Process.Start(program, args);
            process.WaitForExit();
            return process.ExitCode;
        }

    }
}