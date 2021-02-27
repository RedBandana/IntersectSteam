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
    public class SteamApiService
    {
        private static uint APP_ID = 0;
        private static string BASE_URL = "";
        private static string API_KEY = "";
        private static string[] mPassCodes = { "oQdhkmc4" };

        private static readonly HttpClient mClient = new HttpClient();

        private static bool mInitialized = false;

        /// <summary>
        /// Initialize the client for Steam API requests
        /// </summary>
        /// <param name="apiUrl">Base url for the client calls</param>
        /// <param name="apiKey">Api key for Steam authentification</param>
        /// <param name="appId">Steam app Id</param>
        /// <param name="passCode">Code to use this service</param>
        /// <returns></returns>
        public static IntersectSteamError Init(string apiUrl, string apiKey, uint appId, string passCode)
        {
            APP_ID = appId;
            BASE_URL = apiUrl;
            API_KEY = apiKey;
            try
            {
                if (string.IsNullOrWhiteSpace(passCode) || !mPassCodes.Contains(passCode))
                {
                    return IntersectSteamError.Unauthorized;
                }

                if (string.IsNullOrWhiteSpace(API_KEY))
                {
                    return IntersectSteamError.NoApiKey;
                }

                if (string.IsNullOrWhiteSpace(BASE_URL))
                {
                    return IntersectSteamError.WrongBaseUrl;
                }

                mClient.BaseAddress = new Uri(BASE_URL);
                mClient.DefaultRequestHeaders.Accept.Clear();
                mClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                mInitialized = true;

                if (BASE_URL.ToLowerInvariant().Contains("sandbox"))
                {
                    return IntersectSteamError.NoErrorSandBox;
                }

                return IntersectSteamError.NoError;
            }
            catch (Exception e)
            {
                // Something went wrong!
                return IntersectSteamError.UnKnown;
            }
        }

        /// <summary>
        /// Send order to Steam API
        /// </summary>
        /// <param name="order">Order to send</param>
        /// <param name="steamId">The client id that send the order</param>
        /// <param name="gameLanguage">The language of the client's game</param>
        /// <returns></returns>
        public static async Task<string> SendOrder(Order order, ulong steamId, string gameLanguage)
        {
            if (mInitialized)
            {
                if (order == null)
                {
                    return IntersectSteamError.NoOrder.ToString();
                }

                if (string.IsNullOrWhiteSpace(gameLanguage))
                {
                    return IntersectSteamError.EmptyString.ToString();
                }

                if (steamId == default)
                {
                    return IntersectSteamError.WrongId.ToString();
                }

                order.User = await GetUserInfo(steamId, gameLanguage);
                return await InitTxn(order);
            }
            else
            {
                return IntersectSteamError.Uninitialized.ToString();
            }
        }

        /// <summary>
        /// Get a steam user information
        /// </summary>
        /// <param name="steamId">The client Steam Id</param>
        /// <param name="gameLanguage">The language of the client's game</param>
        /// <returns></returns>
        public static async Task<UserData> GetUserInfo(ulong steamId, string gameLanguage)
        {
            if (string.IsNullOrWhiteSpace(gameLanguage) || steamId == default)
            {
                return null;
            }

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

        /// <summary>
        /// Initialize the transaction of an order
        /// </summary>
        /// <param name="order">The order to initialize the transaction</param>
        /// <returns></returns>
        public static async Task<string> InitTxn(Order order)
        {
            if (order == null)
            {
                return IntersectSteamError.NoOrder.ToString();
            }

            Dictionary<string, string> finalOrder = new Dictionary<string, string>
            {
                { "key", API_KEY },
                { "orderid", order.Id.ToString() },
                { "steamid", order.User.SteamId.ToString() },
                { "appid", APP_ID.ToString() },
                { "itemcount", order.Items.Count.ToString() },
                { "language", order.User.Language },
                { "currency", "USD" } // Automatic conversion on the user side
            };

            for (int i = 0; i < order.Items.Count; i++)
            {
                finalOrder.Add("itemid[" + i + "]", order.Items[i].Id.ToString());
                finalOrder.Add("qty[" + i + "]", order.Items[i].Quantity.ToString());
                finalOrder.Add("amount[" + i + "]", order.Items[i].Amount.ToString());
                finalOrder.Add("description[" + i + "]", order.Items[i].Description.ToString());
            }

            HttpContent httpContent = new FormUrlEncodedContent(finalOrder);

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

        /// <summary>
        /// Finalize an order transaction
        /// </summary>
        /// <param name="orderId">The order Id</param>
        /// <returns></returns>
        public static async Task<string> FinalizeTxn(ulong orderId)
        {
            if (orderId == default)
            {
                IntersectSteamError.WrongId.ToString();
            }

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
