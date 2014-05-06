#!/bin/bash
# server=build.palaso.org
# project=libpalaso
# build=palaso-win32-master Continuous
# root_dir=..
# $Id: 50ae191e9e7146711d602e58cde080678d265c48 $

cd "$(dirname "$0")"

# *** Functions ***
force=0
clean=0

while getopts fc opt; do
case $opt in
f) force=1 ;;
c) clean=1 ;;
esac
done

copy_auto() {
if [ "$clean" == "1" ]
then
echo cleaning $2
rm -f ""$2""
else
where_curl=$(type -P curl)
where_wget=$(type -P wget)
if [ "$where_curl" != "" ]
then
copy_curl $1 $2
elif [ "$where_wget" != "" ]
then
copy_wget $1 $2
else
echo "Missing curl or wget"
exit 1
fi
fi
}

copy_curl() {
echo "curl: $2 <= $1"
if [ -e "$2" ] && [ "$force" != "1" ]
then
curl -# -L -z $2 -o $2 $1
else
curl -# -L -o $2 $1
fi
}

copy_wget() {
echo "wget: $2 <= $1"
f=$(basename $2)
d=$(dirname $2)
cd $d
wget -q -L -N $1
cd -
}


# *** Results ***
# build: palaso-win32-master Continuous (bt223)
# project: libpalaso
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt223
# VCS: https://github.com/sillsdev/libpalaso.git []
# dependencies:
# [0] build: L10NSharp continuous (bt196)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt196
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/Release", "L10NSharp.pdb"=>"lib/Release"}
#     VCS: https://bitbucket.org/hatton/l10nsharp []
# [1] build: L10NSharp continuous (bt196)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt196
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/Debug", "L10NSharp.pdb"=>"lib/Debug"}
#     VCS: https://bitbucket.org/hatton/l10nsharp []
# [2] build: icucil-win32-default Continuous (bt14)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt14
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.dll"=>"lib/Release", "*.config"=>"lib/Release"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [3] build: icucil-win32-default Continuous (bt14)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt14
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.dll"=>"lib/Debug", "*.config"=>"lib/Debug"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]

# make sure output directories exist
mkdir -p ../lib/Release
mkdir -p ../lib/Debug

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.dll ../lib/Release/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.pdb ../lib/Release/L10NSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.dll ../lib/Debug/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.pdb ../lib/Debug/L10NSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll ../lib/Release/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icudt40.dll ../lib/Release/icudt40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuin40.dll ../lib/Release/icuin40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuuc40.dll ../lib/Release/icuuc40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll.config ../lib/Release/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll ../lib/Debug/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icudt40.dll ../lib/Debug/icudt40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuin40.dll ../lib/Debug/icuin40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuuc40.dll ../lib/Debug/icuuc40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll.config ../lib/Debug/icu.net.dll.config
# End of script
