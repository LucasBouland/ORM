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

      /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classToInsert"></param>
        /// <param name="ignoredColumnList"></param>
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

            string query = $"INSERT INTO {table.TableName} ({columns}) VALUES ({@values});";
            System.Console.WriteLine(query);

            if (this.OpenConnection() == true)
            {
                DbCommand cmd = GetCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classToUpdate"></param>
        /// <param name="modelClass"></param>
        public void UpdateOne<T>(T classToUpdate, T modelClass)
        {
            TableSql table= NameConverter.GetTableSql(classToUpdate);

            string setValueString = "";
            string whereCondition = "";
            int i = 0;

            IList<PropertyInfo> props = new List<PropertyInfo>(classToUpdate.GetType().GetProperties());

            foreach (PropertyInfo prop in props)
            {
                setValueString += table.ColumnList[i] + " = " + "'" + prop.GetValue(classToUpdate) + "',";
                i++;
            }

            setValueString = setValueString.Remove(setValueString.Length - 1);

            i = 0;

            foreach (PropertyInfo prop in props)
            {
                whereCondition += table.ColumnList[i] + " = " + "'" + prop.GetValue(modelClass) + "'" + " AND ";
                i++;
            }

            whereCondition = whereCondition.Remove(whereCondition.Length - 5);

            string query = $"UPDATE {table.TableName} SET {setValueString} WHERE {whereCondition}";

            if (this.OpenConnection() == true)
            {
                DbCommand cmd = GetCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classToDelete"></param>
        public void Delete<T>(T classToDelete)
        {
            TableSql table = NameConverter.GetTableSql(classToDelete);
            string whereCondition = "";
            int i = 0;

            IList<PropertyInfo> props = new List<PropertyInfo>(classToDelete.GetType().GetProperties());

            foreach (PropertyInfo prop in props)
            {
                whereCondition += table.ColumnList[i] + " = " + "'" + prop.GetValue(classToDelete) + "'" + " AND ";
                i++;
            }

            whereCondition = whereCondition.Remove(whereCondition.Length - 5);

            string query = $"DELETE FROM {table.TableName} WHERE {whereCondition}";

            if (this.OpenConnection() == true)
            {
                DbCommand cmd = GetCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listClassToDelete"></param>
        public void Delete<T>(List<T> listClassToDelete)
        {
            foreach (T classToDelete in listClassToDelete)
            {
                Delete(classToDelete);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classToDelete"></param>
        public void DeleteAll<T>(T classToDelete)
        {
            TableSql table = NameConverter.GetTableSql(classToDelete);

            string query = $"DELETE FROM {table.TableName}";

            if (this.OpenConnection() == true)
            {
                DbCommand cmd = GetCommand(query, connection);
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
                DbCommand cmd = GetCommand(query, connection);
                DbDataReader dataReader = cmd.ExecuteReader();
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
                DbCommand cmd = GetCommand(query, connection);
                DbDataReader dataReader = cmd.ExecuteReader();
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
                DbCommand cmd = GetCommand(query, connection);
                DbDataReader dataReader = cmd.ExecuteReader();
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
                DbCommand cmd = GetCommand(query, connection);
                DbDataReader dataReader = cmd.ExecuteReader();

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

}



