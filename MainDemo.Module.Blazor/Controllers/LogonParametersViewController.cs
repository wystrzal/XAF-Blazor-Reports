using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;

namespace MainDemo.Module.Blazor.Controllers {
    public class LogonParametersViewController : ObjectViewController<DetailView, AuthenticationStandardLogonParameters> {
        protected override void OnActivated() {
            base.OnActivated();
            StringPropertyEditor userNamePropertyEditor = (StringPropertyEditor)View.FindItem("UserName");
            userNamePropertyEditor.NullText = "Sam or John";
        }
    }
}
