using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StretchingStudioAPI.Models;

public class UserSubscription
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public AvailableSubscription Subscription { get; set; } = null!;

    public int SessionsLeft { get; set; }
}