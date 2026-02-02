namespace TaskFlow.Models.ViewModel;

public class UserPanelModel
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ChangePasswordViewModel? ChangePasswordModel { get; set; }
}
