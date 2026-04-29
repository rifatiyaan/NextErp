using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Infrastructure;

namespace NextErp.Application.Tests.Infrastructure;

/// <summary>
/// Builds a fresh <see cref="ApplicationDbContext"/> backed by SQLite in-memory.
/// SQLite is closer to SQL Server semantics than the EF in-memory provider
/// (real FK enforcement, real transactions). Keep one connection per test
/// — closing it disposes the database.
/// </summary>
public static class TestDbContextFactory
{
    public sealed record TestContext(ApplicationDbContext Db, SqliteConnection Connection) : IDisposable
    {
        public void Dispose()
        {
            Db.Dispose();
            Connection.Dispose();
        }
    }

    public static TestContext Create(IBranchProvider? branchProvider = null)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        // Use a SQLite-friendly subclass that strips SQL Server-specific column types
        // (e.g. nvarchar(max)) from the model after the production OnModelCreating runs.
        var ctx = new SqliteFriendlyApplicationDbContext(options, branchProvider);
        ctx.Database.EnsureCreated();
        return new TestContext(ctx, connection);
    }

    /// <summary>
    /// Test-only subclass that replaces SQL Server-specific column types with SQLite-compatible
    /// equivalents. Production OnModelCreating runs first; we then sweep for any "nvarchar(max)" /
    /// "varchar(max)" column types and rewrite them to plain "TEXT". This avoids modifying
    /// production code and lets EnsureCreated emit valid SQLite DDL.
    /// </summary>
    private sealed class SqliteFriendlyApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IBranchProvider? branchProvider)
        : ApplicationDbContext(options, branchProvider)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var prop in entityType.GetProperties())
                {
                    var ct = prop.GetColumnType();
                    if (!string.IsNullOrEmpty(ct))
                    {
                        var lower = ct.ToLowerInvariant();
                        if (lower.Contains("(max)") || lower == "nvarchar(max)" || lower == "varchar(max)")
                            prop.SetColumnType("TEXT");
                    }

                    // SQLite doesn't auto-generate rowversion / timestamp columns.
                    // Drop the IsRowVersion / ValueGeneratedOnAddOrUpdate behavior so the test can
                    // supply a byte[] in the builder and have it accepted on insert.
                    if (prop.ClrType == typeof(byte[]) && prop.IsConcurrencyToken)
                    {
                        prop.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
                        prop.IsConcurrencyToken = false;
                    }

                    // SQLite refuses Sum/Avg on decimal columns. EF translates decimal as TEXT,
                    // and even with column type REAL the translator rejects it. Apply a value
                    // converter decimal <-> double so server-side aggregate queries (used by
                    // GetPagedProductsHandler stock aggregation) translate as REAL math.
                    if (prop.ClrType == typeof(decimal))
                    {
                        prop.SetValueConverter(
                            new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<decimal, double>(
                                v => (double)v,
                                v => (decimal)v));
                    }
                    else if (prop.ClrType == typeof(decimal?))
                    {
                        prop.SetValueConverter(
                            new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<decimal?, double?>(
                                v => v.HasValue ? (double?)(double)v.Value : null,
                                v => v.HasValue ? (decimal?)(decimal)v.Value : null));
                    }
                }
            }
        }
    }
}
