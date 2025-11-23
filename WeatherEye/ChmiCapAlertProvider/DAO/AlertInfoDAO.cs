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
    public partial class AlertInfoDAO : XPLiteObject
    {
        private Guid fId;
        [Key]
        public Guid Id
        {
            get { return fId; }
            set { SetPropertyValue<Guid>(nameof(Id), ref fId, value); }
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

        private string fSenderName;

        [Size(-1)]
        public string SenderName
        {
            get { return fSenderName; }
            set { SetPropertyValue<string>(nameof(SenderName), ref fSenderName, value); }
        }
        private string fHeadline;

        [Size(-1)]
        public string Headline
        {
            get { return fHeadline; }
            set { SetPropertyValue<string>(nameof(Headline), ref fHeadline, value); }
        }
        private string fDescription;

        [Size(-1)]
        public string Description
        {
            get { return fDescription; }
            set { SetPropertyValue<string>(nameof(Description), ref fDescription, value); }
        }
        private string fInstruction;

        [Size(-1)]
        public string Instruction
        {
            get { return fInstruction; }
            set { SetPropertyValue<string>(nameof(Instruction), ref fInstruction, value); }
        }
        private string fWeb;

        [Size(-1)]
        public string Web
        {
            get { return fWeb; }
            set { SetPropertyValue<string>(nameof(Web), ref fWeb, value); }
        }
        private string fContact;

        [Size(-1)]
        public string Contact
        {
            get { return fContact; }
            set { SetPropertyValue<string>(nameof(Contact), ref fContact, value); }
        }
        private DateTime? fOnset;
        public DateTime? Onset
        {
            get { return fOnset; }
            set { SetPropertyValue<DateTime?>(nameof(Onset), ref fOnset, value); }
        }
        private DateTime? fExpires;
        public DateTime? Expires
        {
            get { return fExpires; }
            set { SetPropertyValue<DateTime?>(nameof(Expires), ref fExpires, value); }
        }
        private string fEvent;

        [Size(-1)]
        public string Event
        {
            get { return fEvent; }
            set { SetPropertyValue<string>(nameof(Event), ref fEvent, value); }
        }
        private string fResponseType;

        [Size(-1)]
        public string ResponseType
        {
            get { return fResponseType; }
            set { SetPropertyValue<string>(nameof(ResponseType), ref fResponseType, value); }
        }

        private AlertInfoUrgency fUrgency;
        public AlertInfoUrgency Urgency
        {
            get { return fUrgency; }
            set { SetPropertyValue<AlertInfoUrgency>(nameof(Urgency), ref fUrgency, value); }
        }
        private string fCategory;

        [Size(-1)]
        public string Category
        {
            get { return fCategory; }
            set { SetPropertyValue<string>(nameof(Category), ref fCategory, value); }
        }
        private string fLanguage;
        public string Language
        {
            get { return fLanguage; }
            set { SetPropertyValue<string>(nameof(Language), ref fLanguage, value); }
        }
        private AlertInfoSeverity fSeverity;
        public AlertInfoSeverity Severity
        {
            get { return fSeverity; }
            set { SetPropertyValue<AlertInfoSeverity>(nameof(Severity), ref fSeverity, value); }
        }
        private AlertInfoCertainty fCertainty;
        public AlertInfoCertainty Certainty
        {
            get { return fCertainty; }
            set { SetPropertyValue<AlertInfoCertainty>(nameof(Certainty), ref fCertainty, value); }
        }
        private string fAudience;

        [Size(-1)]
        public string Audience
        {
            get { return fAudience; }
            set { SetPropertyValue<string>(nameof(Audience), ref fAudience, value); }
        }
        private string fEventCode;

        [Size(-1)]
        public string EventCode
        {
            get { return fEventCode; }
            set { SetPropertyValue<string>(nameof(EventCode), ref fEventCode, value); }
        }
        private DateTime? fEffective;
        public DateTime? Effective
        {
            get { return fEffective; }
            set { SetPropertyValue<DateTime?>(nameof(Effective), ref fEffective, value); }
        }

        [Association("AlertRefArea")]
        public XPCollection<AreaDAO> Areas
        {
            get { return GetCollection<AreaDAO>(nameof(Areas)); }
        }

        private AlertDAO fAlert;
        [Association("Alert-AlertInfos")]
        public AlertDAO Alert
        {
            get { return fAlert; }
            set { SetPropertyValue<AlertDAO>(nameof(Alert), ref fAlert, value); }
        }


        public override void AfterConstruction()
        {
            Id = Guid.NewGuid();
            base.AfterConstruction();
        }
        //public List<AreaDAO> Area { get; set; }
        public AlertInfoDAO(Session session) : base(session) { }

        public void SetProperties(AlertInfo info, Alert alert)
        {
            Identifier = alert.Identifier;
            Sent = DateTime.SpecifyKind(alert.Sent, DateTimeKind.Utc);

            if (info.Onset != DateTime.MinValue)
                Onset = DateTime.SpecifyKind(info.Onset, DateTimeKind.Utc);
            if (info.Expires != DateTime.MinValue)
                Expires = DateTime.SpecifyKind(info.Expires, DateTimeKind.Utc);
            Event = info.Event;

            if (info.ResponseTypeSpecified)
            {
                ResponseType = info.ResponseType.Select(rt => rt.ToString()).Aggregate((a, b) => a + ";" + b);
            }
            Urgency = info.Urgency;
            Category = info.Category.Select(c => c.ToString()).Aggregate((a, b) => a + ";" + b);

            Language = info.Language;
            Severity = info.Severity;
            Certainty = info.Certainty;
            Audience = info.Audience;
            EventCode = JsonSerializer.Serialize(info.EventCode);
            if (info.Effective != DateTime.MinValue)
                Effective = DateTime.SpecifyKind(info.Effective, DateTimeKind.Utc);
            SenderName = info.SenderName;
            Headline = info.Headline;
            Description = info.Description;
            Instruction = info.Instruction;
            Web = info.Web;
            Contact = info.Contact;

            foreach (var area in info.Area)
            {
                XPQuery<AreaDAO> query = Session.Query<AreaDAO>();
                var areaDAOExisting = query.Where(a => a.AreaDesc == area.AreaDesc)
                    .FirstOrDefault();

                if (areaDAOExisting is not null)
                {
                    Areas.Add(areaDAOExisting);
                }
                else
                {
                    AreaDAO areaDAO = new AreaDAO(Session);
                    areaDAO.SetProperties(area);
                    areaDAO.Save();
                    Areas.Add(areaDAO);
                }

            }

        }
    }
}
