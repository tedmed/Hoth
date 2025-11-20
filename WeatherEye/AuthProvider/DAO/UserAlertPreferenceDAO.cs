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

        private UserDAO fUser;

        [Association]
        public UserDAO User
        {
            get { return fUser; }
            set { SetPropertyValue<UserDAO>(nameof(User), value); }
        }

    }
}
