using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

[assembly: CLSCompliant(false)]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\GalaSoft.MvvmLight.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\GalaSoft.MvvmLight.Extras.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\GalaSoft.MvvmLight.Platform.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\LibGit2Sharp.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\CommonServiceLocator.dll")]
[assembly: ProvideCodeBase(CodeBase = "$PackageFolder$\\System.Windows.Interactivity.dll")]
