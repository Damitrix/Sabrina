using DSharpPlus.CommandsNext.Attributes;

namespace Sabrina.Commands
{
    [Group("scenario"), Hidden]
    internal class Scenario
    {
        private readonly Dependencies dep;

        public Scenario(Dependencies d)
        {
            dep = d;
        }

        //[Command("create"), Description("Creates an RP Scenario")]
        //public async Task CreateScenario(CommandContext ctx, string scenarioName = null)
        //{
        //}

        //[Command("delete"), Description("Deletes an RP Scenario")]
        //public async Task DeleteScenario(CommandContext ctx, string scenarioName = null)
        //{
        //}

        //[Command("join"), Description("Creates an RP Scenario")]
        //public async Task JoinScenario(CommandContext ctx, string scenarioName = null)
        //{
        //}

        //[Command("start"), Description("Creates an RP Scenario")]
        //public async Task StartScenario(CommandContext ctx, string scenarioName = null)
        //{
        //}
    }
}