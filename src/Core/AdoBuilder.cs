using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Vidalink.Core.Data
{
    public abstract class AdoBuilder : IAdoBuilder
    {
        public const string ConnectionStringEnvironmentKey = "DB_CONNECTION";
        // In seconds
        public const int QueryTimeout = 30;

        private bool _hasBeenDisposed;
        protected bool hasPooling;
        protected bool hasTransaction;

        public virtual string Command { get; protected set; }
        public virtual CommandType CommandType { get; set; }
        public virtual DbConnection Connection { get; protected set; }
        public virtual string ConnectionString { get; protected set; }
        public virtual (bool hasPreparedStatement, bool isParametersRequired) PreparedStatement { get; protected set; }
        public virtual IEnumerable<DbParameter> Parameters { get; set; }
        public virtual int Timeout { get; protected set; }
        public virtual DbTransaction Transaction { get; protected set; }

        /// <summary>
        /// Adds a SQL <paramref name="command"/> to all subsequent calls to a <see cref="IAdoExecution"/> object. The <paramref name="commandType"/> can be <see cref="System.Data.CommandType.Text"/> or <see cref="System.Data.CommandType.StoredProcedure"/>.
        /// </summary>
        /// <param name="command">Command string to be executed.</param>
        /// <param name="commandType">Command type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="command"/> is <see cref="System.Data.CommandType.TableDirect"/>.</exception>
        /// <returns>Returns a <see cref="IAdoBuilder"/> chain object.</returns>
        public virtual IAdoBuilder WithCommand(string command, CommandType commandType)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException(nameof(command));
            if (commandType == CommandType.TableDirect)
                throw new NotSupportedException("Only text and stored procedure commands are currently supported.");

            this.Command = command;

            return this;
        }

        /// <summary>
        /// Adds a procedure SQL <paramref name="command"/> to all subsequent calls to a <see cref="IAdoExecution"/> object.
        /// </summary>
        /// <param name="command">Command string to be executed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null or empty.</exception>
        /// <returns>Returns a <see cref="IAdoBuilder"/> chain object.</returns>
        public IAdoBuilder WithCommand(string command) => this.WithCommand(command, CommandType.StoredProcedure);

        /// <summary>
        /// Adds a specific <see cref="System.Data.CommandType"/> to all subsequent calls to a <see cref="IAdoExecution"/> object.
        /// </summary>
        /// <param name="commandType">Command type.</param>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="commandType"/> is <see cref="System.Data.CommandType.TableDirect"/>.</exception>
        /// <returns>Returns a <see cref="IAdoBuilder"/> chain object.</returns>
        public IAdoBuilder WithCommandType(CommandType commandType)
        {
            if (commandType == CommandType.TableDirect)
                throw new InvalidOperationException("Only stored procedure and text are currently supported.");

            this.CommandType = commandType;

            return this;
        }

        /// <summary>
        /// Compose a list of database parameters to all subsequent calls to a <see cref="IAdoExecution"/> object.
        /// </summary>
        /// <param name="parameters">Database parameters.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> is null.</exception>
        /// <returns>Returns a <see cref="IAdoBuilder"/> chain object.</returns>
        public IAdoBuilder WithParameters(IEnumerable<DbParameter> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.Parameters = parameters;

            return this;
        }

        /// <summary>
        /// Compose database parameters to all subsequent calls to a <see cref="IAdoExecution"/> object.
        /// </summary>
        /// <param name="parameters">Database parameters.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> is null.</exception>
        /// <returns>Returns a <see cref="IAdoBuilder"/> chain object.</returns>
        public IAdoBuilder WithParameters(params DbParameter[] parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.Parameters = parameters;

            return this;
        }

        /// <summary>
        /// Instructs the database to pre-cache queries to be faster on consecutive calls.
        /// </summary>
        /// <returns>Returns a <see cref="IAdoBuilder"/> chain object.</returns>
        public IAdoBuilder WithPreparedStatement()
        {
            this.PreparedStatement = (true, true);

            return this;
        }

        /// <summary>
        /// Instructs the database to pre-cache queries to be faster on consecutive calls.
        /// </summary>
        /// <param name="requireParameters">Database parameters will be required to efectively create a prepared statement.</param>
        /// <returns>Returns a <see cref="IAdoBuilder"/> chain object.</returns>
        public IAdoBuilder WithPreparedStatement(bool requireParameters)
        {
            this.PreparedStatement = (true, requireParameters);

            return this;
        }

        /// <summary>
        /// Amount of time to wait for a database command response.
        /// </summary>
        /// <param name="timeout">Command timeout in seconds.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeout"/> is less than zero.</exception>
        /// <returns>Returns a <see cref="IAdoBuilder"/> chain object.</returns>
        public IAdoBuilder WithTimeout(int timeout)
        {
            if (timeout < 0)
                throw new ArgumentOutOfRangeException(nameof(timeout));

            this.Timeout = timeout;

            return this;
        }

        /// <summary>
        /// Adds a transaction to all method calls on the <see cref="IAdoExecution"/> object.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when there is no database connection.</exception>
        /// <returns>Returns a <see cref="IAdoBuilder"/> chain object.</returns>
        public IAdoBuilder WithTransaction()
        {
            if (this.Connection == null)
                throw new InvalidOperationException("A connection must exist before creating a transaction.");

            this.hasTransaction = true;

            return this;
        }

        /// <summary>
        /// Dispose all resources (managed and non-managed).
        /// </summary>
        public virtual void Dispose()
        {
            if (!this._hasBeenDisposed)
            {
                // Managed resource
                this.Parameters = null;

                // Non-managed resources below
                if (this.Connection.State != ConnectionState.Closed &&
                        this.Connection.State != ConnectionState.Broken)
                {
                    this.Connection.Close();
                }

                this.Transaction?.Dispose();

                if (!this.hasPooling)
                {
                    this.Connection.Dispose();

                    this.Connection = null;
                }

                this._hasBeenDisposed = true;

                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Builds a <see cref="IAdoExecution"/> object with available configurations.
        /// </summary>
        /// <returns>Returns a <see cref="IAdoExecution"/> object.</returns>
        public abstract IAdoExecution Build();

        /// <summary>
        /// Builds a <see cref="IAdoExecution"/> object with available configurations asynchronously.
        /// </summary>
        /// <returns>Returns a <see cref="Task{IAdoExecution}"/> object.</returns>
        public abstract Task<IAdoExecution> BuildAsync();
    }
}
