#!/bin/bash
cd "$(dirname "$0")/.."
root=$PWD
cd build
#If a parameter is defined, then it will be used as the Configuration (defaulting to DebugMonoStrongName)
xbuild "/target:${2-Clean;Compile}" /property:Configuration="${1-DebugMonoStrongName}" /property:RootDir=$root  /property:BUILD_NUMBER="0.0.0.abcd" Palaso.proj
