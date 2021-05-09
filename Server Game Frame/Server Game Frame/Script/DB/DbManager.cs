using MySql.Data.MySqlClient;
using Server_Game_Frame.Script.PlayerDic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Server_Game_Frame.Script.DB
{
    public class DbManager
    {
        static MySqlConnection mysql;

        static JavaScriptSerializer Js = new JavaScriptSerializer();

        public static bool ConnectMySql(string db, string ip, int port, string user, string pw)
        {
            mysql = new MySqlConnection();

            string s = string.Format("Database = {0}; Data Source = {1}; port = {2}; User Id = {3}; Password= {4}", db, ip, port, user, pw);

            mysql.ConnectionString = s;

            try
            {
                mysql.Open();

                Console.WriteLine("[ 数据库 ] Connect Succ");

                return true;
            }
            catch(MySqlException ex)
            {
                Console.WriteLine("[ 数据库 ] Connect Fail ");

                return false;
            }
        }

        public static bool Register(string id, string pw)
        {
            if(!IsSafeString(id) || !IsSafeString(pw))
            {
                Console.WriteLine("[ 数据库 ] Register Fail Id, Pw Is Not Safe");

                return false;
            }

            if(!IsAccountExist(id))
            {
                Console.WriteLine("[ 数据库 ] Register Fail Id Is Exist");

                return false;
            }

            string sql = string.Format("Insert into Account Set Id = '{0}', pw = '{1}';", id, pw);

            try
            {
                MySqlCommand cmd = new MySqlCommand(sql,mysql);

                cmd.ExecuteNonQuery();

                Console.WriteLine("成功插入数据");

                return true;
            }
            catch(MySqlException ex)
            {
                Console.WriteLine("[ 数据库 ] Register Fail " + ex.Message);

                return false;
            }
        }

        public static bool UpdatePlayerData(string id, PlayerData playerData)
        {
            string data = Js.Serialize(playerData);

            string sql = string.Format("Update Player Set Data ='{0}' where id = '{1}';", data, id);

            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, mysql);

                cmd.ExecuteNonQuery();

                return true;
            }
            catch(MySqlException ex)
            {
                Console.WriteLine("[ 数据库 ] UpdatePlayer Err " + ex.ToString());

                return false;
            }
        }

        public static bool CreatePlayer(string id)
        {

            if(!IsSafeString(id) || IsAccountExist(id))
            {
                Console.WriteLine("Please Check Whether It Is A Safe Character Or The User Already Exists ");

                return false;
            }

            PlayerData playerData = new PlayerData();

            string date = Js.Serialize(playerData);

            string sql = string.Format(" Insert Into Player Set id = '{0}', data = '{1}';", id, date);

            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, mysql);

                cmd.ExecuteNonQuery();

                return true;
            }
            catch(MySqlException ex)
            {
                Console.WriteLine("[ 数据库 ] CreatePlayer Err " + ex.ToString());

                return false;
            }

        }

        //检测密码与用户名的正确性
        public static bool CheckPassword(string id, string pw)
        {
            if(!IsSafeString(id)|| !IsSafeString(pw) || IsAccountExist(id))
            {
                Console.WriteLine("[ 数据库 ] CheckPassword Err " + "数据库的检索不到该用户");

                return false;
            }

            string sql = string.Format("Select * From Account Where Id = '{0}' And Pw = '{1}';", id, pw);

            try
            {
                using(MySqlCommand cmd = new MySqlCommand(sql, mysql))
                {
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    return dataReader.HasRows;
                }
            }
            catch(MySqlException ex)
            {
                Console.WriteLine("[ 数据库 ] CheckPassWord Err " + ex.Message);

                return false;
            }
        }

        //清空表中的数据
        public static bool Clear(string s)
        {
            if(!IsSafeString(s))
            {
                Console.WriteLine("[ 数据库 ] Clear Fail ");

                return false;
            }

            string sql1 = string.Format("truncate table {0};",s);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql1, mysql))
                {
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    return !dataReader.HasRows;
                }
            }
            catch(MySqlException ex)
            {
                Console.WriteLine("[ 数据库 ] Clear Err " + ex.Message);

                return false;
            }
        }



        //获取玩家数据
        public static PlayerData GetPlayerData(string id)
        {
            if(!IsSafeString(id))
            {
                Console.WriteLine("[ 数据库 ] GetPlayerData Fail, Id Is Unsafe");

                return null;
            }

            string sql = string.Format("Select * from player where id = '{0}';", id);

            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, mysql);

                MySqlDataReader dataReader = cmd.ExecuteReader();

                if(!dataReader.HasRows)
                {
                    dataReader.Close();

                    return null;
                }

                //读取
                dataReader.Read();

                string data = dataReader.GetString("data");

                //反序列化
                PlayerData playerData = Js.Deserialize<PlayerData>(data);

                dataReader.Close();

                return playerData;

            }
            catch(MySqlException ex)
            {
                Console.WriteLine("[ 数据库 ] GetPlayerData Fail " + ex.ToString());

                return null;
            }
        }

        public static bool IsAccountExist(string id)
        {
            if(!IsSafeString(id))
            {
                return false;
            }

            string s = string.Format("Select * from account where id = '{0}';", id);

            try
            {
                using(MySqlCommand cmd = new MySqlCommand(s, mysql))
                {
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    bool hasRows = dataReader.HasRows;

                    Console.WriteLine("查询的结果是：{0}", hasRows);

                    return !hasRows;
                }

            }
            catch(MySqlException ex)
            {
                Console.WriteLine("[ 数据库 ] IsSafeString err " + ex.ToString());

                return false;
            }
        }

        private static bool IsSafeString(string str)
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
        }

    }
}
