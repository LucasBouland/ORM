using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using MySql.Data.MySqlClient;
using Npgsql;

namespace ORM
{

    public enum DatabaseType
    {
        MySql,
        Postgres,
        SqlServer
    }


    // TODO : Rassembler les trois classes en une gestion unique du type de base
    public class DbConnect
    {
        private DbConnection connection;

        #region Get Database Class
        /// <summary>
        /// Retourne une connexion du type donné en dbType
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static DbConnection GetConnection(DatabaseType dbType, string connectionString)
        {
            switch (dbType)
            {
                case DatabaseType.MySql:
                    return new MySqlConnection(connectionString);
                case DatabaseType.Postgres:
                    return new NpgsqlConnection(connectionString);
                case DatabaseType.SqlServer:
                    return new SqlConnection(connectionString);
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Retourne une commande dont le type correspond à celui de la connection
        /// </summary>
        /// <param name="query"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static DbCommand GetCommand(string query, DbConnection connection)
        {
            string co = connection.GetType().Name;

            switch (co)
            {
                case "MySqlConnection":
                    return new MySqlCommand(query, (MySqlConnection)connection);
                case "SqlConnection":
                    return new SqlCommand(query, (SqlConnection)connection);
                case "NpgsqlConnection":
                    return new NpgsqlCommand(query, (NpgsqlConnection)connection);
                default:
                    throw new NotImplementedException();
            }

        }
        #endregion

        public DbConnect(DatabaseType dbType, string connectionString)
        {
            Initialize(dbType, connectionString);
        }

        private void Initialize(DatabaseType dbType, string connectionString)
        {
            try
            {
                this.connection = GetConnection(dbType, connectionString);

            }
            catch (DbException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            Console.WriteLine(connection);
        }
        // TODO : Faire passer les champs via un fichier xml type settings.xml
        /* private void Initialize(DatabaseType dbType, string connectionString)
         {

             server = "localhost";
             database = "testbdd";
             uid = "root";
             password = "";
             string connectionString;
             connectionString = "SERVER=" + server + ";" + "DATABASE=" +
             database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
             DbConnectionFactory testFactory = new DbConnectionFactory();
             IDbAction testCo = testFactory.GetConnection(DatabaseType.MySql);
             connection = testCo.CreateConnection(connectionString);
             Console.WriteLine(connection.GetType());
         }*/

        // TODO : expliciter la gestion des erreurs
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (DbException ex)
            {
                Console.WriteLine(ex.Message);
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
            catch (DbException ex)
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
            string columns = string.Join("','", table.ColumnList.ToArray());
            columns = columns.Replace("id',", string.Empty); // on enleve l'id en dur pour l'instant
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
                    values += "'";
                    values += propValue;
                    values += "'" + ",";
                }
            }
            System.Console.WriteLine(values);
            values = values.Remove(values.Length - 1);

            string query = $" INSERT INTO {table.TableName} ({columns}') VALUES ({@values});";
            System.Console.WriteLine(query);

            if (this.OpenConnection() == true)
            {
                Console.WriteLine(connection.GetType().Name);
                DbCommand cmd = GetCommand(query, connection);

                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
            else
            {
                Console.Write("lmao");
            }
        }

        /* public void Update()
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
         }*/

        public List<string> SelectOne()
        {
            throw new NotImplementedException();
        }

        /* public List<string>[] Select()
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
         }*/
    }
}
