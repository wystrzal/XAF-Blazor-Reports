﻿using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base.General;
using MainDemo.Module.BusinessObjects;
using System;
using System.Collections;

namespace MainDemo.Module.Controllers {
	public partial class TaskActionsController : ViewController {
		private ChoiceActionItem setPriorityItem;
		private ChoiceActionItem setStatusItem;
        private void UpdateSetTaskActionState() {
            bool isGranted = true;
            
            SecurityStrategy security = Application.GetSecurityStrategy();
            foreach(object selectedObject in View.SelectedObjects) {
                bool isPriorityGranted = security.IsGranted(new PermissionRequest(ObjectSpace, typeof(DemoTask), SecurityOperations.Write, selectedObject, nameof(DemoTask.Priority)));
                bool isStatusGranted = security.IsGranted(new PermissionRequest(ObjectSpace, typeof(DemoTask), SecurityOperations.Write, selectedObject, nameof(DemoTask.Status)));
                if(!isPriorityGranted || !isStatusGranted) {
                    isGranted = false;
                }
            }
            SetTaskAction.Enabled.SetItemValue("SecurityAllowance", isGranted);
        }
		public TaskActionsController() {
            TypeOfView = typeof(ObjectView);
			InitializeComponent();
			RegisterActions(components);

			setPriorityItem = new ChoiceActionItem(CaptionHelper.GetMemberCaption(typeof(DemoTask), "Priority"), null);
			SetTaskAction.Items.Add(setPriorityItem);
			FillItemWithEnumValues(setPriorityItem, typeof(Priority));
			setStatusItem = new ChoiceActionItem(CaptionHelper.GetMemberCaption(typeof(DemoTask), "Status"), null);
			SetTaskAction.Items.Add(setStatusItem);
			FillItemWithEnumValues(setStatusItem, typeof(TaskStatus));
		}
		private void FillItemWithEnumValues(ChoiceActionItem parentItem, Type enumType) {
            EnumDescriptor ed = new EnumDescriptor(enumType);
            foreach(object current in Enum.GetValues(enumType)) {
				ChoiceActionItem item = new ChoiceActionItem(ed.GetCaption(current), current);
				item.ImageName = ImageLoader.Instance.GetEnumValueImageName(current);
				parentItem.Items.Add(item);
			}
		}
		private void TaskActionsController_Activated(object sender, EventArgs e) {
            View.SelectionChanged += new EventHandler(View_SelectionChanged);
            UpdateSetTaskActionState();
		}

        void View_SelectionChanged(object sender, EventArgs e) {
            UpdateSetTaskActionState();
        }
        private DemoTask GetObject(DemoTask obj, IObjectSpace objectSpace, IObjectSpace newObjectSpace, ref int newObjectsCount) {
            if(objectSpace.IsNewObject(obj)) {
                newObjectsCount++;
                return obj;
            }
            return (DemoTask)newObjectSpace.GetObject(obj);
        }
        private void SetTaskAction_Execute(object sender, SingleChoiceActionExecuteEventArgs args) {
            IObjectSpace objectSpace = View is ListView ? Application.CreateObjectSpace(typeof(DemoTask)) : View.ObjectSpace;
            int newObjectsCount = 0;
            ArrayList objectsToProcess = new ArrayList(args.SelectedObjects);
            if(args.SelectedChoiceActionItem.ParentItem == setPriorityItem) {
                foreach(Object obj in objectsToProcess) {
                    DemoTask objInNewObjectSpace = GetObject((DemoTask)obj, View.ObjectSpace, objectSpace, ref newObjectsCount);
                    objInNewObjectSpace.Priority = (Priority)args.SelectedChoiceActionItem.Data;
                }
            }
            else if(args.SelectedChoiceActionItem.ParentItem == setStatusItem) {
                foreach(Object obj in objectsToProcess) {
                    DemoTask objInNewObjectSpace = GetObject((DemoTask)obj, View.ObjectSpace, objectSpace, ref newObjectsCount);
                    objInNewObjectSpace.Status = (TaskStatus)args.SelectedChoiceActionItem.Data;
                }
            }
            if(View is DetailView && ((DetailView)View).ViewEditMode == ViewEditMode.View) {
                objectSpace.CommitChanges();
            }
            if((View is ListView) && (newObjectsCount != objectsToProcess.Count)) {
                objectSpace.CommitChanges();
                View.ObjectSpace.Refresh();
            }
        }
    }
}
