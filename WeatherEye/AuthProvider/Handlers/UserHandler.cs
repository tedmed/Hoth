using DevExpress.ExpressApp.Core;
using DevExpress.Pdf.Native;
using MessagingContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolverine.Attributes;

namespace UserService.Handlers
{
    [WolverineHandler]
    public class UserHandler
    {
        private readonly IObjectSpaceProviderFactory objectSpaceFactory;

        public UserHandler(IObjectSpaceProviderFactory objectSpaceFactory)
        {
            this.objectSpaceFactory = objectSpaceFactory;
        }


    }
}
