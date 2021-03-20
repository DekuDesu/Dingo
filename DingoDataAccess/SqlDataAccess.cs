using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DingoDataAccess
{
    public class SqlDataAccess : ISqlDataAccess
    {

        private readonly IConfiguration _config;
        private readonly ILogger<SqlDataAccess> logger;

        public string ConnectionStringName { get; set; } = "Default";

        private string ConnectionString => _config?.GetConnectionString(ConnectionStringName) ?? string.Empty;

        public SqlDataAccess(IConfiguration config, ILogger<SqlDataAccess> _logger)
        {
            this._config = config;
            logger = _logger;
        }

        private async Task<List<T>> LoadData<T, U>(string query, U parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                logger.LogInformation("Executing SQL Query ({MethodName}: {Parameters}) {Query}", nameof(LoadData), query, parameters);

                var result = await connection.QueryAsync<T>(query, parameters);

                return result.ToList();
            }
        }

        public async Task ExecuteVoidProcedure<U>(string query, U parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                logger.LogInformation("Executing Query ({MethodName}: {Parameters}) {Query}", nameof(ExecuteVoidProcedure), query, parameters);

                await connection.ExecuteAsync(query, parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public async Task<List<T>> ExecuteProcedure<T, U>(string query, U parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                logger.LogInformation("Executing Query ({MethodName}: {Parameters}) {Query}", nameof(ExecuteProcedure), query, parameters);

                var result = await connection.QueryAsync<T>(query, parameters, commandType: System.Data.CommandType.StoredProcedure);

                return result.ToList();
            }
        }

        public async Task<T> ExecuteSingleProcedure<T, U>(string query, U parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                logger.LogInformation("Executing Query ({MethodName}: {Parameters}) {Query}", nameof(ExecuteProcedure), query, parameters);

                var result = default(T);

                try
                {
                    result = await connection.QueryFirstAsync<T>(query, parameters, commandType: System.Data.CommandType.StoredProcedure);
                }
                catch (InvalidOperationException)
                {
                    logger.LogInformation("Query returned no members ({MethodName}: {Parameters}) {Query}", nameof(ExecuteProcedure), query, parameters);
                }

                return result;
            }
        }
    }
}
