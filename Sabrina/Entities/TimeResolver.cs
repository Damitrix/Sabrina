using System;
using System.Text.RegularExpressions;

namespace Sabrina.Entities
{
	internal static class TimeResolver
	{
		public static TimeSpan GetTimeSpan(string time)
		{
			time = time.ToUpperInvariant();
			TimeSpan outtime = new TimeSpan();

			string[] split = Regex.Split(time, @"(?<=[smhdw])", RegexOptions.IgnoreCase);

			foreach (string stringTime in split)
			{
				if (string.IsNullOrWhiteSpace(stringTime))
				{
					continue;
				}

				bool isParceville = int.TryParse(stringTime[0..^1], out int timeNumber);

				if (!isParceville)
				{
					throw new InvalidCastException("Entered Number is not valid");
				}

				// Switch the last character
				outtime += (stringTime[^1]) switch
				{
					'S' => new TimeSpan(0, 0, timeNumber),

					'M' => new TimeSpan(0, timeNumber, 0),

					'H' => new TimeSpan(timeNumber, 0, 0),

					'D' => new TimeSpan(timeNumber, 0, 0, 0),

					'W' => new TimeSpan(timeNumber * 7, 0, 0, 0),

					_ => throw new InvalidCastException("Entered Number is not valid"),
				};
			}

			return outtime;
		}

		public static string TimeToString(TimeSpan time)
		{
			string timeString = string.Empty;

			if (time.Days > 0)
			{
				timeString += $"{time.Days} days";
				if (time.Hours > 0 && time.Minutes > 0)
				{
					timeString += ", ";
				}
				else if (time.Hours > 0 || time.Minutes > 0)
				{
					timeString += " and ";
				}
			}

			if (time.Hours > 0)
			{
				timeString += $"{time.Hours} hours";
				if (time.Minutes > 0)
				{
					timeString += " and ";
				}
			}

			if (time.Minutes > 0)
			{
				timeString += $"{time.Minutes} minutes";
			}

			if (time < TimeSpan.FromSeconds(60))
			{
				timeString += $"{time.Seconds} seconds";
			}

			return timeString;
		}
	}
}