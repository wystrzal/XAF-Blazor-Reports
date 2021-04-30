using System;
using System.Data.SqlClient;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
#if !NETSTANDARD
using DevExpress.ExpressApp.PivotChart;
#endif
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using MainDemo.Module.BusinessObjects;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using System.Drawing;
using DevExpress.ExpressApp.Utils;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MainDemo.Module.DatabaseUpdate {
    public class Updater : ModuleUpdater {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) : base(objectSpace, currentDBVersion) { }


        private Contact FindTellitson() => FindContact("Mary", "Tellitson");
        private Contact FindJanete() => FindContact("Janete", "Limeira");
        private Contact FindContact(string firstName, string lastName) => ObjectSpace.FindObject<Contact>(CriteriaOperator.Parse($"FirstName == '{firstName}' && LastName == '{lastName}'"));
        private Position FindPosition(string positionTitle) => ObjectSpace.FindObject<Position>(CriteriaOperator.Parse("Title=?", positionTitle), true);
        private Department FindDepartment(string departmentTitle) => ObjectSpace.FindObject<Department>(CriteriaOperator.Parse("Title=?", departmentTitle), true);

        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();
            UpdateAnalysisCriteriaColumn();

            // PermissionPolicyRole defaultRole = CreateDefaultRole();

            UpdateStatus("CreateContacts", "", "Creating contacts, departments and positions in the database...");

            ObjectSpace.CommitChanges();
            try {
                CreateDepartments();
                CreateContacts();
            }
            catch(Exception e) {
                Tracing.Tracer.LogText("Cannot initialize contacts, departments and positions from the XML file.");
                Tracing.Tracer.LogError(e);
            }
            InitContactsLocation();


            UpdateStatus("CreatePayments", "", "Creating payments, resumes and scheduler events in the database...");
            IList<Contact> topTenContacts = ObjectSpace.GetObjects<Contact>();
            ObjectSpace.SetCollectionSorting(topTenContacts, new SortProperty[] { new SortProperty("LastName", DevExpress.Xpo.DB.SortingDirection.Ascending) });
            ObjectSpace.SetTopReturnedObjectsCount(topTenContacts, 10);
            string[] notes = {
                "works with customers until their problems are resolved and often goes an extra step to help upset customers be completely surprised by how far we will go to satisfy customers",
                "is very good at making team members feel included. The inclusion has improved the team's productivity dramatically",
                "is very good at sharing knowledge and information during a problem to increase the chance it will be resolved quickly",
                "actively elicits feedback from customers and works to resolve their problems",
                "creates an inclusive work environment where everyone feels they are a part of the team",
                "consistently keeps up on new trends in the industry and applies these new practices to every day work",
                "is clearly not a short term thinker - the ability to set short and long term business goals is a great asset to the company",
                "seems to want to achieve all of the goals in the last few weeks before annual performance review time, but does not consistently work towards the goals throughout the year",
                "does not yet delegate effectively and has a tendency to be overloaded with tasks which should be handed off to subordinates",
                "to be discussed with the top management..."
            };
            for(int i = 0; i < topTenContacts.Count; i++) {
                Contact contact = topTenContacts[i];
                if(ObjectSpace.FindObject<Paycheck>(CriteriaOperator.Parse("Employee=?", contact)) == null) {
                    PayrollSampleDataGenerator.GenerateContactPaychecks(ObjectSpace, contact);
                    ObjectSpace.CommitChanges();
                }
                Resume resume = ObjectSpace.FindObject<Resume>(CriteriaOperator.Parse("Contact=?", contact));
                if(resume == null) {
                    resume = ObjectSpace.CreateObject<Resume>();
                    FileData file = ObjectSpace.CreateObject<FileData>();
                    try {
                        Stream stream = FindContactResume(contact);
                        if(stream != null) {
                            file.LoadFromStream(string.Format("{0}.pdf", contact.FullName), stream);
                        }
                    }
                    catch(Exception e) {
                        Tracing.Tracer.LogText("Cannot initialize FileData for the contact {0}.", contact.FullName);
                        Tracing.Tracer.LogError(e);
                    }
                    resume.File = file;
                    resume.Contact = contact;
                }
                Contact reviewerContact = i < 5 ? FindTellitson() : FindJanete();
                Note note = ObjectSpace.FindObject<Note>(CriteriaOperator.Parse("Contains(Text, ?)", contact.FullName));
                if(note == null) {
                    note = ObjectSpace.CreateObject<Note>();
                    note.Author = reviewerContact.FullName;
                    note.Text = string.Format("<span style='color:#000000;font-family:Tahoma;font-size:8pt;'><b>{0}</b> \r\n{1}</span>", contact.FullName, notes[i]);
                    note.DateTime = DateTime.Now.AddDays(i * (-1));
                }
#if !EASYTEST
                Event appointment = ObjectSpace.FindObject<Event>(CriteriaOperator.Parse("Contains(Subject, ?)", contact.FullName));
                if(appointment == null) {
                    appointment = ObjectSpace.CreateObject<Event>();
                    appointment.Subject = string.Format("{0} - performance review", contact.FullName);
                    appointment.Description = string.Format("{0} \r\n{1}", contact.FullName, notes[i]);
                    appointment.StartOn = note.DateTime.AddDays(5).AddHours(12);
                    appointment.EndOn = appointment.StartOn.AddHours(2);
                    appointment.Location = "101";
                    appointment.AllDay = false;
                    appointment.Status = 0;
                    appointment.Label = i % 2 == 0 ? 2 : 5;
                    Resource reviewerContactResource = ObjectSpace.FindObject<Resource>(CriteriaOperator.Parse("Contains(Caption, ?)", reviewerContact.FullName));
                    if(reviewerContactResource == null) {
                        reviewerContactResource = ObjectSpace.CreateObject<Resource>();
                        reviewerContactResource.Caption = reviewerContact.FullName;
                        reviewerContactResource.Color = reviewerContact == FindTellitson() ? Color.AliceBlue : Color.LightCoral;
                    }
                    appointment.Resources.Add(reviewerContactResource);
                }
#endif
            }

            ObjectSpace.CommitChanges();

            UpdateStatus("CreateTasks", "", "Creating demo tasks in the database...");

#if EASYTEST
            Contact contactMary = FindTellitson();
            Contact contactJohn = FindContact("John", "Nilsen");
            if(ObjectSpace.FindObject<DemoTask>(CriteriaOperator.Parse("Subject == 'Review reports'")) == null) {
                DemoTask task = ObjectSpace.CreateObject<DemoTask>();
                task.Subject = "Review reports";
                task.AssignedTo = contactJohn;
                task.StartDate = DateTime.Parse("May 03, 2008");
                task.DueDate = DateTime.Parse("September 06, 2008");
                task.Status = DevExpress.Persistent.Base.General.TaskStatus.InProgress;
                task.Priority = Priority.High;
                task.EstimatedWorkHours = 60;
                task.Description = "Analyse the reports and assign new tasks to employees.";
            }

            if(ObjectSpace.FindObject<DemoTask>(CriteriaOperator.Parse("Subject == 'Fix breakfast'")) == null) {
                DemoTask task = ObjectSpace.CreateObject<DemoTask>();
                task.Subject = "Fix breakfast";
                task.AssignedTo = contactMary;
                task.StartDate = DateTime.Parse("May 03, 2008");
                task.DueDate = DateTime.Parse("May 04, 2008");
                task.Status = DevExpress.Persistent.Base.General.TaskStatus.Completed;
                task.Priority = Priority.Low;
                task.EstimatedWorkHours = 1;
                task.ActualWorkHours = 3;
                task.Description = "The Development Department - by 9 a.m.\r\nThe R&QA Department - by 10 a.m.";
            }
            if(ObjectSpace.FindObject<DemoTask>(CriteriaOperator.Parse("Subject == 'Task1'")) == null) {
                DemoTask task = ObjectSpace.CreateObject<DemoTask>();
                task.Subject = "Task1";
                task.AssignedTo = contactJohn;
                task.StartDate = DateTime.Parse("June 03, 2008");
                task.DueDate = DateTime.Parse("June 06, 2008");
                task.Status = DevExpress.Persistent.Base.General.TaskStatus.Completed;
                task.Priority = Priority.High;
                task.EstimatedWorkHours = 10;
                task.ActualWorkHours = 15;
                task.Description = "A task designed specially to demonstrate the PivotChart module. Switch to the Reports navigation group to view the generated analysis.";
            }
            if(ObjectSpace.FindObject<DemoTask>(CriteriaOperator.Parse("Subject == 'Task2'")) == null) {
                DemoTask task = ObjectSpace.CreateObject<DemoTask>();
                task.Subject = "Task2";
                task.AssignedTo = contactJohn;
                task.StartDate = DateTime.Parse("July 03, 2008");
                task.DueDate = DateTime.Parse("July 06, 2008");
                task.Status = DevExpress.Persistent.Base.General.TaskStatus.Completed;
                task.Priority = Priority.Low;
                task.EstimatedWorkHours = 8;
                task.ActualWorkHours = 16;
                task.Description = "A task designed specially to demonstrate the PivotChart module. Switch to the Reports navigation group to view the generated analysis.";
            }
#endif
#if !EASYTEST
            IList<Contact> contacts = ObjectSpace.GetObjects<Contact>();
            IList<DemoTask> taskList = GenerateTask(contacts);
            if(taskList.Count > 0) {
                Random rndGenerator = new Random();
                foreach(Contact contact in contacts) {
                    if(taskList.Count == 1) {
                        contact.Tasks.Add(taskList[0]);
                    }
                    else if(taskList.Count == 2) {
                        contact.Tasks.Add(taskList[0]);
                        contact.Tasks.Add(taskList[1]);
                    }
                    else {
                        int index = rndGenerator.Next(1, taskList.Count - 2);
                        contact.Tasks.Add(taskList[index]);
                        contact.Tasks.Add(taskList[index - 1]);
                        contact.Tasks.Add(taskList[index + 1]);
                    }
                }
            }
#endif
            UpdateStatus("CreateAnalysis", "", "Creating analysis reports in the database...");
            CreateDataToBeAnalysed();
            UpdateStatus("CreateSecurityData", "", "Creating users and roles in the database...");
            #region Create a User for the Simple Security Strategy
            //// If a simple user named 'Sam' doesn't exist in the database, create this simple user
            //SecuritySimpleUser adminUser = ObjectSpace.FindObject<SecuritySimpleUser>(new BinaryOperator("UserName", "Sam"));
            //if(adminUser == null) {
            //    adminUser = ObjectSpace.CreateObject<SecuritySimpleUser>();
            //    adminUser.UserName = "Sam";
            //}
            //// Make the user an administrator
            //adminUser.IsAdministrator = true;
            //// Set a password if the standard authentication type is used
            //adminUser.SetPassword("");
            #endregion

            #region Create Users for the Complex Security Strategy
            // If a user named 'Sam' doesn't exist in the database, create this user
            PermissionPolicyUser user1 = ObjectSpace.FindObject<PermissionPolicyUser>(new BinaryOperator("UserName", "Sam"));
            if(user1 == null) {
                user1 = ObjectSpace.CreateObject<PermissionPolicyUser>();
                user1.UserName = "Sam";
                // Set a password if the standard authentication type is used
                user1.SetPassword("");
            }
            // If a user named 'John' doesn't exist in the database, create this user
            PermissionPolicyUser user2 = ObjectSpace.FindObject<PermissionPolicyUser>(new BinaryOperator("UserName", "John"));
            if(user2 == null) {
                user2 = ObjectSpace.CreateObject<PermissionPolicyUser>();
                user2.UserName = "John";
                // Set a password if the standard authentication type is used
                user2.SetPassword("");
            }
            // If a role with the Administrators name doesn't exist in the database, create this role
            PermissionPolicyRole adminRole = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "Administrators"));
            if(adminRole == null) {
                adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                adminRole.Name = "Administrators";
            }
            adminRole.IsAdministrative = true;

            // If a role with the Users name doesn't exist in the database, create this role
            PermissionPolicyRole userRole = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "Users"));
            if(userRole == null) {
                userRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                userRole.Name = "Users";
                userRole.PermissionPolicy = SecurityPermissionPolicy.AllowAllByDefault;
                userRole.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/PermissionPolicyRole_ListView", SecurityPermissionState.Deny);
                userRole.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/PermissionPolicyUser_ListView", SecurityPermissionState.Deny);
                userRole.AddTypePermission<PermissionPolicyRole>(SecurityOperations.FullAccess, SecurityPermissionState.Deny);
                userRole.AddTypePermission<PermissionPolicyUser>(SecurityOperations.FullAccess, SecurityPermissionState.Deny);
                userRole.AddObjectPermission<PermissionPolicyUser>(SecurityOperations.ReadOnlyAccess, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                userRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", null, SecurityPermissionState.Allow);
                userRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "StoredPassword", null, SecurityPermissionState.Allow);
                userRole.AddTypePermission<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                userRole.AddTypePermission<PermissionPolicyTypePermissionObject>("Write;Delete;Create", SecurityPermissionState.Deny);
                userRole.AddTypePermission<PermissionPolicyMemberPermissionsObject>("Write;Delete;Create", SecurityPermissionState.Deny);
                userRole.AddTypePermission<PermissionPolicyObjectPermissionsObject>("Write;Delete;Create", SecurityPermissionState.Deny);
                userRole.AddTypePermission<PermissionPolicyNavigationPermissionObject>("Write;Delete;Create", SecurityPermissionState.Deny);
                userRole.AddTypePermission<PermissionPolicyActionPermissionObject>("Write;Delete;Create", SecurityPermissionState.Deny);
            }

            // Add the Administrators role to the user1
            user1.Roles.Add(adminRole);
            // Add the Users role to the user2
            user2.Roles.Add(userRole);
            #endregion

            ObjectSpace.CommitChanges();
        }
        private void CreateDataToBeAnalysed() {
            Analysis taskAnalysis1 = ObjectSpace.FindObject<Analysis>(CriteriaOperator.Parse("Name='Completed tasks'"));
            if(taskAnalysis1 == null) {
                taskAnalysis1 = ObjectSpace.CreateObject<Analysis>();
                taskAnalysis1.Name = "Completed tasks";
                taskAnalysis1.ObjectTypeName = typeof(DemoTask).FullName;
                taskAnalysis1.Criteria = "[Status] = ##Enum#DevExpress.Persistent.Base.General.TaskStatus,Completed#";
            }
            Analysis taskAnalysis2 = ObjectSpace.FindObject<Analysis>(CriteriaOperator.Parse("Name='Estimated and actual work comparison'"));
            if(taskAnalysis2 == null) {
                taskAnalysis2 = ObjectSpace.CreateObject<Analysis>();
                taskAnalysis2.Name = "Estimated and actual work comparison";
                taskAnalysis2.ObjectTypeName = typeof(DemoTask).FullName;
            }
        }
        private void UpdateAnalysisCriteriaColumn() {
            if(((XPObjectSpace)ObjectSpace).Session.Connection is SqlConnection) {
                int length = (int)ExecuteScalarCommand(@"select CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = 'Analysis' AND COLUMN_NAME = 'Criteria'", true);
                if(length != -1) {
                    ExecuteNonQueryCommand("alter table Analysis alter column Criteria nvarchar(max)", true);
                }
            }
        }

        private void CreateDepartments() {
            DataTable departmentsTable = GetDepartmentsDataTable();
            DataTable employeesTable = GetEmployeesDataTable();
            IList<Department> result = new List<Department>();
            foreach(DataRow departmentData in departmentsTable.Rows) {
                string departmentTitle = Convert.ToString(departmentData["Title"]);
                Department department = FindDepartment(departmentTitle);
                if(department == null) {
                    department = ObjectSpace.CreateObject<Department>();
                    department.Title = departmentTitle;
                    department.Office = Convert.ToString(departmentData["Office"]);
                    department.Location = Convert.ToString(departmentData["Location"]);
                    department.Description = Convert.ToString(departmentData["Description"]);
                    result.Add(department);
                    string departmentHeadId = Convert.ToString(departmentData["DepartmentHead"]);
                    DataRow employee = employeesTable.Select($"Id = '{departmentHeadId}'").FirstOrDefault();
                    if(employee != null) {
                        department.DepartmentHead = GetContact(employee);
                    }
                }
            }

            ObjectSpace.CommitChanges();
        }
        private void CreateContacts() {
            DataTable employeesTable = GetEmployeesDataTable();
            foreach(DataRow employee in employeesTable.Rows) {
                GetContact(employee);
            }
            ObjectSpace.CommitChanges();
        }
        private Contact GetContact(DataRow employee) {
            string email = Convert.ToString(employee["EmailAddress"]);
            Contact contact = ObjectSpace.FindObject<Contact>(CriteriaOperator.Parse("Email=?", email));
            if(contact == null) {
                contact = ObjectSpace.CreateObject<Contact>();
                contact.Email = email;
                contact.FirstName = Convert.ToString(employee["FirstName"]);
                contact.LastName = Convert.ToString(employee["LastName"]);
                contact.Birthday = Convert.ToDateTime(employee["BirthDate"]);
                contact.Photo = Convert.FromBase64String(Convert.ToString(employee["ImageData"]));
                string titleOfCourtesyText = Convert.ToString(employee["Title"]).ToLower();
                if(!string.IsNullOrEmpty(titleOfCourtesyText)) {
                    titleOfCourtesyText = titleOfCourtesyText.Replace(".", "");
                    TitleOfCourtesy titleOfCourtesy;
                    if(Enum.TryParse<TitleOfCourtesy>(titleOfCourtesyText, true, out titleOfCourtesy)) {
                        contact.TitleOfCourtesy = titleOfCourtesy;
                    }
                }
                PhoneNumber phoneNumber = ObjectSpace.CreateObject<PhoneNumber>();
                phoneNumber.Party = contact;
                phoneNumber.Number = Convert.ToString(employee["Phone"]);
                phoneNumber.PhoneType = "Work";

                Address address = ObjectSpace.CreateObject<Address>();
                contact.Address1 = address;
                address.ZipPostal = Convert.ToString(employee["PostalCode"]);
                address.Street = Convert.ToString(employee["AddressLine1"]);
                address.City = Convert.ToString(employee["City"]);
                address.StateProvince = Convert.ToString(employee["StateProvinceName"]);
                string countryName = Convert.ToString(employee["CountryRegionName"]);
                Country country = ObjectSpace.FindObject<Country>(CriteriaOperator.Parse("Name=?", countryName), true);
                if(country == null) {
                    country = ObjectSpace.CreateObject<Country>();
                    country.Name = countryName;
                }
                address.Country = country;

                string departmentTitle = Convert.ToString(employee["GroupName"]);
                contact.Department = FindDepartment(departmentTitle);

                string positionTitle = Convert.ToString(employee["JobTitle"]);
                Position position = FindPosition(positionTitle);
                if(position == null) {
                    position = ObjectSpace.CreateObject<Position>();
                    position.Title = positionTitle;
                    if(contact.Department != null) {
                        position.Departments.Add(contact.Department);
                        contact.Department.Positions.Add(position);
                    }
                }
                contact.Position = position;
            }
            return contact;
        }
        private IList<DemoTask> GenerateTask(IList<Contact> contacts) {
            Random rndGenerator = new Random();
            List<DemoTask> taskList = new List<DemoTask>();
            DataTable tasksTable = GetTasksDataTable();
            foreach(DataRow taskRow in tasksTable.Rows) {
                string taskSubject = UpdateContent(Convert.ToString(taskRow["Subject"]));
                if(ObjectSpace.FindObject<DemoTask>(CriteriaOperator.Parse("Subject == '" + taskSubject + "'")) == null) {
                    DemoTask task = ObjectSpace.CreateObject<DemoTask>();
                    task.Subject = taskSubject;
                    task.Description = UpdateContent(Convert.ToString(taskRow["Description"]));
                    int rndStatus = rndGenerator.Next(0, 5);
                    task.Status = (DevExpress.Persistent.Base.General.TaskStatus)rndStatus;
                    task.DueDate = DateTime.Now.AddHours((90 - rndStatus * 9) + 24).Date;
                    task.EstimatedWorkHours = rndGenerator.Next(10, 20);
                    if(task.Status == DevExpress.Persistent.Base.General.TaskStatus.WaitingForSomeoneElse ||
                       task.Status == DevExpress.Persistent.Base.General.TaskStatus.Completed ||
                       task.Status == DevExpress.Persistent.Base.General.TaskStatus.InProgress) {
                        task.StartDate = DateTime.Now.AddHours(-rndGenerator.Next(720)).Date;
                        task.ActualWorkHours = rndGenerator.Next(task.EstimatedWorkHours - 10, task.EstimatedWorkHours + 10);
                    }
                    task.DueDate = DateTime.Now.AddHours((90 - rndStatus * 9) + 24).Date;
                    task.AssignedTo = contacts[rndGenerator.Next(0, contacts.Count)];
                    task.Priority = (Priority)rndGenerator.Next(3);
                    taskList.Add(task);
                }
            }
            return taskList;
        }
        private Stream FindContactResume(Contact contact) {
            string shortName = contact.FirstName + "_" + contact.LastName + ".pdf";
            string embeddedResourceName = Array.Find<string>(this.GetType().Assembly.GetManifestResourceNames(), (s) => { return s.Contains(shortName); });
            return this.GetType().Assembly.GetManifestResourceStream(embeddedResourceName);
        }
        private bool LocationIsEmpty(Contact contact) {
            return contact.Location == null || contact.Location.Contact == null || (contact.Location.Latitude == 0 && contact.Location.Longitude == 0);
        }
        private void InitContactsLocation() {
            Contact contactMary = FindTellitson();
            if(contactMary != null && LocationIsEmpty(contactMary)) {
                if(contactMary.Location == null)
                    contactMary.Location = ObjectSpace.CreateObject<Location>();

                contactMary.Location.Contact = contactMary;
                contactMary.Location.Latitude = 40.620610;
                contactMary.Location.Longitude = -73.935242;
            }

            Contact contactJohn = FindContact("John", "Nilsen");
            if(contactJohn != null && LocationIsEmpty(contactJohn)) {
                if(contactJohn.Location == null)
                    contactJohn.Location = ObjectSpace.CreateObject<Location>();

                contactJohn.Location.Contact = contactJohn;
                contactJohn.Location.Latitude = 40.711510;
                contactJohn.Location.Longitude = -73.845252;
            }

            Contact contactJanete = FindJanete();
            if(contactJanete != null && LocationIsEmpty(contactJanete)) {
                if(contactJanete.Location == null)
                    contactJanete.Location = ObjectSpace.CreateObject<Location>();

                contactJanete.Location.Contact = contactJanete;
                contactJanete.Location.Latitude = 40.710410;
                contactJanete.Location.Longitude = -73.963262;
            }

            Contact contactKarl = FindContact("Karl", "Jablonski");
            if(contactKarl != null && LocationIsEmpty(contactKarl)) {
                if(contactKarl.Manager == null) {
                    contactKarl.Manager = contactJanete;
                }
                if(contactKarl.Location == null)
                    contactKarl.Location = ObjectSpace.CreateObject<Location>();

                contactKarl.Location.Contact = contactKarl;
                contactKarl.Location.Latitude = 40.792613;
                contactKarl.Location.Longitude = -73.925142;
            }

            ObjectSpace.CommitChanges();
        }

        private DataTable employeesDataTable = null;
        private DataTable GetEmployeesDataTable() {
            if(employeesDataTable == null) {
                string shortName = "EmployeesWithPhoto.xml";
                Stream stream = GetResourceByName(shortName);
                DataSet ds = new DataSet();
                ds.ReadXml(stream);
                employeesDataTable = ds.Tables["Employee"];
            }
            return employeesDataTable;
        }
        private DataTable GetTasksDataTable() {
            string shortName = "Tasks.xml";
            Stream stream = GetResourceByName(shortName);
            DataSet ds = new DataSet();
            ds.ReadXml(stream);
            return ds.Tables["Task"];
        }
        private DataTable GetDepartmentsDataTable() {
            string shortName = "Departments.xml";
            Stream stream = GetResourceByName(shortName);
            DataSet ds = new DataSet();
            ds.ReadXml(stream);
            return ds.Tables["Department"];
        }
        private Stream GetResourceByName(string shortName) {
            string embeddedResourceName = Array.Find<string>(this.GetType().Assembly.GetManifestResourceNames(), (s) => { return s.Contains(shortName); });
            Stream stream = this.GetType().Assembly.GetManifestResourceStream(embeddedResourceName);
            if(stream == null) {
                throw new Exception(string.Format("Cannot read data from the {0} file!", shortName));
            }
            return stream;
        }
        private string UpdateContent(string content) {
            if(!string.IsNullOrEmpty(content)) {
                return content.Replace("LAST_YEAR", (DateTime.Now.Year - 1).ToString()).Replace("CURRENT_YEAR", DateTime.Now.Year.ToString());
            }
            return content;
        }
    }
