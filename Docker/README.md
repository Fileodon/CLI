This package is for Linux x64, not Windows or ARM. To connect to the second
SFTP container instead, use port `2223` and user `poo`.

korrekt formatering på scripts
kørte

```sh
docker compose up -d --force-recreate sftp sftp2
ssh -p 2222 foo@localhost
su
cd upload
mkdir -p fileodon-cli
unzip fileodon-cli-linux-x64.zip -d fileodon-cli
cd fileodon-cli
chmod +x ./cli
./cli --verbose=true --api="http://host.docker.internal:5283" --token="PLE0EIEG" --name="Pleas" --org=18 --port=2222 --host=127.0.0.1
```

For at fjerne volumes under udvikling. Når du fjerne dem skal du huske at fjerne tidligere referencer i din known_hosts
```sh
docker compose down -v
```