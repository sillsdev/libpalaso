#!/bin/bash
cd "$(dirname "$0")"

#If a parameter is defined, then it will be used as the Configuration (defaulting to DebugMono)
xbuild "/target:Clean;Compile" /property:Configuration="${1-Debug}Mono" /property:RootDir=..  /property:BUILD_NUMBER="0.0.0.abcd" build.mono.proj
