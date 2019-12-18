using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitch
{
    public class TimerCommand : Command
    {
        private bool countdown; // true if countdown timer, false if countup timer
        public TimeSpan ts;

        /// <summary>
        /// Constructor for countdown timer.
        /// </summary>
        /// <param name="t">Title of timer (ex: !timer)</param>
        /// <param name="perm">Permission of timer.</param>
        /// <param name="hours">Hours remaining</param>
        /// <param name="minutes">Minutes remaining</param>
        /// <param name="seconds">Seconds remaining</param>
        public TimerCommand(string t, string perm, int hours, int minutes, int seconds)
        {
            title = t;
            permission = perm;
            timeLastUsed = new DateTime(1979, 07, 28, 22, 35, 5);
            cooldown = TimeSpan.Zero;
            countdown = true;
            
            output = $"Time remaining: ";
        }

        /// <summary>
        /// Constructor for counting up timer.
        /// </summary>
        /// <param name="t">Title of timer (ex: !timer)</param>
        /// <param name="perm">Permission of timer.</param>
        public TimerCommand(string t, string perm)
        {
            title = t;
            permission = perm;
            timeLastUsed = DateTime.Now;
            cooldown = TimeSpan.Zero;
            countdown = false;
            output = $"Time elasped: 00:00:00";
        }
    }
}
