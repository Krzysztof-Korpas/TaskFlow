using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models.ViewModel;

public class LoginViewModel
{
    [Required(ErrorMessage = "Validation.EmailRequired")]
    [EmailAddress]
    [Display(Name = "Auth.Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Validation.PasswordRequired")]
    [DataType(DataType.Password)]
    [Display(Name = "Auth.Password")]
    public string? Password { get; set; }

    [Display(Name = "Auth.RememberMe")]
    public bool RememberMe { get; set; }
}
