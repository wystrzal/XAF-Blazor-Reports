using DevExpress.ExpressApp;
using DevExpress.XtraPrinting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MainDemo.Module.Blazor
{
    public class ReportStorage
    {
        private ReportStorage() { }
        private static readonly Dictionary<string, ReportStorage> instance = new Dictionary<string, ReportStorage>();

        public object ObjectInfo { get; set; }
        public XafApplication Application { get; set; }
        public MemoryStream MemoryStream { get; set; }

        public static ReportStorage GetInstance(string userName)
        {
            if (userName == null)
            {
                return null;
            }

            if (!instance.ContainsKey(userName) || instance[userName] == null)
            {
                instance.Add(userName, new ReportStorage());
            }
            return instance[userName];
        }
    }
}
