using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;

namespace ImageViewer.Model
{
    public static class Settings
    {
        private static string settingsFullName { get; } = ConfigurationManager.AppSettings["JsonSettingsFile"];

        public static bool IsAutoPlay { get; set; }

        public static int AutoPlaySec { get; set; }

        public static bool IsCyclePlay { get; set; }

        static Settings()
        {
            if (!File.Exists(settingsFullName))
            {
                Set s = new Set();
                s.IsAutoPlay = true;
                s.AutoPlaySec = 3;
                s.IsCyclePlay = true;

                File.WriteAllText(settingsFullName, JsonConvert.SerializeObject(s));
            }

            using (StreamReader sr = new StreamReader(settingsFullName))
            {
                string json = sr.ReadToEnd();
                Set s = JsonConvert.DeserializeObject<Set>(json);

                IsAutoPlay = s.IsAutoPlay;
                AutoPlaySec = s.AutoPlaySec;
                IsCyclePlay = s.IsCyclePlay;

                sr.Close();
                sr.Dispose();
            }
        }

        public static void Save(bool _isAutoPlay, int _autoPlaySec, bool _isCyclePlay)
        {
            IsAutoPlay = _isAutoPlay;
            AutoPlaySec = _autoPlaySec;
            IsCyclePlay = _isCyclePlay;

            Set s = new Set();
            s.IsAutoPlay = IsAutoPlay;
            s.AutoPlaySec = AutoPlaySec;
            s.IsCyclePlay = IsCyclePlay;

            File.WriteAllText(settingsFullName, JsonConvert.SerializeObject(s));
        }
    }

    public class Set
    {
        public bool IsAutoPlay { get; set; }

        public int AutoPlaySec { get; set; }

        public bool IsCyclePlay { get; set; }
    }
}
