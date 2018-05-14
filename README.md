# async-backend-poc
PoC async backend operations in .NET core using websockets and Redis pub/sub.

## Run the PoC

```
# prerequisites: redis already running (setup conn string in appsettings.json) and dotnet cli
./run.sh --servers=3 --clients=3
```
Options:
* `--servers`: number of servers to run. Port numbers will start from 5000. Default 3.
* `--clients`: number of clients to run. Default 3.
