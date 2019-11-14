using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace AdoBuilder.Core
{
    public interface IAdoBuilder : IDisposable
    {
        IAdoBuilder WithCommand(string command);
        IAdoBuilder WithCommand(string command, CommandType commandType);
        IAdoBuilder WithCommandType(CommandType commandType);
        IAdoBuilder WithParameters(IEnumerable<DbParameter> parameters);
        IAdoBuilder WithParameters(params DbParameter[] parameters);
        IAdoBuilder WithPreparedStatement();
        IAdoBuilder WithPreparedStatement(bool requireParameters);
        IAdoBuilder WithTimeout(int timeout);
        IAdoBuilder WithTransaction();
        IAdoExecution Build();
        Task<IAdoExecution> BuildAsync();
    }
}
