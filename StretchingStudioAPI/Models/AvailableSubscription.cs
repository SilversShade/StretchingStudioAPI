using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StretchingStudioAPI.Models;

public class AvailableSubscription
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public int SessionsNum { get; set; }
    
    [Column(TypeName = "decimal(7, 2)")]
    public decimal Price { get; set; }
}