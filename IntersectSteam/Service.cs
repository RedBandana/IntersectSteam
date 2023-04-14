using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IntersectSteam.Enums;
using IntersectSteam.Models.Orders;
using IntersectSteam.Models.Requests;
using IntersectSteam.Models.Transactions;
using Newtonsoft.Json;

namespace IntersectSteam
{
    //https://partner.steamgames.com/doc/webapi/ISteamMicroTxn
    public static class Service
    {
        private static uint AppId = 0;
        private static string ApiKey = "";
        private static string BaseUrl = "https://partner.steam-api.com/ISteamMicroTxn/";
        private const string SANDBOX_URL = "https://partner.steam-api.com/ISteamMicroTxnSandbox/";

        private static readonly HttpClient mClient = new HttpClient();
        private static bool mInitialized = false;

        /// <summary>
        /// Initialize the service in order to be able to send requests using a sandbox.
        /// </summary>
        /// <param name="apiKey">Api key for Steam authentification.</param>
        /// <param name="appId">Steam app id.</param>
        /// <returns>Status of the request.</returns>
        public static RequestStatus InitializeSandbox(string apiKey, uint appId)
        {
            BaseUrl = SANDBOX_URL;
            var status = Initialize(apiKey, appId);

            return status;
        }

        /// <summary>
        /// Initialize the service in order to be able to send requests.
        /// </summary>
        /// <param name="apiKey">Api key for Steam authentification.</param>
        /// <param name="appId">Steam app id.</param>
        /// <returns>Status of the request.</returns>
        public static RequestStatus Initialize(string apiKey, uint appId)
        {
            if (!CanInitialize(apiKey, appId, out var status))
                return status;

            AppId = appId;
            ApiKey = apiKey;
            InitializeClient();

            mInitialized = true;

            return status;
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

        private static void InitializeClient()
        {
            mClient.BaseAddress = new Uri(BaseUrl);
            mClient.DefaultRequestHeaders.Accept.Clear();
            mClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        }

        /// <summary>
        /// Retrieves details for a user's purchasing info.
        /// </summary>
        /// <param name="steamId">User Steam Id.</param>
        /// <param name="ipaddress"></param>
        /// <returns></returns>
        public static async Task<Response> GetUser(ulong steamId, string ipaddress)
        {
            var status = CanGetUser(steamId);
            if (status != RequestStatus.Success)
                return new Response(status.ToString());
            var response = await GetUserInfo(new UserInfoRequest(ApiKey, AppId, steamId, ipaddress));
            return response;
        }

        private static RequestStatus CanGetUser(ulong steamId)
        {
            if (!mInitialized)
                return RequestStatus.Uninitialized;

            if (steamId == default)
                RequestStatus.InvalidSteamId.ToString();

            return RequestStatus.Success;
        }

        private static async Task<Response> GetUserInfo(UserInfoRequest request)
        {
            var httpContent = (HttpContent)new FormUrlEncodedContent(request.ToDictionary());
            var responseMessage = await mClient.PostAsync("GetUserInfo/v2/", httpContent);
            var responseStatus = await GetResponseStatus(responseMessage);

            return responseStatus;
        }

        /// <summary>
        /// Send Steam order request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<Response> SendOrder(OrderRequest request)
        {
            var status = CanSendOrder(request);
            if (status != RequestStatus.Success)
                return new Response(status.ToString());

            var transactionRequest = request.ToTransactionRequest();
            var requestResponse = await InitializeTransaction(transactionRequest);

            return requestResponse;
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

        private static async Task<Response> InitializeTransaction(InitializeTransactionRequest request)
        {
            var httpContent = (HttpContent) new FormUrlEncodedContent(request.ToDictionary());
            var responseMessage = await mClient.PostAsync("InitTxn/v3/", httpContent);
            var responseStatus = await GetResponseStatus(responseMessage);

            return responseStatus;
        }

        /// <summary>
        /// Completes a purchase that was started by the InitTxn API.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static async Task<Response> FinalizeTransaction(ulong orderId)
        {
            var status = CanFinalizeTransaction(orderId);
            if (status != RequestStatus.Success)
                return new Response(status.ToString());

            var request = new FinalizeTransactionRequest(ApiKey, orderId, AppId);
            var httpContent = (HttpContent)new FormUrlEncodedContent(request.ToDictionary());
            var response = await mClient.PostAsync("FinalizeTxn/v2/", httpContent);
            var responseStatus = await GetResponseStatus(response);

            return responseStatus;
        }

        private static RequestStatus CanFinalizeTransaction(ulong orderId)
        {
            if (!mInitialized)
                return RequestStatus.Uninitialized;

            if (orderId == default)
                RequestStatus.InvalidOrderId.ToString();

            return RequestStatus.Success;
        }

        private static async Task<Response> GetResponseStatus(HttpResponseMessage responseMessage)
        {
            var response = new Response();
            if (responseMessage.IsSuccessStatusCode)
            {
                response = await responseMessage.Content.ReadAsAsync<Response>(
                    new[] { new JsonMediaTypeFormatter() { SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All } } });
            }

            response.GenericError = await responseMessage.Content.ReadAsStringAsync();
            return response;
        }
    }
}
