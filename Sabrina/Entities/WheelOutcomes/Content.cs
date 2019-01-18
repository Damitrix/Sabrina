// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Content.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   The content outcome. Delivers your ultimate Outcome for the next few hours. With a pic ;D
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Entities.WheelOutcomes
{
    using DSharpPlus.Entities;
    using Sabrina.Entities.Persistent;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// The content outcome. Delivers your ultimate Outcome for the next few hours. With a pic ;D
    /// </summary>
    internal sealed class Content : WheelOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Content"/> class.
        /// </summary>
        /// <param name="outcome">
        /// The outcome.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public Content(
            WheelExtension.Outcome outcome,
            Models.UserSettings settings, Models.DiscordContext context)
            : base(outcome, settings, context)
        {
            var denialtext = "please don't break the Bot";

            switch (outcome)
            {
                case WheelExtension.Outcome.Edge:
                    denialtext = "Then spin again";
                    this.Outcome = WheelExtension.Outcome.Edge;
                    break;

                case WheelExtension.Outcome.Denial:
                    denialtext = "deny your orgasm";
                    this.DenialTime = new TimeSpan(8, 0, 0);
                    this.Outcome = WheelExtension.Outcome.Denial;
                    break;

                case WheelExtension.Outcome.Ruin:
                    denialtext = "ruin your orgasm";
                    this.Outcome = WheelExtension.Outcome.Ruin;
                    break;

                case WheelExtension.Outcome.Orgasm:
                    denialtext = "enjoy a full orgasm";
                    this.Outcome = WheelExtension.Outcome.Orgasm;
                    break;

                case WheelExtension.Outcome.Denial | WheelExtension.Outcome.Edge:
                    var chance = Helpers.RandomGenerator.RandomInt(0, 9);
                    if (chance < 5)
                    {
                        denialtext = "deny your orgasm";
                        this.DenialTime = new TimeSpan(8, 0, 0);
                        this.Outcome = WheelExtension.Outcome.Denial;
                    }
                    else
                    {
                        denialtext = "Then spin again";
                        this.Outcome = WheelExtension.Outcome.Edge;
                    }

                    break;
            }

            Link link;

            if (this.Outcome == WheelExtension.Outcome.Edge)
            {
                link = this.GetLinkFromRandomTumblr(this.GetPostCount());
            }
            else
            {
                List<Link> links = Link.LoadAll().Result;

                var randomLinkNr = Helpers.RandomGenerator.RandomInt(0, links.Count);

                if (links.Count <= randomLinkNr)
                {
                    link = new Link()
                    {
                        CreatorName = "Oops.",
                        FileName = "Salem forgot to update the Links again...",
                        Type = Link.ContentType.Picture,
                        Url = "https://Exception.com"
                    };
                }
                else
                {
                    link = links[randomLinkNr];
                }
            }

            var fullSentence = string.Empty;
            var rerollIn = string.Empty;

            switch (link.Type)
            {
                case Link.ContentType.Video:
                    fullSentence = $"Watch {link.CreatorName}' JOI. {denialtext}";
                    break;

                case Link.ContentType.Picture:
                    
                    if (outcome == WheelExtension.Outcome.Edge)
                    {
                        fullSentence = $"Edge to {link.CreatorName}' Picture and take a 30 second break. {denialtext}";
                        rerollIn = "Don't forget to take a break!";
                        this.WheelLockedTime = new TimeSpan(0, 0, 30);
                    }
                    else
                    {
                        fullSentence = $"Edge to {link.CreatorName}' Picture and {denialtext}";
                    }

                    break;
            }
            
            if (this.Outcome != WheelExtension.Outcome.Edge)
            {
                rerollIn = "You are not allowed to re-roll for now.";
                this.WheelLockedTime = new TimeSpan(8, 0, 0);
            }

            this.Text = $"{fullSentence}.{rerollIn}\n" + $"{link.Url}\n";

            var builder = new DiscordEmbedBuilder
            {
                Title = "Click here.",
                Description = fullSentence,
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = rerollIn },
                Url = link.Url,
                Color = link.Type == Link.ContentType.Picture
                                                              ? new DiscordColor("#42f483")
                                                              : new DiscordColor("#acf441"),
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = link.CreatorName
                }
            };

            if (link.Type == Link.ContentType.Picture)
            {
                builder.ImageUrl = link.Url;
            }

            this.Embed = builder.Build();
        }

        /// <summary>
        /// Gets or sets the chance to get this Outcome.
        /// </summary>
        public override int Chance { get; protected set; } = 80;

        /// <summary>
        /// Gets or sets the denial time.
        /// </summary>
        public override TimeSpan DenialTime { get; protected set; }

        /// <summary>
        /// Gets or sets the embed to display the user.
        /// </summary>
        public override DiscordEmbed Embed { get; protected set; }

        /// <summary>
        /// Gets or sets the outcome.
        /// </summary>
        public override WheelExtension.Outcome Outcome { get; protected set; }

        /// <summary>
        /// Gets or sets the text to display the user.
        /// </summary>
        public override string Text { get; protected set; }

        /// <summary>
        /// Gets or sets the wheel locked time.
        /// </summary>
        public override TimeSpan WheelLockedTime { get; protected set; }

        /// <summary>
        /// Get link from random tumblr.
        /// </summary>
        /// <param name="maxPostCount">
        /// The max post count.
        /// </param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        private Link GetLinkFromRandomTumblr(int maxPostCount)
        {
            string json;
            var url = @"http://api.tumblr.com/v2/blog/deliciousanimefeet.tumblr.com/posts/photo";
            url += "?api_key=uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki";
            url += "&limit=1";
            url += $"&offset={Helpers.RandomGenerator.RandomInt(0, maxPostCount - 1)}";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("api_key", "uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki");
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
            {
                json = reader.ReadToEnd();
            }

            var post = TumblrPost.TumblrPost.FromJson(json);

            while (post == null || post.Response.Posts.Length == 0)
            {
                url = @"http://api.tumblr.com/v2/blog/deliciousanimefeet.tumblr.com/posts/photo";
                url += "?api_key=uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki";
                url += "&limit=1";
                url += $"&offset={Helpers.RandomGenerator.RandomInt(0, maxPostCount - 1)}";

                request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("api_key", "uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki");
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
                {
                    json = reader.ReadToEnd();
                }

                post = TumblrPost.TumblrPost.FromJson(json);
            }

            var link = new Link
            {
                CreatorName = post.Response.Posts[0].BlogName,
                Url = post.Response.Posts[0].Photos[0].AltSizes.OrderBy(e => e.Height).Last().Url,
                Type = Link.ContentType.Picture
            };

            return link;
        }

        /// <summary>
        /// Get Count of all Posts
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int GetPostCount()
        {
            string json;
            var url = @"http://api.tumblr.com/v2/blog/deliciousanimefeet.tumblr.com/info";
            url += "?api_key=uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("api_key", "uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki");
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            var blog = TumblrBlog.TumblrBlog.FromJson(json);
            return Convert.ToInt32(blog.Response.Blog.Posts);
        }
    }
}