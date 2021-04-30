using System;
using System.Collections.Generic;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Xpo;
using MainDemo.Blazor.ServerSide.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MainDemo.Blazor.ServerSide {
    public partial class MainDemoBlazorApplication : BlazorApplication {
        class EmptySettingsStorage : SettingsStorage {
            public override string LoadOption(string optionPath, string optionName) => null;
            public override void SaveOption(string optionPath, string optionName, string optionValue) { }
        }
        private bool useMemoryStore;
        private bool isCompatibilityChecked;

        public MainDemoBlazorApplication() {
            InitializeComponent();
            AboutInfo.Instance.Version = "Version " + AssemblyInfo.FileVersion;
            AboutInfo.Instance.Copyright = AssemblyInfo.AssemblyCopyright + " All Rights Reserved";
        }
        protected override void OnSetupStarted() {
            base.OnSetupStarted();
            IConfiguration configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            if(configuration.GetConnectionString("ConnectionString") != null) {
                ConnectionString = configuration.GetConnectionString("ConnectionString");
            }
#if EASYTEST
            if(configuration.GetConnectionString("EasyTestConnectionString") != null) {
                ConnectionString = configuration.GetConnectionString("EasyTestConnectionString");
            }
#endif
#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached && CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
                DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
            }
#endif
        }
#if EASYTEST
        protected override void OnSetupComplete() {
            base.OnSetupComplete();
            ((DevExpress.ExpressApp.Model.IModelListView)Model.Views["Party_LookupListView"]).DataAccessMode = CollectionSourceDataAccessMode.Client;
        }
#endif
        protected override void CreateDefaultObjectSpaceProvider(CreateCustomObjectSpaceProviderEventArgs args) {
            IXpoDataStoreProvider dataStoreProvider = GetDataStoreProvider(args.ConnectionString, args.Connection);
            args.ObjectSpaceProviders.Add(new SecuredObjectSpaceProvider((ISelectDataSecurityProvider)Security, dataStoreProvider, true));
            args.ObjectSpaceProviders.Add(new NonPersistentObjectSpaceProvider(TypesInfo, null));
        }
        private IXpoDataStoreProvider GetDataStoreProvider(string connectionString, System.Data.IDbConnection connection) {
            useMemoryStore = false;
            XpoDataStoreProviderAccessor accessor = ServiceProvider.GetRequiredService<XpoDataStoreProviderAccessor>();
            lock(accessor) {
                if(accessor.DataStoreProvider == null) {
                    accessor.DataStoreProvider = XPObjectSpaceProvider.GetDataStoreProvider(connectionString, connection, !useMemoryStore);
                }
            }
            return accessor.DataStoreProvider;
        }
        private void MainDemoBlazorApplication_DatabaseVersionMismatch(object sender, DatabaseVersionMismatchEventArgs e) {
            e.Updater.Update();
            e.Handled = true;
        }
        protected override bool IsCompatibilityChecked {
            get {
                if(useMemoryStore) {
                    return isCompatibilityChecked;
                }
                else {
                    return base.IsCompatibilityChecked;
                }
            }
            set {
                if(useMemoryStore) {
                    isCompatibilityChecked = value;
                }
                else {
                    base.IsCompatibilityChecked = value;
                }
            }
        }
        protected override SettingsStorage CreateLogonParameterStoreCore() {
            return new EmptySettingsStorage();
        }
        private void MainDemoBlazorApplication_LastLogonParametersRead(object sender, LastLogonParametersReadEventArgs e) {
            if(e.LogonObject is AuthenticationStandardLogonParameters logonParameters && string.IsNullOrEmpty(logonParameters.UserName)) {
                logonParameters.UserName = "Sam";
            }
        }
    }
}
