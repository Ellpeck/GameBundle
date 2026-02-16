![The GameBundle logo](https://raw.githubusercontent.com/Ellpeck/GameBundle/main/Banner.png)

**GameBundle** is a tool to package MonoGame games and other .NET applications into several distributable formats.

It is currently being used for games like Ellpeck's [Tiny Life](https://tinylifegame.com) and Goblin Game Studio's [Vulcard](https://store.steampowered.com/app/3764530/Vulcard/).

# Installing
GameBundle is a `dotnet` tool, meaning you can install it very easily like so:
```
dotnet tool install --global GameBundle
```

# Using
By default, GameBundle builds the `.csproj` file that it finds in the directory that it is run from. The bundled outputs go into `bin/Bundled` by default.

To build and bundle your app for Windows, Linux and Mac, all you have to do is run the following command from the directory that contains your project file:
```
gamebundle -wlm
```

GameBundle will then automatically build a self-contained release of your application for each system using `dotnet publish` and clean up the output directory using [NetBeauty](https://github.com/nulastudio/NetBeauty2) by moving most of the libraries into a `Lib` subdirectory.

## Building a MonoGame Project
If you're building a MonoGame project using MonoGame's DesktopGL version 3.8.2.1105 or later, you can additionally supply the `--mg` argument to automatically exclude MonoGame's native libraries from being moved into the `Lib` subdirectory, which is a requirement for your game to run.

# Configuring
GameBundle takes several optional arguments to modify the way it works. To see a list of all possible arguments, simply run
```
gamebundle --help
```

Here is a list of them as of GameBundle version 1.8.1:
```
  -s, --source              The location of the .csproj file that should be built and bundled. By default, the current directory is scanned for one

  -o, --output              (Default: bin/Bundled) The location of the directory that the bundles should be stored in

  -v, --verbose             Display verbose output while building

  -w, --win                 Bundle for windows

  -l, --linux               Bundle for linux

  -m, --mac                 Bundle for mac

  --win-rid                 (Default: win-x64) The RID to use for windows builds

  --linux-rid               (Default: linux-x64) The RID to use for linux builds

  --mac-rid                 (Default: osx-x64) The RID to use for mac builds

  -W, --win-arm             Bundle for windows arm

  -L, --linux-arm           Bundle for linux arm

  -M, --mac-arm             Bundle for mac arm

  --win-arm-rid             (Default: win-arm64) The RID to use for windows arm builds

  --linux-arm-rid           (Default: linux-arm64) The RID to use for linux arm builds

  --mac-arm-rid             (Default: osx-arm64) The RID to use for mac arm builds

  -z, --zip                 Store the build results in zip files instead of folders

  -b, --mac-bundle          Create an app bundle for mac

  --mac-bundle-resources    (Default: Content *.icns) When creating an app bundle for mac, things that should go into the Resources folder rather than the MacOS folder

  --mac-bundle-ignore       When creating an app bundle for mac, things that should be left out of the mac bundle and stay in the output folder

  --nbeauty2                Use NetBeauty2 for beautifying instead of NetCoreBeauty

  --skip-lib                When bundling, skip beautifying the output by moving files to the library folder

  -e, --exclude             Files that should not be moved to the library folder

  --mg                      Exclude MonoGame's native libraries from being moved to the library folder, which is a requirement for DesktopGL version 3.8.2.1105 or later.
                            This has the same behavior as supplying the --exclude arguments soft_oal.dll, openal.dll, SDL2.dll, libopenal.so.1, libopenal.so, libSDL2-2.0.so.0, libopenal.1.dylib,        
                            libopenal.dylib, libSDL2.dylib, libSDL2-2.0.0.dylib

  --lib-name                (Default: Lib) The name of the library folder that is created

  -t, --trim                Trim the application when publishing

  -A, --aot                 Use NativeAOT compilation mode

  -c, --config              (Default: Release) The build configuration to use

  -a, --build-args          Additional arguments that should be passed to the dotnet publish command

  -n, --name-builds         Name the build output directories by the name of the executable

  -N, --name-addition       An additional string of text that should be included in the names of the output directories

  -V, --include-version     Include the project's version in the names of the output directories

  --help                    Display this help screen.

  --version                 Display version information.
```
