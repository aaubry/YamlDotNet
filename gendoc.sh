#!/bin/sh
rm -rf doc/xml
mkdir doc 2> /dev/null
mkdir doc/xml 2> /dev/null
mkdir doc/html 2> /dev/null
monodocer YamlDotNet.Core/bin/Debug/YamlDotNet.Core.dll -i YamlDotNet.Core/bin/Debug/YamlDotNet.Core.xml -o doc/xml
monodocer YamlDotNet.RepresentationModel/bin/Debug/YamlDotNet.RepresentationModel.dll -i YamlDotNet.RepresentationModel/bin/Debug/YamlDotNet.RepresentationModel.xml -o doc/xml
monodocer YamlDotNet.Converters/bin/Debug/YamlDotNet.Converters.dll -i YamlDotNet.Converters/bin/Debug/YamlDotNet.Converters.xml -o doc/xml
#monodocer YamlDotNet.Configuration/bin/Debug/YamlDotNet.Configuration.dll -i YamlDotNet.Configuration/bin/Debug/YamlDotNet.Configuration.xml -o doc/xml
monodocs2html doc/xml -o doc/html

