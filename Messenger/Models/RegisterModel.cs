using System.ComponentModel.DataAnnotations;
using Messenger.Entities;

namespace Messenger.Models
{
    public class RegisterModel
    {
        [Required]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "Подтверждение пароля")]
        public string PasswordConfirm { get; set; }
        public File Avatar { get; set; }
    }
}