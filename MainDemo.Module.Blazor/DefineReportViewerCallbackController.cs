using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ReportsV2.Blazor;
using DevExpress.ExpressApp.ReportsV2.Blazor.Components.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MainDemo.Blazor.ServerSide
{
    public class DefineReportViewerCallbackController : ViewController<DetailView>
    {
        public DefineReportViewerCallbackController()
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
