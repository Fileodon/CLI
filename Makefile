all:
	dotnet run
build:
	dotnet build
full:
	dotnet run --verbose=true --api="https://api.dev.fileodon.dev" --token="your_token_here" --name="Please" --org=1 --port=22 --host="158.220.124.100"
jakob:
	dotnet run --verbose=true --port=2222 --host="127.0.0.1" --name="Jakob"
delete:
	dotnet run --verbose=true --delete=true
help:
	dotnet run --help
