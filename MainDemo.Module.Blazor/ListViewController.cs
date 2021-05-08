using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.ExpressApp.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace MainDemo.Module.Blazor
{
    public class ListViewController : ViewController<ListView>
    {
        private PrintSelectionBaseController printSelectionBaseController;

        protected override void OnActivated()
        {
            base.OnActivated();
            printSelectionBaseController = Frame.GetController<PrintSelectionBaseController>();
            if (printSelectionBaseController != null)
            {
                printSelectionBaseController.ShowInReportAction.Execute += ShowInReportAction_Execute;
            }
        }

        private void ShowInReportAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var userName = SecuritySystem.CurrentUserName;
            var reportStorage = ReportStorage.GetInstance(userName);
            reportStorage.ObjectInfo = e.SelectedObjects[0];
            reportStorage.Application = Application;
        }
    }
}
