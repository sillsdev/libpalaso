// Copyright (c) 2016-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Reflection;
using System.Runtime.InteropServices;
using SIL.Acknowledgements;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SIL.Windows.Forms.TestApp")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("ad6812d5-377a-4687-aba2-4e88a87527ec")]

// The following Acknowledgements test the AcknowledgementsProvider for SIL AboutBox
[assembly: Acknowledgement("ackAttrKey1", Name = "test acknowledgement name", Url = "http://www.google.com",
	Location = "test location", LicenseUrl = "http://opensource.org/licenses/MIT", Copyright = "test copyright 2017")]

[assembly: Acknowledgement("ackAttrKey2", Name = "my test name", LicenseUrl = "http://opensource.org/licenses/MIT",
	 Copyright = "my test 2017 copyright", Location = "my test location")]
