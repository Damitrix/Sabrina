using System;
using System.IO;

namespace Configuration
{
    public static class Config
    {
        private static string _cDir;
        private static string CDir => _cDir ?? (_cDir = Directory.GetCurrentDirectory());

        private static string _databaseConnectionString;
        public static string DatabaseConnectionString
        {
            get
            {
                if (_databaseConnectionString == null)
                {
                    Load();
                }

                return _databaseConnectionString;
            }
        }

        private static string _token;
        public static string Token
        {
            get
            {
                if (_token == null)
                {
                    Load();
                }

                return _token;
            }
        }

        private static string _sankakuLogin;
        public static string SankakuLogin
        {
            get
            {
                if (_sankakuLogin == null)
                {
                    Load();
                }

                return _sankakuLogin;
            }
        }

        private static string _sankakuPassword;
        public static string SankakuPassword
        {
            get
            {
                if (_sankakuPassword == null)
                {
                    Load();
                }

                return _sankakuPassword;
            }
        }

        public static string ConfigPath = Directory.GetCurrentDirectory() + "/Config.cfg";

        public static void Load()
        {
            if (!File.Exists(ConfigPath))
            {
                throw new FileNotFoundException("Cannot find Config Path");
            }

            var config = File.ReadAllLines(ConfigPath);
            foreach (var line in config)
            {
                var split = line.Split(new string[] { " = " }, StringSplitOptions.None);

                switch (split[0])
                {
                    case "DatabaseConnectionString":
                        _databaseConnectionString = split[1];
                        break;
                    case "Token":
                        _token = split[1];
                        break;
                    case "SankakuLogin":
                        _sankakuLogin = split[1];
                        break;
                    case "SankakuPassword":
                        _sankakuPassword = split[1];
                        break;
                }
            }
        }



        public static class BotFileFolders
        {
            private static string _slaveReports;
            private static string _userData;
            private static string _wheelLinks;
            private static string _wheelResponses;
            private static string _mainFolder;

            public static string SlaveReports => _slaveReports ?? (_slaveReports = Path.Combine(MainFolder, "SlaveReports"));
            public static string UserData => _userData ?? (_userData = Path.Combine(MainFolder, "UserData"));
            public static string WheelLinks => _wheelLinks ?? (_wheelLinks = Path.Combine(MainFolder, "Links"));
            public static string WheelResponses => _wheelResponses ?? (_wheelResponses = Path.Combine(MainFolder, "WheelResponses"));
            public static string MainFolder => _mainFolder ?? (_mainFolder = Path.Combine(CDir, "BotFiles"));
        }

        public static class Channels
        {
            public const string Instruction = "slave-instruction";
            public const ulong InstructionUlong = 426753357647183872;
            public const string Wheel = "wheel-of-misfortune";
            public const ulong WheelUlong = 449840094736678912;
            public const ulong TestChannelUlong = 450793619398459432;
        }

        public static class Emojis
        {
            public static string Blush = ":blush:";
            public static string[] Confirms = new[] { ":white_check_mark:", ":ballot_box_with_check:", ":heavy_check_mark:", ":thumbsup:", ":+1:", ":arrow_up:", ":arrow_up_small:" };
            public static string[] Declines = new[] { ":negative_squared_cross_mark:", ":x:", ":no_entry_sign:", ":thumbsdown:", ":-1:", ":arrow_down:", ":arrow_down_small:" };
            public static string Underage = ":underage:";
            public static string[] Love = new[] { ":heart:", ":blue_heart:", ":green_heart:", ":purple_heart:", ":yellow_heart:" };
            public static string[] Hate = new[] { ":skull_crossbones:", ":deletdis:" };
        }

        public static class Pornhub
        {
            public static string[] Channels = new[] { "youranimeaddiction", "damitrix", "mosbles", "milkduds46", "elukajoi", "pjsx", "konekosalem" };

            public static string IndexedVideoLocation = $"{BotFileFolders.MainFolder}/PornhubVideos";

            public static ulong[] ChannelsToPostTo
            {
                get
                {
#if (DEBUG)
                    return new[] { 450793619398459432ul };
#else
                    return new[] { 457465277395894273ul };
#endif
                }
            }
        }

        public static class Tumblr
        {
            public static ulong[] ChannelsToPostTo = new[] { 448417831067975680ul };
            public static string Url = "deliciousanimefeet.tumblr.com";
        }

        public static class Users
        {
            public const ulong Aki = 335437183127257089ul;
            public const ulong Salem = 249216025931939841ul;
        }
    }
}