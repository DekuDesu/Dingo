using System.Collections.Generic;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public interface ISqlDataAccess
    {
        /// <summary>
        /// The connection string used for this to access a specific database
        /// </summary>
        string ConnectionStringName { get; set; }

        /// <summary>
        /// Executes a stored procedure that has no return value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task ExecuteVoidProcedure<U>(string query, U parameters);

        /// <summary>
        /// Executes a stored procedure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<List<T>> ExecuteProcedure<T, U>(string query, U parameters);

        /// <summary>
        /// Executes a stored procedure that returns a single row
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<T> ExecuteSingleProcedure<T, U>(string query, U parameters);
    }
}