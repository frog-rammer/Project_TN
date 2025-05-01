namespace GameServer.Models.Responses
{
    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Currency { get; set; }
        public int PremiumCurrency { get; set; }
    }
} 