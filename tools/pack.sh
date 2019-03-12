#!/bin/sh
echo -e "\e[36m ---- Packing ---- \e[39m"

PACKAGE_VERSION=$(node -p "require('./package.json').version")
PACKAGE_VERSION_SUFFIX=$(node -p "require('./package.json').versionSuffix")

if [ -z "$PACKAGE_VERSION_SUFFIX" ] && [ -z "$CI" ]; then
	PACKAGE_VERSION_SUFFIX=dev
fi

VERSION=$PACKAGE_VERSION-$PACKAGE_VERSION_SUFFIX

echo version: $VERSION

dotnet pack -p:PackageVersion=$VERSION -p:AssemblyVersion=$PACKAGE_VERSION -o ../../ -c release --include-symbols