using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Sabrina.Models;
using System;
using System.Threading.Tasks;

namespace Sabrina.Entities
{
    public abstract class SettingsModule
    {
        private int _score = 0;

        public SettingsModule(DiscordContext context, DiscordDmChannel dm, CommandContext ctx, long userId)
        {
            _context = context;
            _dm = dm;
            _userId = userId;
            _ctx = ctx;
        }

        public abstract string FriendlyName { get; internal set; }
        internal DiscordContext _context { get; private set; }
        internal CommandContext _ctx { get; private set; }
        internal DiscordDmChannel _dm { get; private set; }
        internal abstract string[] _keys { get; set; }
        internal long _userId { get; private set; }

        public async Task<bool> Execute()
        {
            var hasRun = await Run();

            if (hasRun)
            {
                await _context.SaveChangesAsync();
            }

            return hasRun;
        }

        public int GetScore(string text)
        {
            if (_score == 0)
            {
                foreach (var key in _keys)
                {
                    var dif = CalculateDifference(text, key);

                    if (_score == 0)
                    {
                        _score = text.Length - dif;
                    }
                    else
                    {
                        if (text.Length - dif > _score)
                        {
                            _score = text.Length - dif;
                        }
                    }
                }
            }

            return _score;
        }

        internal abstract Task<bool> Run();

        private static int CalculateDifference(string source1, string source2)
        {
            var source1Length = source1.Length;
            var source2Length = source2.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            // First calculation, if one entry is empty return full length
            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            // Initialization of matrix with row size source1Length and columns size source2Length
            for (var i = 0; i <= source1Length; matrix[i, 0] = i++) { }
            for (var j = 0; j <= source2Length; matrix[0, j] = j++) { }

            // Calculate rows and collumns distances
            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            // return result
            return matrix[source1Length, source2Length];
        }
    }
}