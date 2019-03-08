#!/bin/sh
err(){
	echo -e "\e[31m $* \e[0m" >>/dev/stderr
}

echo -e "\e[36m ---- Publish ---- \e[39m"
echo KEY=$SKETCH7_NUGET_API_KEY
if [ "$SKETCH7_NUGET_API_KEY" == "" ]; then
	err "'SKETCH7_NUGET_API_KEY' environment variable not defined."
	exit 1
fi
dotnet nuget push *.nupkg -k $SKETCH7_NUGET_API_KEY -s https://www.nuget.org/api/v2/package
