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

        [Option('w', "no-win", HelpText = "Skip bundling for windows")]
        public bool NoWindows { get; set; }
        [Option('l', "no-linux", HelpText = "Skip bundling for linux")]
        public bool NoLinux { get; set; }
        [Option('m', "no-mac", HelpText = "Skip bundling for mac")]
        public bool NoMac { get; set; }

        [Option('e', "exclude", Default = new[] {"openal", "oal", "sdl2", "SDL2"}, HelpText = "Files like unmanaged libraries that should not be moved to the /Lib folder")]
        public string[] ExcludedFiles { get; set; }
        [Option("32-bit", HelpText = "Publish for 32 bit instead of 64 bit. Note that this is only possible on Windows")]
        public bool Publish32Bit { get; set; }
        [Option('t', "no-trim", HelpText = "Skip trimming the application when publishing")]
        public bool NoTrim { get; set; }
        [Option('c', "config", Default = "Release", HelpText = "The build configuration to use")]
        public string BuildConfig { get; set; }

    }
}