#if !NETSTANDARD
    public abstract class TaskAnalysis1LayoutUpdaterBase {
        protected IObjectSpace objectSpace;
        protected abstract IAnalysisControl CreateAnalysisControl();
        protected abstract IPivotGridSettingsStore CreatePivotGridSettingsStore(IAnalysisControl control);
        public TaskAnalysis1LayoutUpdaterBase(IObjectSpace objectSpace) {
            this.objectSpace = objectSpace;
        }
        public void Update(Analysis analysis) {
            if(analysis != null && !PivotGridSettingsHelper.HasPivotGridSettings(analysis)) {
                IAnalysisControl control = CreateAnalysisControl();
                control.DataSource = new AnalysisDataSource(analysis, objectSpace.GetObjects(typeof(DemoTask)));
                control.Fields["Priority"].Area = DevExpress.XtraPivotGrid.PivotArea.ColumnArea;
                control.Fields["Subject"].Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
                control.Fields["AssignedTo.DisplayName"].Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;
                PivotGridSettingsHelper.SavePivotGridSettings(CreatePivotGridSettingsStore(control), analysis);
            }
        }
    }
    public abstract class TaskAnalysis2LayoutUpdaterBase {
        protected IObjectSpace objectSpace;
        protected abstract IAnalysisControl CreateAnalysisControl();
        protected abstract IPivotGridSettingsStore CreatePivotGridSettingsStore(IAnalysisControl control);
        public TaskAnalysis2LayoutUpdaterBase(IObjectSpace objectSpace) {
            this.objectSpace = objectSpace;
        }
        public void Update(Analysis analysis) {
            if(analysis != null && !PivotGridSettingsHelper.HasPivotGridSettings(analysis)) {
                IAnalysisControl control = CreateAnalysisControl();
                control.DataSource = new AnalysisDataSource(analysis, objectSpace.GetObjects(typeof(DemoTask)));
                control.Fields["Status"].Area = DevExpress.XtraPivotGrid.PivotArea.ColumnArea;
                control.Fields["Priority"].Area = DevExpress.XtraPivotGrid.PivotArea.ColumnArea;
                control.Fields["EstimatedWorkHours"].Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
                control.Fields["ActualWorkHours"].Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
                control.Fields["AssignedTo.DisplayName"].Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;
                PivotGridSettingsHelper.SavePivotGridSettings(CreatePivotGridSettingsStore(control), analysis);
            }
        }
    }
