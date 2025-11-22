using CAP;
using DevExpress.Xpo;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChmiCapAlertProvider.DAO
{
    public class AreaDAO : XPLiteObject
    {
        private Guid fId;
        [Key]
        public Guid Id
        {
            get { return fId; }
            set { SetPropertyValue<Guid>(nameof(Id), ref fId, value); }
        }
        private string fAreaDesc;
        [Size(-1)]
        public string AreaDesc
        {
            get { return fAreaDesc; }
            set { SetPropertyValue<string>(nameof(AreaDesc), ref fAreaDesc, value); }
        }

        private decimal? fAltitude;
        public decimal? Altitude
        {
            get { return fAltitude; }
            set { SetPropertyValue<decimal?>(nameof(Altitude), ref fAltitude, value); }
        }
        
        private decimal? fCeiling;
        public decimal? Ceiling
        {
            get { return fCeiling; }
            set { SetPropertyValue<decimal?>(nameof(Ceiling), ref fCeiling, value); }
        }

        [Association("AlertRefArea")]
        public XPCollection<AlertDAO> Alerts
        {
            get { return GetCollection<AlertDAO>(nameof(Alerts)); }
        }

        public AreaDAO(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            Id = Guid.NewGuid();
            base.AfterConstruction();
        }

        public void SetProperties(AlertInfoArea area)
        {
            AreaDesc = area.AreaDesc;
            Altitude = area.Altitude;
            Ceiling = area.Ceiling;
        }
    }
}
