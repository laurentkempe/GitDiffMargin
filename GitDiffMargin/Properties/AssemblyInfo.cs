using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Git Diff Margin")]
[assembly:
    AssemblyDescription(
        "Git Diff Margin displays live changes of the currently edited file on Visual Studio margin and scroll bar.")]
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
[assembly: AssemblyVersion("3.8.0.0")]
[assembly: AssemblyFileVersion("3.8.0.0")]
[assembly: AssemblyInformationalVersion("3.8.0.0")]

[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\GalaSoft.MvvmLight.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\GalaSoft.MvvmLight.Extras.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\GalaSoft.MvvmLight.Platform.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\LibGit2Sharp.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\Microsoft.Practices.ServiceLocation.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\System.Windows.Interactivity.dll")]

[assembly: ProvideCodeBase(
    AssemblyName = "Tvl.VisualStudio.Shell.Utility.10",
    Version = "1.0.0.0",
    CodeBase = "$PackageFolder$\\Tvl.VisualStudio.Shell.Utility.10.dll")]
[assembly: ProvideCodeBase(
    AssemblyName = "Tvl.VisualStudio.Text.Utility.10",
    Version = "1.0.0.0",
    CodeBase = "$PackageFolder$\\Tvl.VisualStudio.Text.Utility.10.dll")]

[assembly:
    InternalsVisibleTo(
        "GitDiffMargin.Commands, PublicKey=00240000048000009400000006020000002400005253413100040000010001007df3fa608a609f848a39944defc0b31e504b3e84fc5c7cd6008277f4c323cc8332ce97434c483558e43fb4d6b5c6e4c4ddb3711dabafef0e79bda1f02d49621c7bc1da4b6847707f70417455e6b76cb27c08f4d32ad29a20233124023b809d2be10d3b0a003edeee23c0d8758b384103a18c95ba323c63a451052d84dc7672d0")]
[assembly:
    InternalsVisibleTo(
        "GitDiffMargin.LegacyCommands, PublicKey=00240000048000009400000006020000002400005253413100040000010001007df3fa608a609f848a39944defc0b31e504b3e84fc5c7cd6008277f4c323cc8332ce97434c483558e43fb4d6b5c6e4c4ddb3711dabafef0e79bda1f02d49621c7bc1da4b6847707f70417455e6b76cb27c08f4d32ad29a20233124023b809d2be10d3b0a003edeee23c0d8758b384103a18c95ba323c63a451052d84dc7672d0")]