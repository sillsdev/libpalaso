#!/bin/bash
# server=build.palaso.org
# project=libpalaso
# build=palaso-win32-master     Continuous (bt223)
# build=palaso-precise64-master Continuous (bt322)
# root_dir=..

# *** Functions ***
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
# [4] build: TagLib-Sharp continuous (bt411)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt411
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"taglib-sharp.dll"=>"lib/Release", "taglib-sharp.dll"=>"lib/Debug"}
#     VCS: https://github.com/sillsdev/taglib-sharp []

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
#     VCS: https://bitbucket.org/hatton/l10nsharp [default]
# [1] build: L10NSharp Mono continuous (bt271)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt271
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/DebugMono"}
#     VCS: https://bitbucket.org/hatton/l10nsharp [default]
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
# [4] build: TagLib-Sharp continuous (bt411)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt411
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"taglib-sharp.dll"=>"lib/ReleaseMono", "taglib-sharp.dll"=>"lib/DebugMono"}
#     VCS: https://github.com/sillsdev/taglib-sharp []

# OS determines dependencies and output directories
OS=`uname -s`
case $OS in
Linux)
	L10NSharpID="bt271";
	icuID="bt281";
	taglibSharpID="bt411";
	ReleaseDir="../lib/ReleaseMono";
	DebugDir="../lib/DebugMono";;
MINGW32*)
	L10NSharpID="bt196";
	icuID="bt14";
	taglibSharpID="bt411";
	ReleaseDir="../lib/Release";
	DebugDir="../lib/Debug";;
*)
	echo "ERROR: $OS not supported";
	exit;;
esac

# make sure output directories exist
mkdir -p $ReleaseDir
mkdir -p $DebugDir

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/$L10NSharpID/latest.lastSuccessful/L10NSharp.dll $ReleaseDir/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/$L10NSharpID/latest.lastSuccessful/L10NSharp.pdb $ReleaseDir/L10NSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/$L10NSharpID/latest.lastSuccessful/L10NSharp.dll $DebugDir/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/$L10NSharpID/latest.lastSuccessful/L10NSharp.pdb $DebugDir/L10NSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/$icuID/latest.lastSuccessful/icu.net.dll $ReleaseDir/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/$icuID/latest.lastSuccessful/icu.net.dll.config $ReleaseDir/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/$icuID/latest.lastSuccessful/icu.net.dll.config $DebugDir/icu.net.dll.config

if [ "$(expr substr $OS 1 10)" == "MINGW32_NT" ]; then
	# dependencies that are only applicable to Windows
	copy_auto http://build.palaso.org/guestAuth/repository/download/$icuID/latest.lastSuccessful/icudt40.dll $ReleaseDir/icudt40.dll
	copy_auto http://build.palaso.org/guestAuth/repository/download/$icuID/latest.lastSuccessful/icuin40.dll $ReleaseDir/icuin40.dll
	copy_auto http://build.palaso.org/guestAuth/repository/download/$icuID/latest.lastSuccessful/icuuc40.dll $ReleaseDir/icuuc40.dll

	copy_auto http://build.palaso.org/guestAuth/repository/download/$icuID/latest.lastSuccessful/icu.net.dll $DebugDir/icu.net.dll
	copy_auto http://build.palaso.org/guestAuth/repository/download/$icuID/latest.lastSuccessful/icudt40.dll $DebugDir/icudt40.dll
	copy_auto http://build.palaso.org/guestAuth/repository/download/$icuID/latest.lastSuccessful/icuin40.dll $DebugDir/icuin40.dll
	copy_auto http://build.palaso.org/guestAuth/repository/download/$icuID/latest.lastSuccessful/icuuc40.dll $DebugDir/icuuc40.dll
fi

copy_auto http://build.palaso.org/guestAuth/repository/download/$taglibSharpID/latest.lastSuccessful/taglib-sharp.dll $ReleaseDir/taglib-sharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/$taglibSharpID/latest.lastSuccessful/taglib-sharp.dll $DebugDir/taglib-sharp.dll
# End of script
