using System;
using System.Collections.Generic;
using CommandLine;

namespace GameBundle;

public class Options {

    [Option('s', "source", HelpText = "The location of the .csproj file that should be built and bundled. By default, the current directory is scanned for one")]
    public string SourceFile { get; set; }
    [Option('o', "output", Default = "bin/Bundled", HelpText = "The location of the directory that the bundles should be stored in")]
    public string OutputDirectory { get; set; }
    [Option('v', "verbose", HelpText = "Display verbose output while building")]
    public bool Verbose { get; set; }

    [Option('w', "win", HelpText = "Bundle for windows")]
    public bool BuildWindows { get; set; }
    [Option('l', "linux", HelpText = "Bundle for linux")]
    public bool BuildLinux { get; set; }
    [Option('m', "mac", HelpText = "Bundle for mac")]
    public bool BuildMac { get; set; }
    [Option("win-rid", Default = "win-x64", HelpText = "The RID to use for windows builds")]
    public string WindowsRid { get; set; }
    [Option("linux-rid", Default = "linux-x64", HelpText = "The RID to use for linux builds")]
    public string LinuxRid { get; set; }
    [Option("mac-rid", Default = "osx-x64", HelpText = "The RID to use for mac builds")]
    public string MacRid { get; set; }

    [Option('W', "win-arm", HelpText = "Bundle for windows arm")]
    public bool BuildWindowsArm { get; set; }
    [Option('L', "linux-arm", HelpText = "Bundle for linux arm")]
    public bool BuildLinuxArm { get; set; }
    [Option('M', "mac-arm", HelpText = "Bundle for mac arm")]
    public bool BuildMacArm { get; set; }
    [Option("win-arm-rid", Default = "win-arm64", HelpText = "The RID to use for windows arm builds")]
    public string WindowsArmRid { get; set; }
    [Option("linux-arm-rid", Default = "linux-arm64", HelpText = "The RID to use for linux arm builds")]
    public string LinuxArmRid { get; set; }
    [Option("mac-arm-rid", Default = "osx-arm64", HelpText = "The RID to use for mac arm builds")]
    public string MacArmRid { get; set; }

    [Option('z', "zip", HelpText = "Store the build results in zip files instead of folders")]
    public bool Zip { get; set; }
    [Option('b', "mac-bundle", HelpText = "Create an app bundle for mac")]
    public bool MacBundle { get; set; }
    [Option("mac-bundle-resources", Default = new[] {"Content", "*.icns"}, HelpText = "When creating an app bundle for mac, things that should go into the Resources folder rather than the MacOS folder")]
    public IEnumerable<string> MacBundleResources { get; set; }
    [Option("mac-bundle-ignore", HelpText = "When creating an app bundle for mac, things that should be left out of the mac bundle and stay in the output folder")]
    public IEnumerable<string> MacBundleIgnore { get; set; }

    [Option("skip-lib", HelpText = "When bundling, skip beautifying the output by moving files to the library folder")]
    public bool SkipLib { get; set; }
    [Option('e', "exclude", HelpText = "Files that should not be moved to the library folder")]
    public IEnumerable<string> ExcludedFiles { get; set; }
    [Option("mg", HelpText = "Exclude MonoGame's native libraries from being moved to the library folder, which is a requirement for DesktopGL version 3.8.2.1105 or later.\nThis has the same behavior as supplying the --exclude arguments soft_oal.dll, SDL2.dll, libopenal.so.1, libSDL2-2.0.so.0, libopenal.1.dylib and libSDL2.dylib")]
    public bool MonoGameExclusions { get; set; }
    [Option("lib-name", Default = "Lib", HelpText = "The name of the library folder that is created")]
    public string LibFolder { get; set; }

    [Option('t', "trim", HelpText = "Trim the application when publishing")]
    public bool Trim { get; set; }
    [Option('c', "config", Default = "Release", HelpText = "The build configuration to use")]
    public string BuildConfig { get; set; }
    [Option('a', "build-args", HelpText = "Additional arguments that should be passed to the dotnet publish command")]
    public string BuildArgs { get; set; }
    [Option('n', "name-builds", HelpText = "Name the build output directories by the name of the executable")]
    public bool NameBuilds { get; set; }
    [Option('N', "name-addition", HelpText = "An additional string of text that should be included in the names of the output directories")]
    public string NameAddition { get; set; }
    [Option('V', "include-version", HelpText = "Include the project's version in the names of the output directories")]
    public bool IncludeVersion { get; set; }

}
