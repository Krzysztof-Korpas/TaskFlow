using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models.ViewModel;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Validation.EmailRequired")]
    [EmailAddress]
    [Display(Name = "Auth.Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Validation.PasswordRequired")]
    [StringLength(100, ErrorMessage = "Validation.PasswordLength", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Auth.Password")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Auth.ConfirmPassword")]
    [Compare("Password", ErrorMessage = "Validation.PasswordsDoNotMatch")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "Auth.DisplayName")]
    [StringLength(100)]
    public string? DisplayName { get; set; }
}
