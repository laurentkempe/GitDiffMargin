using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Git Diff Margin")]
[assembly: AssemblyDescription("Git Diff Margin displays live changes of the currently edited file on Visual Studio margin and scroll bar.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Laurent Kempé")]
[assembly: AssemblyProduct("Git Diff Margin")]
[assembly: AssemblyCopyright("Laurent Kempé")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(false)]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

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
[assembly: AssemblyVersion("3.7.1.0")]
[assembly: AssemblyFileVersion("3.7.1.0")]
[assembly: AssemblyInformationalVersion("3.7.1.0")]

[assembly: ProvideCodeBase(
    AssemblyName = "Tvl.VisualStudio.Shell.Utility.10",
    Version = "1.0.0.0",
    CodeBase = "$PackageFolder$\\Tvl.VisualStudio.Shell.Utility.10.dll")]
[assembly: ProvideCodeBase(
    AssemblyName = "Tvl.VisualStudio.Text.Utility.10",
    Version = "1.0.0.0",
    CodeBase = "$PackageFolder$\\Tvl.VisualStudio.Text.Utility.10.dll")]
