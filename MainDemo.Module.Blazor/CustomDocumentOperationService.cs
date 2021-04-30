using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Web.WebDocumentViewer;
using DevExpress.XtraReports.Web.WebDocumentViewer.DataContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MainDemo.Module.Blazor
{
    public class CustomDocumentOperationService : DocumentOperationService
    {
        public override bool CanPerformOperation(DocumentOperationRequest request)
        {
            return true;
        }

        public override DocumentOperationResponse PerformOperation(DocumentOperationRequest request, PrintingSystemBase initialPrintingSystem, PrintingSystemBase printingSystemWithEditingFields)
        {
            //printingSystemWithEditingFields.ExportToPdf();
            return base.PerformOperation(request, initialPrintingSystem, printingSystemWithEditingFields);
        }
    }
}
