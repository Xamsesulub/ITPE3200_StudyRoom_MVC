using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class LoginViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }

    // Strongly-typed ReturnUrl property
    public string? ReturnUrl { get; set; }
}