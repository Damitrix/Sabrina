// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Information.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the Information type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Commands
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Sabrina.Entities;
    using Sabrina.Entities.Persistent;
    using Sabrina.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// The information Command Group.
    /// </summary>
    internal class Information
    {
        /// <summary>
        /// The get chances Command.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("chances"), RequireRolesAttribute("mistress", "minion", "techno kitty")]
        public async Task GetChancesAsync(CommandContext ctx)
        {
            List<WheelOutcome> wheelOutcomes = ReflectiveEnumerator.GetEnumerableOfType<WheelOutcome>(WheelExtension.Outcome.Edge)
                .ToList();
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithDescription(
                "I've compiled the current List of Chances. First, you'll see a List of Modules (Sabrina.Entities.WheelOutcomes.XXX).\n"
                + "Under the Module Titles for each of these, is a freshly generated example, under that, you'll see the Chance for this to be chosen.\n"
                + "The Bot will add all Chance Values, and then generate a Random Number, to determine which Module will be chosen.\n\n"
                + string.Empty
                + "Under that, you'll see the Chances for what the chosen Module will output at the end.\n"
                + "If the Module does not have a fitting response (Ban can never end in an orgasm for example), it will be ignored.");
            foreach (var wheelOutcome in wheelOutcomes)
            {
                builder.AddField(
                    wheelOutcome.ToString(),
                    "``" + wheelOutcome.Text + "``\n" + wheelOutcome.Chance);
            }

            // builder.AddField("Task", OrgasmWheel.TaskChance.ToString(), true);
            // builder.AddField("Denial", OrgasmWheel.DenialChance.ToString(), true);
            // builder.AddField("Ruin", OrgasmWheel.RuinChance.ToString(), true);
            // builder.AddField("Orgasm", OrgasmWheel.OrgasmChance.ToString(), true);
            await ctx.RespondAsync(embed: builder.Build());
        }

        [Command("random")]
        public async Task RollRandomAsync(CommandContext ctx, int Start, int End)
        {
            await ctx.RespondAsync(Helpers.RandomGenerator.RandomInt(Start, End + 1).ToString());
        }
    }
}