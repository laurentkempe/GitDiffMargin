using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Tvl.VisualStudio.Shell;

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
[assembly: AssemblyVersion("3.2.1.0")]
[assembly: AssemblyFileVersion("3.2.1.0")]
[assembly: AssemblyInformationalVersion("3.2.1.0")]

#if true // Use this block if Visual Studio 2010 still needs to be supported
[assembly: Guid("c279cada-3e46-4971-9355-50f43053d1b3")]
[assembly: ProvideBindingPath]
#else // Switch to this block when we only need Visual Studio 2012 or newer
[assembly: ProvideCodeBase(
    AssemblyName = "Tvl.VisualStudio.Shell.Utility.10",
    Version = "1.0.0.0",
    CodeBase = "$PackageFolder$\\Tvl.VisualStudio.Shell.Utility.10.dll")]
[assembly: ProvideCodeBase(
    AssemblyName = "Tvl.VisualStudio.Text.Utility.10",
    Version = "1.0.0.0",
    CodeBase = "$PackageFolder$\\Tvl.VisualStudio.Text.Utility.10.dll")]
#endif
