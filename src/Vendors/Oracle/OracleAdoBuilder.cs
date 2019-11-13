using System;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace Vidalink.Core.Data.Oracle
{
    public partial class OracleAdoBuilder : AdoBuilder
    {
        public new const string ConnectionStringEnvironmentKey = "CORE__ORACLE_CONNECTION_STRING";

        public OracleAdoBuilder() => this.Create(System.Environment.GetEnvironmentVariable(OracleAdoBuilder.ConnectionStringEnvironmentKey), false);

        public OracleAdoBuilder(bool pooling) => this.Create(System.Environment.GetEnvironmentVariable(OracleAdoBuilder.ConnectionStringEnvironmentKey), pooling);

        public OracleAdoBuilder(string connectionString) => this.Create(connectionString, false);

        public OracleAdoBuilder(string connectionString, bool pooling) => this.Create(connectionString, pooling);

        /// <summary>
        /// Creates a <see cref="OracleConnection"/> with a given <paramref name="connectionString"/> and <paramref name="pooling"/>.
        /// </summary>
        /// <param name="connectionString">An Oracle connection string.</param>
        /// <param name="pooling">If pooling is enabled to the connection.</param>
        private void Create(string connectionString, bool pooling)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            try
            {
                // Has connection reuse?
                base.hasPooling = pooling;

                // Get connection string from .env.
                base.ConnectionString = connectionString;
                base.CommandType = CommandType.Text;
                base.Timeout = AdoBuilder.QueryTimeout;

                if (base.hasPooling)
                {
                    base.Connection = LazyConnectionFactory<OracleConnection>.Instance;
                }
                else
                {
                    base.Connection = new OracleConnection(base.ConnectionString);
                }
            }
            catch (OracleException oracleException) /*when (oracleException.Number == 451 || oracleException.Number == 452)*/ // Retry, according to documentation (https://docs.oracle.com/cd/B28359_01/server.111/b28278/e0.htm#ORA-00000).
            {
                if (!oracleException.IsRecoverable)
                    throw new AdoBuilderException("Error establishing a connection with the database provider.", oracleException);

                base.Connection = new OracleConnection(base.ConnectionString);
            }
        }

        /// <summary>
        /// Builds a <see cref="IAdoExecution"/> object with available configurations.
        /// </summary>
        /// <returns>Returns a <see cref="IAdoExecution"/> object.</returns>
        public override IAdoExecution Build()
        {
            base.Connection.Open();

            if (base.hasTransaction)
            {
                base.Transaction = base.Connection.BeginTransaction();
            }

            return new OracleAdoExecution(this);
        }

        /// <summary>
        /// Builds a <see cref="IAdoExecution"/> object with available configurations asynchronously.
        /// </summary>
        /// <returns>Returns a <see cref="Task{IAdoExecution}"/> object.</returns>
        public override async Task<IAdoExecution> BuildAsync()
        {
            await base.Connection.OpenAsync();

            if (base.hasTransaction)
            {
                base.Transaction = base.Connection.BeginTransaction();
            }

            return new OracleAdoExecution(this);
        }
    }
}