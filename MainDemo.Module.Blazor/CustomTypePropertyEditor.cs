using System;
using System.Collections.Generic;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;

namespace MainDemo.Module.Blazor {
    [PropertyEditor(typeof(Type), true)]
    public class CustomTypePropertyEditor : TypePropertyEditor {
        private readonly static ICollection<Type> suitableTypes = new HashSet<Type>() {
          ///*   Action Permissions */ typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyActionPermissionObject),
          ///*   Address */ typeof(DevExpress.Persistent.BaseImpl.Address),
          ///*   Analytics */ typeof(DevExpress.Persistent.BaseImpl.Analysis),
          ///*   Audit Event */ typeof(DevExpress.Persistent.BaseImpl.AuditDataItemPersistent),
          ///*   Audited Object Weak Reference */ typeof(DevExpress.Persistent.BaseImpl.AuditedObjectWeakReference),
          ///*   Base Role */ typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyRoleBase),
          ///*   Base Task */ typeof(DevExpress.Persistent.BaseImpl.Task),
            /* + Contact */ typeof(MainDemo.Module.BusinessObjects.Contact),
          ///*   Country */ typeof(DevExpress.Persistent.BaseImpl.Country),
            /* + Department */ typeof(MainDemo.Module.BusinessObjects.Department),
          ///*   File Data */ typeof(DevExpress.Persistent.BaseImpl.FileData),
          ///*   Location */ typeof(MainDemo.Module.BusinessObjects.Location),
            /* U Member Operation Permissions */ typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyMemberPermissionsObject),
          ///*   Navigation Item Permissions */ typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyNavigationPermissionObject),
          ///*   Note */ typeof(DevExpress.Persistent.BaseImpl.Note),
            /* U Object Operation Permissions */ typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyObjectPermissionsObject),
          ///*   Party */ typeof(DevExpress.Persistent.BaseImpl.Party),
            /* + Paycheck */ typeof(MainDemo.Module.BusinessObjects.Paycheck),
          ///*   Person */ typeof(DevExpress.Persistent.BaseImpl.Person),
          ///*   Phone Number */ typeof(DevExpress.Persistent.BaseImpl.PhoneNumber),
          ///*   Phone Type */ typeof(DevExpress.Persistent.BaseImpl.PhoneType),
          ///*   Portfolio File Data */ typeof(MainDemo.Module.BusinessObjects.PortfolioFileData),
            /* + Position */ typeof(MainDemo.Module.BusinessObjects.Position),
          ///*   Resource */ typeof(DevExpress.Persistent.BaseImpl.Resource),
            /* + Resume */ typeof(MainDemo.Module.BusinessObjects.Resume),
            /* U Role */ typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyRole),
          ///*   Scheduler Event */ typeof(DevExpress.Persistent.BaseImpl.Event),
            /* + Task */ typeof(MainDemo.Module.BusinessObjects.DemoTask),
            /* U Type Operation Permissions */ typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyTypePermissionObject),
            /* U User */ typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyUser)
          ///*   XPWeak Reference */ typeof(DevExpress.Xpo.XPWeakReference)
        };
        public CustomTypePropertyEditor(Type objectType, IModelMemberViewItem model) : base(objectType, model) { }
        protected override bool IsSuitableType(Type type) {
            return base.IsSuitableType(type) && suitableTypes.Contains(type);
        }
    }
}
