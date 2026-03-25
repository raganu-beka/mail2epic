using System.ComponentModel.DataAnnotations;
using Pgvector;

namespace Blue.Mail2Epic.Infrastructure.Models;

public class Epic
{
    public int Id { get; set; }
    [MaxLength(50)] public required string Key { get; set; }
    [MaxLength(200)] public string? Summary { get; set; }
    [MaxLength(5000)] public string? Description { get; set; }
    [MaxLength(200)] public string? ContactPersonEmail { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Vector? Embedding { get; set; }
}