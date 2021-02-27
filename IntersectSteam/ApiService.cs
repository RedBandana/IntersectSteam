using IntersectSteam.Enums;
using IntersectSteam.Models.Orders;
using IntersectSteam.Models.Transactions;
using IntersectSteam.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace IntersectSteam.Models
{
    public class ApiService
    {
        private static uint APP_ID = 0;
        private static string BASE_URL = "";
        private static string API_KEY = "";

        private static readonly HttpClient mClient = new HttpClient();

        private static bool mInitialized = false;

        public static InitError Init(string apiUrl, string apiKey, uint appId)
        {
            APP_ID = appId;
            BASE_URL = apiUrl;
            API_KEY = apiKey;
            try
            {
                if (string.IsNullOrEmpty(API_KEY))
                {
                    return InitError.NoApiKey;
                }

                mClient.BaseAddress = new Uri(BASE_URL);
                mClient.DefaultRequestHeaders.Accept.Clear();
                mClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                mInitialized = true;

                if (BASE_URL.ToLowerInvariant().Contains("sandbox"))
                {
                    return InitError.NoErrorSandBox;
                }
                return InitError.NoError;
            }
            catch (Exception e)
            {
                // Something went wrong!
                return InitError.UnKnown;
            }
        }

        public static async Task<string> SendOrder(Order order, ulong steamClientId, string gameLanguage)
        {
            if (mInitialized)
            {
                order.User = await GetUserInfo(steamClientId, gameLanguage);
                return await InitTxn(order);
            }
            else
            {
                return "API was not initialized. Probably no API Key.";
            }
        }

        public static async Task<UserData> GetUserInfo(ulong steamId, string gameLanguage)
        {
            UserData user = new UserData();

            HttpResponseMessage response = await mClient.GetAsync("GetUserInfo/v2/?steamid=" + steamId + "&key=" + API_KEY);
            if (response.IsSuccessStatusCode)
            {
                UserApiResponse res = await response.Content.ReadAsAsync<UserApiResponse>();

                if (res.Response.Result == "OK")
                {
                    user.ApiInfo = res.Response.Params;
                    user.SteamId = steamId;
                    var x = Iso639.Language.FromName(gameLanguage, true)
                        .Where(l => !string.IsNullOrEmpty(l.Part1) && l.Type == Iso639.LanguageType.Living)
                        .FirstOrDefault();

                    if (x != null)
                    {
                        user.Language = x.Part1;
                    }
                    else
                    {
                        user.Language = "en";
                    }
                }
                else if (res.Response.Result == "Failure")
                {
                    user.ErrorMsg = res.Response.Error.ErrorCode + ": " + res.Response.Error.ErrorDesc;
                }

                return user;
            }

            user.ErrorMsg = await response.Content.ReadAsStringAsync();
            return user;
        }

        public static async Task<string> InitTxn(Order o)
        {
            Dictionary<string, string> order = new Dictionary<string, string>
            {
                { "key", API_KEY },
                { "orderid", o.Id.ToString() },
                { "steamid", o.User.SteamId.ToString() },
                { "appid", APP_ID.ToString() },
                { "itemcount", o.Items.Count.ToString() },
                { "language", o.User.Language },
                { "currency", "USD" } // Automatic conversion on the user side
            };

            for (int i = 0; i < o.Items.Count; i++)
            {
                order.Add("itemid[" + i + "]", o.Items[i].Id.ToString());
                order.Add("qty[" + i + "]", o.Items[i].Quantity.ToString());
                order.Add("amount[" + i + "]", o.Items[i].Amount.ToString());
                order.Add("description[" + i + "]", o.Items[i].Description.ToString());
            }

            HttpContent httpContent = new FormUrlEncodedContent(order);

            HttpResponseMessage response = await mClient.PostAsync("InitTxn/v3/", httpContent);
            if (response.IsSuccessStatusCode)
            {
                TxnApiResponse res = await response.Content.ReadAsAsync<TxnApiResponse>();

                if (res.Response.Result == "OK")
                {
                    return "OK";
                }
                else if (res.Response.Result == "Failure")
                {
                    return res.Response.Error.ErrorCode + ": " + res.Response.Error.ErrorDesc;
                }
            }
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> FinalizeTxn(ulong orderId)
        {
            Dictionary<string, string> order = new Dictionary<string, string>
            {
                { "key", API_KEY },
                { "orderid", orderId.ToString() },
                { "appid", APP_ID.ToString() },
            };

            HttpContent httpContent = new FormUrlEncodedContent(order);

            HttpResponseMessage response = await mClient.PostAsync("FinalizeTxn/v2/", httpContent);
            if (response.IsSuccessStatusCode)
            {
                TxnApiResponse res = await response.Content.ReadAsAsync<TxnApiResponse>();

                if (res.Response.Result == "OK")
                {
                    return "OK";
                }
                else if (res.Response.Result == "Failure")
                {
                    return res.Response.Error.ErrorCode + ": " + res.Response.Error.ErrorDesc;
                }
            }
            return await response.Content.ReadAsStringAsync();
        }
    }
}
