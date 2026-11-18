# Fileodon CLI

Fileodon CLI er et kommandolinjeværktøj til at oprette og konfigurere en SFTP-forbindelse fra en Linux-server til Fileodon.

## Formål

Værktøjet modtager en autentificeringskode fra Fileodon-administration, sender serverens adresse til API'en og afventer godkendelse. Når forbindelsen er godkendt, installerer CLI'en den returnerede SSH-nøgle lokalt og opretter den nødvendige `fileodon`-bruger og uploadmappe.

Værktøjet understøtter i øjeblikket kun Linux.

## Teknologi

- .NET 10
- Spectre.Console CLI
- REST API-kald
- SSH og lokal Linux-brugeradministration
- Docker-eksempler til lokal test

## Sådan bruges det

Kør den kompilerede fil med en autentificeringskode:

```sh
sudo ./cli <autentificeringskode>
```

Du kan også angive værdier som optioner:

```sh
sudo ./cli <autentificeringskode> --api="https://din-api-url" --host="din-server" --port=22 --verbose=true
```

Se alle tilgængelige optioner med:

```sh
./cli --help
```

For en mere detaljeret lokal opsætning kan du se [Installationsguide](InstallationGuide.md).

## Udvikling

Byg projektet fra roden af repository'et:

```powershell
dotnet build .\cli.slnx
```

## Byg en Linux-release

Fra Windows kan du oprette en selvstændig Linux x64-build:

```powershell
dotnet publish .\cli.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o .\artifacts\linux-x64
Compress-Archive -Path .\artifacts\linux-x64\* -DestinationPath .\artifacts\fileodon-cli-linux-x64.zip -Force
```

På Linux kan filen kræve eksekveringsrettigheder:

```sh
chmod +x ./cli
```
