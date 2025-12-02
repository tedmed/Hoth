using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChmiCapAlertProvider.DAO
{
    public class SpecificAreaDAO : XPLiteObject
    {
        private Guid fId;
        [Key]
        public Guid Id
        {
            get { return fId; }
            set { SetPropertyValue<Guid>(nameof(Id), ref fId, value); }
        }
        private string fDescription;
        [Size(400)]
        public string Description
        {
            get { return fDescription; }
            set { SetPropertyValue<string>(nameof(Description), ref fDescription, value); }
        }

        private string fCisorpId;
        [Size(50)]
        public string CisorpId
        {
            get { return fCisorpId; }
            set { SetPropertyValue<string>(nameof(CisorpId), ref fCisorpId, value); }
        }

        private string fEmmaId;
        [Size(50)]
        public string EmmaId
        {
            get { return fEmmaId; }
            set { SetPropertyValue<string>(nameof(EmmaId), ref fEmmaId, value); }
        }

        private AreaDAO fArea;

        [Association("AreaRefSpecificAreas")]
        public AreaDAO Area
        {
            get { return fArea; }
            set { SetPropertyValue<AreaDAO>(nameof(Area), ref fArea, value); }
        }


        [Association("AlertRefSpecificArea")]
        public XPCollection<AlertInfoDAO> Alerts
        {
            get { return GetCollection<AlertInfoDAO>(nameof(Alerts)); }
        }

        public SpecificAreaDAO(Session session) : base(session) { }
        public override void AfterConstruction()
        {
            Id = Guid.NewGuid();
            base.AfterConstruction();
        }
    }
}
