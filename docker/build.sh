#!/bin/bash

docker build --build-arg userId=`id -u` --build-arg groupId=`id -g` -t "aaubry/yamldotnet" .
