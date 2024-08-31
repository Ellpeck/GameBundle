dotnet build ../GameBundle/GameBundle.csproj
rmdir /S /Q "bin/Bundled"
"../GameBundle/bin/Debug/net8.0/GameBundle.exe" -wlmWL -bnV -s Test.csproj -o bin/Bundled -v --mac-bundle-ignore macmain.txt -N beta
