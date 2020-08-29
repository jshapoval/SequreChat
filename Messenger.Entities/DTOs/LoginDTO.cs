namespace Messenger.Common.DTOs
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public LoginDTO()
        {
        }
        public LoginDTO(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
