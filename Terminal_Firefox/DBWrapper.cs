using System.Data.SqlServerCe;

namespace Terminal_Firefox {
    public sealed class DBWrapper {

        private static readonly DBWrapper instace = new DBWrapper();

        public readonly SqlCeConnection Connection = new SqlCeConnection();
        public SqlCeCommand Command = new SqlCeCommand();

        private DBWrapper() {
            Connection.ConnectionString = @"Data Source=|DataDirectory|\db\terminal.sdf;Password=sdfsafd7897&^^*&;Persist Security Info=True";
            Command.Connection = Connection;
            Connection.Open();
        }

        public static DBWrapper Instance {
            get { return instace; }
        }
    }
}
