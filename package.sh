#!/bin/sh
VERSION=`cat VERSION`

echo Current version is $VERSION.

BINARY_PACKAGE=../releases/YamlDotNet_$VERSION.tar.gz
SOURCE_PACKAGE=../releases/YamlDotNet_src_$VERSION.tar.gz
DOCUMENTATION_PACKAGE=../releases/YamlDotNet_doc_$VERSION.tar.gz

echo Removing existing files.

rm -f $BINARY_PACKAGE
rm -f $SOURCE_PACKAGE
rm -f $DOCUMENTATION_PACKAGE

echo Generating documentation package.

./gendoc.sh > /dev/null
tar --exclude=.svn -czf $DOCUMENTATION_PACKAGE doc/html

echo Generating source package.

tar --exclude=.svn --exclude=bin --exclude=obj --exclude=doc --exclude=test-results -czf $SOURCE_PACKAGE *

echo Generating binary package.

mkdir tmp 2> /dev/null
cp YamlDotNet.Core/bin/Release/YamlDotNet.Core.dll tmp
cp YamlDotNet.RepresentationModel/bin/Release/YamlDotNet.RepresentationModel.dll tmp
cp YamlDotNet.Converters/bin/Release/YamlDotNet.Converters.dll tmp
cp YamlDotNet.Configuration/bin/Release/YamlDotNet.Configuration.dll tmp
cd tmp
tar -czf ../$BINARY_PACKAGE *
cd ..
rm -rf tmp

echo Done.
