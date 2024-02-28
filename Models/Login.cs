#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SecondExamBledar.Models;
public class Login
{    
    [Required(ErrorMessage = "Email is required")]
    public string LoginUserName { get; set; }    
    
    [Required(ErrorMessage = "The Password is required")]
    [DataType(DataType.Password)]
    public string LoginPassword { get; set; } 
}