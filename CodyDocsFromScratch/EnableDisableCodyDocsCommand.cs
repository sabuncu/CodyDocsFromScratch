using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

// 5) This file is created as a result of Add Command, for CodyDocs.

namespace CodyDocsFromScratch
{
    internal sealed class EnableDisableCodyDocsCommand
    {
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("69c0b337-59d8-4591-8cbd-a59603459d56");

        private readonly AsyncPackage package;

        private EnableDisableCodyDocsCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            // 7) a) Modified for CodyDocs.
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            // 3) a) Following settings item added to the Settings file for CodyDocs.
            menuItem.Checked = GeneralSettings.Default.EnableCodyDocs;
            commandService.AddCommand(menuItem);
        }

        public static EnableDisableCodyDocsCommand Instance
        {
            get;
            private set;
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new EnableDisableCodyDocsCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            // 7) b) Modified for CodyDocs.

            GeneralSettings.Default.EnableCodyDocs = !GeneralSettings.Default.EnableCodyDocs;
            GeneralSettings.Default.Save();
            var command = sender as MenuCommand;
            command.Checked = GeneralSettings.Default.EnableCodyDocs;

            ThreadHelper.ThrowIfNotOnUIThread();
            string title = "EnableDisableCodyDocsCommand";
            string message = GeneralSettings.Default.EnableCodyDocs ? "Enabled" : "Disabled";

            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
