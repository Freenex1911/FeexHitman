﻿using fr34kyn01535.Uconomy;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;

namespace Freenex.Hitman
{
    public class CommandBounty : IRocketCommand
    {
        public string Name
        {
            get { return "bounty"; }
        }
        public string Help
        {
            get { return "Add bounty to players"; }
        }

        public bool AllowFromConsole
        {
            get { return false; }
        }

        public string Syntax
        {
            get { return "<player> <amount>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>()
                {
                    "bounty"
                };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (command.Length == 2)
            {
                UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[0]);

                if (otherPlayer == null)
                {
                    UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_general_not_found"));
                    return;
                }

                if (caller.Id == otherPlayer.Id)
                {
                    UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_add_self"));
                    return;
                }

                decimal bounty = 0;
                if (!Decimal.TryParse(command[1], out bounty) || bounty <= 0)
                {
                    UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_add_amount"));
                    return;
                }

                decimal myBalance = Uconomy.Instance.Database.GetBalance(caller.Id);
                if ((myBalance - bounty) < 0)
                {
                    UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_add_balance"));
                    return;
                }

                if (bounty < Hitman.Instance.Configuration.Instance.MinimumBounty)
                {
                    UnturnedChat.Say(caller, Hitman.Instance.Translations.Instance.Translate("hitman_add_minimum", Hitman.Instance.Configuration.Instance.MinimumBounty));
                    return;
                }

                if (Hitman.Instance.HitmanDatabase.CheckExists(otherPlayer.CSteamID))
                {
                    UnturnedChat.Say(Hitman.Instance.Translations.Instance.Translate("hitman_general_chat_increased", otherPlayer.DisplayName, bounty, Convert.ToDecimal(Hitman.Instance.HitmanDatabase.GetBounty(otherPlayer.CSteamID)) + bounty), UnityEngine.Color.yellow);
                }
                else
                {
                    UnturnedChat.Say(Hitman.Instance.Translations.Instance.Translate("hitman_general_chat_created", otherPlayer.DisplayName, bounty), UnityEngine.Color.yellow);
                }

                Uconomy.Instance.Database.IncreaseBalance(caller.Id.ToString(), -bounty);
                Hitman.Instance.HitmanDatabase.AddUpdateVictimAccount(otherPlayer.CSteamID, bounty, otherPlayer.DisplayName);
            }
        }
    }
}
