namespace GitDiffMargin
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid("F82C1EF6-3B52-425E-BC28-4934E6073A32")]

    [ProvideMenuResource("Menus.ctmenu", 1)]

    // https://github.com/laurentkempe/GitDiffMargin/issues/165
    [ProvideBindingPath]
    public class GitDiffMarginPackage : Package
    {
    }
}
