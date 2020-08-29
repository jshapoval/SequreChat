namespace Messenger.Common.DTOs
{
    public class AuthObject
    {
        public string Token { get; set; }
        public bool Success { get; set; }
        public string ErrorText { get; set; }
        public UserObject User { get; set; }
        public class UserObject
        {
            public string Id { get; set; }
            public string Email { get; set; }
        }
        public AuthObject (string id, string email, string token)
        {
            Success = true;

            Token = token;

            User = new UserObject
            {
                Id = id,
                Email = email
            };
        }

        public AuthObject(string exception)
        {
            Success = false;
            ErrorText = exception;
        }

        public AuthObject()
        {
        }
    }
}
