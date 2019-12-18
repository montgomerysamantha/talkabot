using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitch
{
    public class Command
    {
        public string title, output, permission;
        public DateTime timeLastUsed;
        public TimeSpan cooldown;
        public TimeSpan timerlength;
        public DateTime timerstart;
        public long ticks;

        public Command() { }

        /// <summary>
        /// Default constructor for a command object
        /// </summary>
        /// <param name="t">the title of the command, ex: "!hello"</param>
        /// <param name="sortie">the output of the command, "sortie" is output in French b/c I'm bad with variable names</param>
        /// <param name="perm">the permission of the command, ex: mods, subscribers, everyone</param>
        public Command(string t, string sortie, string perm)
        {
            title = t;
            output = sortie;
            permission = perm;
            timeLastUsed = new DateTime(1979, 07, 28, 22, 35, 5);
            cooldown = TimeSpan.Zero;
            timerlength = TimeSpan.Zero;
            timerstart = new DateTime(1979, 07, 28, 22, 35, 5);
        }

        /// <summary>
        /// Other constructor for a command object, but with a cooldown TimeSpan as well. 
        /// </summary>
        /// <param name="t">the title of the command, ex: "!hello"</param>
        /// <param name="sortie">the output of the command, "sortie" is output in French b/c I'm bad with variable names</param>
        /// <param name="perm">the permission of the command, ex: mods, subscribers, everyone</param>
        /// <param name="cd">the TimeSpan cooldown</param>
        public Command(string t, string sortie, string perm, TimeSpan cd)
        {
            title = t;
            output = sortie;
            permission = perm;
            timeLastUsed = new DateTime(1979, 07, 28, 22, 35, 5); // use as default time last used
            cooldown = cd;
            ticks = cd.Ticks;
            timerlength = TimeSpan.Zero;
            timerstart = new DateTime(1979, 07, 28, 22, 35, 5);
        }

        /// <summary>
        /// Constructor for a countdown timer.  
        /// </summary>
        /// <param name="t">the title of the command, ex: "!hello"</param>
        /// <param name="sortie">the output of the command, "sortie" is output in French b/c I'm bad with variable names</param>
        /// <param name="perm">the permission of the command, ex: mods, subscribers, everyone</param>
        /// <param name="cd">the TimeSpan cooldown</param>
        /// <param name="ts">the TimeSpan for length of countdown timer</param>
        /// <param name="start">the DateTime for the start of countdown timer</param>
        public Command(string t, string sortie, string perm, TimeSpan cd, TimeSpan ts, DateTime start)
        {
            title = t;
            output = sortie;
            permission = perm;
            timeLastUsed = new DateTime(1979, 07, 28, 22, 35, 5); // use as default time last used
            cooldown = cd;
            ticks = cd.Ticks;
            timerlength = ts;
            timerstart = start;
        }

        /// <summary>
        /// Constructor for a count up timer.  
        /// </summary>
        /// <param name="t">the title of the command, ex: "!hello"</param>
        /// <param name="sortie">the output of the command, "sortie" is output in French b/c I'm bad with variable names</param>
        /// <param name="perm">the permission of the command, ex: mods, subscribers, everyone</param>
        /// <param name="cd">the TimeSpan cooldown</param>
        /// <param name="ts">the TimeSpan for length of count up timer (zero)</param>
        /// <param name="start">the DateTime for the start of count up timer</param>
        public Command(string t, string sortie, string perm, TimeSpan cd, DateTime start)
        {
            title = t;
            output = sortie;
            permission = perm;
            timeLastUsed = new DateTime(1979, 07, 28, 22, 35, 5); // use as default time last used
            cooldown = cd;
            ticks = cd.Ticks;
            timerlength = TimeSpan.Zero;
            timerstart = start;
        }
    }
}
