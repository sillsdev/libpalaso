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
[assembly: Acknowledgement("ackAttrKey1", "test acknowledgement name",
	"test acknowledgement license SIL", "test copyright 2017", "test location")]

[assembly: Acknowledgement("ackAttrKey2", "my test name",
	"my SIL test license", "my test 2017 copyright", "my test location")]
