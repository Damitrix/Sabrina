using Configuration;
using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Sabrina.Entities
{
    public class SlaveReport
    {
        public DateTime TimeOfReport = DateTime.Now;
        public ulong UserID;
        public int Edges = 0;

        [XmlIgnore]
        public TimeSpan timeSpan
        {
            get
            {
                return TimeSpan.FromTicks(TimeSpanTicks);
            }
            set
            {
                TimeSpanTicks = value.Ticks;
            }
        }

        public Outcome SessionOutcome = Outcome.denial;

        public long TimeSpanTicks;

        private SlaveReport()
        {
        }

        public SlaveReport(DateTime datetime, DiscordUser user, int edges, TimeSpan time, Outcome outcome)
        {
            TimeOfReport = datetime;
            UserID = user.Id;
            Edges = edges;
            timeSpan = time;
            SessionOutcome = outcome;
        }

        public SlaveReport(DateTime datetime, DiscordUser user, int edges, TimeSpan time, string outcome) : this(datetime, user, edges, time, (Outcome)Enum.Parse(typeof(Outcome), outcome))
        {
        }

        public void Save()
        {
            var fileName = $"{UserID}_{TimeOfReport.ToString("MM_dd_yyyy_HH_mm_ss")}";
            var fileLocation = $"{Config.BotFileFolders.SlaveReports}/{fileName}.xml";

            XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(SlaveReport) })[0];

            using (var stream = File.Create(fileLocation))
            {
                xmlSerializer.Serialize(stream, this);
            }
        }

        /// <summary>
        /// Loads a specific Slave Report
        /// </summary>
        /// <param name="fileLocation"></param>
        /// <returns></returns>
        private async static Task<SlaveReport> Load(string fileLocation)
        {
            return await Task.Run(() => DeserializeReport(fileLocation));
        }

        private static SlaveReport DeserializeReport(string fileLocation)
        {
            using (var reader = File.OpenRead(fileLocation))
            {
                using (XmlReader xmlReader = XmlReader.Create(reader))
                {
                    XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(SlaveReport) })[0];
                    SlaveReport report = (SlaveReport)xmlSerializer.Deserialize(xmlReader);
                    return report;
                }
            }
        }

        /// <summary>
        /// Load all Slave Reports from a User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<SlaveReport[]> Load(DiscordUser user)
        {
            List<SlaveReport> outReports = new List<SlaveReport>();
            foreach (var file in Directory.GetFiles(Config.BotFileFolders.SlaveReports))
            {
                outReports.Add(await Load(file));
            }

            return outReports.ToArray();
        }

        /// <summary>
        /// Load all Slave reports between two Dates
        /// </summary>
        /// <param name="user"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<SlaveReport[]> Load(DiscordUser user, DateTime startDate, DateTime endDate)
        {
            var reports = await Load(user);
            var outReports = new List<SlaveReport>();
            foreach (var report in reports)
            {
                if (report.TimeOfReport > startDate && report.TimeOfReport < endDate)
                {
                    outReports.Add(report);
                }
            }

            return outReports.ToArray();
        }

        /// <summary>
        /// Load all SlaveReports between specified Dates
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static async Task<SlaveReport[]> Load(DateTime startDate, DateTime endDate)
        {
            var allReports = new List<SlaveReport>();
            foreach (string file in Directory.GetFiles(Config.BotFileFolders.SlaveReports))
            {
                allReports.Add(await Load(file));
            }

            var outReports = new List<SlaveReport>();
            foreach (var report in allReports)
            {
                if (report.TimeOfReport > startDate && report.TimeOfReport < endDate)
                {
                    outReports.Add(report);
                }
            }

            return outReports.ToArray();
        }

        [Flags]
        public enum Outcome
        {
            denial = 1,
            ruin = 2,
            orgasm = 4,
            task = 8
        }
    }
}