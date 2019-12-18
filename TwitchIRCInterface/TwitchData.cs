using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitch
{
    public class Tags
    {
        public string userId = "0";

        public bool isMod = false;
        public bool isTurbo = false;
        public bool isSubscriber = false;

        public string color = "0";
    }
    public class TwitchData
    {
        public Tags tags;
        public string username = null;
        public string message = null;
        public string messageId = null;
        public TwitchData()
        {
            tags = new Tags();
        }

        public TwitchData(string user, string m, string chatcolor)
        {
            tags = new Tags();
            username = user;
            message = m;
            tags.color = chatcolor;
        }
    }
}
