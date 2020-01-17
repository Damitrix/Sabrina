using Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Sabrina.Entities.Persistent
{
	public class Link
	{
		public string CreatorName;

		public string FileName;

		public ContentType Type;

		public string Url;

		public enum ContentType
		{
			Video,
			Picture
		}

		public static async Task<List<Link>> LoadAll()
		{
			List<Link> allLinks = new List<Link>();
			var WheelLinks = Path.Combine(Config.BotFileFolders.WheelResponses, "Links");

			if (!Directory.Exists(WheelLinks))
			{
				Directory.CreateDirectory(WheelLinks);
			}

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

			string fileLocation;
			do
			{
				fileLocation = $"{Config.BotFileFolders.WheelLinks}/{fileId}.xml";
				fileId++;
			} while (File.Exists(fileLocation));

			XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(Link) })[0];

			using FileStream stream = File.Create(fileLocation);
			xmlSerializer.Serialize(stream, this);
		}

		private static Link DeserializeLink(string fileLocation)
		{
			using FileStream reader = File.OpenRead(fileLocation);
			using XmlReader xmlReader = XmlReader.Create(reader);
			XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(Link) })[0];
			Link link = (Link)xmlSerializer.Deserialize(xmlReader);
			link.FileName = Path.GetFileNameWithoutExtension(fileLocation);
			return link;
		}

		private static async Task<Link> Load(string fileLocation)
		{
			return await Task.Run(() => DeserializeLink(fileLocation));
		}
	}
}