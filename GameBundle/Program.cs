using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;

namespace GameBundle; 

internal static class Program {

    private static int Main(string[] args) {
        return new Parser(c => {
            c.HelpWriter = Console.Error;
            c.EnableDashDash = true;
        }).ParseArguments<Options>(args).MapResult(Program.Run, _ => -1);
    }

    private static int Run(Options options) {
        // make sure all of the required tools are installed
        if (Program.RunProcess(options, "dotnet", "tool restore", AppDomain.CurrentDomain.BaseDirectory) != 0) {
            Console.WriteLine("dotnet tool restore failed, aborting");
            return -1;
        }

        var proj = Program.GetProjectFile(options);
        if (proj == null || !proj.Exists) {
            Console.WriteLine("Project file not found, aborting");
            return -1;
        }
        Console.WriteLine($"Bundling project {proj.FullName}");

        var builtAnything = false;
        var toBuild = new List<BuildConfig> {
            // regular builds
            new("windows", "win", options.WindowsRid, options.BuildWindows),
            new("linux", "linux", options.LinuxRid, options.BuildLinux),
            new("mac", "mac", options.MacRid, options.BuildMac, false, d => options.MacBundle ? Program.CreateMacBundle(options, d) : 0),
            // arm builds
            new("windows arm", "win-arm", options.WindowsArmRid, options.BuildWindowsArm, true),
            new("linux arm", "linux-arm", options.LinuxArmRid, options.BuildLinuxArm, true),
            new("mac arm", "mac-arm", options.MacArmRid, options.BuildMacArm, true, d => options.MacBundle ? Program.CreateMacBundle(options, d) : 0)
        };
        foreach (var config in toBuild) {
            if (config.ShouldBuild) {
                Console.WriteLine($"Bundling for {config.DisplayName}");
                var res = Program.Publish(options, proj, config);
                if (res != 0)
                    return res;
                builtAnything = true;
            }
        }
        if (!builtAnything)
            Console.WriteLine("No build took place. Supply -w, -l or -m arguments or see available arguments using --help.");

        Console.WriteLine("Done");
        return 0;
    }

