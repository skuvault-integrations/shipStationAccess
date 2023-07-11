using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[ assembly : AssemblyTitle( "ShipStationAccess" ) ]
[ assembly : AssemblyConfiguration( "" ) ]
[ assembly : InternalsVisibleTo( "ShipStationAccessTests" ) ]
// Need to use Substitute.For< IWebRequestServices > in tests
[ assembly : InternalsVisibleTo( "DynamicProxyGenAssembly2" ) ]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[ assembly : Guid( "c42ca23c-549e-4bf1-8403-3e85a2a02f2e" ) ]