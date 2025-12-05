using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Text;

namespace UserService.DAO
{
    public class UserAlertPreferenceDAO : XPLiteObject
    {
        public UserAlertPreferenceDAO(Session session) : base(session)
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

        private string fAreaDesc;
        [Size(1000)]
        public string AreaDesc
        {
            get { return fAreaDesc; }
            set { SetPropertyValue<string>(nameof(AreaDesc), ref fAreaDesc, value); }
        }

        private string fSpecificAreaDesc;
        [Size(1000)]
        public string SpecificAreaDesc
        {
            get { return fSpecificAreaDesc; }
            set { SetPropertyValue<string>(nameof(SpecificAreaDesc), ref fSpecificAreaDesc, value); }
        }

        private bool fEmailNotification;

        public bool EmailNotification
        {
            get { return fEmailNotification; }
            set { SetPropertyValue<bool>(nameof(EmailNotification), ref fEmailNotification, value); }
        }

        private bool fInAppNotification;
        public bool InAppNotification
        {
            get { return fInAppNotification; }
            set { SetPropertyValue<bool>(nameof(InAppNotification), ref fInAppNotification, value); }
        }

        private CAP.AlertInfoSeverity fSeverity;
        public CAP.AlertInfoSeverity Severity
        {
            get { return fSeverity; }
            set { SetPropertyValue<CAP.AlertInfoSeverity>(nameof(Severity), ref fSeverity, value); }
        }
        private CAP.AlertInfoCertainty fCertainty;
        public CAP.AlertInfoCertainty Certainty
        {
            get { return fCertainty; }
            set { SetPropertyValue<CAP.AlertInfoCertainty>(nameof(Certainty), ref fCertainty, value); }
        }

        private UserDAO fUser;

        [Association("UserRefAlertPreference")]
        public UserDAO User
        {
            get { return fUser; }
            set { SetPropertyValue<UserDAO>(nameof(User), ref fUser, value); }
        }

    }
}
