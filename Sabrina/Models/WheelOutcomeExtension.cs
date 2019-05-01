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
            return await GetLastOutcome(WheelExtension.Outcome.All, userId, isUserReport, context);
        }

        public static async Task<WheelOutcome> GetLastOutcome(WheelExtension.Outcome type, long userId, bool? isUserReport = null, DiscordContext context = null)
        {
            if (context == null)
            {
                context = new DiscordContext();
            }

            if (isUserReport != null)
            {
                return context.WheelOutcome.OrderByDescending(outcome => outcome.Time)
                                            .First(outcome => outcome.UserId == userId
                                                && outcome.IsUserReport == (isUserReport.Value == false ? 0 : 1)
                                                && type.HasFlag((WheelExtension.Outcome)outcome.Type));
            }
            else
            {
                return await context.WheelOutcome.OrderByDescending(outcome => outcome.Time)
                                            .FirstOrDefaultAsync(outcome => outcome.UserId == userId
                                                && type.HasFlag((WheelExtension.Outcome)outcome.Type));
            }
        }

        public static async Task<IEnumerable<WheelOutcome>> GetLastOutcomesUntil(WheelOutcome lastOutcome, DiscordContext context = null)
        {
            if (context == null)
            {
                context = new DiscordContext();
            }

            return await context.WheelOutcome.Where(outcome => outcome.UserId == lastOutcome.UserId && outcome.Id > lastOutcome.Id).ToListAsync();
        }
    }
}