using IntersectSteam.Models.Errors;

namespace IntersectSteam.Models.Transactions
{
    public class Response
    {
        public string Result { get; set; }
        public ApiError Error { get; set; }

        public Response() { }

        public Response(string result, ApiError error)
        {
            Result = result;
            Error = error;
        }
    }
}
