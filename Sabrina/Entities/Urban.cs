namespace Sabrina.Entities
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public static class Serialize
    {
        public static string ToJson(this Urban self) => JsonConvert.SerializeObject(self, Sabrina.Entities.Converter.Settings);
    }

    public partial class List
    {
        [J("author")] public string Author { get; set; }
        [J("current_vote")] public string CurrentVote { get; set; }
        [J("defid")] public long Defid { get; set; }
        [J("definition")] public string Definition { get; set; }
        [J("example")] public string Example { get; set; }
        [J("permalink")] public Uri Permalink { get; set; }
        [J("sound_urls")] public List<object> SoundUrls { get; set; }
        [J("thumbs_down")] public long ThumbsDown { get; set; }
        [J("thumbs_up")] public long ThumbsUp { get; set; }
        [J("word")] public string Word { get; set; }
        [J("written_on")] public DateTimeOffset WrittenOn { get; set; }
    }

    public partial class Urban
    {
        [J("list")] public List<List> List { get; set; }
    }

    public partial class Urban
    {
        public static Urban FromJson(string json) => JsonConvert.DeserializeObject<Urban>(json, Sabrina.Entities.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}