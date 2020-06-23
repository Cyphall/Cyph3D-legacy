@echo off
dotnet publish -r win10-x64 -c Release -o "Cyph3D/bin/Publish" --self-contained true -p:PublishTrimmed=true -p:PublishSingleFile=true -p:DebugType=none
explorer "Cyph3D\bin\Publish"