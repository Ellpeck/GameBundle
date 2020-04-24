using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;

namespace GameBundle {
    internal static class Program {

        private static int Main(string[] args) {
            return Parser.Default.ParseArguments<Options>(args).MapResult(Run, _ => -1);
        }

        private static int Run(Options options) {
            // make sure all of the required tools are installed
            if (RunProcess(options, "dotnet", "tool restore", AppDomain.CurrentDomain.BaseDirectory) != 0)
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
                Publish(options, proj, $"{bundleDir.FullName}/win", options.Publish32Bit ? "win-x86" : "win-x64");
            }
            if (options.BuildLinux) {
                Console.WriteLine("Bundling for linux");
                Publish(options, proj, $"{bundleDir.FullName}/linux", "linux-x64");
            }
            if (options.BuildMac) {
                Console.WriteLine("Bundling for mac");
                var dir = $"{bundleDir.FullName}/mac";
                Publish(options, proj, dir, "osx-x64");
                if (options.MacBundle)
                    CreateMacBundle(options, new DirectoryInfo(dir), proj.FullName);
            }

            Console.WriteLine("Done");
            return 0;
        }

        private static void Publish(Options options, FileInfo proj, string path, string rid) {
            RunProcess(options, "dotnet", $"publish {proj.FullName} -o {path} -r {rid} -c {options.BuildConfig} /p:PublishTrimmed={!options.NoTrim}");

            // Run beauty
            var excludes = '"' + string.Join(";", options.ExcludedFiles) + '"';
            var log = options.Verbose ? "Detail" : "Error";
            RunProcess(options, "dotnet", $"ncbeauty --loglevel={log} --force=True {path} {options.LibFolder} {excludes}", AppDomain.CurrentDomain.BaseDirectory);

            // Remove the beauty file since it's just a marker
            var beautyFile = new FileInfo(Path.Combine(path, "NetCoreBeauty"));
            if (beautyFile.Exists)
                beautyFile.Delete();
        }

        private static int RunProcess(Options options, string program, string args, string workingDir = "") {
            if (options.Verbose)
                Console.WriteLine($"> {program} {args}");
            var info = new ProcessStartInfo(program, args) {WorkingDirectory = workingDir};
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
            var dir = new DirectoryInfo(".");
            foreach (var file in dir.EnumerateFiles()) {
                if (Path.GetExtension(file.FullName).Contains("proj"))
                    return file;
            }
            return null;
        }

        private static void CreateMacBundle(Options options, DirectoryInfo dir, string proj) {
            var app = dir.CreateSubdirectory($"{Path.GetFileNameWithoutExtension(proj)}.app");
            var contents = app.CreateSubdirectory("Contents");
            var resources = contents.CreateSubdirectory("Resources");
            var macOs = contents.CreateSubdirectory("MacOS");
            var resRegex = GlobRegex(options.MacBundleResources);
            foreach (var file in dir.GetFiles()) {
                var destDir = resRegex.IsMatch(file.Name) ? resources : macOs;
                if (file.Name.EndsWith("plist"))
                    destDir = app;
                file.MoveTo(Path.Combine(destDir.FullName, file.Name), true);
            }
            foreach (var sub in dir.GetDirectories()) {
                if (sub.Name == app.Name)
                    continue;
                var destDir = resRegex.IsMatch(sub.Name) ? resources : macOs;
                var dest = new DirectoryInfo(Path.Combine(destDir.FullName, sub.Name));
                if (dest.Exists)
                    dest.Delete(true);
                sub.MoveTo(dest.FullName);
            }
        }

        private static Regex GlobRegex(IEnumerable<string> strings) {
            return new Regex('(' + string.Join("|", strings.Select(s => s.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."))) + ')');
        }

    }
}