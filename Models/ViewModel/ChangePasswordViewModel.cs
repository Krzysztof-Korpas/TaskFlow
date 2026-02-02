using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models.ViewModel;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Validation.CurrentPasswordRequired")]
    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }
    
    [Required(ErrorMessage = "Validation.PasswordRequired")]
    [StringLength(100, ErrorMessage = "Validation.PasswordLength", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }
    
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Validation.PasswordsDoNotMatch")]
    public string? ConfirmPassword { get; set; }
}
