using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace MainDemo.Module.BusinessObjects {
    [FileAttachment(nameof(File))]
    [DefaultClassOptions, ImageName("BO_Resume")]
    public class Resume : BaseObject {
        private Contact contact;
        private FileData file;
        public Resume(Session session)
            : base(session) {
        }
        [Aggregated, ExpandObjectMembers(ExpandObjectMembers.Never)]
        [RuleRequiredField]
        public FileData File {
            get {
                return file;
            }
            set {
                SetPropertyValue(nameof(File), ref file, value);
            }
        }
        [RuleRequiredField]
        public Contact Contact {
            get {
                return contact;
            }
            set {
                SetPropertyValue(nameof(Contact), ref contact, value);
            }
        }
        [Aggregated, Association("Resume-PortfolioFileData")]
        public XPCollection<PortfolioFileData> Portfolio {
            get {
                return GetCollection<PortfolioFileData>(nameof(Portfolio));
            }
        }
    }
}
