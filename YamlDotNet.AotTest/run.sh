#!/bin/sh
set -e
msbuild /p:Configuration=Debug-AOT /t:Rebuild YamlDotNet.sln
mono --aot=full YamlDotNet.AotTest/bin/Debug/YamlDotNet.dll
mono --aot=full YamlDotNet.AotTest/bin/Debug/YamlDotNet.AotTest.exe
mono --full-aot YamlDotNet.AotTest/bin/Debug/YamlDotNet.AotTest.exe
