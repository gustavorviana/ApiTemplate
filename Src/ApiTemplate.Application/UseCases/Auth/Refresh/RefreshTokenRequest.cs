namespace ApiTemplate.Application.UseCases.Auth.Refresh
{
    public class RefreshTokenRequest
    {
        public int UserId { get; set; }
        public required string RefreshToken { get; set; }
    }
}
