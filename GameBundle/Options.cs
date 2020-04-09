using System.Collections.Generic;
using CommandLine;

namespace GameBundle {
    public class Options {

        [Option('s', "source", Required = true, HelpText = "The location of the .csproj file that should be built and bundled")]
        public string SourceFile { get; set; }
        [Option('o', "output", Required = true, HelpText = "The location of the directory that the bundles should be stored in")]
        public string OutputDirectory { get; set; }
        [Option('v', "verbose", Default = false)]
        public bool Verbose { get; set; }

        [Option('w', "win", Default = true, HelpText = "Bundle for windows")]
        public bool BundleWindows { get; set; }
        [Option('l', "linux", Default = true, HelpText = "Bundle for linux")]
        public bool BundleLinux { get; set; }
        [Option('m', "mac", Default = true, HelpText = "Bundle for mac")]
        public bool BundleMac { get; set; }

        [Option('e', "exclude", Default = new[] {"openal", "oal", "sdl2", "SDL2"}, HelpText = "Files like unmanaged libraries that should not be moved to the /Lib folder")]
        public string[] ExcludedFiles { get; set; }
        [Option("32bit", Default = false, HelpText = "Publish for 32 bit instead of 64 bit. Note that this is only possible on Windows")]
        public bool Publish32Bit { get; set; }
        [Option("trim", Default = true, HelpText = "If the application should be trimmed when being published")]
        public bool Trim { get; set; }

    }
}