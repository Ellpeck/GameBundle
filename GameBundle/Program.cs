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

            var proj = GetProjectFile(options);
            if (proj == null || !proj.Exists) {
                Console.WriteLine("Project file not found");
                return -1;
            }
            Console.WriteLine("Bundling project " + proj.FullName);

            var bundleDir = new DirectoryInfo(options.OutputDirectory);
            if (!bundleDir.Exists)
                bundleDir.Create();

            if (options.BuildWindows) {
                Console.WriteLine("Bundling for windows");
                Publish(options, proj, $"{bundleDir}/win", options.Publish32Bit ? "win-x86" : "win-x64");
            }
            if (options.BuildLinux) {
                Console.WriteLine("Bundling for linux");
                Publish(options, proj, $"{bundleDir}/linux", "linux-x64");
            }
            if (options.BuildMac) {
                Console.WriteLine("Bundling for mac");
                Publish(options, proj, $"{bundleDir}/mac", "osx-x64");
            }

            Console.WriteLine("Done");
            return 0;
        }

        private static void Publish(Options options, FileInfo proj, string path, string rid) {
            RunProcess(options, "dotnet", $"publish {proj.FullName} -o {path} -r {rid} -c {options.BuildConfig} /p:PublishTrimmed={!options.NoTrim}");

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
                Console.WriteLine($"> {program} {args}");
            var info = new ProcessStartInfo(program, args);
            if (!options.Verbose)
                info.CreateNoWindow = true;
            var process = Process.Start(info);
            process.WaitForExit();
            if (options.Verbose)
                Console.WriteLine($"{program} finished with exit code {process.ExitCode}");
            return process.ExitCode;
        }

        private static FileInfo GetProjectFile(Options options) {
            if (!string.IsNullOrEmpty(options.SourceFile))
                return new FileInfo(options.SourceFile);
            var dir = new DirectoryInfo(Environment.CurrentDirectory);
            foreach (var file in dir.EnumerateFiles()) {
                if (Path.GetExtension(file.FullName).Contains("proj"))
                    return file;
            }
            return null;
        }

    }
}