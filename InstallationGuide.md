# Installations guide af Fileodon lokal opsætning
Denne guide er til hvis du bare vil køre projektet uden viden omkring det. Den dækker også at køre lokale kontainere hvis du ikke har nogle eksterne serverere at forbinde. 

### Forudsætninger
* .NET 10
* (optional) Docker 

## Build
* byg til linux fra windows
```sh
dotnet publish .\cli.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o .\artifacts\linux-x64
Compress-Archive -Path .\artifacts\linux-x64\* -DestinationPath .\artifacts\fileodon-cli-linux-x64.zip -Force
```

## (optional) Kør docker kontainerer og forbind dem til din server
* Gør sftp forbindelse klar til at kunne oprette forbindelse.
```sh
docker compose up -d 
ssh -p 2222 foo@localhost
su
apt-get update && apt install -y unzip libicu-dev
cd upload
mkdir -p fileodon-cli
unzip fileodon-cli-linux-x64.zip -d fileodon-cli
cd fileodon-cli
chmod +x ./cli
sudo ./cli <Kode fra admin panelet> --verbose=true --api="http://host.docker.internal:5283" --port=2222 --host=127.0.0.1
```
* info: port og host er optional, port 22 vil blive brugt pr standart, og host fundet ud fra et endpoint fra den indtastet API. 
* For at se alle muligheder
```sh
./cli --help
```

* accepter derefter forbindsel i admin panelet, og forbindelsen opsættes næste gang der bliver pollet, indenfor 5 sekunder.


sudo ./cli XSNDC8S9 --verbose=true --api="http://host.docker.internal:5283" --port=2222 --host=127.0.0.1
