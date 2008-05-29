using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ObfuscateAssembly(false, StripAfterObfuscation=false)]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("JsonFx")]
[assembly: AssemblyDescription("The JSON-Savvy Ajax Framework")]
[assembly: AssemblyProduct("JsonFx.NET")]
[assembly: AssemblyCopyright("Copyright © 2006-2008 Stephen M. McKamey. All rights reserved.")]
[assembly: AssemblyCompany("http://jsonfx.net")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("1E6A05D1-48C6-414D-A317-E2A7ECA81B96")]

