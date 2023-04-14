
namespace IntersectSteam.Models
{
    public class ApiError
    {
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }

        public ApiError() { }

        public ApiError(string errorCode, string errorDesc)
        {
            ErrorCode = errorCode;
            ErrorDesc = errorDesc;
        }
    }
}
