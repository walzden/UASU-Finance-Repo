using System.Data;
using Microsoft.Data.SqlClient;

namespace UASU_VoucherApprovals.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

// Thin factory around the connection string in appsettings.json.
// Services open a connection per call and let Dapper/using dispose it -
// no connection pooling logic needed here, ADO.NET already pools
// under the hood.
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("UasuFinance")
            ?? throw new InvalidOperationException("Connection string 'UasuFinance' not found in appsettings.json.");
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
