using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Threading.Tasks;

namespace WellnessApis
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Add your DbSets here
        // public DbSet<YourEntity> YourEntities { get; set; }

        // Method to execute stored procedure and return dynamic list
        public async Task<List<dynamic>> ExecuteStoredProcedureDynamic(string sql)
        {
            var result = new List<dynamic>();

            using (var connection = Database.GetDbConnection())
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var expandoObject = new ExpandoObject() as IDictionary<string, object>;

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                expandoObject.Add(reader.GetName(i), reader.GetValue(i));
                            }

                            result.Add(expandoObject);
                        }
                    }
                }
            }

            return result;
        }

        // Method to execute stored procedure and return DataTable (alternative)
        public async Task<DataTable> ExecuteStoredProcedure(string sql)
        {
            var dataTable = new DataTable();

            using (var connection = Database.GetDbConnection())
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }
                }
            }

            return dataTable;
        }
    }
}