using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Twitch
{
    public class ConfigObject
    {
        public string username;
        public string oauth;
        public string channel;

        public List<Command> commandList = new List<Command>();
    }

    public static class ConfigManager
    {
        public static void SaveConfig(List<Command> commandList, string username, string oauth, string channel)
        {
            using(FileStream fs = new FileStream("Config.xml", FileMode.Create, FileAccess.Write))
            {
                XmlSerializer xs = new XmlSerializer(typeof(ConfigObject));

                ConfigObject obj = new ConfigObject();

                obj.oauth = oauth;
                obj.username = username;
                obj.channel = channel;
                obj.commandList = commandList;

                xs.Serialize(fs, obj);
            }
        }

        public static ConfigObject LoadConfig()
        {
            ConfigObject obj = new ConfigObject();

            try
            {
                using (FileStream fs = new FileStream("Config.xml", FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ConfigObject));

                    obj = (ConfigObject)xs.Deserialize(fs);
                    foreach(Command cmd in obj.commandList)
                    {
                        cmd.cooldown = new TimeSpan(cmd.ticks);
                    }
                }
            }
            catch(FileNotFoundException e)
            {
                
            }

            return obj;
        }
    }
}
