using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Vidalink.Core.Data
{
    public interface IAdoExecution : IDisposable
    {
        bool IsConnected { get; }

        IAdoExecution WithCommandType(CommandType commandType);
        IAdoExecution WithParameters(IEnumerable<DbParameter> parameters);
        IAdoExecution WithParameters(params DbParameter[] parameters);
        IAdoExecution ClearParameters();

        bool Execute(string commandText);
        Task<bool> ExecuteAsync(string commandText);
        IEnumerable<TReturnType> Read<TReturnType>(string commandText, Func<DbDataReader, TReturnType> function);
        IEnumerable<TReturnType> Read<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, TReturnType> function);
        TReturnType ReadFirst<TReturnType>(string commandText, Func<DbDataReader, TReturnType> function);
        TReturnType ReadFirst<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, TReturnType> function);
        Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, Func<DbDataReader, TReturnType> function);
        Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, TReturnType> function);
        Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, Func<DbDataReader, Task<TReturnType>> function);
        Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, Task<TReturnType>> function);
        Task<TReturnType> ReadFirstAsync<TReturnType>(string commandText, Func<DbDataReader, TReturnType> function);
        Task<TReturnType> ReadFirstAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, TReturnType> function);
        Task<TReturnType> ReadFirstAsync<TReturnType>(string commandText, Func<DbDataReader, Task<TReturnType>> function);
        Task<TReturnType> ReadFirstAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, Task<TReturnType>> function);

        void Open();
        void Close();
        void Commit();
        void Rollback();
    }
}