#!/bin/bash
cd $(dirname "$0")
xbuild /t:BeforeBuild /p:Configuration=DebugMono build/Download.targets
xbuild "/target:Clean;Compile" /property:Configuration=DebugMono /property:RootDir=..  /property:BUILD_NUMBER="0.0.0.abcd" build.mono.proj