#endif
    class PayrollSampleDataGenerator {
        public static void GenerateContactPaychecks(DevExpress.ExpressApp.IObjectSpace objectSpace, Contact employee) {
            int year = DateTime.Now.Year - 1;
            var taxRates = new double[] { 0.20, 0.22, 0.30 };
            var hoursBaseline = new int[] { 35, 30 };
            var rand = new Random();
            var payRate = rand.Next(10, 12) + rand.Next(0, 3) + rand.Next(5, 9);
            var overtimePayRate = rand.Next(15, 25) + rand.Next(5, 5);
            var taxRated = taxRates[rand.Next(0, 3)];
            var hours = rand.Next(0, 5) == 0 ? hoursBaseline[rand.Next(0, 1)] : 40;
            for(int month = 1; month <= 12; month++) {
                GeneratePaycheck(objectSpace, employee, rand, payRate, overtimePayRate, taxRated, hours,
                    2 * month - 1, new DateTime(day: 1, month: month, year: year), new DateTime(day: 15, month: month, year: year));
                GeneratePaycheck(objectSpace, employee, rand, payRate, overtimePayRate, taxRated, hours,
                    2 * month, new DateTime(day: 16, month: month, year: year), new DateTime(day: DateTime.DaysInMonth(year, month), month: month, year: year));
            }
        }

        private static void GeneratePaycheck(IObjectSpace objectSpace, Contact employee, Random rand, int payRate, int overtimePayRate, double taxRated, int hours, int period, DateTime start, DateTime end) {
            var paycheck = objectSpace.CreateObject<Paycheck>();
            if(rand.Next(0, 5) == 0) {
                hours -= rand.Next(0, 2) + rand.Next(0, 2) + rand.Next(0, 2);
            }
            int overtimeHours = 0;
            if(rand.Next(0, 5) == 0) {
                overtimeHours += rand.Next(2, 5) + rand.Next(1, 6);
            }
            paycheck.PayPeriod = period;
            paycheck.PayPeriodStart = start;
            paycheck.PayPeriodEnd = end;
            paycheck.PaymentDate = end;
            paycheck.Employee = employee;
            paycheck.PayRate = payRate;
            paycheck.Hours = hours;
            paycheck.OvertimePayRate = overtimePayRate;
            paycheck.OvertimeHours = overtimeHours;
            paycheck.TaxRate = taxRated;
        }
    }

}
