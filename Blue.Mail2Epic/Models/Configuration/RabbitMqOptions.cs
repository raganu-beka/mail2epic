namespace Blue.Mail2Epic.Models.Configuration;

public class RabbitMqOptions
{
    public static readonly string SectionName = "RabbitMq";
    public required string Host { get; init; }
    public required string VirtualHost { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}