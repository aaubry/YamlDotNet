#!/bin/sh

mono --aot=full ./bin/Release/net45/YamlDotNet.dll && \
    mono --aot=full ./bin/Release/net45/YamlDotNet.AotTest.exe && \
    mono --full-aot ./bin/Release/net45/YamlDotNet.AotTest.exe

echo $? > exitcode.txt
