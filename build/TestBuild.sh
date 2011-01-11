#!/bin/bash
xbuild /target:Build /property:Configuration=DebugMono /property:RootDir=..  /property:BUILD_NUMBER="0.0.0.abcd" build.mono.proj
