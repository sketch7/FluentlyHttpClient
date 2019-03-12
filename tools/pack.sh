#!/bin/sh
echo -e "\e[36m ---- Packing ---- \e[39m"

PACKAGE_VERSION=$(node -p "require('./package.json').version")
PACKAGE_VERSION_PREFIX=$(node -p "require('./package.json').versionPrefix")

if [ -z "$PACKAGE_VERSION_PREFIX" ] && ([ -z "$CI" ] || [ "$CI" == false ]); then
	PACKAGE_VERSION_PREFIX=dev
fi

UPCOMING_VERSION=$PACKAGE_VERSION-$PACKAGE_VERSION_PREFIX

echo version: $UPCOMING_VERSION

dotnet pack -p:PackageVersion=$UPCOMING_VERSION -p:AssemblyVersion=$PACKAGE_VERSION -o ../../ -c release --include-symbols