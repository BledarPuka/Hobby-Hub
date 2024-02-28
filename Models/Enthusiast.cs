#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SecondExamBledar.Models;

public class Enthusiast
{
    [Key]
    public int EnthusiastId { get; set; }
    public int? UserId { get; set; }
    public int? HobbyId { get; set; }
    public string Proficiency { get; set; }
    public DateTime CreatedAt {get;set;} = DateTime.Now;   
    public DateTime UpdatedAt {get;set;} = DateTime.Now;
    public User? UserEnthusiast { get; set; }
    public Hobby? HobbyEnthusiast { get; set; }

}