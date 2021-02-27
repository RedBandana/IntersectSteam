
namespace IntersectSteam.Models.Users
{
    public class UserData
    {
        public ulong SteamId { get; set; }
        public UserParams ApiInfo { get; set; }
        public string Language { get; set; }
        public string ErrorMsg { get; set; }
    }
}
