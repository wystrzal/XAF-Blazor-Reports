using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.ExpressApp.ReportsV2.Blazor;
using DevExpress.ExpressApp.ReportsV2.Blazor.Components.Models;
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
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            View.CustomizeViewItemControl<ReportViewerViewItem>(this, CustomizeReportViewerViewItem);
        }

        private void CustomizeReportViewerViewItem(ReportViewerViewItem reportViewerViewItem)
        {
            ReportViewerViewItem.DxDocumentViewerAdapter adapter = (ReportViewerViewItem.DxDocumentViewerAdapter)reportViewerViewItem.Control;
            DxDocumentViewerCallbacksModel callbacks = adapter.CallbacksModel;
            callbacks.CustomizeMenuActions = callbacks.CustomizeMenuActions = "onCustomizeMenuActions";
        }
    }
}
