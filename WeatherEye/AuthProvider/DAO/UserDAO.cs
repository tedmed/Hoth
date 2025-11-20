using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Text;

namespace UserService.DAO
{
    public class UserDAO : XPLiteObject
    {
        public Guid fOid;
        [Key]
        public Guid Oid
        {
            get { return fOid; }
            set { SetPropertyValue<Guid>(nameof(Oid), ref fOid, value); }
        }
        private string fUsername;
        [Size(200)]
        public string Username
        {
            get { return fUsername; }
            set { SetPropertyValue<string>(nameof(Username), ref fUsername, value); }
        }
        private string fEmail;
        [Size(500)]
        public string Email
        {
            get { return fEmail; }
            set { SetPropertyValue<string>(nameof(Email), ref fEmail, value); }
        }
        private string fMobAppDeviceId;
        [Size(1000)]
        public string MobAppDeviceId
        {
            get { return fMobAppDeviceId; }
            set { SetPropertyValue<string>(nameof(MobAppDeviceId), ref fMobAppDeviceId, value); }
        }

        [Association]
        public XPCollection<UserAlertPreferenceDAO> AlertPreferences
        {
            get { return GetCollection<UserAlertPreferenceDAO>(nameof(AlertPreferences)); }
        }

        public override void AfterConstruction()
        {
            Oid = Guid.NewGuid();
            base.AfterConstruction();
        }

        public UserDAO(Session session) : base(session) { }
    }
}
