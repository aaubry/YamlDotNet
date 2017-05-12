#!/bin/bash

if [ -d ../YamlDotNet.wiki ]
  then
    docker run -u=`id -u` -v `pwd`:/build/YamlDotNet -v `pwd`/../YamlDotNet.wiki:/build/YamlDotNet.wiki -w /build/YamlDotNet -it aaubry/yamldotnet.local ./build.sh "$@"
  else
    docker run -u=`id -u` -v `pwd`:/build/YamlDotNet -w /build/YamlDotNet -it aaubry/yamldotnet.local ./build.sh "$@"
fi
