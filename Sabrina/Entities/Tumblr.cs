// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using ImgurOffline;
//
//    var tumblrPost = TumblrPost.FromJson(jsonString);

// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using ImgurOffline;
//
//    var tumblrPost = TumblrPost.FromJson(jsonString);

namespace Sabrina.Entities.TumblrBlog
{
    // To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
    // using ImgurOffline;
    // var tumblrBlog = TumblrBlog.FromJson(jsonString);

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Globalization;
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public static class Serialize
    {
        public static string ToJson(this TumblrBlog self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public partial class Blog
    {
        [J("ask")] public bool Ask { get; set; }
        [J("ask_anon")] public bool AskAnon { get; set; }
        [J("description")] public string Description { get; set; }
        [J("likes")] public long Likes { get; set; }
        [J("name")] public string Name { get; set; }
        [J("posts")] public long Posts { get; set; }
        [J("title")] public string Title { get; set; }
        [J("updated")] public long Updated { get; set; }
        [J("url")] public string Url { get; set; }
    }

    public partial class Meta
    {
        [J("msg")] public string Msg { get; set; }
        [J("status")] public long Status { get; set; }
    }

    public partial class Response
    {
        [J("blog")] public Blog Blog { get; set; }
    }

    public partial class TumblrBlog
    {
        [J("meta")] public Meta Meta { get; set; }
        [J("response")] public Response Response { get; set; }
    }

    public partial class TumblrBlog
    {
        public static TumblrBlog FromJson(string json) => JsonConvert.DeserializeObject<TumblrBlog>(json, Converter.Settings);
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

namespace Sabrina.Entities.TumblrPost
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Globalization;
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public static class Serialize
    {
        public static string ToJson(this TumblrPost self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public partial class Exif
    {
        [JsonProperty("Camera")]
        public string Camera { get; set; }
    }

    public partial class Meta
    {
        [J("msg")] public string Msg { get; set; }
        [J("status")] public long Status { get; set; }
    }

    public partial class Photo
    {
        [JsonProperty("alt_sizes")]
        public Size[] AltSizes { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }

        [JsonProperty("exif")]
        public Exif Exif { get; set; }

        [JsonProperty("original_size")]
        public Size OriginalSize { get; set; }
    }

    public partial class Post
    {
        [J("blog_name")]
        public string BlogName { get; set; }

        [J("can_like")]
        public bool CanLike { get; set; }

        [J("can_reblog")]
        public bool CanReblog { get; set; }

        [J("can_reply")]
        public bool CanReply { get; set; }

        [J("can_send_in_message")]
        public bool CanSendInMessage { get; set; }

        [J("caption")]
        public string Caption { get; set; }

        [J("date")]
        public string Date { get; set; }

        [J("display_avatar")]
        public bool DisplayAvatar { get; set; }

        [J("format")]
        public string Format { get; set; }

        [J("id")]
        public long Id { get; set; }

        [J("image_permalink")]
        public string ImagePermalink { get; set; }

        [J("is_blocks_post_format")]
        public bool IsBlocksPostFormat { get; set; }

        [J("note_count")]
        public long NoteCount { get; set; }

        [J("photos")]
        public Photo[] Photos { get; set; }

        [J("post_url")]
        public string PostUrl { get; set; }

        [J("reblog")]
        public Reblog Reblog { get; set; }

        [J("reblog_key")]
        public string ReblogKey { get; set; }

        [J("recommended_color")]
        public object RecommendedColor { get; set; }

        [J("recommended_source")]
        public object RecommendedSource { get; set; }

        [J("short_url")]
        public string ShortUrl { get; set; }

        [J("slug")]
        public string Slug { get; set; }

        [J("state")]
        public string State { get; set; }

        [J("summary")]
        public string Summary { get; set; }

        [J("tags")]
        public string[] Tags { get; set; }

        [J("timestamp")]
        public long Timestamp { get; set; }

        [J("trail")]
        public object[] Trail { get; set; }

        [J("type")]
        public string Type { get; set; }
    }

    public partial class Reblog
    {
        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("tree_html")]
        public string TreeHtml { get; set; }
    }

    public partial class Response
    {
        [J("posts")] public Post[] Posts { get; set; }
        [J("total_posts")] public long TotalPosts { get; set; }
    }

    public partial class Size
    {
        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }
    }

    public partial class TumblrPost
    {
        [J("meta")] public Meta Meta { get; set; }
        [J("response")] public Response Response { get; set; }
    }

    public partial class TumblrPost
    {
        public static TumblrPost FromJson(string json) => JsonConvert.DeserializeObject<TumblrPost>(json, Converter.Settings);
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