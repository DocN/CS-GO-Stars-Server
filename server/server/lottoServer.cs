using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace server
{
    class lottoServer
    {
        private lottery liveLotto;
        //lottoserver constructor

        //update status timer 
        private static Timer betSubmissionTimer;
        private static Timer restartLottoTimer;
        public lottoServer()
        {
            lottery liveLotto = new lottery();

            //begin update timer to keep the values up to date with database
            // Create a timer with a two second interval.
            betSubmissionTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            betSubmissionTimer.Elapsed += checkSubmittedBets;
            betSubmissionTimer.Enabled = true;

            restartLottoTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            restartLottoTimer.Elapsed += checkRestartLotto;
            restartLottoTimer.Enabled = true;
        }
        //update values timer 
        private void checkSubmittedBets(Object source, ElapsedEventArgs e)
        {
            starsDB database = new starsDB();
            database.sendToBetQue();
        }

        //checks when the lottery should restart a new 
        private void checkRestartLotto(Object source, ElapsedEventArgs e)
        {
            starsDB database = new starsDB();
            bool distroStatus = database.checkDistroComplete();
            Console.WriteLine(distroStatus);
            if (distroStatus == true)
            {
                liveLotto = new lottery();
            }

        }
    }
}
