// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Content.cs" company="SalemsTools">
//     Do whatever
// </copyright>
// <summary>
// The content outcome. Delivers your ultimate Outcome for the next few hours. With a pic ;D
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Sabrina.Bots;

namespace Sabrina.Entities.WheelOutcomes
{
	using DSharpPlus.Entities;
	using Sabrina.Entities.Persistent;
	using Sabrina.Models;
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Threading.Tasks;
	using WheelOutcome = Persistent.WheelOutcome;

	/// <summary>
	/// The content outcome. Delivers your ultimate Outcome for the next few hours. With a pic ;D
	/// </summary>
	internal sealed class Content : WheelOutcome
	{
		private WaifuJoi.Shared.Models.Content Image;

		/// <summary>
		/// Initializes a new instance of the <see cref="Content"/> class.
		/// </summary>
		/// <param name="outcome">The outcome.</param>
		/// <param name="settings">The settings.</param>
		public Content(
			UserSetting.Outcome outcome,
			Dictionary<UserSetting.SettingID, UserSetting> settings, List<WheelUserItem> items, IServiceProvider services)
			: base(outcome, settings, items, services)
		{
		}

		private Content()
		{
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
		public override UserSetting.Outcome Outcome { get; protected set; }

		/// <summary>
		/// Gets or sets the text to display the user.
		/// </summary>
		public override string Text { get; protected set; }

		/// <summary>
		/// Gets or sets the wheel locked time.
		/// </summary>
		public override TimeSpan WheelLockedTime { get; protected set; }

		public override async Task BuildAsync()
		{
			Outcome = Outcome == UserSetting.Outcome.Task ? UserSetting.Outcome.Edge : Outcome;

			var denialText = GetDenialText();

			Link link = null;

			if (this.Outcome == UserSetting.Outcome.Edge)
			{
				Image = await ((WaifuJOIBot)_services.GetService(typeof(WaifuJOIBot))).GetRandomPicture();

				HttpClient client = new HttpClient();
				using var response = await client.GetAsync(WaifuJOIBot.GetCreatorUrl(Image.CreatorId));
				var creator = await MessagePack.MessagePackSerializer.DeserializeAsync<WaifuJoi.Shared.Features.User.GetUserResponse>(
					await response.Content.ReadAsStreamAsync());

				link = new Link
				{
					CreatorName = creator.User.Name,
					Url = WaifuJOIBot.GetImageUrl(Image.Id),
					Type = Link.ContentType.Picture
				};
			}
			else
			{
				List<Link> links = await Link.LoadAll();

				var randomLinkNr = Helpers.RandomGenerator.RandomInt(0, links.Count);

				if (links.Count <= randomLinkNr)
				{
					link = new Link()
					{
						CreatorName = Properties.Resources.ForgotLinkUpdate,
						FileName = Properties.Resources.ForgotLinkUpdate,
						Type = Link.ContentType.Picture,
						Url = Properties.Resources.ForgotLinkUpdate
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
					fullSentence = $"Watch {link.CreatorName}' JOI. {denialText}";
					break;

				case Link.ContentType.Picture:

					if (Outcome == UserSetting.Outcome.Edge)
					{
						fullSentence = $"Edge to {link.CreatorName}'s Picture and take a 30 second break. {denialText}";
						rerollIn = "Don't forget to take a break! If you cum/ruin, use ``//came`` or ``//ruined``";
						this.WheelLockedTime = new TimeSpan(0, 0, 30);
					}
					else
					{
						fullSentence = $"Edge to {link.CreatorName}'s Picture and {denialText}";
					}

					break;
			}

			if (this.Outcome != UserSetting.Outcome.Edge)
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

		public void CleanUp(DiscordContext context)
		{
			if (Image == null)
			{
				return;
			}

			context.WaifuJoiContentPost.Add(new WaifuJoiContentPost()
			{
				ContentId = Image.Id,
				Time = DateTime.Now
			});
		}

		private string GetDenialText()
		{
			var denialtext = Properties.Resources.BotReturnErrorText;

			switch (Outcome)
			{
				case UserSetting.Outcome.Edge:
					denialtext = Properties.Resources.SpinAgain;
					this.Outcome = UserSetting.Outcome.Edge;
					break;

				case UserSetting.Outcome.Denial:
					denialtext = Properties.Resources.DenyOrgasm;
					this.DenialTime = new TimeSpan(8, 0, 0);
					this.Outcome = UserSetting.Outcome.Denial;
					break;

				case UserSetting.Outcome.Ruin:
					denialtext = Properties.Resources.RuinOrgasm;
					this.Outcome = UserSetting.Outcome.Ruin;
					break;

				case UserSetting.Outcome.Orgasm:
					denialtext = Properties.Resources.FullOrgasm;
					this.Outcome = UserSetting.Outcome.Orgasm;
					break;

				case UserSetting.Outcome.Denial | UserSetting.Outcome.Edge:
					var chance = Helpers.RandomGenerator.RandomInt(0, 9);
					if (chance < 5)
					{
						denialtext = Properties.Resources.DenyOrgasm;
						this.DenialTime = new TimeSpan(8, 0, 0);
						this.Outcome = UserSetting.Outcome.Denial;
					}
					else
					{
						denialtext = Properties.Resources.SpinAgain + Properties.Resources.CameRuinedInfo;
						this.Outcome = UserSetting.Outcome.Edge;
					}

					break;
			}

			return denialtext;
		}
	}
}