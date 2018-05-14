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

clean () {
  for i in $(seq 1 $CLIENTS); do docker rm async-client-$i -f; done
  for i in $(seq 1 $SERVERS); do docker rm async-backend-$i -f; done
  docker rm redis -f
  docker network rm async-backend-poc
}

trap 'clean; exit 1' INT

docker network create async-backend-poc > /dev/null

echo "Starting redis server"
docker run -d --rm --name redis --network async-backend-poc redis:4.0

echo "Building server docker image"
docker build --force-rm -f Dockerfile-backend -t async-backend . > /dev/null

echo "Starting $SERVERS servers"

for i in $(seq 1 $SERVERS)
do
  docker run -i --rm --name async-backend-$i --network async-backend-poc -p 500$i:80 async-backend &
done

sleep 5

echo "Building client docker image"
docker build --force-rm -f Dockerfile-client -t async-client . > /dev/null

echo "Starting $CLIENTS clients"
for i in $(seq 1 $CLIENTS)
do
  SERVER=$(shuf -i1-$SERVERS -n1)
  docker run -i --rm --name async-client-$i --network async-backend-poc async-client async-backend-$SERVER:80 &
done

wait

echo "Process finished, cleaning it up.."
clean