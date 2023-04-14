using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntersectSteam.Models.Requests
{
    public class UserInfoRequest
    {
        public string Key { get; set; }
        public uint AppId { get; set; }
        public ulong SteamId { get; set; }
        public string IpAddress { get; set; }

        public UserInfoRequest(string key, uint appId, ulong steamId, string ipAddress)
        {
            Key = key;
            AppId = appId;
            SteamId = steamId;
            IpAddress = ipAddress;
        }

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>
        {
            { "key", Key },
            { "appid", AppId.ToString() },
            { "steamid", SteamId.ToString() },
            { "ipaddress", IpAddress }
        };
            return dict;
        }
    }

}
