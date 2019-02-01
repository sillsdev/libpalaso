#!groovy
// Copyright (c) 2019 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

@Library('lsdev-pipeline-library') _

xplatformBuildAndRunTests {
	winNodeSpec = 'windows && supported && vs2017'
	linuxNodeSpec = 'linux64 && !packager && ubuntu && mono5'
	winTool = 'msbuild15'
	linuxTool = 'mono-msbuild15'
	configuration = 'Release'
	uploadNuGet = true
	restorePackages = true
	clean = true
}
