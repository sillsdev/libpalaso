#!/bin/bash
# server=build.palaso.org
# project=libpalaso
# build=palaso-precise64-master Continuous
# root_dir=..
# $Id: d32984f53cd52f171a9cba46cd3879538ad23431 $

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

shift $((OPTIND - 1))

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
# build: palaso-precise64-master Continuous (bt322)
# project: libpalaso
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt322
# VCS: https://github.com/sillsdev/libpalaso.git [master]
# dependencies:
# [0] build: L10NSharp Mono continuous (bt271)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt271
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/ReleaseMono"}
#     VCS: https://bitbucket.org/sillsdev/l10nsharp []
# [1] build: L10NSharp Mono continuous (bt271)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt271
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/DebugMono"}
#     VCS: https://bitbucket.org/sillsdev/l10nsharp []
# [2] build: icucil-precise64-Continuous (bt281)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt281
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.dll"=>"lib/ReleaseMono", "*.config"=>"lib/ReleaseMono"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [3] build: icucil-precise64-Continuous (bt281)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt281
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.dll"=>"lib/DebugMono", "*.config"=>"lib/DebugMono"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [4] build: TagLib-Sharp Continuous (bt411)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt411
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"taglib-sharp.dll"=>"lib/ReleaseMono"}
#     VCS: https://github.com/sillsdev/taglib-sharp.git [develop]
# [5] build: TagLib-Sharp Continuous (bt411)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt411
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"taglib-sharp.dll"=>"lib/DebugMono"}
#     VCS: https://github.com/sillsdev/taglib-sharp.git [develop]

# make sure output directories exist
mkdir -p ../lib/DebugMono
mkdir -p ../lib/ReleaseMono

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/latest.lastSuccessful/L10NSharp.dll ../lib/ReleaseMono/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/latest.lastSuccessful/L10NSharp.dll ../lib/DebugMono/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll ../lib/ReleaseMono/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll.config ../lib/ReleaseMono/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll ../lib/DebugMono/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll.config ../lib/DebugMono/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt411/latest.lastSuccessful/taglib-sharp.dll ../lib/ReleaseMono/taglib-sharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt411/latest.lastSuccessful/taglib-sharp.dll ../lib/DebugMono/taglib-sharp.dll
# End of script
