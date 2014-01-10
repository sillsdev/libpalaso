#!/bin/bash
# server=build.palaso.org
# project=libpalaso
# build=palaso-win32-develop-continuous
# root_dir=..

#### Results ####
# build: palaso-win32-develop-continuous (bt330)
# project: libpalaso
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt330
# VCS:  []
# dependencies:
# [0] build: L10NSharp continuous (bt196)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt196
#     VCS: https://bitbucket.org/hatton/l10nsharp []
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib\\Release", "L10NSharp.pdb"=>"lib\\Release"}
# [1] build: L10NSharp continuous (bt196)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt196
#     VCS: https://bitbucket.org/hatton/l10nsharp []
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib\\Debug", "L10NSharp.pdb"=>"lib\\Debug"}
# [2] build: icucil-win32-default Continuous (bt14)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt14
#     VCS:  []
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.dll"=>"lib\\Release", "*.config"=>"lib\\Release"}
# [3] build: icucil-win32-default Continuous (bt14)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt14
#     VCS:  []
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.dll"=>"lib\\Debug", "*.config"=>"lib\\Debug"}

# make sure output directories exist
mkdir -p ../lib/Release
mkdir -p ../lib/Debug

# download artifact dependencies
curl -L -o ../lib/Release/L10NSharp.dll http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.dll
curl -L -o ../lib/Release/L10NSharp.pdb http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.pdb
curl -L -o ../lib/Debug/L10NSharp.dll http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.dll
curl -L -o ../lib/Debug/L10NSharp.pdb http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.pdb
curl -L -o ../lib/Release/icu.net.dll http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll
curl -L -o ../lib/Release/icudt40.dll http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icudt40.dll
curl -L -o ../lib/Release/icuin40.dll http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuin40.dll
curl -L -o ../lib/Release/icuuc40.dll http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuuc40.dll
curl -L -o ../lib/Release/icu.net.dll.config http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll.config
curl -L -o ../lib/Debug/icu.net.dll http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll
curl -L -o ../lib/Debug/icudt40.dll http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icudt40.dll
curl -L -o ../lib/Debug/icuin40.dll http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuin40.dll
curl -L -o ../lib/Debug/icuuc40.dll http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuuc40.dll
curl -L -o ../lib/Debug/icu.net.dll.config http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll.config
