using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Validation.AllContextsView;
using MainDemo.Module.Controllers;

namespace MainDemo.Module.Blazor.Controllers {
    public class DisableActionsController : ViewController {
        protected override void OnActivated() {
            base.OnActivated();
            DeactivateController<PopupNotesController>();
#if !DEBUG
            DeactivateController<DevExpress.ExpressApp.SystemModule.ObjectMethodActionsViewController>();
#endif
            if(View is ListView) {
                Frame.GetController<DevExpress.ExpressApp.ViewVariantsModule.ChangeVariantController>()?.ChangeVariantAction.Active.SetItemValue("BlazorTemporary", false);
            }
        }
        private void DeactivateController<T>() where T : Controller {
            Frame.GetController<T>()?.Active.SetItemValue("BlazorTemporary", false);
        }
        private void ActivateController<T>() where T : Controller {
            Frame.GetController<T>()?.Active.SetItemValue("BlazorTemporary", true);
        }
    }
}
