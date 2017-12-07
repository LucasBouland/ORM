using System;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ORM;
using System.Collections.Generic;

namespace TestsORM
{
    // Classe representant la table Users
    internal class Users
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public int AddressId { get; set; }
    }

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
            s1 = @"CREATE TABLE IF NOT EXISTS `users` (
`idusers` int(11) NOT NULL AUTO_INCREMENT,
`name` varchar(45) DEFAULT NULL,
`age` int(11) DEFAULT NULL,
PRIMARY KEY(`idusers`),
UNIQUE KEY `idusers_UNIQUE` (`idusers`)
) ENGINE = MyISAM AUTO_INCREMENT = 54 DEFAULT CHARSET = utf8;";
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
            conn.Open();

            s1 = "TRUNCATE TABLE `users`";
            cmd = new MySqlCommand(s1, conn);
            cmd.ExecuteNonQuery();

            s2 = "INSERT INTO users (name, age) VALUES ('Ahab', '45'), ('Ishmael', '18'), ('Janine', '45')";
            cmd = new MySqlCommand(s2, conn);
            cmd.ExecuteNonQuery();

            conn.Close();
        }
        #endregion
    }


    [TestClass]
    public class CommandTest
    {
        // TODO : Affiner tests avec le mapping demandé (II.1)

        // TODO : Mettre test prenant en compte les opérateurs ainsi que le AND / OR
        [TestMethod]
        public void SelectAllTest()
        {
            /*DBConnect db = new DBConnect();
             users u = new users();
             // SELECT * FROM Users
             List<string>[] list = db.Select(u);
             Assert.AreEqual(list[1][0], "Ahab");
             Assert.AreEqual(list[2][0], "Ishmael");
             // SELECT age FROM bddtest.users WHERE bddtest.users.age = 45
             List<string> parameters = new List<string>();
             parameters.Add("45");
             List<string>[] list = db.Select(u.age, parameters);
             // SELECT age FROM bddtest.users WHERE bddtest.users.age = 45
             parameters.Clear();
             parameters.Add("34");

             List<string>[] none = db.Select(u.age, parameters); // Chercher "Jackie"
             Assert.AreEqual(none, null);*/
        }

        [TestMethod]
        public void SelectOneTest()
        {
            DBConnect db = new DBConnect();
            // SELECT * FROM bddtest.users WHERE bddtest.users = "Ahab" LIMIT 1
            List<string> found = db.SelectOne(); // Cherche "Ahab"
            Assert.AreEqual(found[1], "Ahab");

            List<string> notFound = db.SelectOne(); // Chercher "Jackie"
            Assert.AreEqual(found, null);

        }

        [TestMethod]
        public void InsertTest()
        {
            DBConnect db = new DBConnect();
            // INSERT INTO bddtest.users (name, age) VALUES ('Jacques', '78');
            db.Insert(); // Inserer "Jacques"
            List<string> list = db.SelectOne();
            Assert.AreEqual(list[1], "Jacques");
        }

        [TestMethod]
        public void UpdateTest()
        {
            DBConnect db = new DBConnect();
            // UPDATE bddtest.users SET name='Francois' WHERE name='Ishamel'
            //OU UPDATE bddtest.users SET name='Francois' WHERE idusers=2
            db.Update(); // Changer "Ishamel" en "Francois"
            List<string> list = db.SelectOne();
            Assert.AreEqual(list[1], "Francois");
        }

        [TestMethod]
        public void DeleteTest()
        {
            DBConnect db = new DBConnect();
            // DELETE FROM bddtest.users WHERE name="Ahab"
            db.Delete(); // Supprimer "Ahab"
            List<string> list = db.SelectOne(); // select Ahab
            Assert.AreEqual(list, null);
        }
    }
}



