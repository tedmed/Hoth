using CAP;
using CAP.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingContracts
{
    public record AlertRequest;
    public record AlertResponse(IEnumerable<AlertInfoDTO> records);

    public record AlertAreaRequest;
    public record AlertAreaResponse(IEnumerable<string> regions);

    public record NewAlertCreated(AlertInfoDTO AlertInfo);
}
