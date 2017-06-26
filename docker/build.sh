#!/bin/bash

#docker build -t "aaubry/yamldotnet" .
docker build --build-arg userId=`id -u` --build-arg groupId=`id -g` -t "aaubry/yamldotnet.local" -f Dockerfile.local .
