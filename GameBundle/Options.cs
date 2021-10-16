using System;
using System.Collections.Generic;
using CommandLine;

namespace GameBundle {
    public class Options {

        [Option('s', "source", HelpText = "The location of the .csproj file that should be built and bundled. By default, the current directory is scanned for one")]
        public string SourceFile { get; set; }
        [Option('o', "output", Default = "bin/Bundled", HelpText = "The location of the directory that the bundles should be stored in")]
        public string OutputDirectory { get; set; }
        [Option('v', "verbose")]
        public bool Verbose { get; set; }
        [Option('w', "win", HelpText = "Bundle for windows")]
        public bool BuildWindows { get; set; }
        [Option('l', "linux", HelpText = "Bundle for linux")]
        public bool BuildLinux { get; set; }
        [Option('m', "mac", HelpText = "Bundle for mac")]
        public bool BuildMac { get; set; }
        [Option('b', "mac-bundle", HelpText = "Create an app bundle for mac")]
        public bool MacBundle { get; set; }
        [Option("mac-bundle-resources", Default = new[] {"Content", "*.icns"}, HelpText = "When creating an app bundle for mac, things that should go into the Resources folder rather than the MacOS folder")]
        public IEnumerable<string> MacBundleResources { get; set; }
        [Option("mac-bundle-ignore", Default = new string[0], HelpText = "When creating an app bundle for mac, things that should be left out of the mac bundle and stay in the output folder")]
        public IEnumerable<string> MacBundleIgnore { get; set; }
        [Option('z', "zip", HelpText = "Store the build results in zip files instead of folders")]
        public bool Zip { get; set; }
        [Option('e', "exclude", HelpText = "Files that should not be moved to the library folder")]
        public IEnumerable<string> ExcludedFiles { get; set; }
        [Option("32-bit", HelpText = "Publish for 32 bit instead of 64 bit. Note that this is only possible on Windows")]
        public bool Publish32Bit { get; set; }
        [Option('t', "trim", HelpText = "Trim the application when publishing")]
        public bool Trim { get; set; }
        [Option('c', "config", Default = "Release", HelpText = "The build configuration to use")]
        public string BuildConfig { get; set; }
        [Option("lib-name", Default = "Lib", HelpText = "The name of the library folder that is created")]
        public string LibFolder { get; set; }
        [Option('n', "name-builds", HelpText = "Name the build output directories by the project's name")]
        public bool NameBuilds { get; set; }
        [Option('d', "display-name", HelpText = "The name that should be used for named builds and the mac app bundle instead of the project's name")]
        public string DisplayName { get; set; }
        [Option('a', "build-args", HelpText = "Additional arguments that should be passed to the dotnet publish command")]
        public string BuildArgs { get; set; }

    }
}