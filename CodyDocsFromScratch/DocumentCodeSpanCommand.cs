using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

using Microsoft.VisualStudio.TextManager.Interop;
using EnvDTE;

namespace CodyDocsFromScratch
{
    internal sealed class DocumentCodeSpanCommand
    {
        public const int CommandId = 256;

        public static readonly Guid CommandSet = new Guid("ead5342a-ac56-49a0-8f2f-de611268440b");

        private readonly AsyncPackage package;

        private DocumentCodeSpanCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.MenuItemCallbackAsync, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static DocumentCodeSpanCommand Instance
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
            Instance = new DocumentCodeSpanCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "DocumentCodeSpanCommand";

            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        //public async void btn1_ClickAsync(object sender, RoutedEventArgs e)
        // From here: https://stackoverflow.com/a/52024801/360840
        private async void MenuItemCallbackAsync(object sender, EventArgs e)
        {
            TextViewSelection selection = await GetSelectionAsync(ServiceProvider);
            //string activeDocumentPath = GetActiveDocumentFilePath(ServiceProvider);
            ShowAddDocumentationWindow();
        }

        private void ShowAddDocumentationWindow()
        {
            var documentationControl = new AddDocumentationWindow();
            documentationControl.ShowDialog();
        }

        private string GetActiveDocumentFilePath(Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE80.DTE2 applicationObject = serviceProvider.GetServiceAsync(typeof(DTE)) as EnvDTE80.DTE2;
            return applicationObject.ActiveDocument.FullName;
        }

        private async System.Threading.Tasks.Task<TextViewSelection> GetSelectionAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetServiceAsync(typeof(SVsTextManager));
            var textManager = await service as IVsTextManager2;
            IVsTextView view;
            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);

            view.GetSelection(out int startLine, out int startColumn, out int endLine, out int endColumn);
            var start = new TextViewPosition(startLine, startColumn);
            var end = new TextViewPosition(endLine, endColumn);

            view.GetSelectedText(out string selectedText);

            TextViewSelection selection = new TextViewSelection(start, end, selectedText);
            return selection;
        }
    }
}
