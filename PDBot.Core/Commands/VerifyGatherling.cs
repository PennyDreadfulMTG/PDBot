using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.Interfaces;
using System.Net;
using System.Xml.Linq;
using PDBot.API;
using Gatherling;

namespace PDBot.Commands
{
    class VerifyGatherling : ICommand
    {
        public string[] Handle => new string[] { "!verify" };

        public bool AcceptsGameChat => false;

        public bool AcceptsPM => true;

        public async Task<string> RunAsync(string player, IMatch game, string[] args)
        {
            var server = args.FirstOrDefault();
            switch (server?.ToLower())
            {
                case null:
                    return "Please provide the full verify command.";
                case "pdg":
                    return await GatherlingClient.PennyDreadful.GetVerificationCodeAsync(player);
                case "g":
                case "gatherling":
                case "gatherling.com":
                    return await GatherlingClient.GatherlingDotCom.GetVerificationCodeAsync(player);
                case "one":
                    //return await GatherlingClient.One.GetVerificationCodeAsync(player);
                default:
                    return "Unknown servercode.";
            }
        }
    }

    class ResetGatherling : ICommand
    {
        public string[] Handle => new string[] { "!reset" };

        public bool AcceptsGameChat => false;

        public bool AcceptsPM => true;

        public async Task<string> RunAsync(string player, IMatch game, string[] args)
        {
            var server = args.FirstOrDefault();
            switch (server?.ToLower())
            {
                case null:
                    return "Please provide the full reset command.";
                case "pdg":
                    return await GatherlingClient.PennyDreadful.ResetPasswordAsync(player);
                case "g":
                case "gatherling":
                case "gatherling.com":
                    return await GatherlingClient.GatherlingDotCom.ResetPasswordAsync(player);

                case "one":
                    //return await GatherlingClient.One.ResetPasswordAsync(player);
                default:
                    return "Unknown servercode.";
            }
        }
    }

}
