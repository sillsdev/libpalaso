#!/bin/bash
xbuild /target:Build /property:Configuration=ReleaseMono /property:teamcity_build_checkoutDir=/media/mono/palaso  /property:teamcity_dotnet_nunitlauncher_msbuild_task="notthere" /property:BUILD_NUMBER="1.5.1.abcd" /property:Minor="1" build.proj
