using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using AdoBuilder.Core;
using Oracle.ManagedDataAccess.Client;

namespace AdoBuilder.Oracle
{
    public partial class OracleAdoBuilder
    {
        protected class OracleAdoExecution : AdoExecution<OracleCommand>
        {
            public OracleAdoExecution(AdoBuilderBase builder) : base(builder)
            { }

            /// <summary>
            /// Executes a database non-query command and returns a <see cref="bool"/> status.
            /// </summary>
            /// <param name="commandText">Command text to be executed.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandText"/> is null or empty.</exception>
            /// <exception cref="OracleException">Thrown when database related errors occurs.</exception>
            /// <returns>Returns a <see cref="IAdoExecution"/> object.</returns>
            public override bool Execute(string commandText)
            {
                if (string.IsNullOrEmpty(commandText))
                    throw new ArgumentNullException(nameof(commandText));

                using (var command = base.CreateCommand(commandText))
                {
                    command.CommandTimeout = base.builder.Timeout;

                    try
                    {
                        return command.ExecuteNonQuery() >= OracleAdoExecution.NonQuerySuccessfulReturnNumber;
                    }
                    catch (OracleException oracleException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database error; code: o{oracleException.Number}.", oracleException);
                    }
                }
            }

            /// <summary>
            /// Executes a database non-query command and returns a task containing an <see cref="bool"/> status.
            /// </summary>
            /// <param name="commandText">Command text to be executed.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandText"/> is null or empty.</exception>
            /// <exception cref="OracleException">Thrown when database related errors occurs.</exception>
            /// <returns>Returns a <see cref="IAdoExecution"/> object.</returns>
            public override async Task<bool> ExecuteAsync(string commandText)
            {
                if (string.IsNullOrEmpty(commandText))
                    throw new ArgumentNullException(nameof(commandText));

                using (var command = base.CreateCommand(commandText))
                {
                    command.CommandTimeout = base.builder.Timeout;

                    try
                    {
                        return await command.ExecuteNonQueryAsync() >= OracleAdoExecution.NonQuerySuccessfulReturnNumber;
                    }
                    catch (OracleException oracleException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database error; code: o{oracleException.Number}.", oracleException);
                    }
                }
            }

            /// <summary>
            /// Executes a database command and returns an <see cref="IEnumerable{TReturnType}"/>.
            /// </summary>
            /// <typeparam name="TReturnType">Any type.</typeparam>
            /// <param name="commandText">Command text to be executed.</param>
            /// <param name="parameters">Database parameters to be prepared into command text.</param>
            /// <param name="function">A function that will be called for each row fetched from database.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandText"/> is null or empty.</exception>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="function"/> is null or empty.</exception>
            /// <exception cref="OracleException">Thrown when database related errors occurs.</exception>
            /// <returns>Returns a <see cref="IAdoExecution"/> object.</returns>
            public override IEnumerable<TReturnType> Read<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, TReturnType> function)
            {
                if (string.IsNullOrEmpty(commandText))
                    throw new ArgumentNullException(nameof(commandText));
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                using (var command = base.CreateCommand(commandText, parameters))
                {
                    command.CommandTimeout = base.builder.Timeout;

                    try
                    {
                        var reader = command.ExecuteReader();

                        return base.FetchRows(reader, function);
                    }
                    catch (OracleException oracleException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database error; code: o{oracleException.Number}.", oracleException);
                    }
                }
            }

            /// <summary>
            /// Executes a database command and returns a task containing an <see cref="IEnumerable{TReturnType}"/>.
            /// </summary>
            /// <typeparam name="TReturnType">Any type.</typeparam>
            /// <param name="commandText">Command text to be executed.</param>
            /// <param name="parameters">Database parameters to be prepared into command text.</param>
            /// <param name="function">A function that will be called for each row fetched from database.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandText"/> is null or empty.</exception>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="function"/> is null or empty.</exception>
            /// <exception cref="OracleException">Thrown when database related errors occurs.</exception>
            /// <returns>Returns a <see cref="IAdoExecution"/> object.</returns>
            public override async Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, TReturnType> function)
            {
                if (string.IsNullOrEmpty(commandText))
                    throw new ArgumentNullException(nameof(commandText));
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                using (var command = base.CreateCommand(commandText, parameters))
                {
                    command.CommandTimeout = base.builder.Timeout;

                    try
                    {
                        var reader = await command.ExecuteReaderAsync();

                        return await base.FetchRowsAsync(reader, function);
                    }
                    catch (OracleException oracleException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database error; code: o{oracleException.Number}.", oracleException);
                    }
                }
            }

            /// <summary>
            /// Executes a database command and returns a task containing an <see cref="IEnumerable{TReturnType}"/>.
            /// </summary>
            /// <typeparam name="TReturnType">Any type.</typeparam>
            /// <param name="commandText">Command text to be executed.</param>
            /// <param name="parameters">Database parameters to be prepared into command text.</param>
            /// <param name="function">A async function that will be called for each row fetched from database.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandText"/> is null or empty.</exception>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="function"/> is null or empty.</exception>
            /// <exception cref="OracleException">Thrown when database related errors occurs.</exception>
            /// <returns>Returns a <see cref="IAdoExecution"/> object.</returns>
            public override async Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, Task<TReturnType>> function)
            {
                if (string.IsNullOrEmpty(commandText))
                    throw new ArgumentNullException(nameof(commandText));
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                using (var command = base.CreateCommand(commandText, parameters))
                {
                    command.CommandTimeout = base.builder.Timeout;

                    try
                    {
                        var reader = await command.ExecuteReaderAsync();

                        return await base.FetchRowsAsync(reader, function);
                    }
                    catch (OracleException oracleException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database error; code: o{oracleException.Number}.", oracleException);
                    }
                }
            }
        }
    }
}
