﻿namespace Sabrina.Entities.Sankaku
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using J = Newtonsoft.Json.JsonPropertyAttribute;
    using R = Newtonsoft.Json.Required;
    using N = Newtonsoft.Json.NullValueHandling;
    using System.Runtime.Serialization;

    public partial class Image
    {
        [J("id")] public long Id { get; set; }
        [J("rating")] public Rating Rating { get; set; }
        [J("status")] public Status Status { get; set; }
        [J("author")] public Author Author { get; set; }
        [J("sample_url")] public Uri SampleUrl { get; set; }
        [J("sample_width")] public long SampleWidth { get; set; }
        [J("sample_height")] public long SampleHeight { get; set; }
        [J("preview_url")] public Uri PreviewUrl { get; set; }
        [J("preview_width")] public long PreviewWidth { get; set; }
        [J("preview_height")] public long PreviewHeight { get; set; }
        [J("file_url")] public Uri FileUrl { get; set; }
        [J("width")] public long Width { get; set; }
        [J("height")] public long Height { get; set; }
        [J("file_size")] public long FileSize { get; set; }
        [J("file_type", NullValueHandling = N.Ignore)] public FileType? FileType { get; set; }
        [J("created_at")] public CreatedAt CreatedAt { get; set; }
        [J("has_children")] public bool HasChildren { get; set; }
        [J("has_comments")] public bool HasComments { get; set; }
        [J("has_notes")] public bool HasNotes { get; set; }
        [J("is_favorited")] public bool IsFavorited { get; set; }
        [J("md5")] public string Md5 { get; set; }
        [J("parent_id")] public long? ParentId { get; set; }
        [J("change")] public long Change { get; set; }
        [J("fav_count")] public long FavCount { get; set; }
        [J("recommended_posts")] [JsonConverter(typeof(DecodingChoiceConverter))] public long RecommendedPosts { get; set; }
        [J("recommended_score")] public long RecommendedScore { get; set; }
        [J("vote_count")] public long VoteCount { get; set; }
        [J("total_score")] public long TotalScore { get; set; }
        [J("source")] public string Source { get; set; }
        [J("in_visible_pool")] public bool InVisiblePool { get; set; }
        [J("is_premium")] public bool IsPremium { get; set; }
        [J("tags")] public List<Tag> Tags { get; set; }
    }

    public partial class Author
    {
        [J("id")] public long Id { get; set; }
        [J("name")] public string Name { get; set; }
        [J("avatar")] public Uri Avatar { get; set; }
        [J("avatar_rating")] public Rating AvatarRating { get; set; }
    }

    public partial class CreatedAt
    {
        [J("json_class")] public JsonClass JsonClass { get; set; }
        [J("s")] public long S { get; set; }
        [J("n")] public long N { get; set; }
    }

    public partial class Tag
    {
        [J("id")] public long Id { get; set; }
        [J("name")] public string Name { get; set; }
        [J("name_ja")] public string NameJa { get; set; }
        [J("type")] public long Type { get; set; }
        [J("count")] public long Count { get; set; }
    }

    public enum Rating { E, Q, S };

    public enum JsonClass { Time };

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FileType
    {
        [EnumMember(Value = "image/gif")]
        ImageGif,
        [EnumMember(Value = "image/jpeg")]
        ImageJpeg,
        [EnumMember(Value = "image/png")]
        ImagePng,
        [EnumMember(Value = "video/webm")]
        VideoWebm,
        [EnumMember(Value = "video/mp4")]
        VideoMp4
    };

    public enum Status
    {
        Active,
        Pending,
        Flagged
    };
    
    public partial class Image
    {
        public static List<Image> FromJson(string json) => JsonConvert.DeserializeObject<List<Image>>(json, Sabrina.Entities.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<Image> self) => JsonConvert.SerializeObject(self, Sabrina.Entities.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                RatingConverter.Singleton,
                JsonClassConverter.Singleton,
                FileTypeConverter.Singleton,
                StatusConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class RatingConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Rating) || t == typeof(Rating?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "e":
                    return Rating.E;
                case "q":
                    return Rating.Q;
                case "s":
                    return Rating.S;
            }
            throw new Exception("Cannot unmarshal type Rating");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Rating)untypedValue;
            switch (value)
            {
                case Rating.E:
                    serializer.Serialize(writer, "e");
                    return;
                case Rating.Q:
                    serializer.Serialize(writer, "q");
                    return;
                case Rating.S:
                    serializer.Serialize(writer, "s");
                    return;
            }
            throw new Exception("Cannot marshal type Rating");
        }

        public static readonly RatingConverter Singleton = new RatingConverter();
    }

    internal class JsonClassConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(JsonClass) || t == typeof(JsonClass?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "Time")
            {
                return JsonClass.Time;
            }
            throw new Exception("Cannot unmarshal type JsonClass");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (JsonClass)untypedValue;
            if (value == JsonClass.Time)
            {
                serializer.Serialize(writer, "Time");
                return;
            }
            throw new Exception("Cannot marshal type JsonClass");
        }

        public static readonly JsonClassConverter Singleton = new JsonClassConverter();
    }

    internal class FileTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(FileType) || t == typeof(FileType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "image/gif":
                    return FileType.ImageGif;
                case "image/jpeg":
                    return FileType.ImageJpeg;
                case "image/png":
                    return FileType.ImagePng;
                case "video/webm":
                    return FileType.VideoWebm;
                case "video/mp4":
                    return FileType.VideoMp4;
            }
            throw new Exception("Cannot unmarshal type FileType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (FileType)untypedValue;
            switch (value)
            {
                case FileType.ImageGif:
                    serializer.Serialize(writer, "image/gif");
                    return;
                case FileType.ImageJpeg:
                    serializer.Serialize(writer, "image/jpeg");
                    return;
                case FileType.ImagePng:
                    serializer.Serialize(writer, "image/png");
                    return;
                case FileType.VideoWebm:
                    serializer.Serialize(writer, "video/webm");
                    return;
                case FileType.VideoMp4:
                    serializer.Serialize(writer, "video/mp4");
                    return;
            }
            throw new Exception("Cannot marshal type FileType");
        }

        public static readonly FileTypeConverter Singleton = new FileTypeConverter();
    }

    internal class DecodingChoiceConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return integerValue;
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    long l;
                    if (Int64.TryParse(stringValue, out l))
                    {
                        return l;
                    }
                    break;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value);
            return;
        }

        public static readonly DecodingChoiceConverter Singleton = new DecodingChoiceConverter();
    }
    
    internal class StatusConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Status) || t == typeof(Status?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "active")
            {
                return Status.Active;
            }
            if (value == "pending")
            {
                return Status.Pending;
            }
            if (value == "flagged")
            {
                return Status.Flagged;
            }
            throw new Exception("Cannot unmarshal type Status");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Status)untypedValue;
            if (value == Status.Active)
            {
                serializer.Serialize(writer, "active");
                return;
            }
            if (value == Status.Pending)
            {
                serializer.Serialize(writer, "pending");
                return;
            }
            if (value == Status.Flagged)
            {
                serializer.Serialize(writer, "flagged");
                return;
            }
            throw new Exception("Cannot marshal type Status");
        }

        public static readonly StatusConverter Singleton = new StatusConverter();
    }
}