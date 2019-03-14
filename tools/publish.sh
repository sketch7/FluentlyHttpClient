#!/bin/sh
err(){
	echo -e "\e[31m $* \e[0m" >>/dev/stderr
}

echo -e "\e[36m ---- Publish ---- \e[39m"
if [ "$SKETCH7_NUGET_API_KEY" == "" ]; then
	err "'SKETCH7_NUGET_API_KEY' environment variable not defined."
	exit 1
fi
find *.nupkg | xargs -i dotnet nuget push {} -k $SKETCH7_NUGET_API_KEY -s https://api.nuget.org/v3/index.json