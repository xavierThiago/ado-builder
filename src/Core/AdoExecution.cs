using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Vidalink.Core.Data
{
    public abstract class AdoExecution<TCommand> : IAdoExecution
        where TCommand : DbCommand, new()
    {
        protected const int NonQuerySuccessfulReturnNumber = 1;

        private bool _hasBeenDisposed = false;
        protected AdoBuilder builder;

        public bool IsConnected
        {
            get
            {
                return this.builder?.Connection?.State == ConnectionState.Open;
            }
        }

        public AdoExecution(AdoBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            this.builder = builder;
        }

        private void AddParametersIntoCommand(TCommand command)
        {
            this.AddParametersIntoCommand(command, this.builder.Parameters);
        }

        private void AddParametersIntoCommand(TCommand command, IEnumerable<DbParameter> parameters)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (parameters != null)
            {
                var enumerator = parameters.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    command.Parameters.Add(enumerator.Current);
                }
            }
        }

        protected TCommand CreateCommand(string commandText)
        {
            return this.CreateCommand(commandText, this.builder.Parameters);
        }

        protected TCommand CreateCommand(string commandText, IEnumerable<DbParameter> parameters)
        {
            var command = new TCommand
            {
                CommandText = commandText,
                Connection = this.builder.Connection,
                CommandType = this.builder.CommandType
            };

            this.AddParametersIntoCommand(command, parameters);

            var (hasPreparedStatement, isParametersRequired) = this.builder.PreparedStatement;

            if (hasPreparedStatement && ((isParametersRequired && command.Parameters.Count > 0) || !isParametersRequired))
            {
                return this.CreatePreparedCommand(command);
            }

            return command;
        }

        protected virtual TCommand CreatePreparedCommand(string commandText)
        {
            return this.CreatePreparedCommand(commandText, this.builder.Parameters);
        }

        protected virtual TCommand CreatePreparedCommand(string commandText, IEnumerable<DbParameter> parameters)
        {
            var command = this.CreateCommand(commandText, parameters);

            return this.CreatePreparedCommand(command);
        }

        protected virtual TCommand CreatePreparedCommand(TCommand command)
        {
            if (this.builder.CommandType == CommandType.Text)
            {
                command.Prepare();
            }

            return command;
        }

        protected List<TReturnType> FetchRows<TReturnType>(DbDataReader reader, Func<DbDataReader, TReturnType> function) /*where TEntity : IDataEntity*/
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            if (reader.HasRows)
            {
                var result = new List<TReturnType>();

                while (reader.Read())
                {
                    result.Add(function.Invoke(reader));
                }

                reader.Close();

                return result;
            }

            return null;
        }

        protected async Task<List<TReturnType>> FetchRowsAsync<TReturnType>(DbDataReader reader, Func<DbDataReader, TReturnType> function) /*where TReturnType : IDataEntity*/
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            if (reader.HasRows)
            {
                var result = new List<TReturnType>();

                while (await reader.ReadAsync())
                {
                    result.Add(function.Invoke(reader));
                }

                reader.Close();

                return result;
            }

            return null;
        }

        protected async Task<List<TReturnType>> FetchRowsAsync<TReturnType>(DbDataReader reader, Func<DbDataReader, Task<TReturnType>> function) /*where TReturnType : IDataEntity*/
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            if (reader.HasRows)
            {
                var result = new List<TReturnType>();

                while (await reader.ReadAsync())
                {
                    result.Add(await function.Invoke(reader));
                }

                reader.Close();

                return result;
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._hasBeenDisposed)
            {
                this.builder.Dispose();

                this._hasBeenDisposed = true;
            }
        }

        public IAdoExecution WithCommandType(CommandType commandType)
        {
            if (commandType == CommandType.TableDirect)
                throw new NotSupportedException("Only text and stored procedure commands are currently supported.");

            this.builder.CommandType = commandType;

            return this;
        }

        public IAdoExecution ClearParameters()
        {
            this.builder.Parameters = null;

            return this;
        }

        public void Open()
        {
            if (this.builder.Connection?.State == ConnectionState.Closed ||
                    this.builder.Connection?.State == ConnectionState.Broken)
            {
                this.builder.Connection.Open();
            }
        }

        public Task OpenAsync()
        {
            if (this.builder.Connection?.State == ConnectionState.Closed ||
                    this.builder.Connection?.State == ConnectionState.Broken)
            {
                return this.builder.Connection.OpenAsync();
            }

            return Task.CompletedTask;
        }

        public void Close()
        {
            if (this.builder.Connection?.State != ConnectionState.Closed)
            {
                this.builder.Connection.Close();
            }
        }

        public void Commit()
        {
            if (this.builder.Transaction?.Connection.State == ConnectionState.Open)
            {
                this.builder.Transaction.Commit();
            }
        }

        public void Rollback()
        {
            if (this.builder.Transaction?.Connection.State == ConnectionState.Open)
            {
                this.builder.Transaction.Rollback();
            }
        }

        /// <summary>
        /// Executes a database command and returns an <see cref="IEnumerable{TReturnType}"/>.
        /// </summary>
        /// <typeparam name="TReturnType">Any type.</typeparam>
        /// <param name="commandText">Command text to be executed.</param>
        /// <param name="function">A function that will be called for each row fetched from database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandText"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="function"/> is null or empty.</exception>
        /// <exception cref="PostgresException">Thrown when database engine related errors occurs.</exception>
        /// <exception cref="NpgsqlException">Thrown when database driver related errors occurs.</exception>
        /// <returns>Returns a <see cref="IAdoExecution"/> object.</returns>
        public IEnumerable<TReturnType> Read<TReturnType>(string commandText, Func<DbDataReader, TReturnType> function) => this.Read(commandText, this.builder.Parameters, function);

        public virtual TReturnType ReadFirst<TReturnType>(string commandText, Func<DbDataReader, TReturnType> function) => this.ReadFirst(commandText, this.builder.Parameters, function);

        public TReturnType ReadFirst<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, TReturnType> function)
        {
            var entities = this.Read<TReturnType>(commandText, parameters, function);

            if (entities != null)
            {
                var enumerator = entities.GetEnumerator();

                enumerator.MoveNext();

                return enumerator.Current;
            }

            return default(TReturnType);
        }

        /// <summary>
        /// Executes a database command and returns a task containing an <see cref="IEnumerable{TReturnType}"/>.
        /// </summary>
        /// <typeparam name="TReturnType">Any type.</typeparam>
        /// <param name="commandText">Command text to be executed.</param>
        /// <param name="function">A function that will be called for each row fetched from database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandText"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="function"/> is null or empty.</exception>
        /// <exception cref="PostgresException">Thrown when database engine related errors occurs.</exception>
        /// <exception cref="NpgsqlException">Thrown when database driver related errors occurs.</exception>
        public Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, Func<DbDataReader, TReturnType> function) => this.ReadAsync(commandText, this.builder.Parameters, function);

        /// <summary>
        /// Executes a database command and returns a task containing an <see cref="IEnumerable{TReturnType}"/>.
        /// </summary>
        /// <typeparam name="TReturnType">Any type.</typeparam>
        /// <param name="commandText">Command text to be executed.</param>
        /// <param name="function">A function that will be called for each row fetched from database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandText"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="function"/> is null or empty.</exception>
        /// <exception cref="PostgresException">Thrown when database engine related errors occurs.</exception>
        /// <exception cref="NpgsqlException">Thrown when database driver related errors occurs.</exception>
        public Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, Func<DbDataReader, Task<TReturnType>> function) => this.ReadAsync(commandText, this.builder.Parameters, function);

        public virtual Task<TReturnType> ReadFirstAsync<TReturnType>(string commandText, Func<DbDataReader, TReturnType> function) => this.ReadFirstAsync(commandText, this.builder.Parameters, function);

        public async Task<TReturnType> ReadFirstAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, TReturnType> function)
        {
            var entities = await this.ReadAsync<TReturnType>(commandText, parameters, function);

            if (entities != null)
            {
                var enumerator = entities.GetEnumerator();

                enumerator.MoveNext();

                return enumerator.Current;
            }

            return default(TReturnType);
        }

        public virtual Task<TReturnType> ReadFirstAsync<TReturnType>(string commandText, Func<DbDataReader, Task<TReturnType>> function) => this.ReadFirstAsync<TReturnType>(commandText, this.builder.Parameters, function);

        public async Task<TReturnType> ReadFirstAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, Task<TReturnType>> function)
        {
            var entities = await this.ReadAsync<TReturnType>(commandText, parameters, function);

            if (entities != null)
            {
                var enumerator = entities.GetEnumerator();

                enumerator.MoveNext();

                return enumerator.Current;
            }

            return default(TReturnType);
        }

        public virtual IAdoExecution WithParameters(IEnumerable<DbParameter> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.builder.WithParameters(parameters);

            return this;
        }

        public virtual IAdoExecution WithParameters(params DbParameter[] parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.builder.WithParameters(parameters);

            return this;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract bool Execute(string commandText);
        public abstract Task<bool> ExecuteAsync(string commandText);
        public abstract IEnumerable<TReturnType> Read<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, TReturnType> function);
        public abstract Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, TReturnType> function);
        public abstract Task<IEnumerable<TReturnType>> ReadAsync<TReturnType>(string commandText, IEnumerable<DbParameter> parameters, Func<DbDataReader, Task<TReturnType>> function);
    }
}