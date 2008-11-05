#!/bin/sh
rm -rf doc/xml
mkdir doc 2> /dev/null
mkdir doc/xml 2> /dev/null
mkdir doc/html 2> /dev/null
monodocer -assembly YamlDotNet.Core/bin/Debug/YamlDotNet.Core.dll -importslashdoc YamlDotNet.Core/bin/Debug/YamlDotNet.Core.xml -path doc/xml
monodocer -assembly YamlDotNet.RepresentationModel/bin/Debug/YamlDotNet.RepresentationModel.dll -importslashdoc YamlDotNet.RepresentationModel/bin/Debug/YamlDotNet.RepresentationModel.xml -path doc/xml
monodocer -assembly YamlDotNet.Converters/bin/Debug/YamlDotNet.Converters.dll -importslashdoc YamlDotNet.Converters/bin/Debug/YamlDotNet.Converters.xml -path doc/xml
monodocer -assembly YamlDotNet.Configuration/bin/Debug/YamlDotNet.Configuration.dll -importslashdoc YamlDotNet.Configuration/bin/Debug/YamlDotNet.Configuration.xml -path doc/xml
monodocs2html -source doc/xml -dest doc/html

