using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAP.DTOs
{
    public class AlertInfoDTO
    {
        public string SenderName { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;
        public string Urgency { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Certainty { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public DateTime Onset { get; set; }
        public DateTime Expires { get; set; }
        public string Headline { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instruction { get; set; } = string.Empty;
        
        public string AreaDesc { get; set; } = string.Empty;




        public AlertInfoDTO()
        {

        }
    }
}
