namespace Messenger.Common.DTOs
{
    public class RegisterModelDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public FileDTO Avatar { get; set; }
        public bool HasAvatar { get; set; }
    }
}
