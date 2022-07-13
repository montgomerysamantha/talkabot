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
        /// In charge of deserializing the config file and handling exception for this task
        /// </summary>
        /// <param name="fs">The FileStream handle which points to the file to be processed as the config file</param>
        /// <returns>If the task is a success, ConfigObject will be returned. Otherwise, null will be returned to signal an error</returns>
        private static ConfigObject DeserializeConfigFile(FileStream fs)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(ConfigObject));

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

        /// <summary>
        /// Handles the loading of the config file which saves the username and oauth token
        /// </summary>
        /// <returns>ConfigObject</returns>
        public static ConfigObject LoadConfig()
        {
            try
            {
                using (FileStream fs = new FileStream(configFileName, FileMode.Open, FileAccess.Read))
                {
                    // Deserialize and check if it failed. If it did, just delete the old file since it's invalid
                    var configObject = DeserializeConfigFile(fs);
                    if(configObject == null)
                    {
                        // Close the handle before deleting it to gain access
                        fs.Close();
                        File.Delete(configFileName);

                        // Just return a new object to start a brand new session
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
