using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class Comment
{
    public int Id { get; init; }
    [MaxLength(800)]
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int AuthorId { get; set; }
    public ApplicationUser Author { get; set; } = null!;
}
