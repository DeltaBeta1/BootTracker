using System;
using System.IO;
using System.Xml.Serialization;

namespace BootTracker.Helpers
{
    [Serializable]
    public class AppConfig
    {
        public bool FirstRun { get; set; }

        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BootTracker");

        private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.xml");

        public AppConfig()
        {
            FirstRun = true;
        }

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    var serializer = new XmlSerializer(typeof(AppConfig));
                    using (var reader = new StreamReader(ConfigFile))
                    {
                        return (AppConfig)serializer.Deserialize(reader);
                    }
                }
            }
            catch { }

            var config = new AppConfig();
            config.Save();
            return config;
        }

        public void Save()
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                    Directory.CreateDirectory(ConfigDir);

                var serializer = new XmlSerializer(typeof(AppConfig));
                using (var writer = new StreamWriter(ConfigFile))
                {
                    serializer.Serialize(writer, this);
                }
            }
            catch { }
        }
    }
}
