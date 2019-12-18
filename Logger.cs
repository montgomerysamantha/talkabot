using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Twitch
{
    public class Logger
    {
        Mutex mtx = new Mutex();
        private string logFile = "logs.txt";

        public Logger()
        {
            //if (!File.Exists(logFile))
                //File.Create(logFile);
        }

        public void PushError(string message)
        {
            var time = DateTime.Now;

            mtx.WaitOne();
            using (StreamWriter sw = File.AppendText("logs.txt"))
            {
                sw.WriteLine($"[{time.ToString()}] {message}\n");
            }
            mtx.ReleaseMutex();
        }
    }
}
