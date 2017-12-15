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
        private string query;
        private string SELECT;
        private string FROM;
        private string JOIN;
        private string WHERE;

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
        public T SelectOne<T>(T @class, List<String> selects = null, string correspondance = null, string wheres = " ", (string, string, string) join = default((string, string, string)))
        {
            TableSql table = NameConverter.GetTableSql(@class);
            string wherequery = "";
            string selectquery = "";
            string joinquery = "";
            string tablenamequery = table.TableName;

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
            if (selects == null)
                selectquery = "*";
            else
            {
                //Si l'utilisateur ajoute des champs à select, on les définis pour la requête
                foreach (string select in selects)
                {
                    selectquery = $"{selectquery}, {select.ToLower()}";
                }
                selectquery = selectquery.Remove(0, 1);
            }

            //Si l'utilisateur ajoute un join, le prepare pour l'ajouter a la requete
            if (join.Item1 != default((string, string, string)).Item1)
            {
                joinquery = $"JOIN {join.Item1} j ON j.{join.Item2} = f.{join.Item3}";
                tablenamequery = $"{tablenamequery} f";
            }

            //Prépare la requête de base (avec les selects, la table et la correspondance si elle existe)
            string query = $"SELECT {selectquery} FROM {tablenamequery} {joinquery} {wherequery}";


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
                        try
                        {
                            var a = dataReader[NameConverter.ToSql(property.Name)];
                            Console.WriteLine("this is " + a);
                            property.SetValue(@class, a);
                        }
                        catch (System.IndexOutOfRangeException e)
                        {

                        }
                    }
                }
                //Ferme la connection et l'objet de lecture
                dataReader.Close();
                this.CloseConnection();
                return @class;
            }
            throw new Exception("No Connection");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classname"></param>
        /// <param name="correspondance"></param>
        /// <param name="wheres"></param>
        /// <param name="selects"></param>
        /// <returns></returns>
        public string SelectOne(string classname, List<String> selects = null, string correspondance = null, string wheres = " ", (string, string, string) join = default((string, string, string)))
        {
            string wherequery = "";
            string selectquery = "";
            string joinquery = "";

            //Defini n where de base si l'utilisateur en a précisé un
            if (correspondance != null)
            {
                try
                {
                    wherequery = $"WHERE {correspondance}";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            //Si l'utilisateur n'ajoute aucun champs à select, on defini un select de base sur all (*)
            if (selects == null)
                selectquery = "*";
            else
            {
                //Si l'utilisateur ajoute des champs à select, on les définis pour la requête
                foreach (string select in selects)
                {
                    selectquery = $"{selectquery}, {select.ToLower()}";
                }
                selectquery = selectquery.Remove(0, 1);
            }

            //Si l'utilisateur ajoute un join, le prepare pour l'ajouter a la requete
            if (join.Item1 != default((string, string, string)).Item1)
            {
                joinquery = $"JOIN {join.Item1} j ON j.{join.Item2} = f.{join.Item3}";
                classname = $"{classname} f";
            }

            //Prépare la requête de base (avec les selects, la table et la correspondance si elle existe)
            string query = $"SELECT {selectquery} FROM {classname} {wherequery}";


            //Si l'utilisateur ajoute des wheres, les ajoutes à la requête
            query = $"{query} {wheres}";

            Console.WriteLine("query = " + query);
            if (this.OpenConnection() == true)
            {
                //Prépare la commande, l'execute puis recupere un objet de lecture de la reponse
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                string retour = "";

                //lit l'object de lecture
                while (dataReader.Read())
                {
                    //recupere la valeur de chaque champs de l'object de lecture
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        retour = $"{retour},{dataReader.GetValue(i)}";
                    }
                }
                retour = retour.Remove(0, 1);
                //Ferme la connection et l'objet de lecture
                dataReader.Close();
                this.CloseConnection();
                return retour;
            }
            throw new Exception("No Connection");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="class"></param>
        /// <param name="correspondance"></param>
        /// <param name="wheres"></param>
        /// <param name="join"></param>
        /// <param name="selects"></param>
        /// <returns></returns>
        public List<T> SelectAll<T>(T @class, List<String> selects = null, string correspondance = null, string wheres = " ", (string, string, string) join = default((string, string, string)))
        {
            TableSql table = NameConverter.GetTableSql(@class);
            string wherequery = "";
            string selectquery = "";
            string joinquery = "";
            string tablenamequery = table.TableName;
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
            if (selects == null)
                selectquery = "*";
            else
            {
                //Si l'utilisateur ajoute des champs à select, on les définis pour la requête
                foreach (string select in selects)
                {
                    selectquery = $"{selectquery}, {select.ToLower()}";
                }
                selectquery = selectquery.Remove(0, 1);
            }

            //Si l'utilisateur ajoute un join, le prepare pour l'ajouter a la requete
            if (join.Item1 != default((string, string, string)).Item1)
            {
                joinquery = $"JOIN {join.Item1} j ON j.{join.Item2} = f.{join.Item3}";
                tablenamequery = $"{tablenamequery} f";
            }

            //Prépare la requête de base (avec les selects, la table et la correspondance si elle existe)
            string query = $"SELECT {selectquery} FROM {tablenamequery} {joinquery} {wherequery}";


            //Si l'utilisateur ajoute des wheres, les ajoutes à la requête
            query = $"{query} {wheres}";

            Console.WriteLine("query = " + query);
            if (this.OpenConnection() == true)
            {
                //Prépare la commande, l'execute puis recupere un objet de lecture de la reponse
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                PropertyInfo[] properties = @class.GetType().GetProperties();
                int compteur = 0;
                //lit et parcours les propietes de la classe a completer
                while (dataReader.Read())
                {
                    foreach (PropertyInfo property in properties)
                    {
                        for (int i = compteur; i < compteur + properties.Length; i++)
                        {
                            if (property.Name.ToLower() == dataReader.GetName(i% dataReader.FieldCount))
                            {
                                property.SetValue(@class, dataReader[i% dataReader.FieldCount]);
                            }
                        }
                    }
                    compteur += properties.Length;
                    T t = (T) Activator.CreateInstance(typeof(T));
                    list.Add(t);
                    list.FindLastIndex(delegate (T @object)
                    {
                        foreach (PropertyInfo property  in @object.GetType().GetProperties())
                        {
                            property.SetValue(@object, @class.GetType().GetProperty(property.Name).GetValue(@class));
                        }
                        return true;
                    });
                }
                //Ferme la connection et l'objet de lecture
                dataReader.Close();
                this.CloseConnection();
                return list;
            }
            throw new Exception("No Connection");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classname"></param>
        /// <param name="correspondance"></param>
        /// <param name="wheres"></param>
        /// <param name="join"></param>
        /// <param name="selects"></param>
        /// <returns></returns>
        public List<string> SelectAll(string classname, List<String> selects = null, string correspondance = null, string wheres = " ", (string, string, string) join = default((string, string, string)))
        {
            string wherequery = "";
            string selectquery = "";
            string joinquery = "";
            List<string> list = new List<string>();

            //Defini n where de base si l'utilisateur en a précisé un
            if (correspondance != null)
            {
                try
                {
                    wherequery = $"WHERE {correspondance}";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            //Si l'utilisateur n'ajoute aucun champs à select, on defini un select de base sur all (*)
            if (selects == null)
                selectquery = "*";
            else
            {
                //Si l'utilisateur ajoute des champs à select, on les définis pour la requête
                foreach (string select in selects)
                {
                    selectquery = $"{selectquery}, {select.ToLower()}";
                }
                selectquery = selectquery.Remove(0, 1);
            }

            //Si l'utilisateur ajoute un join, le prepare pour l'ajouter a la requete
            if (join.Item1 != " ")
            {
                joinquery = $"JOIN {join.Item1} j ON j.{join.Item2} = f.{join.Item3}";
                classname = $"{classname} f";
            }

            //Prépare la requête de base (avec les selects, la table et la correspondance si elle existe)
            string query = $"SELECT {selectquery} FROM {classname} {wherequery}";


            //Si l'utilisateur ajoute des wheres, les ajoutes à la requête
            query = $"{query} {wheres}";

            Console.WriteLine("query = " + query);
            if (this.OpenConnection() == true)
            {
                //Prépare la commande, l'execute puis recupere un objet de lecture de la reponse
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //lit l'object de lecture
                while (dataReader.Read())
                {

                    string retour = "";
                    //recupere la valeur de chaque champs de l'object de lecture
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        retour = $"{retour},{dataReader.GetValue(i)}";
                    }
                    retour = retour.Remove(0, 1);
                    list.Add(retour);
                }
                //Ferme la connection et l'objet de lecture
                dataReader.Close();
                this.CloseConnection();
                return list;
            }
            throw new Exception("No Connection");
        }

    }
    #endregion

}
