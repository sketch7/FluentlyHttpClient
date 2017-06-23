#!/bin/sh
echo -e "\e[36m ---- Packing ---- \e[39m"

PACKAGE_VERSION=$(node -p "require('./package.json').version")
VERSION_SUFFIX=dev
echo version: $PACKAGE_VERSION

dotnet pack //p:PackageVersion=$PACKAGE_VERSION-$VERSION_SUFFIX -o ../../ -c release