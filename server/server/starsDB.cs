using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace server
{
    class starsDB
    {
        //mysql config
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //constructor
        public starsDB()
        {
            Initialize();
        }

        //initialize the database information so we can establbish a connection when need be
        private void Initialize()
        {
            server = "csgostars.com";
            database = "csgostarsDB";
            uid = "csgostarsDB";
            password = "0mh-dkm,=vF2";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
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

        //Close connection
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

        public bool clearPreviousLotto()
        {
            string query = "SELECT * FROM liveLotterySummary";

            //Create a list to store the result
            List<string>[] list = new List<string>[10];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();
            list[3] = new List<string>();
            list[4] = new List<string>();
            list[5] = new List<string>();
            list[6] = new List<string>();
            list[7] = new List<string>();
            list[8] = new List<string>();
            list[9] = new List<string>();

            try
            {
                //Open connection
                if (this.OpenConnection() == true)
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        //store all the values in the table
                        list[0].Add(dataReader["lotteryID"] + "");
                        list[1].Add(dataReader["numberOfItems"] + "");
                        list[2].Add(dataReader["totalValueOfPot"] + "");
                        list[3].Add(dataReader["startTime"] + "");
                        list[4].Add(dataReader["maxNumberOfItems"] + "");
                        list[5].Add(dataReader["maxValueOfPot"] + "");
                        list[6].Add(dataReader["maxTimeLimit"] + "");
                        list[7].Add(dataReader["ended"] + "");
                        list[8].Add(dataReader["winnerID"] + "");
                        list[9].Add(dataReader["distroComplete"] + "");
                    }

                    //close Data Reader
                    dataReader.Close();

                    //copy the values to a new table
                    for (int i = 0; i < list[0].Count; i++)
                    {
                        string insertQ = "INSERT INTO liveLotterySummaryHistory(lotteryID, numberOfItems, totalValueOfPot, startTime, maxNumberOfItems, maxValueOfPot, maxTimeLimit, ended, winnerID, distroComplete) VALUES('" + list[0][i] + "', '" + list[1][i] + "', '" + list[2][i] + "', '" + list[3][i] + "', '" + list[4][i] + "', '" + list[5][i] + "', '" + list[6][i] + "', '" + list[7][i] + "', '" + list[8][i] + "', '" + list[9][i] + "')";
                        cmd = new MySqlCommand(insertQ, connection);
                        cmd.ExecuteNonQuery();
                        //delete the items 
                        string deleteQ = "DELETE FROM liveLotterySummary WHERE lotteryID='"+ list[0][i] +"'";
                        cmd = new MySqlCommand(deleteQ, connection);
                        cmd.ExecuteNonQuery();
                    }

                    //close Connection
                    this.CloseConnection();
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        //getLotterySetup sets up the lottery with default settings from database 
        public bool getLotterySetup(out string lotteryID, out int maxNumberOfItems, out double maxValueOfItems, out long maxTimeLimit, int type) {
            bool foundSetting = false;
            string query = "SELECT * FROM lotterySettings WHERE type='" + type + "'";
            
            //default the values
            lotteryID = "1";
            maxNumberOfItems = 1000;
            maxValueOfItems = 11000000000000;
            maxTimeLimit = 110000000000;
            try
            {
                //Open connection
                if (this.OpenConnection() == true)
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        foundSetting = true;
                        //set values for lottery
                        lotteryID = (dataReader["currentLottery"] + "");
                        maxNumberOfItems = Convert.ToInt32(dataReader["maxPoolItems"] + "");
                        maxValueOfItems = Convert.ToDouble(dataReader["maxPoolValue"] + "");
                        maxTimeLimit = Convert.ToInt64(dataReader["maxTime"] + "");
                        break;
                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection();
                }
                //check if there is a case where the settings aren't found striaght up reject this shit
                if (foundSetting == true)
                {
                    //increment the lottery so next one starts with a higher value
                    incrementLotto(lotteryID);
                    return true;
                }
                else
                {
                    //not found settings case
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool addToLotterySummary(string lotteryID, int numberOfItems, double totalValueOfPot, long startTime, int maxNumberOfItems, double maxValueOfPot, long maxTimeLimit) 
        {
            string query = "INSERT INTO liveLotterySummary (lotteryID, numberOfItems, totalValueOfPot, startTime, maxNumberOfItems, maxValueOfPot, maxTimeLimit, ended, winnerID, distroComplete) VALUES('" + lotteryID + "', '" + numberOfItems + "', '" + totalValueOfPot + "', '" + startTime + "', '" + maxNumberOfItems + "', '" + maxValueOfPot + "','" + maxTimeLimit + "', 'False', '00000000', 'False')";
            try
            {
                //open connection
                if (this.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor
                    MySqlCommand cmd = new MySqlCommand(query, connection);
        
                    //Execute command
                    cmd.ExecuteNonQuery();

                    //close connection
                    this.CloseConnection();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        //incrmementLotto function increments the lottery to the next value so when we're starting our next lotto we get a new ID 
        public bool incrementLotto(string currentLottoID)
        {
            long incrementedLotto = Convert.ToInt64(currentLottoID) + 1;
            string query = "UPDATE lotterySettings SET currentLottery='" + incrementedLotto + "' WHERE currentLottery='" + currentLottoID + "'";
            try
            {
                //Open connection
                if (this.OpenConnection() == true)
                {
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    //Execute query
                    cmd.ExecuteNonQuery();

                    //close connection
                    this.CloseConnection();
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        //updateLottoStats function grabs the numberOfItems current value and totalValueOfPot's current value 
        public bool updateLottoStats(string lotteryID, out int numberOfItems, out double totalValueOfPot) 
        {
            //set values incase we can't contact DB
            numberOfItems = -1;
            totalValueOfPot = -1;

            //attempt to contact DB
            try
            {
                //cmd query to find new values
                string query = "SELECT * FROM liveLotterySummary WHERE lotteryID='"+ lotteryID +"'";
                //Open connection
                if (this.OpenConnection() == true)
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        //grab the data and update it 
                        Console.WriteLine("grabbing new values");
                        numberOfItems = Convert.ToInt32(dataReader["numberOfItems"] + "");
                        totalValueOfPot = Convert.ToDouble(dataReader["totalValueOfPot"] + "");
                        break;
                    }
                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        //submit all the approved bets to the que for trading 
        public bool sendToBetQue()
        {
            bool betsFound = false;
            try
            {
                string query = "SELECT * FROM betsSubmitted WHERE approved='True'";

                //Create a list to store the result
                List<string>[] list = new List<string>[8];
                list[0] = new List<string>();
                list[1] = new List<string>();
                list[2] = new List<string>();
                list[3] = new List<string>();
                list[4] = new List<string>();
                list[5] = new List<string>();
                list[6] = new List<string>();
                list[7] = new List<string>();

                //Open connection
                if (this.OpenConnection() == true)
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        betsFound = true;
                        list[0].Add(dataReader["botID"] + "");
                        list[1].Add(dataReader["steamID"] + "");
                        list[2].Add(dataReader["tradeID"] + "");
                        list[3].Add(dataReader["valueTotal"] + "");
                        list[4].Add(dataReader["numberOfItems"] + "");
                        list[5].Add(dataReader["offerSentTime"] + "");
                        list[6].Add(dataReader["approved"] + "");
                        list[7].Add(dataReader["requestSent"] + "");
                    }

                    //close Data Reader
                    dataReader.Close();

                    for (int i = 0; i < list[0].Count; i++)
                    {
                        //execute insert into new table for processing
                        string insertQ = "INSERT INTO betQue (botID, tradeID, steamID, valueTotal, numberOfItems, offerSentTime) VALUES('" + list[0][i] + "', '" + list[2][i] + "', '" + list[1][i] + "', '" + list[3][i] + "', '" + list[4][i] + "', '" + list[5][i] + "')";
                        cmd = new MySqlCommand(insertQ, connection);
                        cmd.ExecuteNonQuery();
                        //execute delete
                        string deleteQ = "DELETE FROM betsSubmitted WHERE tradeID='" + list[2][i] + "'";
                        cmd = new MySqlCommand(deleteQ, connection);
                        cmd.ExecuteNonQuery();
                    }
                    //close Connection
                    this.CloseConnection();
                }
                if (betsFound)
                {
                    Console.WriteLine("Found bets adding bets to que for Processing");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        //endLotteryfunction ends the lotto so we stop collecting 
        public bool endLotto(string lotteryID)
        {
            try
            {
                string query = "UPDATE liveLotterySummary SET ended='True' WHERE lotteryID='" + lotteryID + "'";

                //Open connection
                if (this.OpenConnection() == true)
                {
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    //Execute query
                    cmd.ExecuteNonQuery();

                    //close connection
                    this.CloseConnection();
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool checkDistroComplete()
        {
            bool distroStatus = false;
            try
            {
                string query = "SELECT * FROM liveLotterySummary";
                
                //Open connection
                if (this.OpenConnection() == true)
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        distroStatus = Convert.ToBoolean(dataReader["distroComplete"] + "");
                        break;
                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection();
                }
            }
            catch
            {

            }
            return distroStatus;
        }

    }
}
