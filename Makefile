all:
	dotnet run
build:
	dotnet build
partial:
	sudo dotnet run BNHCPTAW --verbose=true --api="http://172.28.48.1:5283"
full:
	dotnet run LZVMWTOL --verbose=true --api="https://127.0.0.1:7248" --name="automatic_run" --port=22
jakob:
	dotnet run --verbose=true --port=2222 --host="127.0.0.1" --name="Jakob"
delete:
	dotnet run --verbose=true --delete=true
help:
	dotnet run --help
