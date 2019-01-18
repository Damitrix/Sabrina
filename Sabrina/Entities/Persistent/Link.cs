using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Configuration;

namespace Sabrina.Entities.Persistent
{
    public class Link
    {
        public enum ContentType
        {
            Video,
            Picture
        }

        public string CreatorName;
        public string FileName;
        public ContentType Type;
        public string Url;

        public static async Task<List<Link>> LoadAll()
        {
            List<Link> allLinks = new List<Link>();
            var CDir = $"{Directory.GetCurrentDirectory()}";
            var MainFolder = Path.Combine(CDir, "BotFiles");
            var WheelResponses = Path.Combine(MainFolder, "WheelResponses");
            var SlaveReports = Path.Combine(MainFolder, "SlaveReports");
            var UserData = Path.Combine(MainFolder, "UserData");
            var WheelLinks = Path.Combine(WheelResponses, "Links");


            foreach (var file in Directory.GetFiles(WheelLinks)) allLinks.Add(await Load(file));

            return allLinks;
        }

        public void Delete()
        {
            var fileLocation = $"{Config.BotFileFolders.WheelLinks}/{FileName}.xml";
            File.Delete(fileLocation);
        }

        public void Save()
        {
            var fileId = 0;

            var fileLocation = string.Empty;

            do
            {
                fileLocation = $"{Config.BotFileFolders.WheelLinks}/{fileId}.xml";
                fileId++;
            } while (File.Exists(fileLocation));

            XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] {typeof(Link)})[0];

            using (FileStream stream = File.Create(fileLocation))
            {
                xmlSerializer.Serialize(stream, this);
            }
        }

        private static Link DeserializeLink(string fileLocation)
        {
            using (FileStream reader = File.OpenRead(fileLocation))
            {
                using (XmlReader xmlReader = XmlReader.Create(reader))
                {
                    XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] {typeof(Link)})[0];
                    Link link = (Link) xmlSerializer.Deserialize(xmlReader);
                    link.FileName = Path.GetFileNameWithoutExtension(fileLocation);
                    return link;
                }
            }
        }

        private static async Task<Link> Load(string fileLocation)
        {
            return await Task.Run(() => DeserializeLink(fileLocation));
        }
    }
}