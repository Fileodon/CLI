all:
	dotnet run
build:
	dotnet build
full:
	dotnet run --verbose=true --api="https://localhost" --token="your_token_here" --name="Please" --org=1 --port=7248 --host="127.0.0.1"
jakob:
	dotnet run --verbose=true --port=2222 --host="127.0.0.1" --name="Jakob"
delete:
	dotnet run --verbose=true --delete=true
help:
	dotnet run --help
