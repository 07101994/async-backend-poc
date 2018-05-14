# async-backend-poc
PoC async backend operations in .NET core using websockets and Redis pub/sub.

## Run the PoC

```
# note: it needs docker to work
./run.sh --servers=3 --clients=3
```
Options:
* `--servers`: number of servers to run. Port numbers will start from 5000. Default 3.
* `--clients`: number of clients to run. Default 3.
