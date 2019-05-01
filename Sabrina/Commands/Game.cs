using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Sabrina.Entities;
using Sabrina.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sabrina.Commands
{
    [Group("game")]
    [Aliases("i")]
    public class Game
    {
        private const string ConfirmRegex = "\\b[Yy][Ee]?[Ss]?\\b|\\b[Nn][Oo]?\\b";
        private const string HitRegex = "[Hh][Ii][Tt]?";
        private const string HitStayRegex = "\\b[Hh][Ii][Tt]?\\b|\\b[Ss][Tt][Aa][Nn][Dd]?\\b";
        private const string NoRegex = "[Nn][Oo]?";
        private const string StandRegex = "[Ss][Tt][Aa][Nn][Dd]?";
        private const string YesRegex = "[Yy][Ee]?[Ss]?";
        private readonly DiscordContext _context;

        public Game()
        {
            _context = new DiscordContext();
        }

        [Command("blackjack")]
        [Description("Play BlackJack to gamble for reduced Edges")]
        public async Task BlackjackAsync(CommandContext ctx, int betEdges)
        {
            if (betEdges < 1)
            {
                await ctx.RespondAsync("No. >.<");
                return;
            }

            if (betEdges > 30)
            {
                await ctx.RespondAsync("Sorry, max bet is 20 for now");
                return;
            }

            // Create a new Game and Deal to Player and House
            var game = new BlackJackGame(betEdges, ctx, _context);
            var playerHolds = false;

            await ctx.RespondAsync("Game Start!\n");

            string drawingsText = string.Empty;

            drawingsText +=
                $"You draw a **{game.PlayerCards[0].Variation.ToString()} __{game.PlayerCards[0].Value.ToString()}__**\n";
            drawingsText +=
                $"You draw a **{game.PlayerCards[1].Variation.ToString()} __{game.PlayerCards[1].Value.ToString()}__**\n";
            drawingsText +=
                $"The House draws a {game.HouseCards[0].Variation.ToString()} __{game.HouseCards[0].Value.ToString()}__\n";
            drawingsText += $"The House draws a Card.\n";

            await ctx.RespondAsync(drawingsText);

            if (BlackJackGame.IsBlackJack(game.PlayerCards) && BlackJackGame.IsBlackJack(game.HouseCards))
            {
                // Both Blackjack, push
                await ctx.RespondAsync("Both of you have Blackjack!");
                game.OnNeutral();
                return;
            }

            if (BlackJackGame.IsBlackJack(game.HouseCards))
            {
                // House Blackjack, House wins
                await ctx.RespondAsync("House has BlackJack!");
                game.OnLoose();
                return;
            }

            // Loop as long as Player does not hold
            while (!game.PlayerBusts && !playerHolds && !BlackJackGame.IsBlackJack(game.PlayerCards))
            {
                await ctx.RespondAsync("Hit or Stand?");
                var m = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                            x => x.Channel.Id == ctx.Channel.Id && x.Author.Id == ctx.Member.Id
                                                                && Regex.IsMatch(x.Content, HitStayRegex),
                            TimeSpan.FromSeconds(60));

                if (m == null)
                {
                    await ctx.RespondAsync(
                        $"You didn't answer me in 60 seconds, i see that as *giving up*. Well, {game.BetEdges} Edges to your wallet :)");
                    await ctx.RespondAsync("House wins.");
                    game.OnLoose();
                }

                // If user says hit
                if (Regex.IsMatch(m.Message.Content, HitRegex))
                {
                    game.DrawCardPlayer();
                    await ctx.RespondAsync(
                        $"You got a {game.PlayerCards[game.PlayerCards.Count - 1].Variation.ToString()} {game.PlayerCards[game.PlayerCards.Count - 1].Value.ToString()}");
                }

                // If user says stand
                else if (Regex.IsMatch(m.Message.Content, StandRegex))
                {
                    await ctx.RespondAsync("You Stand.");
                    playerHolds = true;
                }
            }

            if (game.PlayerBusts)
            {
                // House Wins#
                await ctx.RespondAsync("You bust!");
                game.OnLoose();
                return;
            }

            if (BlackJackGame.IsBlackJack(game.PlayerCards))
            {
                if (game.HouseCards[0].Value == BlackJackGame.Card.CardValue.Ace)
                {
                    // Ask for even
                    await ctx.RespondAsync(
                        "You have a Blackjack, but the House has an Ace, do you want to take even money?");
                    var m = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                                x => x.Channel.Id == ctx.Channel.Id && x.Author.Id == ctx.Member.Id
                                                                    && Regex.IsMatch(x.Content, ConfirmRegex));

                    if (m == null)
                    {
                        await ctx.RespondAsync(
                            $"You didn't answer me in 60 seconds, i see that as *giving up*. Well, {game.BetEdges} Edges to your wallet :)");
                        await ctx.RespondAsync("House wins.");
                        game.OnLoose();
                    }

                    if (Regex.IsMatch(m.Message.Content, YesRegex))
                    {
                        // Even Money
                        await ctx.RespondAsync("Alrighty! Even it is.");
                        return;
                    }

                    await ctx.RespondAsync("Good Luck.");
                }
                else
                {
                    // Player wins with BlackJack (3/2)
                    await ctx.RespondAsync("Blackjack! You win!");
                    game.OnSpecialWin();
                    return;
                }
            }
            else
            {
                if (game.HouseCards[0].Value == BlackJackGame.Card.CardValue.Ace)
                {
                    // Ask for even
                    await ctx.RespondAsync("House has an Ace, do you want to take insurance?");
                    var m = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                                x => x.Channel.Id == ctx.Channel.Id && x.Author.Id == ctx.Member.Id
                                                                    && Regex.IsMatch(x.Content, ConfirmRegex));

                    if (m == null)
                    {
                        await ctx.RespondAsync(
                            $"You didn't answer me in 60 seconds, i see that as *giving up*. Well, {game.BetEdges} Edges to your wallet :)");
                        await ctx.RespondAsync("House wins.");
                        game.OnLoose();
                    }

                    if (Regex.IsMatch(m.Message.Content, YesRegex))
                    {
                        // Insurance
                        await ctx.RespondAsync("Alrighty! Insurance it is.");
                        game.OnNeutral();
                        return;
                    }

                    await ctx.RespondAsync("Good Luck.");
                }
            }

            // Flip House' Card
            await ctx.RespondAsync(
                $"House' Card #2 is a {game.HouseCards[game.HouseCards.Count - 1].Variation.ToString()} {game.HouseCards[game.HouseCards.Count - 1].Value.ToString()}\n");

            // Draw for House while Value is under 17
            while (BlackJackGame.GetMaxSumWithoutOvershoot(game.HouseCards) < 17)
            {
                game.DrawCardHouse();
                await ctx.RespondAsync(
                    $"House got a {game.HouseCards[game.HouseCards.Count - 1].Variation.ToString()} {game.HouseCards[game.HouseCards.Count - 1].Value.ToString()}\n");

                if (BlackJackGame.GetMaxSumWithoutOvershoot(game.HouseCards)
                    == BlackJackGame.GetMaxSumWithoutOvershoot(game.PlayerCards))
                {
                    // Draw, no one wins
                    await ctx.RespondAsync("It's a Draw!");
                    game.OnNeutral();
                    return;
                }

                if (BlackJackGame.GetMinSum(game.HouseCards) > 21)
                {
                    // House overshoots, you win
                    await ctx.RespondAsync("House busts. You win!");
                    game.OnWin();
                    return;
                }
            }

            if (BlackJackGame.IsBlackJack(game.PlayerCards) && BlackJackGame.IsBlackJack(game.HouseCards))
            {
                // Both Blackjack, push
                await ctx.RespondAsync("Both have Blackjack!");
                game.OnNeutral();
                return;
            }

            if (BlackJackGame.GetMaxSumWithoutOvershoot(game.HouseCards)
                == BlackJackGame.GetMaxSumWithoutOvershoot(game.PlayerCards))
            {
                // Draw, no one wins
                await ctx.RespondAsync("It's a Draw!");
                game.OnNeutral();
                return;
            }

            if (BlackJackGame.GetMaxSumWithoutOvershoot(game.HouseCards)
                > BlackJackGame.GetMaxSumWithoutOvershoot(game.PlayerCards))
            {
                // House wins
                await ctx.RespondAsync("House wins.");
                game.OnLoose();
            }
            else if (BlackJackGame.IsBlackJack(game.PlayerCards))
            {
                // Player wins cause of more Points AND Blackjack, so Money is 3/2
                await ctx.RespondAsync("You win!");
                game.OnSpecialWin();
            }
            else
            {
                // Player wins cause of more Points
                await ctx.RespondAsync("You win!");
                game.OnWin();
            }
        }
    }

    internal class BlackJackGame
    {
        public List<Card> Deck = new List<Card>(104);

        public List<Card> HouseCards = new List<Card>();

        public bool PlayerBusts;

        public List<Card> PlayerCards = new List<Card>();

        private readonly CommandContext ctx;
        private readonly Users user;

        private DiscordContext _context;

        public BlackJackGame(int betEdges, CommandContext ctx, DiscordContext context)
        {
            _context = new DiscordContext();
            this.user = UserExtension.GetUser(ctx.Message.Author.Id).GetAwaiter().GetResult();
            this.ctx = ctx;

            // 2 Decks
            for (var i = 0; i < 2; i++)

                // Iterate through each Card Variant
                foreach (Card.CardVariation variation in Enum.GetValues(typeof(Card.CardVariation)))
                    foreach (Card.CardValue cardValue in Enum.GetValues(typeof(Card.CardValue)))
                        this.Deck.Add(new Card { Variation = variation, Value = cardValue });

            this.BetEdges = betEdges;
            for (var i = 0; i < 2; i++)
            {
                this.DrawCardHouse();
                this.DrawCardPlayer();
            }
        }

        public enum GameOutcome
        {
            Undecided,
            Won,
            Lost
        }

        public int BetEdges { get; }

        public static int GetMaxSum(List<Card> cards)
        {
            var sum = 0;

            foreach (var card in cards)
            {
                if (card.Value == Card.CardValue.Ace)
                {
                    sum += 11;
                    continue;
                }

                sum += (int)card.Value < 11 ? (int)card.Value : 10;
            }

            return sum;
        }

        public static int GetMaxSumWithoutOvershoot(List<Card> cards)
        {
            var sum = 0;

            foreach (var card in cards)
            {
                if (card.Value == Card.CardValue.Ace && sum + 11 <= 21)
                {
                    sum += 11;
                    continue;
                }

                if (card.Value == Card.CardValue.Ace)
                {
                    sum += 1;
                    continue;
                }

                sum += (int)card.Value < 11 ? (int)card.Value : 10;
            }

            return sum;
        }

        public static int GetMinSum(List<Card> cards)
        {
            var sum = 0;

            foreach (var card in cards)
                sum += (int)card.Value < 11 ? (int)card.Value : 10;

            return sum;
        }

        public static bool HasAce(List<Card> cards)
        {
            return cards.Any(e => e.Value == Card.CardValue.Ace);
        }

        public static bool IsBlackJack(List<Card> cards)
        {
            return GetMaxSumWithoutOvershoot(cards) == 21 && cards.Any(e => e.Value == Card.CardValue.Ace);
        }

        public void DrawCardHouse()
        {
            this.HouseCards.Add(this.Deck.Skip(Helpers.RandomGenerator.RandomInt(0, this.Deck.Count)).Take(1).First());
        }

        public void DrawCardPlayer()
        {
            this.PlayerCards.Add(this.Deck.Skip(Helpers.RandomGenerator.RandomInt(0, this.Deck.Count)).Take(1).First());

            if (GetMinSum(this.PlayerCards) > 21) this.PlayerBusts = true;
        }

        public async void OnLoose()
        {
            this.user.WalletEdges += this.BetEdges;
            await _context.SaveChangesAsync();
            await this.ctx.RespondAsync(
                $"You Lose! {PostFixes.Bad[Helpers.RandomGenerator.RandomInt(0, PostFixes.Bad.Length)]}{Environment.NewLine}" +
                $"I will add {this.BetEdges} edges to your balance.{Environment.NewLine}" +
                $"You now have {this.user.WalletEdges}");
        }

        public async void OnNeutral()
        {
            await this.ctx.RespondAsync(
                $"You Win! No, You Loose! uhhh... neither. {PostFixes.Neutral[Helpers.RandomGenerator.RandomInt(0, PostFixes.Neutral.Length)]}{Environment.NewLine}" +
                $"You still have {this.user.WalletEdges} edges.");
        }

        public async void OnSpecialWin()
        {
            this.user.WalletEdges -= this.BetEdges / 5 * 3;
            await _context.SaveChangesAsync();
            await this.ctx.RespondAsync(
                $"You Win! {PostFixes.Special[Helpers.RandomGenerator.RandomInt(0, PostFixes.Special.Length)]}{Environment.NewLine}" +
                $"I will deduct {this.BetEdges / 5 * 3} edges from your balance.{Environment.NewLine}" +
                $"You now have {this.user.WalletEdges}");
        }

        public async void OnWin()
        {
            this.user.WalletEdges -= this.BetEdges;
            await _context.SaveChangesAsync();
            await this.ctx.RespondAsync(
                $"You Win! {PostFixes.Good[Helpers.RandomGenerator.RandomInt(0, PostFixes.Good.Length)]}{Environment.NewLine}" +
                $"I will deduct {this.BetEdges} edges from your balance.{Environment.NewLine}" +
                $"You now have {this.user.WalletEdges}");
        }

        public class Card
        {
            public CardValue Value;

            public CardVariation Variation;

            public enum CardValue
            {
                Ace = 1,
                Two = 2,
                Three = 3,
                Four = 4,
                Five = 5,
                Six = 6,
                Seven = 7,
                Eight = 8,
                Nine = 9,
                Ten = 10,
                Jack = 11,
                Queen = 12,
                King = 13
            }

            public enum CardVariation
            {
                Spades,
                Diamonds,
                Clubs,
                Hearts
            }
        }

        private static class PostFixes
        {
            public static readonly string[] Bad =
                {"Hehe.", "Don't worry, you can get that back later :)", "No luck for you."};

            public static readonly string[] Good = { "Yay!", "Nice!", "You've just got lucky..." };
            public static readonly string[] Neutral = { "huh... that's lame.", "ok...", "ehhh...." };

            public static readonly string[] Special =
            {
                "I can give you 3 Rubber Ducks for that.",
                "Great, you've won... how 'bout you tell people in #general-chat-1 from your immense victory now? ._.",
                "I'm sure Aki has something to make up for it :J"
            };
        }
    }
}