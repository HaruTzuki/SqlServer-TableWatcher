using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace libwatcher_mssql
{
    public class Watcher
    {
        private readonly string ConnectionString;
        private readonly SqlConnection Connection;
        private long TableCheckSumTemp;
        

        //Public Properties
        public string TableName;
        public long TableCheckSum;

        //Events
        public delegate void OnDbSchemaChanged();
        public event OnDbSchemaChanged OnDbSchema_Changed;


        #region Constructors
        public Watcher()
        {
            this.ConnectionString = string.Empty;
            this.Connection = null;
            this.TableName = string.Empty;
        }

        public Watcher(string TableName)
        {
            this.TableName = TableName;
        }

        public Watcher(string ConnectionString, string TableName)
        {
            this.ConnectionString = ConnectionString;
            this.Connection = new SqlConnection(this.ConnectionString);
            this.TableName = TableName;
        }

        public Watcher(SqlConnection Connection, string TableName)
        {
            this.Connection = Connection;
            this.TableName = TableName;
        }

        public Watcher(string Hostname, string UserId, string Password, string Database, string TableName)
        {
            this.ConnectionString = $"Integrated Security=false;Pooling=false;User ID={UserId};Password={Password};Data Source={Hostname};Initial Catalog={Database}";
            this.Connection = new SqlConnection(this.ConnectionString);
            this.TableName = TableName;
        } 
        #endregion

        /// <summary>
        /// Async method that start to watch a table.
        /// </summary>
        /// <returns></returns>
        public async Task StartWatchAsync()
        {
            if (string.IsNullOrEmpty(this.TableName))
                throw new Exception("One or more property not defined");

            string Query = $"SELECT cast(CHECKSUM_AGG(BINARY_CHECKSUM(*)) as bigint) as Checksums FROM {this.TableName} WITH (NOLOCK);";

            while (true)
            {
                try
                {
                    if (this.Connection.State != ConnectionState.Open)
                        await this.Connection.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand(Query, this.Connection))
                    {
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        while (await reader.ReadAsync())
                        {
                            this.TableCheckSumTemp = await reader.IsDBNullAsync(reader.GetOrdinal("Checksums")) ? 0 : reader.GetInt64(reader.GetOrdinal("Checksums"));
                        }

                        if (this.TableCheckSum != this.TableCheckSumTemp)
                        {
                            this.TableCheckSum = this.TableCheckSumTemp;
                            OnDbSchema_Changed?.Invoke();
                        }

                        reader?.Close();

                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }


            }
        }
        
    }
}
