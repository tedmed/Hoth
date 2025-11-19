using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingContracts
{
    public record UserLoginRequest(string Username, string Password);
    public record UserLoginResponse(bool IsSuccessful, string Token);
}
