using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using MySql.Data.MySqlClient;
using Npgsql;

namespace ORM
{

    // TODO : Rassembler les trois classes en une gestion unique du type de base

    #region Mysql
    public class DbConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        public DbConnect()
        {
            Initialize();
        }
        // TODO : Faire passer les champs via un fichier xml type settings.xml
        private void Initialize()
        {
            server = "localhost";
            database = "bddtest";
            uid = "root";
            password = "";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        // TODO : expliciter la gestion des erreurs
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void Insert<T>(T classToInsert, List<string> ignoredColumnList = null)
        {
            if (ignoredColumnList == null)
            {
                ignoredColumnList = new List<string>() { "Id" };
            }
            TableSql table = NameConverter.GetTableSql(classToInsert);
            string columns = string.Join(",", table.ColumnList.ToArray());
            columns = columns.Replace("id,", string.Empty); // on enleve l'id en dur pour l'instant
            Type myType = classToInsert.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
            string values = "";
            foreach (PropertyInfo prop in props)
            {
                System.Console.WriteLine(prop.Name);
                object propValue = prop.GetValue(classToInsert, null);
                // on verifie si la propieté fait partie des champs ignorés
                if (!ignoredColumnList.Any(o => string.Equals(prop.Name, o, StringComparison.OrdinalIgnoreCase)))
                {
                    values += '"';
                    values += propValue;
                    values += '"' + ",";
                }
            }
            System.Console.WriteLine(values);
            values = values.Remove(values.Length - 1);

            string query = "INSERT INTO " + table.TableName + " (" + columns + ") VALUES (" + @values + ");";
            System.Console.WriteLine(query);

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
            else
            {
                Console.Write("lmao");
            }
        }
        
        public void Update()
        {
            string query = "UPDATE users SET name='Joe', age='22' WHERE name='joey'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }
        
        public void Delete<T>(T classToDelete, List<T> objectToDelete)
        {
            string query = "DELETE FROM ";

            TableSql table = NameConverter.GetTableSql(classToDelete);

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        public void DeleteAll<T>(T classToDelete)
        {
            string query = "DELETE FROM ";

            TableSql table = NameConverter.GetTableSql(classToDelete);

            query += table.TableName;

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        public List<string> SelectOne()
        {
            throw new NotImplementedException();
        }

        public List<string>[] Select()
        {
            string query = "SELECT * FROM users";

            List<string>[] list = new List<string>[3];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    list[0].Add(dataReader["idusers"] + "");
                    list[1].Add(dataReader["name"] + "");
                    list[2].Add(dataReader["age"] + "");
                }
                dataReader.Close();
                this.CloseConnection();
                return list;
            }
            else
            {
                return list;
            }
        }
    }
    #endregion

}