    private static int Publish(Options options, FileInfo proj, BuildConfig config) {
        var buildDir = Program.GetBuildDir(options, config.DirectoryName);
        var publishResult = Program.RunProcess(options, "dotnet", $"publish \"{proj.FullName}\" -o \"{buildDir.FullName}\" -r {config.Rid} --self-contained -c {options.BuildConfig} /p:PublishTrimmed={options.Trim} {options.BuildArgs}");
        if (publishResult != 0)
            return publishResult;

        // Run beauty
        if (!options.SkipLib && !config.SkipLib) {
            var excludes = $"\"{string.Join(";", options.ExcludedFiles)}\"";
            var log = options.Verbose ? "Detail" : "Error";
            var beautyResult = Program.RunProcess(options, "dotnet", $"ncbeauty --loglevel={log} --force=True --noflag=True \"{buildDir.FullName}\" \"{options.LibFolder}\" {excludes}", AppDomain.CurrentDomain.BaseDirectory);
            if (beautyResult != 0)
                return beautyResult;
        }

        // Rename build folder if named builds are enabled
        if (options.NameBuilds) {
            var name = Program.GetBuildName(options, buildDir);
            if (name == null) {
                Console.WriteLine("Couldn't determine build name, aborting");
                return -1;
            }
            var dest = Path.Combine(buildDir.Parent.FullName, $"{name}-{buildDir.Name}");
            if (Directory.Exists(dest))
                Directory.Delete(dest, true);
            buildDir.MoveTo(dest);
            if (options.Verbose)
                Console.WriteLine($"Moved build directory to {buildDir.FullName}");
        }

        // Run any additional actions like creating the mac bundle
        if (config.AdditionalAction != null) {
            var result = config.AdditionalAction.Invoke(buildDir);
            if (result != 0)
                return result;
        }

        // Zip the output if required
        if (options.Zip) {
            var zipLocation = Path.Combine(buildDir.Parent.FullName, $"{buildDir.Name}.zip");
            if (File.Exists(zipLocation))
                File.Delete(zipLocation);
            ZipFile.CreateFromDirectory(buildDir.FullName, zipLocation, CompressionLevel.Optimal, true);
            buildDir.Delete(true);
            if (options.Verbose)
                Console.WriteLine($"Zipped build to {zipLocation}");
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

    private static int CreateMacBundle(Options options, DirectoryInfo buildDir) {
        var buildName = Program.GetBuildName(options, buildDir);
        var app = buildDir.CreateSubdirectory($"{buildName}.app");
        var contents = app.CreateSubdirectory("Contents");
        var resources = contents.CreateSubdirectory("Resources");
        var macOs = contents.CreateSubdirectory("MacOS");
        var resRegex = options.MacBundleResources.Select(Program.GlobRegex).ToArray();
        var ignoreRegex = options.MacBundleIgnore.Select(Program.GlobRegex).ToArray();

        if (options.Verbose)
            Console.WriteLine($"Creating app bundle {app}");

        foreach (var file in buildDir.GetFiles()) {
            if (ignoreRegex.Any(r => r.IsMatch(file.Name)))
                continue;
            var destDir = resRegex.Any(r => r.IsMatch(file.Name)) ? resources : macOs;
            if (file.Name.EndsWith("plist") || file.Name == "PkgInfo")
                destDir = contents;
            file.MoveTo(Path.Combine(destDir.FullName, file.Name), true);
        }
        foreach (var sub in buildDir.GetDirectories()) {
            if (sub.Name == app.Name || ignoreRegex.Any(r => r.IsMatch(sub.Name)))
                continue;
            var destDir = resRegex.Any(r => r.IsMatch(sub.Name)) ? resources : macOs;
            var dest = new DirectoryInfo(Path.Combine(destDir.FullName, sub.Name));
            if (dest.Exists)
                dest.Delete(true);
            sub.MoveTo(dest.FullName);
        }

        var info = Path.Combine(contents.FullName, "PkgInfo");
        if (!File.Exists(info)) {
            File.WriteAllText(info, "APPL????");
            if (options.Verbose)
                Console.WriteLine($"Creating package info at {info}");
        }

        return 0;
    }

    private static Regex GlobRegex(string s) {
        return new Regex(s.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));
    }

    private static DirectoryInfo GetBuildDir(Options options, string name) {
        if (options.NameAddition != null)
            name = $"{options.NameAddition}-{name}";
        return new DirectoryInfo(Path.Combine(Path.GetFullPath(options.OutputDirectory), name));
    }

    private static string GetBuildName(Options options, DirectoryInfo buildDir) {
        // determine build name based on the names of the exe or binary that have a matching dll file
        var files = buildDir.GetFiles();
        foreach (var file in files) {
            if (file.Extension != ".exe" && file.Extension != string.Empty)
                continue;
            var name = Path.GetFileNameWithoutExtension(file.Name);
            if (files.Any(f => f.Extension == ".dll" && Path.GetFileNameWithoutExtension(f.Name) == name))
                return name;
        }
        return null;
    }

    private readonly struct BuildConfig {

        public readonly string DisplayName;
        public readonly string DirectoryName;
        public readonly string Rid;
        public readonly bool ShouldBuild;
        public readonly bool SkipLib;
        public readonly Func<DirectoryInfo, int> AdditionalAction;

        public BuildConfig(string displayName, string directoryName, string rid, bool shouldBuild, bool skipLib = false, Func<DirectoryInfo, int> additionalAction = null) {
            this.DisplayName = displayName;
            this.DirectoryName = directoryName;
            this.Rid = rid;
            this.ShouldBuild = shouldBuild;
            this.SkipLib = skipLib;
            this.AdditionalAction = additionalAction;
        }

    }

}