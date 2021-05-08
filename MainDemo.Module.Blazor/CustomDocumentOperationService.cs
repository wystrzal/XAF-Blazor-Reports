using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Web.WebDocumentViewer;
using DevExpress.XtraReports.Web.WebDocumentViewer.DataContracts;
using MainDemo.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MainDemo.Module.Blazor
{
    public class CustomDocumentOperationService : DocumentOperationService
    {
        public override bool CanPerformOperation(DocumentOperationRequest request)
        {
            return true;
        }

        private string userName;

        public override DocumentOperationResponse PerformOperation(DocumentOperationRequest request, PrintingSystemBase initialPrintingSystem, PrintingSystemBase printingSystemWithEditingFields)
        {
            var reportStorage = ReportStorage.GetInstance(request.CustomData);
            var application = reportStorage.Application;
            var contact = reportStorage.ObjectInfo as Contact;
            userName = request.CustomData;
            var os = application.CreateObjectSpace(typeof(Contact));
            var myContact = os.GetObject(contact);

            printingSystemWithEditingFields.ExportToPdf(@"c:\\Temp\Test.pdf");
            var memoryStream = new MemoryStream();
            printingSystemWithEditingFields.ExportToPdf(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            reportStorage.MemoryStream = memoryStream;

            var dv = application.CreateDetailView(os, myContact);

            application.ShowViewStrategy.ShowViewInPopupWindow(dv,
                () => SendEmail(),
                () => application.ShowViewStrategy.ShowMessage("Cancelled.")
            );

            return base.PerformOperation(request, initialPrintingSystem, printingSystemWithEditingFields);
        }

        private void SendEmail()
        {
            var reportStorage = ReportStorage.GetInstance(userName);
            var memoryStream = reportStorage.MemoryStream;
            var contact = reportStorage.ObjectInfo as Contact;

            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress("test.gabos.report@gmail.com");
            message.To.Add(new MailAddress("pwyprzal@gabos.pl"));
            message.Subject = "Recepta";
            message.Body = "Recepta utworzona dla Jacka: " + DateTime.Now.ToLongDateString();
            message.Attachments.Add(new Attachment(memoryStream, "recepta.pdf"));
            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("test.gabos.report@gmail.com", "Te$towy1!");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Send(message);
        }
    }
}
