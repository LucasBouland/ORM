using System;
using System.Text;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ORM;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TestsORM
{
   
    #region test classes
    // Classe representant la table user
    public class User
    {
        public int? Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int AddressId { get; set; }
    }
    // Classe representant la table Address
    public class Address 
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Zipcode { get; set; }

    }
    #endregion

    [TestClass]
    public class FactoryTests
    {

        [TestMethod]
        public void MySqlConnectTest()
        {
            string server = "localhost";
            string database = "bddtest";
            string uid = "root";
            string password = "";
            string connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                                      database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            DbConnect db = new DbConnect(DatabaseType.MySql, connectionString);

        }
        [TestMethod]
        public void SqlServerConnectTest()
        {
            string server = "localhost";
            string database = "bddtest";
            string uid = "root";
            string password = "";
            string connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                                      database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            DbConnect db = new DbConnect(DatabaseType.SqlServer, connectionString);

        }
        [TestMethod]
        public void PostgresConnectTest()
        {
            string server = "localhost";
            string database = "bddtest";
            string uid = "root";
            string password = "";
            string connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                                      database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            DbConnect db = new DbConnect(DatabaseType.Postgres, connectionString);

        }

    }
    [TestClass]
    public class SetupTests
    {
        // TODO : mettre les champs en parametres XML
        private string server = "localhost";
        private string database = "bddtest";
        private string uid = "root";
        private string password = "";

        //MySQL
        // TODO : simplifier les requetes
        // TODO : refactor les noms des variables
        // TODO : Code pour les trois types de base de données
        // TODO : trois fois le meme try/catch -> passer en méthode
        // TODO : Utiliser using plutot que try catch : https://stackoverflow.com/questions/42152084/can-i-use-multiple-queries-in-using-single-mysqlconnection-in-c-sharp
        // TODO : Supprimer la base après les tests
        #region DB and Tables Creation
        [TestMethod]
        public void CreateSetup()
        {
            TestCreateDatabase();
            TestCreateTable();
            TestCreateRows();
        }

        public void TestCreateDatabase()
        {
            string connStr = "SERVER=" + server + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            //string connStr = "server=localhost;user=root;port=3306;password=;";
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand cmd;
            string s1;
            conn.Open();
            s1 = "CREATE DATABASE IF NOT EXISTS `bddtest`;";
            cmd = new MySqlCommand(s1, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void TestCreateTable()
        {
            string connStr = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand cmd;
            string s1;

            conn.Open();
            s1 = @"
CREATE TABLE IF NOT EXISTS `bddtest`.`address` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `street` VARCHAR(255) NOT NULL,
  `city` VARCHAR(45) NULL,
  `country` VARCHAR(45) NULL,
  `zipcode` VARCHAR(45) NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `id_UNIQUE` (`Id` ASC));

CREATE TABLE IF NOT EXISTS `bddtest`.`user` (
  `id` INT(11) NOT NULL AUTO_INCREMENT,
  `username` VARCHAR(16) NOT NULL,
  `email` VARCHAR(255) NULL,
  `password` VARCHAR(32) NOT NULL,
  `address_id` INT(11) NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `id_UNIQUE` (`Id` ASC),
  INDEX `address_id_idx` (`address_id` ASC),
  CONSTRAINT `address_id`
    FOREIGN KEY (`address_id`)
    REFERENCES `bddtest`.`address` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);
";
            cmd = new MySqlCommand(s1, conn);
            cmd.ExecuteNonQuery();
            conn.Close();

        }

        public void TestCreateRows()
        {
            string connStr = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand cmd;
            string s1;
            string s2;
            string s3;
            conn.Open();

            s1 = "TRUNCATE TABLE `user`";
            cmd = new MySqlCommand(s1, conn);
            cmd.ExecuteNonQuery();
            s2 = @"INSERT INTO `bddtest`.`address` (`street`, `city`, `country`, `zipcode`) VALUES ('15 rue José Bové', 'Paris', 'France', '75480');
            INSERT INTO `bddtest`.`address` (`street`, `city`, `country`, `zipcode`) VALUES('2b karambolage', 'Berlin', 'Allemagne', '45014B');
            ";

            cmd = new MySqlCommand(s2, conn);
            cmd.ExecuteNonQuery();
            s3 = "INSERT INTO user (username, email, password, address_id) VALUES " +
                 "('Ahab', 'Ahab@WhiteWhale.com', 'hunter2', '1')," +
                 " ('Ishmael', 'Perdu@EnMer.com', 'wololo', '1')," +
                 " ('Janine', 'Janine.D@aol.fr', '14/12/72', '2')";
            cmd = new MySqlCommand(s3, conn);
            cmd.ExecuteNonQuery();

            conn.Close();
        }
        #endregion
    }

    [TestClass]
    public class ConverterTest
    {
        #region stringTest
        [TestMethod]
        public void ToSqlTest()
        {
            User user = new User();
            string test = "ArIdA";
            // System.Console.WriteLine(NameConverter.ToSql(test));
        }

        [TestMethod]
        public void ToCsharpTest()
        {
            User user = new User();
            string test = "ar_id_a";
            System.Console.WriteLine(NameConverter.ToCsharp(test));
        }
        #endregion

        [TestMethod]
        public void ClassToSqlTest()
        {
            /*User user = new User();

            TableSql a = NameConverter.GetTableSql(user);
            System.Console.WriteLine(a.TableName);
            foreach (var p in a.ColumnList)
            {
                System.Console.WriteLine(p);
            }*/
        }

        [TestMethod]
        public void SqlToClassTest()
        {

        }

        [TestMethod]
        public void TableSqltoClassTest()
        {
            TableSql t = new TableSql();
            t.TableName = "user";
            string type = NameConverter.ToCsharp(t.TableName);
            //            object keepo = Activator.CreateInstance(Type.GetType("ORM."+type));
            var myObj = Activator.CreateInstance(Type.GetType("TestsORM" + "." + type)); // namespace + type
            Type test = myObj.GetType();

            var u = NameConverter.GetClassCsharp<object>(t);
            System.Console.WriteLine(u.GetType());
        }

    }

    [TestClass]
    public class CommandTest
    {
        static string server { get; set; } = "localhost"; 
        static string database = "bddtest";
        static string uid = "root";
        static string password = "";
        static string connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

        // TODO : Affiner tests avec le mapping demandé (II.1)

        // TODO : Mettre test prenant en compte les opérateurs ainsi que le AND / OR
        [TestMethod]
        public void SelectAllTest()
        {

            DbConnect db = new DbConnect(DatabaseType.MySql, connectionString);
            User u = new User();

            List<User> user = db.SelectAll(u);
            Assert.AreEqual(user.Count,3);
            Assert.AreEqual(user[0].Email, "Ahab@WhiteWhale.com");
            Assert.AreEqual(user[2].Email, "Janine.D@aol.fr");
            // SELECT * FROM Users

            List<User> users = db.SelectAll(new User());
            Console.WriteLine(users[0].Username);
            Console.WriteLine(users[1].Username);
            Console.WriteLine(users[0].Email);
            Console.WriteLine(users[1].Email);
            Assert.AreEqual(users[0].Username, "Ahab");
            Assert.AreEqual(users[1].Username, "Ishmael");            

        }

        [TestMethod]
        public void SelectOneTest()
        {

            DbConnect db = new DbConnect(DatabaseType.MySql, connectionString);
            
            Console.WriteLine("Test de Janine");
            List<string> selects = new List<string>();
            selects.Add("username");
            selects.Add("email");
            string[] reponse = db.SelectOne("user",selects, "username='janine'", " ", default((string,string,string))).Split(',');
            Assert.AreEqual(reponse[0], "Janine");
            Assert.AreEqual(reponse[1], "Janine.D@aol.fr");
            
            Console.WriteLine("\nTest de Ahab");
            User u = new User{Username = "Ahab"};
            u = db.SelectOne(u,null, "Username"); // Cherche "Ahab"
            Assert.AreEqual(u.Email, "Ahab@WhiteWhale.com");

            Console.WriteLine("\nTest de Jackie");
            u = new User { Username = "Jackie" };
            u = db.SelectOne(u,null, "Username"); // Chercher "Jackie"
            Assert.AreEqual(u.Email, null);

        }

        [TestMethod]
        public void InsertTest()
        {
            DbConnect db = new DbConnect(DatabaseType.MySql, connectionString);
            User user = new User();
            user.Id = 0;
            user.Username = "Jacques";
            user.Password = "Test";
            user.Email = @"Jacques.Test@mail.com";
            user.AddressId = 1;
            // INSERT INTO bddtest.users (name, age) VALUES ('Jacques', '78');
            db.Insert(user); // Inserer "Jacques"
            User u = new User { Username = "Jacques" };
            u = db.SelectOne(u,null,"Username");
            Assert.AreEqual(u.Email, "Jacques.Test@mail.com");
        }

        [TestMethod]
        public void UpdateTest()
        {
            DbConnect db = new DbConnect(DatabaseType.MySql, connectionString);
            
            User user = new User();
            user.Id = 3;
            user.Username = "Janine";
            user.Password = "14/12/72";
            user.Email = @"Janine.d@aol.fr";
            user.AddressId = 2;

            User user2 = new User();
            user2.Id = 3;
            user2.Username = "Janone";
            user2.Password = "14/13/72";
            user2.Email = @"Janone@mail.com";
            user2.AddressId = 3;

            db.UpdateOne(user2, user);

        }

        [TestMethod]
        public void DeleteTest()
        {
            DbConnect db = new DbConnect(DatabaseType.MySql, connectionString);

            User user = new User();
            user.Id = 1;
            user.Username = "Ahab";
            user.Password = "hunter2";
            user.Email = @"Ahab@WhiteWhale.com";
            user.AddressId = 1;

            db.Delete(user);
            User u = new User { Username = "Jacques" };
            u = db.SelectOne(u, null, "Username");
            Assert.AreEqual(u.Email, null);

        }
    }
}



