using IntersectSteam.Models.Api;

namespace IntersectSteam.Models.Transactions
{
    public class Response
    {
        public string Result { get; set; }
        public IParams Params { get; set; }
        public ApiError Error { get; set; }
        public string GenericError { get; set; }

        public Response() { }

        public Response(string genericError)
        {
            GenericError = genericError;
        }
    }
}
