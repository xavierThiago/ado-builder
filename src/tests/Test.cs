using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Vidalink.Core.Data.Oracle;

namespace Vidalink.Core.Data
{
    public class Test
    {
        public async Task<TestEntity> TestDbBuilder()
        {
            using (var execution = await new OracleAdoBuilder()
                                    .WithParameters(
                                        new OracleParameter("name", OracleDbType.Varchar2, "Xavier", ParameterDirection.Input),
                                        new OracleParameter("age", OracleDbType.Int16, 27, ParameterDirection.Input)
                                    )
                                    .WithCommandType(CommandType.StoredProcedure)
                                    .WithTimeout(10)
                                    .WithTransaction()
                                    .BuildAsync())
            {
                var result = await execution.ReadFirstAsync<TestEntity>("get_all_users", (reader) =>
                            {
                                return new TestEntity
                                {
                                    Name = reader["db_name"].ToString(),
                                    Age = reader.GetByte(1)
                                };
                            });

                bool saved = await execution.WithParameters(new OracleParameter[]
                                    {
                                        new OracleParameter("nickname", OracleDbType.Varchar2, "'" + result.Name + "'", ParameterDirection.Input)
                                    })
                                    .ExecuteAsync("save_user_nickname");

                if (saved)
                {
                    if (await execution.ClearParameters().ExecuteAsync("delete_all_other_users"))
                    {
                        execution.Commit();
                    }
                    else
                    {
                        execution.Rollback();
                    }
                }
                else
                {
                    execution.Rollback();
                }

                return result;
            }
        }

        public class TestEntity
        {
            public string Name { get; set; }
            public byte Age { get; set; }
        }
    }
}
