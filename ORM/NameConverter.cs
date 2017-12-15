using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORM
{
    public class TableSql
    {
        public string TableName { get; set; }
        public string ShemaName { get; set; }
        public List<string> ColumnList { get; set; }

        public TableSql()
        {
            ColumnList = new List<string>();
        }
    }

    public class ClassCsharp
    {
        public string ClassName { get; set; }
        public List<string> PropertyList { get; set; }
    }

    public class NameConverter
    {
        public static TableSql GetTableSql<T>(T objectClass, DatabaseType type)
        {
            TableSql table = new TableSql();

            switch (type)
            {
                case DatabaseType.Postgres:
                    table.TableName = "public." + ToSql(typeof(T).Name);
                    break;
                case DatabaseType.MySql:
                    table.TableName = ToSql(typeof(T).Name);
                    break;
                case DatabaseType.SqlServer:
                    table.TableName = "[dbo." + ToSql(typeof(T).Name) + "]";
                    break;
            }

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                table.ColumnList.Add(ToSql(prop.Name));
            }
            return table;
        }

        public static T GetClassCsharp<T>(TableSql tableSql)
        {
            string className = "TestsORM" + "." + "User";
            var myObj = Activator.CreateInstance(Type.GetType(className)); // namespace + type
            Type test = myObj.GetType();
            return (T)myObj;
        }
        #region stringTo
        // prend un string en CamelCase, retourne un string en snake_case
        //rajoute un underscore devant les majuscules, passe la string en minuscule et enleve le premier underscore
        public static string ToSql(string str)
        {
            return string.Concat(str.Select(x => Char.IsUpper(x) ? "_" + x : x.ToString())).TrimStart(' ').ToLower().Substring(1);
        }

        // prend un string en snake_case, retourne un string en CamelCase
        public static string ToCsharp(string str)
        {
            // TODO passer en linq
            var temp = str.Split('_');
            List<string> wordlList = new List<string>();
            foreach (string t in temp)
            {
                string after = "";
                if (t[0] == '_')
                {
                    string t2 = t.Substring(0);
                    wordlList.Add(char.ToUpper(t2.First()) + t2.Substring(1).ToLower());
                }
                else
                {
                    wordlList.Add(char.ToUpper(t.First()) + t.Substring(1).ToLower());
                }
            }
            return string.Join("", wordlList.ToArray());
        }
        #endregion
    }
}

