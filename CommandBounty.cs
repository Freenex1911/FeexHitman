using fr34kyn01535.Uconomy;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;

namespace Freenex.FeexHitman
{
    public class CommandBounty : IRocketCommand
    {
        public string Name => "bounty";

        public string Help => "Add bounty to players";

        public string Syntax => "<player> <amount>";

        public List<string> Aliases => new List<string>();

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public List<string> Permissions =>
            new List<string>()
            {
                "bounty"
            };

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (command.Length == 2)
            {
                var otherPlayer = UnturnedPlayer.FromName(command[0]);

                if (otherPlayer == null)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_general_not_found") !=
                        "hitman_general_not_found")
                        UnturnedChat.Say(caller,
                            FeexHitman.Instance.Translations.Instance.Translate("hitman_general_not_found"));
                    return;
                }

                if (caller.Id == otherPlayer.Id)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_add_self") != "hitman_add_self")
                        UnturnedChat.Say(caller,
                            FeexHitman.Instance.Translations.Instance.Translate("hitman_add_self"));
                    return;
                }

                decimal bounty = 0;
                if (!decimal.TryParse(command[1], out bounty) || bounty <= 0)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_add_amount") != "hitman_add_amount")
                        UnturnedChat.Say(caller,
                            FeexHitman.Instance.Translations.Instance.Translate("hitman_add_amount"));
                    return;
                }

                var myBalance = Uconomy.Instance.Database.GetBalance(caller.Id);
                if (myBalance - bounty < 0)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_add_balance") !=
                        "hitman_add_balance")
                        UnturnedChat.Say(caller,
                            FeexHitman.Instance.Translations.Instance.Translate("hitman_add_balance"));
                    return;
                }

                if (bounty < FeexHitman.Instance.Configuration.Instance.MinimumBounty)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_add_minimum") !=
                        "hitman_add_minimum")
                        UnturnedChat.Say(caller,
                            FeexHitman.Instance.Translations.Instance.Translate("hitman_add_minimum",
                                FeexHitman.Instance.Configuration.Instance.MinimumBounty));
                    return;
                }

                if (FeexHitman.Instance.FeexHitmanDatabase.CheckExists(otherPlayer.CSteamID))
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_general_chat_increased") !=
                        "hitman_general_chat_increased")
                        UnturnedChat.Say(
                            FeexHitman.Instance.Translations.Instance.Translate("hitman_general_chat_increased",
                                otherPlayer.CharacterName, bounty,
                                Convert.ToDecimal(
                                    FeexHitman.Instance.FeexHitmanDatabase.GetBounty(otherPlayer.CSteamID)) + bounty),
                            UnityEngine.Color.yellow);
                }
                else
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_general_chat_created") !=
                        "hitman_general_chat_created")
                        UnturnedChat.Say(
                            FeexHitman.Instance.Translations.Instance.Translate("hitman_general_chat_created",
                                otherPlayer.CharacterName, bounty), UnityEngine.Color.yellow);
                }

                Uconomy.Instance.Database.IncreaseBalance(caller.Id.ToString(), -bounty);
                FeexHitman.Instance.FeexHitmanDatabase.AddUpdateVictimAccount(otherPlayer.CSteamID, bounty,
                    otherPlayer.CharacterName);
            }
            else
            {
                if (FeexHitman.Instance.Translations.Instance.Translate("hitman_general_invalid_parameter") !=
                    "hitman_general_invalid_parameter")
                    UnturnedChat.Say(caller,
                        FeexHitman.Instance.Translations.Instance.Translate("hitman_general_invalid_parameter"));
            }
        }
    }
}