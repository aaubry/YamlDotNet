#!/bin/bash

docker build --build-arg userId=`id -u` --build-arg groupId=`id -g` -t "yamldotnet" tools/docker/ && docker run -u=`id -u` -v `pwd`:/build -w /build -it yamldotnet ./build.sh "$@"
 #./build.sh --target Build --configuration Release-DotNetCore
