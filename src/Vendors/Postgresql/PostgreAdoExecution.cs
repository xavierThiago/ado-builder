using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Vidalink.Core.Data.Postgre
{
    public partial class PostgreAdoBuilder
    {
        protected class PostgreAdoExecution : AdoExecution<NpgsqlCommand>
        {
            public PostgreAdoExecution(AdoBuilder builder) : base(builder)
            { }

            private Task<NpgsqlCommand> CreateCommandAsync(string commandText)
            {
                return this.CreateCommandAsync(commandText, this.builder.Parameters);
            }

            private async Task<NpgsqlCommand> CreateCommandAsync(string commandText, IEnumerable<DbParameter> parameters)
            {
                var command = base.CreateCommand(commandText, parameters);
                var (hasPreparedStatement, isParametersRequired) = this.builder.PreparedStatement;

                if (hasPreparedStatement && ((isParametersRequired && command.Parameters.Count > 0) || !isParametersRequired))
                {
                    if (base.builder.CommandType == CommandType.Text)
                    {
                        await command.PrepareAsync();

                        if (!command.IsPrepared)
                        {
                            throw new AdoBuilderException("Prepared statement exception.", new InvalidOperationException("Could not prepare the statement"));
                        }
                    }
                }

                return command;
            }

            /// <summary>
            /// Executes a database non-query command and returns a <see cref="bool"/> status.
            /// </summary>
            /// <param name="commandText">Command text to be executed.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandText"/> is null or empty.</exception>
            /// <exception cref="PostgresException">Thrown when database engine related errors occurs.</exception>
            /// <exception cref="NpgsqlException">Thrown when database driver related errors occurs.</exception>
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
                        return command.ExecuteNonQuery() >= PostgreAdoExecution.NonQuerySuccessfulReturnNumber;
                    }
                    catch (PostgresException postgreException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database engine error; code: p{postgreException.SqlState}.", postgreException);
                    }
                    catch (NpgsqlException npgsqlException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database driver error.", npgsqlException);
                    }
                }
            }

            /// <summary>
            /// Executes a database non-query command and returns a task containing an <see cref="bool"/> status.
            /// </summary>
            /// <param name="commandText">Command text to be executed.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandText"/> is null or empty.</exception>
            /// <exception cref="PostgresException">Thrown when database engine related errors occurs.</exception>
            /// <exception cref="NpgsqlException">Thrown when database driver related errors occurs.</exception>
            /// <returns>Returns a <see cref="IAdoExecution"/> object.</returns>
            public override async Task<bool> ExecuteAsync(string commandText)
            {
                if (string.IsNullOrEmpty(commandText))
                    throw new ArgumentNullException(nameof(commandText));

                using (var command = await this.CreateCommandAsync(commandText))
                {
                    command.CommandTimeout = base.builder.Timeout;

                    try
                    {
                        return await command.ExecuteNonQueryAsync() >= PostgreAdoExecution.NonQuerySuccessfulReturnNumber;
                    }
                    catch (PostgresException postgreException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database engine error; code: p{postgreException.SqlState}.", postgreException);
                    }
                    catch (NpgsqlException npgsqlException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database driver error.", npgsqlException);
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
            /// <exception cref="PostgresException">Thrown when database engine related errors occurs.</exception>
            /// <exception cref="NpgsqlException">Thrown when database driver related errors occurs.</exception>
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
                    catch (PostgresException postgreException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database engine error; code: p{postgreException.SqlState}.", postgreException);
                    }
                    catch (NpgsqlException npgsqlException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database driver error.", npgsqlException);
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
            /// <exception cref="PostgresException">Thrown when database engine related errors occurs.</exception>
            /// <exception cref="NpgsqlException">Thrown when database driver related errors occurs.</exception>
            public override async Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, TReturnType> function)
            {
                if (string.IsNullOrEmpty(commandText))
                    throw new ArgumentNullException(nameof(commandText));
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                using (var command = await this.CreateCommandAsync(commandText, parameters))
                {
                    command.CommandTimeout = base.builder.Timeout;

                    try
                    {
                        var reader = await command.ExecuteReaderAsync();

                        return await base.FetchRowsAsync(reader, function); ;
                    }
                    catch (PostgresException postgreException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database engine error; code: p{postgreException.SqlState}.", postgreException);
                    }
                    catch (NpgsqlException npgsqlException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database driver error.", npgsqlException);
                    }
                }
            }

            public override async Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, Task<TReturnType>> function)
            {
                if (string.IsNullOrEmpty(commandText))
                    throw new ArgumentNullException(nameof(commandText));
                if (function == null)
                    throw new ArgumentNullException(nameof(function));

                using (var command = await this.CreateCommandAsync(commandText, parameters))
                {
                    command.CommandTimeout = base.builder.Timeout;

                    try
                    {
                        var reader = await command.ExecuteReaderAsync();

                        return await base.FetchRowsAsync(reader, function);
                    }
                    catch (PostgresException postgreException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database engine error; code: p{postgreException.SqlState}.", postgreException);
                    }
                    catch (NpgsqlException npgsqlException)
                    {
                        base.Close();

                        throw new AdoBuilderException($"Database driver error.", npgsqlException);
                    }
                }
            }
        }
    }
}
