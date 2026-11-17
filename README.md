## Build a Linux release

From PowerShell on Windows, publish a self-contained Linux x64 build:

```powershell
dotnet publish .\cli.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o .\artifacts\linux-x64
Compress-Archive -Path .\artifacts\linux-x64\* -DestinationPath .\artifacts\fileodon-cli-linux-x64.zip -Force
```

Upload `artifacts\fileodon-cli-linux-x64.zip`as at asset when creating a release in Github.
Give the release a version tag.
Test the build before sharing it. 
On Linux, users may need to run `chmod +x ./cli` before launching it.
 
