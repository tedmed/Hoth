using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChmiCapAlertProvider.DAO
{
    public class AlertDAO : XPLiteObject
    {
        public AlertDAO(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            Oid = Guid.NewGuid();
            base.AfterConstruction();
        }

        private Guid fOid;
        [Key]
        public Guid Oid
        {
            get { return fOid; }
            set { SetPropertyValue<Guid>(nameof(Oid), ref fOid, value); }
        }
        private string fIdentifier;
        [Size(-1)]
        public string Identifier
        {
            get { return fIdentifier; }
            set { SetPropertyValue<string>(nameof(Identifier), ref fIdentifier, value); }
        }
        private DateTime? fSent;
        public DateTime? Sent
        {
            get { return fSent; }
            set { SetPropertyValue<DateTime?>(nameof(Sent), ref fSent, value); }
        }

        [Association("Alert-AlertInfos")]
        public XPCollection<AlertInfoDAO> AlertInfos
        {
            get
            {
                return GetCollection<AlertInfoDAO>(nameof(AlertInfos));
            }
        }
    }
}
