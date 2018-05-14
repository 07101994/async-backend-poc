#!/bin/bash

USAGE='Usage: ./run.sh [OPTIONS]
\n\nOptions:
\n\t --servers \t Sets the number of servers (default 3)
\n\t --clients \t Sets the number of clients (default 3)
\n\nRuns a N clients that send messages to M servers that perform async operations and send the responses to their client.'

SERVERS=3
CLIENTS=3

for i in "$@"
do
case $i in
-h|--help)
  echo -e $USAGE
  exit 1
  ;;
--servers=*)
  SERVERS="${i#*=}"
  ;;
--clients=*)
  CLIENTS="${i#*=}"
  ;;
esac
done

docker network create async-backend-poc
echo "Building server docker image.."
docker build --force-rm -f Dockerfile-backend -t async-backend .

echo "Starting $SERVERS servers"

for ((i=1; i<=$SERVERS; i++))
do
  docker run --rm -it --name async-backend-$i --network async-backend-poc -p 500$i:80 async-backend &
done
wait

sleep 5000

echo "Building client docker image.."
docker build --force-rm -f Dockerfile-client -t async-client .

echo "Starting $CLIENTS clients"
cd $ROOT
for ((i=1; i<=$CLIENTS; i++))
do
  docker run --rm -it --name async-client-$i --network async-backend-poc async-client async-backend-1:5001 &
done
wait

echo "Process finished, cleaning it up.."
docker networm rm async-backend-poc