using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StretchingStudioAPI.Models;

public class Schedule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public SessionType SessionType { get; set; }
    
    public int DayOfWeek { get; set; }

    public TimeOnly StartingTime { get; set; }
}