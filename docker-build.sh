#!/bin/bash

docker run -u=`id -u` -v `pwd`:/build -w /build -it aaubry/yamldotnet.local ./build.sh "$@"
