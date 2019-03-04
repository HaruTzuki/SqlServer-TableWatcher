using libwatcher_mssql;
using System.Collections.Generic;
using System.Data.SqlClient;
using static System.Console;

namespace libwatcher_mssql_sample
{
    class Program
    {
        static private List<string> DataList;
        static private string TableName;
        static private Watcher watcher;

        static void Main(string[] args)
        {

            //Descriptions
            WriteLine("This is a Watcher Sample");
            WriteLine("//====//");

            //Reading from User table Name
            TableName = ReadLine();

            //Print Table Name
            WriteLine($"The table is: {TableName}");

            //Initialize Objects
            InitData();

            StartWatch();
            
            //Wait until user click a button
            ReadKey();

        }

        private async static void StartWatch()
        {
            await watcher.StartWatchAsync();
        }

        private static void InitData()
        {
            DataList = new List<string>();
            watcher = new Watcher("(local)", "sa", "1234", "TestDB", TableName.Trim());
            watcher.OnDbSchema_Changed += Watcher_OnDbSchema_Changed;
        }

        private static void Watcher_OnDbSchema_Changed()
        {
            DataList.Clear();
            string ConString = $"Integrated Security=false;Pooling=false;User ID=sa;Password=1234;Data Source=(local);Initial Catalog=TestDB";

            using (SqlConnection conn = new SqlConnection(ConString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand($"Select Name from {TableName.Trim()}", conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        DataList.Add(reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name")));
                    }

                    reader?.Close();
                }
            }

            PrintData();
        }

        private static void PrintData()
        {
            foreach(var StrObj in DataList)
            {
                WriteLine(StrObj);
            }

            WriteLine("//====//");

        }
    }
}
