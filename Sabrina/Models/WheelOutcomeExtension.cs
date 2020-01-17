using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sabrina.Models
{
	public static class WheelOutcomeExtension
	{
		public static async Task<WheelOutcome> GetLastOutcome(long userId, bool isUserReport, DiscordContext context = null)
		{
			return await GetLastOutcome(UserSetting.Outcome.All, userId, isUserReport, context);
		}

		public static async Task<WheelOutcome> GetLastOutcome(UserSetting.Outcome type, long userId, bool? isUserReport = null, DiscordContext context = null)
		{
			var dispose = context == null;

			if (context == null)
			{
#pragma warning disable IDE0068 // Use recommended dispose pattern
				context = new DiscordContext();
#pragma warning restore IDE0068 // Use recommended dispose pattern
			}

			WheelOutcome outcome;

			if (isUserReport != null)
			{
				outcome = await context.WheelOutcome.OrderByDescending(outcome => outcome.Time)
											.FirstOrDefaultAsync(outcome => outcome.UserId == userId
												&& outcome.IsUserReport == (isUserReport.Value == false ? 0 : 1)
												&& type.HasFlag((UserSetting.Outcome)outcome.Type));
			}
			else
			{
				outcome = await context.WheelOutcome.OrderByDescending(outcome => outcome.Time)
											.FirstOrDefaultAsync(outcome => outcome.UserId == userId
												&& type.HasFlag((UserSetting.Outcome)outcome.Type));
			}

			if (dispose)
				context.Dispose();

			return outcome;
		}

		public static async Task<IEnumerable<WheelOutcome>> GetLastOutcomesUntil(WheelOutcome lastOutcome, DiscordContext context = null)
		{
			var dispose = context == null;

			if (context == null)
			{
#pragma warning disable IDE0068 // Use recommended dispose pattern
				context = new DiscordContext();
#pragma warning restore IDE0068 // Use recommended dispose pattern
			}

			var outcomes = await context.WheelOutcome.Where(outcome => outcome.UserId == lastOutcome.UserId && outcome.Id > lastOutcome.Id).ToListAsync();

			if (dispose)
			{
				context.Dispose();
			}

			return outcomes;
		}
	}
}