using System;
using System.IO;

namespace Configuration
{
	public static class Config
	{
		public static string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "Config.cfg");
		private static string _adminConnectionString;
		private static string _cDir;
		private static string _databaseConnectionString;
		private static string _sankakuConnectionString;
		private static string _sankakuLogin;
		private static string _sankakuPassword;
		private static string _token;

		public static string AdminConnectionString
		{
			get
			{
				if (_adminConnectionString == null)
				{
					Load();
				}

				return _adminConnectionString;
			}
		}

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

		public static string SankakuConnectionString
		{
			get
			{
				if (_sankakuConnectionString == null)
				{
					Load();
				}

				return _sankakuConnectionString;
			}
		}

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

		private static string CDir => _cDir ?? (_cDir = Directory.GetCurrentDirectory());

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

					case "SankakuConnectionString":
						_sankakuConnectionString = split[1];
						break;

					case "AdminConnectionString":
						_adminConnectionString = split[1];
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
			private static string _mainFolder;
			private static string _media;
			private static string _slaveReports;
			private static string _userData;
			private static string _wheelLinks;
			private static string _wheelResponses;

			public static string MainFolder => _mainFolder ?? (_mainFolder = Path.Combine(CDir, "BotFiles"));
			public static string Media => _media ?? (_media = Path.Combine(MainFolder, "Media"));
			public static string SlaveReports => _slaveReports ?? (_slaveReports = Path.Combine(MainFolder, "SlaveReports"));
			public static string UserData => _userData ?? (_userData = Path.Combine(MainFolder, "UserData"));
			public static string WheelLinks => _wheelLinks ?? (_wheelLinks = Path.Combine(MainFolder, "Links"));
			public static string WheelResponses => _wheelResponses ?? (_wheelResponses = Path.Combine(MainFolder, "WheelResponses"));
		}

		public static class Emojis
		{
			public static string Blush = ":blush:";
			public static string[] Confirms = new[] { ":white_check_mark:", ":ballot_box_with_check:", ":heavy_check_mark:", ":thumbsup:", ":+1:", ":arrow_up:", ":arrow_up_small:" };
			public static string[] Declines = new[] { ":negative_squared_cross_mark:", ":x:", ":no_entry_sign:", ":thumbsdown:", ":-1:", ":arrow_down:", ":arrow_down_small:" };
			public static string[] Hate = new[] { ":skull_crossbones:", ":deletdis:" };
			public static string[] Love = new[] { ":heart:", ":blue_heart:", ":green_heart:", ":purple_heart:", ":yellow_heart:" };
			public static string Smug = ":smug:";
			public static string Underage = ":underage:";
		}

		public static class Users
		{
			public const ulong Aki = 335437183127257089ul;
			public const ulong Salem = 249216025931939841ul;
		}
	}
}