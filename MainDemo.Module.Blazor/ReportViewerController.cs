using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.ExpressApp.ReportsV2.Blazor;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace MainDemo.Module.Blazor
{
    public class ReportViewerController : ViewController<DetailView>
    {
        public ReportViewerController()
        {
            TargetViewId = ReportsBlazorModuleV2.ReportViewerDetailViewName;
            PopupWindowShowAction action = new PopupWindowShowAction(this, "ShowCurrentObject", "View");
            action.CustomizePopupWindowParams += Action_CustomizePopupWindowParams;
        }
        private void Action_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IReportDataV2 reportData = (IReportDataV2)View.CurrentObject;
            XtraReport report = ReportDataProvider.ReportsStorage.LoadReport(reportData);
            ReportsModuleV2 reportsModule = ReportsModuleV2.FindReportsModule(Application.Modules);
            reportsModule.ReportsDataSourceHelper.SetupBeforePrint(report);
            report.CreateDocument();
            Object currentReportObject = report.GetCurrentRow();
            IObjectSpace objectSpace = Application.CreateObjectSpace(currentReportObject.GetType());
            e.View = Application.CreateDetailView(objectSpace, objectSpace.GetObject(currentReportObject));

            report.ExportToPdf(@"c:\\Temp\Test.pdf");
        }
    }
}
