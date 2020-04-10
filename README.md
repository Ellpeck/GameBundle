<img src="Logo.png" width="25%" >

**GameBundle** is a tool to package MonoGame and other .NET Core applications into several distributable formats.

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

GameBundle will then build a self-contained release of your application for each system using `dotnet publish` and clean up the output directory using [NetCoreBeauty](https://github.com/nulastudio/NetCoreBeauty) by moving most of the libraries into a `Lib` subdirectory.

# Configuring
GameBundle takes several optional arguments to modify the way it works. To see a list of all possible arguments, simply run
```
gamebundle --help
```