using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Timers;

namespace server
{
    class lottery
    {
        //lotteryID
        private string lotteryID;
        //current values
        private int numberOfItems;
        private double totalValueOfPot;
        private long startTime;
        //limits
        private int maxNumberOfItems;
        private double maxValueOfItems;
        private long maxTimeLimit;

        //contactDB
        private starsDB database;

        //update status timer 
        private static Timer aTimer;

        private bool endLotto;

        public lottery()
        {
            database = new starsDB();

            //clear previous lotto
            database.clearPreviousLotto();

            //set custom settings
            bool success = database.getLotterySetup(out lotteryID, out maxNumberOfItems, out maxValueOfItems, out maxTimeLimit, 1);

            //check if the settings were properly initialized
            if (success == false)
            {
                Console.WriteLine("Failed to contact database for getting lottery settings");
            }
            else
            {
                //set other values that don't depend on DB settings
                numberOfItems = 0;
                totalValueOfPot = 0;
                startTime = UnixTimeNow();
                endLotto = false;
                //database call to create lottery summary
                database.addToLotterySummary(lotteryID, numberOfItems, totalValueOfPot, startTime, maxNumberOfItems, maxValueOfItems, maxTimeLimit);
            }

            //begin update timer to keep the values up to date with database
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;
        }

        //update values timer 
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //disable timer for duration of update
            aTimer.Enabled = false;
            //call the update status function
            updateStatus();

            Console.WriteLine("Number Of items " + numberOfItems);
            Console.WriteLine("Total Value Of Pot " + totalValueOfPot);
            //what to do when the condition is met 
            if (checkLimit())
            {
                endLotto = true;
                database.endLotto(lotteryID);
            }
            if (endLotto == true)
            {
                //stop the timer
                aTimer.Enabled = false;
            }
            else
            {
                //reinitialize the repeater to continue checking
                aTimer.Enabled = true;
            }

        }

        public bool checkDistroComplete()
        {
            bool distroStatus = false;
            try
            {
                starsDB database = new starsDB();
                distroStatus = database.checkDistroComplete();
            }
            catch
            {

            }
            return distroStatus;
        }
        //check that the limit has been reached on the pot 
        private bool checkLimit()
        {
            bool limit = false;
            if (numberOfItems >= maxNumberOfItems)
            {
                limit = true;
                Console.WriteLine("lotteryID " + lotteryID + " has reached numberOfItems Limit " + " items: " + numberOfItems);
            }
            if (totalValueOfPot >= maxValueOfItems)
            {
                limit = true;
                Console.WriteLine("lotteryID " + lotteryID + " has reached total value Limit " + " value: " + totalValueOfPot);
            }
            long currentTime = this.UnixTimeNow();
            long timeLapsed = currentTime - startTime;

            if (timeLapsed >= maxTimeLimit)
            {
                limit = true;
                Console.WriteLine("lotteryID " + lotteryID + " has reached time limit of " + maxTimeLimit);
            }
            return limit;
        }
        //updateStatus connection to starsDB and checks the current value of numberOfItems and totalValueOfPot to see if the limit has been reached
        public void updateStatus() {
            //temp holders for values 
            int outNumberOfItems;
            double outTotalValueOfPot;

            //contact the DB
            starsDB databaseCall = new starsDB();

            //update the lottery values
            databaseCall.updateLottoStats(lotteryID, out outNumberOfItems, out outTotalValueOfPot);
            
            if (outNumberOfItems == -1 || outTotalValueOfPot == -1)
            {
                //don't update failed to get better values
            }
            else
            {
                //set if there are values to accept
                numberOfItems = outNumberOfItems;
                totalValueOfPot = outTotalValueOfPot;
            }
        }

        /**
        *UnixTimeNow function outputs the time in unix standard at 1970,1,1,0,0,0 
        *@return long currentTime
        */
        public long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
    }
}
