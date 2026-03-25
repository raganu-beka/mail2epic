using System.ComponentModel.DataAnnotations;

namespace Blue.Mail2Epic.Infrastructure.Models;

public class JobExecutionInfo
{
    public int Id { get; set; }
    [MaxLength(50)] public required string JobName { get; set; }
    public DateTimeOffset LastExecutionTime { get; set; }
}