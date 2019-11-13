using Npgsql;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Vidalink.Core.Data.Postgre
{
    public partial class PostgreAdoBuilder : AdoBuilder
    {
        public new const string ConnectionStringEnvironmentKey = "CORE__POSTGRE_CONNECTION_STRING";

        public PostgreAdoBuilder() => this.Create(System.Environment.GetEnvironmentVariable(PostgreAdoBuilder.ConnectionStringEnvironmentKey), false);

        public PostgreAdoBuilder(bool pooling) => this.Create(System.Environment.GetEnvironmentVariable(PostgreAdoBuilder.ConnectionStringEnvironmentKey), pooling);

        public PostgreAdoBuilder(string connectionString) => this.Create(connectionString, false);

        public PostgreAdoBuilder(string connectionString, bool pooling) => this.Create(connectionString, pooling);

        /// <summary>
        /// Creates a <see cref="NpgsqlConnection"/> with a given <paramref name="connectionString"/> and <paramref name="pooling"/>.
        /// </summary>
        /// <param name="connectionString">A Postgre connection string.</param>
        /// <param name="pooling">If pooling is enabled to the connection.</param>
        private void Create(string connectionString, bool pooling)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            try
            {
                // Has connection reuse?
                base.hasPooling = pooling;
                base.ConnectionString = connectionString;
                base.CommandType = CommandType.Text;
                base.Timeout = AdoBuilder.QueryTimeout;

                if (base.hasPooling)
                {
                    base.Connection = LazyConnectionFactory<NpgsqlConnection>.Instance;
                }
                else
                {
                    base.Connection = new NpgsqlConnection(base.ConnectionString);
                }
            }
            catch (NpgsqlException postgreException) /*when (oracleException.Number == 451 || oracleException.Number == 452)*/ // Retry, according to documentation (https://docs.oracle.com/cd/B28359_01/server.111/b28278/e0.htm#ORA-00000).
            {
                if (!postgreException.IsTransient)
                    throw new AdoBuilderException("Error establishing a connection with the database provider.", postgreException);

                base.Connection = new NpgsqlConnection(base.ConnectionString);
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

            return new PostgreAdoExecution(this);
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

            return new PostgreAdoExecution(this);
        }
    }
}