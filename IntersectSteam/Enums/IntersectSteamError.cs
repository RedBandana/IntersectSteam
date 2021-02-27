using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntersectSteam.Enums
{
    public enum IntersectSteamError
    {
        NoError = 0,

        NoErrorSandBox,

        NoApiKey,

        WrongBaseUrl,

        WrongId,

        Unauthorized,

        NoOrder,

        EmptyString,

        Uninitialized,

        UnKnown,
    }
}
