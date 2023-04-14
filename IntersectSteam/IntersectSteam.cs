using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IntersectSteam.Enums;
using IntersectSteam.Models.Orders;
using IntersectSteam.Models.Transactions;

namespace IntersectSteam
{
    //https://partner.steamgames.com/doc/webapi/
    public static class IntersectSteam
    {
        private static uint APP_ID = 0;
        private static string API_KEY = "";
        private static string BASE_URL = "https://partner.steam-api.com/ISteamMicroTxn/";
        private const string SANDBOX_URL = "https://partner.steam-api.com/ISteamMicroTxnSandbox/";

        private static readonly HttpClient mClient = new HttpClient();
        private static bool mInitialized = false;

        /// <summary>
        /// Initialize the service in order to be able to send requests using a sandbox.
        /// </summary>
        /// <param name="apiKey">Api key for Steam authentification</param>
        /// <param name="appId">Steam app id</param>
        /// <returns>Status of the request.</returns>
        public static RequestStatus InitializeSandbox(string apiKey, uint appId)
        {
            BASE_URL = SANDBOX_URL;
            var status = Initialize(apiKey, appId);

            return status;
        }

        /// <summary>
        /// Initialize the service in order to be able to send requests.
        /// </summary>
        /// <param name="apiKey">Api key for Steam authentification</param>
        /// <param name="appId">Steam app id</param>
        /// <returns>Status of the request.</returns>
        public static RequestStatus Initialize(string apiKey, uint appId)
        {
            if (!CanInitialize(apiKey, appId, out var status))
                return status;

            APP_ID = appId;
            API_KEY = apiKey;
            InitializeClient();

            mInitialized = true;

            return status;
        }

        /// <summary>
        /// Send order request
        /// </summary>
        /// <param name="request">The request containing all the order information</param>
        /// <returns>Response of the request</returns>
        public static async Task<string> SendOrder(OrderRequest request)
        {
            var status = CanSendOrder(request);
            if (status != RequestStatus.Success)
                return status.ToString();

            var transactionRequest = await GetTransactionRequest(request);
            var requestResponse = await InitializeTransaction(transactionRequest);

            return requestResponse;
        }

        /// <summary>
        /// Finalize an order transaction
        /// </summary>
        /// <param name="orderId">The order Id</param>
        /// <returns>Response of the request</returns>
        public static async Task<string> FinalizeTransaction(ulong orderId)
        {
            var status = CanFinalizeTransaction(orderId);
            if (status != RequestStatus.Success)
                return status.ToString();

            var transactionParams = GetFinalizeTransactionParams(orderId);
            var httpContent = (HttpContent)new FormUrlEncodedContent(transactionParams);
            var response = await mClient.PostAsync("FinalizeTxn/v2/", httpContent);
            var responseStatus = await GetResponseStatus(response);

            return responseStatus;
        }

        private static bool CanInitialize(string apiKey, uint appId, out RequestStatus status)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                status = RequestStatus.InvalidApiKey;
                return false;
            }

            if (appId == default)
            {
                status = RequestStatus.InvalidAppId;
                return false;
            }

            status = RequestStatus.Success;
            return true;
        }

        private static RequestStatus CanSendOrder(OrderRequest request)
        {
            if (!mInitialized)
                return RequestStatus.Uninitialized;

            if (request == null)
                return RequestStatus.NoOrder;

            if (string.IsNullOrWhiteSpace(request.Language))
                return RequestStatus.EmptyString;

            if (request.SteamId == default)
                return RequestStatus.InvalidSteamId;

            return RequestStatus.Success;
        }

        private static RequestStatus CanFinalizeTransaction(ulong orderId)
        {
            if (!mInitialized)
                return RequestStatus.Uninitialized;

            if (orderId == default)
                RequestStatus.InvalidOrderId.ToString();

            return RequestStatus.Success;
        }

        private static void InitializeClient()
        {
            mClient.BaseAddress = new Uri(BASE_URL);
            mClient.DefaultRequestHeaders.Accept.Clear();
            mClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        }

        private static async Task<string> InitializeTransaction(TransactionRequest request)
        {
            if (request == null)
                return RequestStatus.NoOrder.ToString();

            var transactionParams = GetInitializeTransactionParams(request);
            var httpContent = (HttpContent) new FormUrlEncodedContent(transactionParams);
            var response = await mClient.PostAsync("InitTxn/v3/", httpContent);
            var responseStatus = await GetResponseStatus(response);

            return responseStatus;
        }

        private static string GetIso639Language(string language)
        {
            var iso639Language = Iso639.Language.FromName(language, true)
                        .Where(l => !string.IsNullOrEmpty(l.Part1) && l.Type == Iso639.LanguageType.Living)
                        .FirstOrDefault();

            if (iso639Language != null)
                return iso639Language.Part1;
            else
                return "en";
        }

        private static async Task<TransactionRequest> GetTransactionRequest(OrderRequest request)
        {
            var transactionRequest = new TransactionRequest();
            transactionRequest.SteamId = request.SteamId;
            transactionRequest.Order = request.Order;
            transactionRequest.Currency = request.Currency;
            transactionRequest.Language = GetIso639Language(request.Language);

            return transactionRequest;
        }

        private static async Task<string> GetResponseStatus(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                TransactionResponse res = await response.Content.ReadAsAsync<TransactionResponse>();

                if (res.Response.Result == "OK")
                    return "OK";
                else if (res.Response.Result == "Failure")
                    return res.Response.Error.ErrorCode + ": " + res.Response.Error.ErrorDesc;
            }

            return await response.Content.ReadAsStringAsync();
        }

        private static Dictionary<string, string> GetInitializeTransactionParams(TransactionRequest request)
        {
            var transactionParams = new Dictionary<string, string>
            {
                { "key", API_KEY },
                { "appid", APP_ID.ToString() },
                { "currency", request.Currency }, // Automatic conversion on the user side
                { "steamid", request.SteamId.ToString() },
                { "language", request.Language },
                { "orderid", request.Order.Id.ToString() },
                { "itemcount", request.Order.Items.Count.ToString() }
            };

            for (int i = 0; i < request.Order.Items.Count; i++)
            {
                transactionParams.Add("itemid[" + i + "]", request.Order.Items[i].Id.ToString());
                transactionParams.Add("qty[" + i + "]", request.Order.Items[i].Quantity.ToString());
                transactionParams.Add("amount[" + i + "]", request.Order.Items[i].Amount.ToString());
                transactionParams.Add("description[" + i + "]", request.Order.Items[i].Description.ToString());
            }

            return transactionParams;
        }

        private static Dictionary<string, string> GetFinalizeTransactionParams(ulong orderId)
        {
            var transactionParams = new Dictionary<string, string>
            {
                { "key", API_KEY },
                { "appid", APP_ID.ToString() },
                { "orderid", orderId.ToString() }
            };

            return transactionParams;
        }
    }
}
