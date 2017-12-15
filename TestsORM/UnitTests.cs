using System;
using System.Text;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ORM;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace TestsORM
{
    #region classes
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
    public class Address : DbConnect
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Zipcode { get; set; }

        public Address(int Id)
        {
            this.Id = Id;
            this.SelectOne(this,null,"Id");
        }
    }
    #endregion

    [TestClass]
    public class SetupTests
    {
        // TODO : mettre les champs en parametres XML
        private string server = "localhost";
        private string database = "bddtest";
        private string uid = "root";
        private string password = "root";

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
            User user = new User();
            TableSql a = NameConverter.GetTableSql(user);
            System.Console.WriteLine(a.TableName);
            foreach (var p in a.ColumnList)
            {
                System.Console.WriteLine(p);
            }
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
        // TODO : Affiner tests avec le mapping demandé (II.1)

        // TODO : Mettre test prenant en compte les opérateurs ainsi que le AND / OR
        [TestMethod]
        public void SelectAllTest()
        {
            DbConnect db = new DbConnect();
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
            
            // SELECT age FROM bddtest.users WHERE bddtest.users.age = 45
            List<string> parameters = new List<string>();
            parameters.Add("45");
            List<string> selects = new List<string>();
            selects.Add("age");
            List<string>[] list = db.SelectAll(u,selects, parameters);
            // SELECT age FROM bddtest.users WHERE bddtest.users.age = 45
            parameters.Clear();
            parameters.Add("34");

            List<string>[] none = db.SelectAll(u.age, parameters); // Chercher "Jackie"
            Assert.AreEqual(none, null);
            
        }

        [TestMethod]
        public void SelectOneTest()
        {
            DbConnect db = new DbConnect();

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

            Console.WriteLine("\nTest de addresse de Janine");
            u = new User { Username = "Janine" };
            selects = new List<string>();
            selects.Add("id");
            Address address = new Address(db.SelectOne(u,selects, "Username"," ", default((string, string, string))).Id.GetValueOrDefault());
            Assert.AreEqual(address.Street, "15 rue José Bové");

            Console.WriteLine("\nTest de qui est à l'adresse");
            address = new Address(2);
            u = db.SelectOne(u, null, null, " ", (NameConverter.GetTableSql(address).TableName, "id", "address_id"));
            Assert.AreEqual(u.Email, "Janine.D@aol.fr");
        }

        [TestMethod]
        public void InsertTest()
        {
            DbConnect db = new DbConnect();
            User user = new User();
            user.Id = 0;
            user.Username = "Jacques";
            user.Password = "Test";
            user.Email = @"Jacques.Test@mail.com";
            user.AddressId = 1;
            // INSERT INTO bddtest.users (name, age) VALUES ('Jacques', '78');
            //db.Insert(user); // Inserer "Jacques"
            /*List<string> list = db.SelectOne();
            Assert.AreEqual(list[1], "Jacques");*/
        }

        [TestMethod]
        public void UpdateTest()
        {
            DbConnect db = new DbConnect();
            // UPDATE bddtest.users SET name='Francois' WHERE name='Ishamel'
            //OU UPDATE bddtest.users SET name='Francois' WHERE idusers=2
            db.Update(); // Changer "Ishamel" en "Francois"
            /*string list = db.SelectOne("bddtest.users", "bddtest.users = 'Francois'");
            Assert.AreEqual(list, "Francois");*/
        }

        [TestMethod]
        public void DeleteTest()
        {
            DbConnect db = new DbConnect();
            // DELETE FROM bddtest.users WHERE name="Ahab"
            db.Delete(); // Supprimer "Ahab"
            //string list = db.SelectOne("bddtest.users", "bddtest.users = 'Ahab'"); // select Ahab
            //Assert.AreEqual(list, null);
        }
    }
}



