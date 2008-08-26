#!/bin/sh
rm -rf doc/xml
mkdir doc
mkdir doc/xml
mkdir doc/html
monodocer -assembly YamlDotNet.Core/bin/Debug/YamlDotNet.Core.dll -importslashdoc YamlDotNet.Core/bin/Debug/YamlDotNet.Core.xml -path doc/xml
monodocer -assembly YamlDotNet.RepresentationModel/bin/Debug/YamlDotNet.RepresentationModel.dll -importslashdoc YamlDotNet.RepresentationModel/bin/Debug/YamlDotNet.RepresentationModel.xml -path doc/xml
monodocs2html -source doc/xml -dest doc/html
tar --exclude=.svn -cvf YamlDotNet_doc.tar.gz doc/html
echo YamlDotNet_doc.tar.gz generated
