using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Bud.Exec")]
[assembly: AssemblyDescription("Bud.Exec is a wrapper around the `System.Diagnostics.Process` API. Bud.Exec provides " +
                               "a number of static methods for executing processes. Bud.Exec's API has been inspired " +
                               "by Python's subprocess functions.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Matej Urbas")]
[assembly: AssemblyProduct("Bud.Exec")]
[assembly: AssemblyCopyright("Copyright © 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7647a7d1-bb14-4a70-9b0a-551b051cc125")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: InternalsVisibleTo("Bud.Exec.Test")]