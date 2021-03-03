#!/bin/bash
cd "$(dirname "$0")/.."
root=$PWD
cd build
#If a parameter is defined, then it will be used as the Configuration (defaulting to Debug)
msbuild "/target:${2-Clean;Compile}" /property:Configuration="${1-Debug}" /property:RootDir=$root Palaso.proj
