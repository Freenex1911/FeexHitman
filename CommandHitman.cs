using fr34kyn01535.Uconomy;
using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;

namespace Freenex.Hitman
{
    public class CommandHitman : IRocketCommand
    {
        public string Name
        {
            get { return "hitman"; }
        }
        public string Help
        {
            get { return "Hitman (add/payout/check)"; }
        }

        public string Syntax
        {
            get { return "<add/payout/check> [<player>] [<amount>]"; }
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
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_general_not_found") != "hitman_general_not_found")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_general_not_found"));
                    }
                    return;
                }

                if (caller.Id == otherPlayer.Id)
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_add_self") != "hitman_add_self")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_add_self"));
                    }
                    return;
                }

                decimal bounty = 0;
                if (!Decimal.TryParse(command[2], out bounty) || bounty <= 0)
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_add_amount") != "hitman_add_amount")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_add_amount"));
                    }
                    return;
                }

                decimal myBalance = Uconomy.Instance.Database.GetBalance(caller.Id);
                if ((myBalance - bounty) < 0)
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_add_balance") != "hitman_add_balance")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_add_balance"));
                    }
                    return;
                }

                if (bounty < Hitman.Instance.Configuration.Instance.MinimumBounty)
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_add_minimum") != "hitman_add_minimum")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_add_minimum", Hitman.Instance.Configuration.Instance.MinimumBounty));
                    }
                    return;
                }

                if (Hitman.Instance.HitmanDatabase.CheckExists(otherPlayer.CSteamID))
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_general_chat_increased") != "hitman_general_chat_increased")
                    {
                        UnturnedChat.Say(Hitman.Instance.Translations.Instance.Translate("hitman_general_chat_increased", otherPlayer.DisplayName, bounty, Convert.ToDecimal(Hitman.Instance.HitmanDatabase.GetBounty(otherPlayer.CSteamID)) + bounty), UnityEngine.Color.yellow);
                    }
                }
                else
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_general_chat_created") != "hitman_general_chat_created")
                    {
                        UnturnedChat.Say(Hitman.Instance.Translations.Instance.Translate("hitman_general_chat_created", otherPlayer.DisplayName, bounty), UnityEngine.Color.yellow);
                    }
                }

                Uconomy.Instance.Database.IncreaseBalance(caller.Id.ToString(), -bounty);
                Hitman.Instance.HitmanDatabase.AddUpdateVictimAccount(otherPlayer.CSteamID, bounty, otherPlayer.DisplayName);
            }

            if (command.Length == 2 && command[0] == "payout" && caller.HasPermission("hitman.payout"))
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[1]);

                if (otherPlayer == null)
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_general_not_found") != "hitman_general_not_found")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_general_not_found"));
                    }
                    return;
                }

                if (Hitman.Instance.HitmanDatabase.CheckExists(otherPlayer.CSteamID))
                {
                    decimal bounty = Hitman.Instance.HitmanDatabase.GetBounty(otherPlayer.CSteamID);
                    Uconomy.Instance.Database.IncreaseBalance(caller.Id, bounty);
                    Hitman.Instance.HitmanDatabase.RemoveVictimAccount(otherPlayer.CSteamID);
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_payout") != "hitman_payout")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_payout", otherPlayer.DisplayName, bounty));
                    }
                }
                else
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_check_false") != "hitman_check_false")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_check_false", otherPlayer.DisplayName));
                    }
                }
            }

            if (command.Length == 2 && command[0] == "check" && caller.HasPermission("hitman.check"))
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[1]);

                if (otherPlayer == null)
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_general_not_found") != "hitman_general_not_found")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_general_not_found"));
                    }
                    return;
                }

                if (Hitman.Instance.HitmanDatabase.CheckExists(otherPlayer.CSteamID))
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_check_true") != "hitman_check_true")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_check_true", otherPlayer.DisplayName, Hitman.Instance.HitmanDatabase.GetBounty(otherPlayer.CSteamID)));
                    }
                }
                else
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_check_false") != "hitman_check_false")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_check_false", otherPlayer.DisplayName));
                    }
                }
            }

            if (command.Length == 1 && command[0] == "list" && caller.HasPermission("hitman.list"))
            {
                if (Hitman.Instance.HitmanDatabase.GetBountyCount() == 0)
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_list_false") != "hitman_list_false")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_list_false"));
                    }
                }
                else
                {
                    if (Hitman.Instance.Translations.Instance.Translate("hitman_list_true") != "hitman_list_true")
                    {
                        UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_list_true", Hitman.Instance.HitmanDatabase.GetBountyCount(), Hitman.Instance.HitmanDatabase.GetBountyList()));
                    }
                }
            }
        }

    }
}
