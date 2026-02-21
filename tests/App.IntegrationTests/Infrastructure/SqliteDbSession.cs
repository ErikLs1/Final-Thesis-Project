using App.EF;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace App.IntegrationTests.Infrastructure;

public sealed class SqliteDbSession : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext DbContext { get; }

    private SqliteDbSession(SqliteConnection connection, AppDbContext dbContext)
    {
        _connection = connection;
        DbContext = dbContext;
    }

    public static async Task<SqliteDbSession> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        
        // Align SQLite test runtime with PostgreSQL default UUID function used in model mapping.
        connection.CreateFunction("gen_random_uuid", () => Guid.NewGuid().ToString());

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        return new SqliteDbSession(connection, dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
