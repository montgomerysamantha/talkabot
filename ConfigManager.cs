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
        private static string configFileName = "Config.xml";

        public static void SaveConfig(List<Command> commandList, string username, string oauth, string channel)
        {
            using(FileStream fs = new FileStream(configFileName, FileMode.Create, FileAccess.Write))
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

        /// <summary>
        /// In charge of deserializing the config file into a ConfigObject class
        /// </summary>
        /// <param name="fs">The FileStream handle of the config file to be deserialized</param>
        /// <returns>The ConfigObject is returned if everything went well. Otherwise, null will be returned to signal an invalid file</returns>
        private static ConfigObject DeserializeConfig(FileStream fs)
        {
            XmlSerializer xs = new XmlSerializer(typeof(ConfigObject));

            try
            {
                ConfigObject configObj = (ConfigObject)xs.Deserialize(fs);
                foreach (Command cmd in configObj.commandList)
                {
                    cmd.cooldown = new TimeSpan(cmd.ticks);
                }

                return configObj;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public static ConfigObject LoadConfig()
        {
            try
            {
                using (FileStream fs = new FileStream(configFileName, FileMode.Open, FileAccess.Read))
                {
                    var configObject = DeserializeConfig(fs);
                    if(configObject == null)
                    {
                        File.Delete(configFileName);
                        return new ConfigObject();
                    }

                    return configObject;
                }
            }
            catch(FileNotFoundException)
            {
                return new ConfigObject();
            }
        }
    }
}
