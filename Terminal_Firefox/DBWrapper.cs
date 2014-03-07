using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using NLog;

namespace Terminal_Firefox {
    //public sealed class DBWrapper {

    //    private static readonly DBWrapper instace = new DBWrapper();

    //    private readonly SQLiteConnection _connection = new SQLiteConnection();
    //    public readonly SQLiteCommand Command = new SQLiteCommand();

    //    public const string ConnectionString = @"Data Source=|DataDirectory|\db\terminal.sqlite;";
    //    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    //    private DBWrapper() {
    //        try {
    //            _connection.ConnectionString = ConnectionString;
    //            Command.Connection = _connection;
    //            _connection.Open();

    //        } catch (Exception exception) {
    //            Log.Error("Невозможно соедениться с базой данных");
    //        }
    //    }

    //    public static DBWrapper Instance {
    //        get { return instace; }
    //    }
    //}


    internal class SQLiteDatabase {
        public static String DbConnection = "Data Source=db\\terminal.sqlite";

        /// <summary>
        ///     Default Constructor for SQLiteDatabase Class.
        /// </summary>
        

        /// <summary>
        ///     Allows the programmer to run a query against the Database.
        /// </summary>
        /// <param name="sql">The SQL to run</param>
        /// <returns>A DataTable containing the result set.</returns>
        public DataTable GetDataTable(string sql) {
            DataTable dt = new DataTable();
            try {
                SQLiteConnection cnn = new SQLiteConnection(DbConnection);
                cnn.Open();
                SQLiteCommand mycommand = new SQLiteCommand(cnn);
                mycommand.CommandText = sql;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                dt.Load(reader);
                reader.Close();
                cnn.Close();
            } catch (Exception e) {
                throw new Exception(e.Message);
            }
            return dt;
        }

        /// <summary>
        ///     Allows the programmer to interact with the database for purposes other than a query.
        /// </summary>
        /// <param name="sql">The SQL to be run.</param>
        /// <returns>An Integer containing the number of rows updated.</returns>
        public int ExecuteNonQuery(string sql) {
            SQLiteConnection cnn = new SQLiteConnection(DbConnection);
            cnn.Open();
            SQLiteCommand mycommand = new SQLiteCommand(cnn);
            mycommand.CommandText = sql;
            int rowsUpdated = mycommand.ExecuteNonQuery();
            cnn.Close();
            return rowsUpdated;
        }

        /// <summary>
        ///     Allows the programmer to retrieve single items from the DB.
        /// </summary>
        /// <param name="sql">The query to run.</param>
        /// <returns>A string.</returns>
        public string ExecuteScalar(string sql) {
            SQLiteConnection cnn = new SQLiteConnection(DbConnection);
            cnn.Open();
            SQLiteCommand mycommand = new SQLiteCommand(cnn);
            mycommand.CommandText = sql;
            object value = mycommand.ExecuteScalar();
            cnn.Close();
            if (value != null) {
                return value.ToString();
            }
            return "";
        }
    }
}
