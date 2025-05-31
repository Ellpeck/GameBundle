dotnet build ../GameBundle/GameBundle.csproj
rmdir /S /Q "bin/Bundled"
"../GameBundle/bin/Debug/net8.0/GameBundle.exe" -wlmWL -bnV --mg -s Test.csproj -o bin/Bundled -v --mac-bundle-ignore macmain.txt -N ncbeauty
"../GameBundle/bin/Debug/net8.0/GameBundle.exe" -wlmWL -bnV --mg -s Test.csproj -o bin/Bundled -v --mac-bundle-ignore macmain.txt -N nbeauty2 --nbeauty2
