using fr34kyn01535.Uconomy;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;

namespace Freenex.FeexHitman
{
    public class CommandHitman : IRocketCommand
    {
        public string Name
        {
            get { return "hitman"; }
        }

        public string Help
        {
            get { return "Hitman (add/payout/check/list)"; }
        }

        public string Syntax
        {
            get { return "<add/payout/check/list> [<player>] [<amount>]"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>()
                {
                    "hitman.add",
                    "hitman.payout",
                    "hitman.check",
                    "hitman.list"
                };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (command.Length == 3 && command[0] == "add" && caller.HasPermission("hitman.add"))
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[1]);

                if (otherPlayer == null)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_general_not_found") != "hitman_general_not_found")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_general_not_found"));
                    }
                    return;
                }

                if (caller.Id == otherPlayer.Id)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_add_self") != "hitman_add_self")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_add_self"));
                    }
                    return;
                }

                decimal bounty = 0;
                if (!Decimal.TryParse(command[2], out bounty) || bounty <= 0)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_add_amount") != "hitman_add_amount")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_add_amount"));
                    }
                    return;
                }

                decimal myBalance = Uconomy.Instance.Database.GetBalance(caller.Id);
                if ((myBalance - bounty) < 0)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_add_balance") != "hitman_add_balance")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_add_balance"));
                    }
                    return;
                }

                if (bounty < FeexHitman.Instance.Configuration.Instance.MinimumBounty)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_add_minimum") != "hitman_add_minimum")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_add_minimum", FeexHitman.Instance.Configuration.Instance.MinimumBounty));
                    }
                    return;
                }

                if (FeexHitman.Instance.FeexHitmanDatabase.CheckExists(otherPlayer.CSteamID))
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_general_chat_increased") != "hitman_general_chat_increased")
                    {
                        UnturnedChat.Say(FeexHitman.Instance.Translations.Instance.Translate("hitman_general_chat_increased", otherPlayer.DisplayName, bounty, Convert.ToDecimal(FeexHitman.Instance.FeexHitmanDatabase.GetBounty(otherPlayer.CSteamID)) + bounty), UnityEngine.Color.yellow);
                    }
                }
                else
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_general_chat_created") != "hitman_general_chat_created")
                    {
                        UnturnedChat.Say(FeexHitman.Instance.Translations.Instance.Translate("hitman_general_chat_created", otherPlayer.DisplayName, bounty), UnityEngine.Color.yellow);
                    }
                }

                Uconomy.Instance.Database.IncreaseBalance(caller.Id.ToString(), -bounty);
                FeexHitman.Instance.FeexHitmanDatabase.AddUpdateVictimAccount(otherPlayer.CSteamID, bounty, otherPlayer.DisplayName);
            }
            else if (command.Length == 2 && command[0] == "payout" && caller.HasPermission("hitman.payout"))
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[1]);

                if (otherPlayer == null)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_general_not_found") != "hitman_general_not_found")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_general_not_found"));
                    }
                    return;
                }

                if (FeexHitman.Instance.FeexHitmanDatabase.CheckExists(otherPlayer.CSteamID))
                {
                    decimal bounty = FeexHitman.Instance.FeexHitmanDatabase.GetBounty(otherPlayer.CSteamID);
                    Uconomy.Instance.Database.IncreaseBalance(caller.Id, bounty);
                    FeexHitman.Instance.FeexHitmanDatabase.RemoveVictimAccount(otherPlayer.CSteamID);
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_payout") != "hitman_payout")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_payout", otherPlayer.DisplayName, bounty));
                    }
                }
                else
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_check_false") != "hitman_check_false")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_check_false", otherPlayer.DisplayName));
                    }
                }
            }
            else if (command.Length == 2 && command[0] == "check" && caller.HasPermission("hitman.check"))
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[1]);

                if (otherPlayer == null)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_general_not_found") != "hitman_general_not_found")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_general_not_found"));
                    }
                    return;
                }

                if (FeexHitman.Instance.FeexHitmanDatabase.CheckExists(otherPlayer.CSteamID))
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_check_true") != "hitman_check_true")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_check_true", otherPlayer.DisplayName, FeexHitman.Instance.FeexHitmanDatabase.GetBounty(otherPlayer.CSteamID)));
                    }
                }
                else
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_check_false") != "hitman_check_false")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_check_false", otherPlayer.DisplayName));
                    }
                }
            }
            else if (command.Length == 1 && command[0] == "list" && caller.HasPermission("hitman.list"))
            {
                if (FeexHitman.Instance.FeexHitmanDatabase.GetBountyCount() == 0)
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_list_false") != "hitman_list_false")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_list_false"));
                    }
                }
                else
                {
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_list_true") != "hitman_list_true")
                    {
                        UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_list_true", FeexHitman.Instance.FeexHitmanDatabase.GetBountyCount(), FeexHitman.Instance.FeexHitmanDatabase.GetBountyList()));
                    }
                }
            }
            else
            {
                if (FeexHitman.Instance.Translations.Instance.Translate("hitman_general_invalid_parameter") != "hitman_general_invalid_parameter")
                {
                    UnturnedChat.Say(caller, FeexHitman.Instance.Translations.Instance.Translate("hitman_general_invalid_parameter"));
                }
            }
        }
    }
}
