#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SecondExamBledar.Models;
public class Hobby
{    
    [Key]    
    public int HobbyId { get; set; }
    public int? UserId { get; set; }
    public User? HobbyCreator { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }    
    
    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } 
    public List<Enthusiast>? EnthusiastList { get; set; } = new List<Enthusiast>();

    public DateTime CreatedAt {get;set;} = DateTime.Now;   
    public DateTime UpdatedAt {get;set;} = DateTime.Now;
}