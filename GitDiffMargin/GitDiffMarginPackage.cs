namespace GitDiffMargin
{
    using System;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid("F82C1EF6-3B52-425E-BC28-4934E6073A32")]

    [ProvideMenuResource("Menus.ctmenu", 1)]

    public class GitDiffMarginPackage : Package
    {
        protected override void Initialize()
        {
            base.Initialize();

            var menuCommandService = (MenuCommandService)GetService(typeof(IMenuCommandService));
            AddCommand(menuCommandService, GitDiffMarginCommand.PreviousChange);
            AddCommand(menuCommandService, GitDiffMarginCommand.NextChange);
            AddCommand(menuCommandService, GitDiffMarginCommand.RollbackChange);
            AddCommand(menuCommandService, GitDiffMarginCommand.ShowDiff);
            AddCommand(menuCommandService, GitDiffMarginCommand.CopyOldText);
        }

        private void AddCommand(MenuCommandService menuCommandService, GitDiffMarginCommand command)
        {
            EventHandler invokeHandler = HandleCommandInvoke;
            EventHandler changeHandler = HandleCommandChange;
            EventHandler beforeQueryStatus = HandleCommandBeforeQueryStatus;
            CommandID id = new CommandID(typeof(GitDiffMarginCommand).GUID, (int)command);
            string text = null;
            menuCommandService.AddCommand(new OleMenuCommand(invokeHandler, changeHandler, beforeQueryStatus, id, text));
        }

        private void HandleCommandInvoke(object sender, EventArgs e)
        {
        }

        private void HandleCommandChange(object sender, EventArgs e)
        {
        }

        private void HandleCommandBeforeQueryStatus(object sender, EventArgs e)
        {
        }
    }
}
