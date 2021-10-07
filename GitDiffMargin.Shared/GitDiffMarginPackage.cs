namespace GitDiffMargin
{
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid("F82C1EF6-3B52-425E-BC28-4934E6073A32")]

    [ProvideMenuResource("Menus.ctmenu", 1)]

    public class GitDiffMarginPackage : Package
    {
    }
}
