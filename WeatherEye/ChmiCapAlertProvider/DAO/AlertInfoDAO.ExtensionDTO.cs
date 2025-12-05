using CAP;
using CAP.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChmiCapAlertProvider.DAO
{
    public partial class AlertInfoDAO
    {
        public IList<AlertInfoDTO> TransformToDTOs()
        {

            IList<AlertInfoDTO> dtos = new List<AlertInfoDTO>();
            foreach (var info in this.SpecificAreas)
            {
                dtos.Add(new AlertInfoDTO()
                {
                    SenderName = this.SenderName,
                    Event = this.Event,
                    Urgency = (int)this.Urgency,
                    Severity = (int)this.Severity,
                    Certainty = (int)this.Certainty,
                    Language = this.Language,
                    Onset = this.Onset ?? DateTime.MinValue,
                    Expires = this.Expires ?? DateTime.MinValue,
                    Headline = this.Headline,
                    Description = this.Description,
                    Instruction = this.Instruction,
                    AreaDesc = info.Area?.AreaDesc ?? string.Empty,
                    SpecificAreaDesc = info.Description
                });

            }

            return dtos;

        }
    }
}
