namespace WebApp.Redis.Client.Impl;

public class RedisOptions
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public string? User { get; set; }
    public string? Password { get; set; }
}