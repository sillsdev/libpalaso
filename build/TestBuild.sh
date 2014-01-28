#!/bin/bash
#If a parameter is defined, then it will be used as the Configuration (defaulting to DebugMono)
xbuild "/target:Clean;Compile" /property:Configuration="${1-DebugMono}" /property:RootDir=..  /property:BUILD_NUMBER="0.0.0.abcd" build.mono.proj
