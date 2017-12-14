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
            database = "bddtest";
            uid = "root";
            password = "root";
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="class"></param>
        /// <param name="correspondance"></param>
        /// <param name="wheres"></param>
        /// <param name="selects"></param>
        /// <returns></returns>
        public T SelectOne<T>(T @class, string correspondance, string wheres = " ", params string[] selects)
        {
            TableSql table = NameConverter.GetTableSql(@class);
            string wherequery = "";
            string selectquery = "";
            List<T> list = new List<T>();

            //Vérifie si un where doit être fait dans la requête par rapport à un champ nommé en c# "correspondance" de la table
            if (correspondance != null)
            {
                try
                {
                    PropertyInfo corresp = @class.GetType().GetProperty(correspondance);
                    object propvalue = corresp.GetValue(@class);
                    string propname = corresp.Name.ToLower();
                    wherequery = $"WHERE {propname}='{propvalue}'";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            //Si l'utilisateur n'ajoute aucun champs à select, on defini un select de base sur all (*)
            if (selects.Length == 0)
                selectquery = "*";

            //Si l'utilisateur ajoute des champs à select, on les définis pour la requête
            foreach (string select in selects)
            {
                selectquery = $"{selectquery}, {select.ToLower()}";
            }

            //Prépare la requête de base (avec les selects, la table et la correspondance si elle existe)
            string query = $"SELECT {selectquery} FROM {table.TableName} {wherequery}";


            //Si l'utilisateur ajoute des wheres, les ajoutes à la requête
            query = $"{query} {wheres}";
            Console.WriteLine("query = " + query);
            if (this.OpenConnection() == true)
            {
                //Prépare la commande, l'execute puis recupere un objet de lecture de la reponse
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                PropertyInfo[] properties = @class.GetType().GetProperties();

                //lit et parcours les propietes de la classe a completer
                while (dataReader.Read())
                {
                    foreach (PropertyInfo property in properties)
                    {
                        //recupere la valeur de chaque champs de la classe et les attribues aux membres de la classe
                        var a = dataReader[NameConverter.ToSql(property.Name)];
                        property.SetValue(@class, a);
                    }
                }
                //Ferme la connection et l'objet de lecture
                dataReader.Close();
                this.CloseConnection();
                return @class;
            }
            return default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="class"></param>
        /// <param name="correspondance"></param>
        /// <param name="wheres"></param>
        /// <param name="selects"></param>
        /// <returns></returns>
       /* public string SelectOne(string classname, string correspondance, string[] wheres, params string[] selects)
        {
            TableSql table = NameConverter.GetTableSql(classname);
            string wherequery = "";
            string selectquery = "";
            //List<T> list = new List<T>();

            //Vérifie si un where doit être fait dans la requête par rapport à un champ nommé en c# "correspondance" de la table
            //if (correspondance.GetValue(@class) != null)
            //{
            //    object propvalue = correspondance.GetValue(@class);
                //object propname = correspondance.Name;
              //  wherequery = $"WHERE {propname}='{propvalue}'";
            //}

            wherequery = $"WHERE {correspondance}";

            //Si l'utilisateur n'ajoute aucun champs à select, on defini un select de base sur all (*)
            if (selects.Length == 0)
                selectquery = "*";

            //Si l'utilisateur ajoute des champs à select, on les définis pour la requête
            foreach (string select in selects)
            {
                selectquery = $"{selectquery}, {select}";
            }

            //Prépare la requête de base (avec les selects, la table et la correspondance si elle existe)
            string query = $"SELECT {selectquery} FROM {table.TableName} {wherequery}";

            //Si l'utilisateur ajoute des wheres, les ajoutes à la requête
            foreach (string where in wheres)
            {
                query = $"{query} {where}";
            }

            if (this.OpenConnection() == true)
            {
                //T obj = default(T);
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                //PropertyInfo[] properties = typeof(T).GetProperties();
                while (dataReader.Read())
                {
                    //T obj = Activator.CreateInstance(typeof(T));
                    //foreach (PropertyInfo property in properties)
                    {

                        var a = dataReader.ToString();
                        //property.SetValue(obj, a);
                       // Console.WriteLine(property.Name);
                        return a;
                    }
                }
                dataReader.Close();
                this.CloseConnection();
            }
            return null;
        }
        */
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
