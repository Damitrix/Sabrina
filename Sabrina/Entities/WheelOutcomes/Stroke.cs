using DSharpPlus.Entities;
using Sabrina.Entities.Persistent;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Sabrina.Entities.WheelOutcomes
{
    class Stroke : WheelOutcome
    {
        private const int _baseStrokeMultiplier = 40; 

        public Stroke(WheelExtension.Outcome outcome, UserSettings settings, DiscordContext context) : base(
            outcome, settings, context)
        {
            var strokes = _baseStrokeMultiplier * ((int) settings.WheelDifficulty + 1);

            var strokeVariation = strokes / 10 * 2;

            strokes = Helpers.RandomGenerator.RandomInt(strokes - strokeVariation, strokes + strokeVariation);

            while (strokes % 5 != 0)
            {
                strokes++;
            }

            var speed = (StrokeSpeed)Helpers.RandomGenerator.RandomInt(0, Enum.GetNames(typeof(StrokeSpeed)).Length);

            var speedText = StrokeSpeedToFlavorText(speed);

            string text = $"I want you to stroke {strokes} times, {speedText}.";

            if ((strokes > 100 && speed == StrokeSpeed.medium) || (strokes > 50 && speed > StrokeSpeed.medium))
            {
                text += "If you get to the Edge, take a 30 second break and continue.";
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.MidnightBlue,
                Title = "Time to stroke!",
                Description = text
            };

            Embed = builder.Build();
            Text = text;

            WheelLockedTime = new TimeSpan(0,0, strokes * Convert.ToInt32(1.3f / ((int)speed + 1)));

            Outcome = WheelExtension.Outcome.Task;
        }

        private enum StrokeSpeed
        {
            slowest,
            slow,
            medium,
            fast,
            fastest
        }

        private string StrokeSpeedToFlavorText(StrokeSpeed strokeSpeed)
        {
            string[] possibleStrings = null;

            switch (strokeSpeed)
            {
                    case StrokeSpeed.slowest:
                        possibleStrings = new[] {"excrutiatingly slow", "really slow", "extremely slow", "like, *really* slow", "as slow as you can, without getting flaccid"};
                        break;
                    case StrokeSpeed.slow:
                        possibleStrings = new[] { "slow", "slower than you would normally", "fast! haha, no, slow", "slow", "nice and slow" };
                    break;
                    case StrokeSpeed.medium:
                        possibleStrings = new[] { "at about average speed", "at medium speed", "kinda like normal", "normally" };
                    break;
                    case StrokeSpeed.fast:
                        possibleStrings = new[] { "fast", "nice and fast", "fast and good", "fast...ish" };
                    break;
                    case StrokeSpeed.fastest:
                        possibleStrings = new[] { "really fast", "as fast as you can", "extremely fast", "like, *really* fast" };
                    break;
            }

            return possibleStrings[Helpers.RandomGenerator.RandomInt(0, possibleStrings.Length)];
        }

        /// <summary>
        /// Gets or sets the chance of this being used.
        /// </summary>
        public override int Chance { get; protected set; } = 80;

        /// <summary>
        /// Gets or sets the denial time.
        /// </summary>
        public override TimeSpan DenialTime { get; protected set; }

        /// <summary>
        /// Gets or sets the embed to display to the user.
        /// </summary>
        public override DiscordEmbed Embed { get; protected set; }

        /// <summary>
        /// Gets or sets the outcome.
        /// </summary>
        public override WheelExtension.Outcome Outcome { get; protected set; }

        /// <summary>
        /// Gets or sets the text to display to the user.
        /// </summary>
        public override string Text { get; protected set; }

        /// <summary>
        /// Gets or sets the wheel locked time.
        /// </summary>
        public override TimeSpan WheelLockedTime { get; protected set; }
    }
}
