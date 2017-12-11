using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using System.Reflection;

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
            database = "testbdd";
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
        
        public void Insert()
        {
            string query = "INSERT INTO users (name, age) VALUES ('joey', '18'), ('Jamel', '45')";

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
        
        public void Delete()
        {
            string query = "DELETE FROM users WHERE name='Joe'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        public T SelectOne<T>(T classe, PropertyInfo prop, params String[] wheres)
        {
            TableSql table = NameConverter.GetTableSql(classe);
            string whers = "";
            if (prop.GetValue(classe) != null)
            {
                object propvalue = prop.GetValue(classe);
                object propname = prop.Name;
                whers = $"WHERE {propname}='{propvalue}'";
            }

            string query = $"SELECT * FROM {table.TableName} {whers}";

            List<T> list = new List<T>();

            foreach (string where in wheres)
            {
                query = $"{query} {where}";
            }

            if (this.OpenConnection() == true)
            {
                T obj = default(T);
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                PropertyInfo[] properties = typeof(T).GetProperties();
                while (dataReader.Read())
                {
                    //T obj = Activator.CreateInstance(typeof(T));
                    foreach (PropertyInfo property in properties)
                    {

                        var a = dataReader[NameConverter.ToSql(property.Name)].ToString();
                        property.SetValue(obj, a);
                        Console.WriteLine(property.Name);
                    }
                }
                dataReader.Close();
                this.CloseConnection();
                return obj;
            }
            return default(T);
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
