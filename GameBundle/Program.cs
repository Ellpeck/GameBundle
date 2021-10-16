using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;

namespace GameBundle {
    internal static class Program {

        private static int Main(string[] args) {
            return new Parser(c => {
                c.HelpWriter = Console.Error;
                c.EnableDashDash = true;
            }).ParseArguments<Options>(args).MapResult(Run, _ => -1);
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

            var builtAnything = false;
            if (options.BuildWindows) {
                Console.WriteLine("Bundling for windows");
                var res = Publish(options, proj, GetBuildDir(options, proj, "win"), options.Publish32Bit ? "win-x86" : "win-x64");
                if (res != 0)
                    return res;
                builtAnything = true;
            }
            if (options.BuildLinux) {
                Console.WriteLine("Bundling for linux");
                var res = Publish(options, proj, GetBuildDir(options, proj, "linux"), "linux-x64");
                if (res != 0)
                    return res;
                builtAnything = true;
            }
            if (options.BuildMac) {
                Console.WriteLine("Bundling for mac");
                var dir = GetBuildDir(options, proj, "mac");
                var res = Publish(options, proj, dir, "osx-x64", () => {
                    if (options.MacBundle)
                        CreateMacBundle(options, new DirectoryInfo(dir), proj);
                });
                if (res != 0)
                    return res;
                builtAnything = true;
            }

            if (!builtAnything)
                Console.WriteLine("No build took place. Supply -w, -l or -m arguments or see available arguments using --help.");
            Console.WriteLine("Done");
            return 0;
        }

        private static int Publish(Options options, FileInfo proj, string path, string rid, Action additionalAction = null) {
            var publishResult = RunProcess(options, "dotnet", $"publish \"{proj.FullName}\" -o \"{path}\" -r {rid} -c {options.BuildConfig} /p:PublishTrimmed={options.Trim} {options.BuildArgs}");
            if (publishResult != 0)
                return publishResult;

            // Run beauty
            if (!options.SkipLib) {
                var excludes = $"\"{string.Join(";", options.ExcludedFiles)}\"";
                var log = options.Verbose ? "Detail" : "Error";
                var beautyResult = RunProcess(options, "dotnet", $"ncbeauty --loglevel={log} --force=True \"{path}\" \"{options.LibFolder}\" {excludes}", AppDomain.CurrentDomain.BaseDirectory);
                if (beautyResult != 0)
                    return beautyResult;

                // Remove the beauty file since it's just a marker
                var beautyFile = new FileInfo(Path.Combine(path, "NetCoreBeauty"));
                if (beautyFile.Exists)
                    beautyFile.Delete();
            }

            // Run any additional actions like creating the mac bundle
            additionalAction?.Invoke();

            // Zip the output if required
            if (options.Zip) {
                var zipLocation = Path.Combine(Directory.GetParent(path).FullName, Path.GetFileName(path) + ".zip");
                File.Delete(zipLocation);
                ZipFile.CreateFromDirectory(path, zipLocation, CompressionLevel.Optimal, true);
                Directory.Delete(path, true);
            }
            return 0;
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

        private static void CreateMacBundle(Options options, DirectoryInfo dir, FileInfo proj) {
            var files = dir.GetFiles();
            var dirs = dir.GetDirectories();

            // figure out the app name, which should match the binary (and dll) name
            var appName = Path.GetFileNameWithoutExtension(proj.Name);
            foreach (var file in files) {
                if (!string.IsNullOrEmpty(file.Extension))
                    continue;
                if (files.Any(f => f.Extension == ".dll" && Path.GetFileNameWithoutExtension(f.Name) == file.Name)) {
                    if (options.Verbose)
                        Console.WriteLine($"Choosing app name {file.Name} from binary");
                    appName = file.Name;
                    break;
                }
            }

            var app = dir.CreateSubdirectory($"{appName}.app");
            var contents = app.CreateSubdirectory("Contents");
            var resources = contents.CreateSubdirectory("Resources");
            var macOs = contents.CreateSubdirectory("MacOS");
            var resRegex = options.MacBundleResources.Select(GlobRegex).ToArray();
            var ignoreRegex = options.MacBundleIgnore.Select(GlobRegex).ToArray();

            foreach (var file in files) {
                if (ignoreRegex.Any(r => r.IsMatch(file.Name)))
                    continue;
                var destDir = resRegex.Any(r => r.IsMatch(file.Name)) ? resources : macOs;
                if (file.Name.EndsWith("plist"))
                    destDir = contents;
                file.MoveTo(Path.Combine(destDir.FullName, file.Name), true);
            }
            foreach (var sub in dirs) {
                if (ignoreRegex.Any(r => r.IsMatch(sub.Name)))
                    continue;
                var destDir = resRegex.Any(r => r.IsMatch(sub.Name)) ? resources : macOs;
                var dest = new DirectoryInfo(Path.Combine(destDir.FullName, sub.Name));
                if (dest.Exists)
                    dest.Delete(true);
                sub.MoveTo(dest.FullName);
            }
            File.WriteAllText(Path.Combine(contents.FullName, "PkgInfo"), "APPL????");
        }

        private static Regex GlobRegex(string s) {
            return new Regex(s.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));
        }

        private static string GetBuildDir(Options options, FileInfo proj, string osName) {
            var dir = Path.GetFullPath(options.OutputDirectory);
            if (options.NameBuilds)
                return $"{dir}/{Path.GetFileNameWithoutExtension(proj.Name)}-{osName}";
            return $"{dir}/{osName}";
        }

    }
}