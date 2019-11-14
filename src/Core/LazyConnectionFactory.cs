using System;
using System.Data.Common;

namespace Vidalink.Core.Data
{
    public class LazyConnectionFactory<TConnectionProvider> where TConnectionProvider : DbConnection, new()
    {
        public const string ConnectionStringEnvironmentKey = "CORE__DB_CONNECTION_STRING";

        private static readonly Lazy<TConnectionProvider> _initializer = new Lazy<TConnectionProvider>(() =>
        {
            var connection = new TConnectionProvider();
            connection.ConnectionString = System.Environment.GetEnvironmentVariable(ConnectionStringEnvironmentKey);

            return connection;
        });

        public static TConnectionProvider Instance
        {
            get
            {
                return _initializer.Value;
            }
        }

        private LazyConnectionFactory()
        { }
    }
}
