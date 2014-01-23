using System.Data.SqlServerCe;

namespace Terminal_Firefox {
    public sealed class DBWrapper {

        private static readonly DBWrapper instace = new DBWrapper();

        private readonly SqlCeConnection _connection = new SqlCeConnection();
        public readonly SqlCeCommand Command = new SqlCeCommand();

        private DBWrapper() {
            _connection.ConnectionString = @"Data Source=|DataDirectory|\db\terminal.sdf;Password=sdfsafd7897&^^*&;Persist Security Info=True";
            Command.Connection = _connection;
            _connection.Open();
        }

        public static DBWrapper Instance {
            get { return instace; }
        }
    }
}
