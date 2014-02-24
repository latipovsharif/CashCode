using System;
using System.Data.SqlServerCe;
using NLog;

namespace Terminal_Firefox {
    public sealed class DBWrapper {

        private static readonly DBWrapper instace = new DBWrapper();

        private readonly SqlCeConnection _connection = new SqlCeConnection();
        public readonly SqlCeCommand Command = new SqlCeCommand();

        public const string ConnectionString = @"Data Source=|DataDirectory|\db\terminal.sdf;Password=sdfsafd7897&^^*&;Persist Security Info=True";
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private DBWrapper() {
            try {
            _connection.ConnectionString = ConnectionString;
            Command.Connection = _connection;
            _connection.Open();

            } catch (Exception exception) {
                Log.Error("Невозможно соедениться с базой данных");                
            }
        }

        public static DBWrapper Instance {
            get { return instace; }
        }
    }
}
