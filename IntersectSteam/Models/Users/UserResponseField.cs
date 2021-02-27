using IntersectSteam.Models.Errors;

namespace IntersectSteam.Models.Users
{
    public class UserResponseField
    {
        public string Result { get; set; }
        public UserParams Params { get; set; }
        public ApiError Error { get; set; }
    }
}
