using Microsoft.Data.SqlClient;

namespace Subscrio.Abp.Sample;

public static class LocalDbDatabase
{
    private static string DatabaseName => Environment.GetEnvironmentVariable("SUBSCRIO_SAMPLE_DATABASE") ?? "SubscrioAbpSample";
    private static string LocalDbServer => Environment.GetEnvironmentVariable("SUBSCRIO_SAMPLE_SQLSERVER") ?? @"(localdb)\MSSQLLocalDB";
    private static bool UsesLocalDb => LocalDbServer.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase);

    public static string DataFilePath => Path.Combine(GetDataDirectory(), "subscrio-abp.mdf");

    private static string LogFilePath => Path.Combine(GetDataDirectory(), "subscrio-abp_log.ldf");

    public static string ConnectionString => new SqlConnectionStringBuilder
    {
        DataSource = LocalDbServer,
        InitialCatalog = DatabaseName,
        IntegratedSecurity = true,
        TrustServerCertificate = true,
        MultipleActiveResultSets = true,
        ConnectTimeout = 30
    }.ConnectionString;

    public static async Task EnsureCreatedAsync()
    {
        if (UsesLocalDb) Directory.CreateDirectory(GetDataDirectory());

        var masterConnectionString = new SqlConnectionStringBuilder(ConnectionString)
        {
            InitialCatalog = "master"
        }.ConnectionString;

        await using var connection = new SqlConnection(masterConnectionString);

        try
        {
            await connection.OpenAsync();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "Cannot connect to SQL Server. Install LocalDB or set SUBSCRIO_SAMPLE_SQLSERVER to an accessible instance.",
                exception);
        }

        await using var existsCommand = new SqlCommand(
            "SELECT CASE WHEN DB_ID(@databaseName) IS NULL THEN 0 ELSE 1 END",
            connection);
        existsCommand.Parameters.AddWithValue("@databaseName", DatabaseName);

        if (Convert.ToInt32(await existsCommand.ExecuteScalarAsync()) == 1)
        {
            Console.WriteLine($"Database: {DatabaseName}");
            return;
        }

        var dataFile = EscapeSqlLiteral(DataFilePath);
        var logFile = EscapeSqlLiteral(LogFilePath);
        var databaseIdentifier = EscapeSqlIdentifier(DatabaseName);

        var createSql = !UsesLocalDb ? $"CREATE DATABASE [{databaseIdentifier}]" : File.Exists(DataFilePath)
            ? $"CREATE DATABASE [{databaseIdentifier}] ON " +
              $"(FILENAME = N'{dataFile}'), (FILENAME = N'{logFile}') FOR ATTACH"
            : $"CREATE DATABASE [{databaseIdentifier}] ON PRIMARY " +
              $"(NAME = N'{databaseIdentifier}', FILENAME = N'{dataFile}') " +
              $"LOG ON (NAME = N'{databaseIdentifier}_log', FILENAME = N'{logFile}')";

        await using var createCommand = new SqlCommand(createSql, connection);
        await createCommand.ExecuteNonQueryAsync();

        Console.WriteLine($"Database: {DatabaseName}");
    }

    private static string GetDataDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Subscrio.Abp.Sample.slnx")))
            {
                return Path.Combine(directory.FullName, "data");
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "data");
    }

    private static string EscapeSqlLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);

    private static string EscapeSqlIdentifier(string value) => value.Replace("]", "]]", StringComparison.Ordinal);
}
