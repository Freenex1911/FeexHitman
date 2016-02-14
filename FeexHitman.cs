using fr34kyn01535.Uconomy;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Steamworks;

namespace Freenex.FeexHitman
{
    public class FeexHitman : RocketPlugin<FeexHitmanConfiguration>
    {
        public DatabaseManager FeexHitmanDatabase;
        public static FeexHitman Instance;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList(){
                {"hitman_general_chat_created","Bounty placed: {0} ({1} $)"},
                {"hitman_general_chat_increased","Bounty increased by {1} $: {0} ({2} $)"},
                {"hitman_general_chat_received","{0} killed {1} and received the bounty of {2} $ ({3} %)."},
                {"hitman_general_not_found","Player not found."},
                {"hitman_general_invalid_parameter","Invalid parameter."},
                {"hitman_add_self","You can't place bounty on yourself."},
                {"hitman_add_amount","That's not a valid amount."},
                {"hitman_add_balance","You do not have enough money to place a bounty."},
                {"hitman_add_minimum","The bounty must be at least {0} $."},
                {"hitman_payout","The bounty of {0} has been paid to you. ({1} $)."},
                {"hitman_check_true","{0} has a bounty of {1} $."},
                {"hitman_check_false","{0} has no bounty."},
                {"hitman_list_true","{0} player/s on bounty list: {1}"},
                {"hitman_list_false","The bounty list is empty."}
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            FeexHitmanDatabase = new DatabaseManager();
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
            Logger.Log("Freenex's FeexHitman has been loaded!");
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            Logger.Log("Freenex's FeexHitman has been unloaded!");
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            if (FeexHitman.Instance.FeexHitmanDatabase.CheckExists(player.CSteamID))
            {
                FeexHitman.Instance.FeexHitmanDatabase.UpdateVictimDisplayName(player.CSteamID, player.DisplayName);
            }
        }

        private void UnturnedPlayerEvents_OnPlayerDeath(UnturnedPlayer player, SDG.Unturned.EDeathCause cause, SDG.Unturned.ELimb limb, CSteamID murderer)
        {
            UnturnedPlayer UPmurderer = UnturnedPlayer.FromCSteamID(murderer);

            try
            {
                if (player.Id == UPmurderer.Id) { return; }
            }
            catch { return; }

            decimal BountyPercentage = 0;

            foreach (Rocket.API.Serialisation.Permission playerPermission in UPmurderer.GetPermissions())
            {
                if (playerPermission.Name.ToLower().Contains("hitman.receive."))
                {
                    string BountyPermission = playerPermission.Name.ToLower().Replace("hitman.receive.", string.Empty);

                    decimal BountyPercentagePermission;
                    bool isPercentageNumeric = decimal.TryParse(BountyPermission, out BountyPercentagePermission);
                    if (!isPercentageNumeric) { Logger.LogError(BountyPermission + " is not numeric."); return; }

                    if (BountyPercentagePermission > BountyPercentage)
                    {
                        BountyPercentage = BountyPercentagePermission;
                    }
                }
            }

            if (BountyPercentage != 0)
            {
                if (FeexHitman.Instance.FeexHitmanDatabase.CheckExists(player.CSteamID))
                {
                    decimal amount = FeexHitman.Instance.FeexHitmanDatabase.GetBounty(player.CSteamID);
                    amount = System.Math.Round(amount * (BountyPercentage / 100), 2);
                    Uconomy.Instance.Database.IncreaseBalance(UPmurderer.Id, amount);
                    FeexHitman.Instance.FeexHitmanDatabase.RemoveVictimAccount(player.CSteamID);
                    if (FeexHitman.Instance.Translations.Instance.Translate("hitman_general_chat_received") != "hitman_general_chat_received")
                    {
                        UnturnedChat.Say(FeexHitman.Instance.Translations.Instance.Translate("hitman_general_chat_received", UPmurderer.DisplayName, player.DisplayName, amount.ToString(), BountyPercentage), UnityEngine.Color.yellow);
                    }
                }
            }
        }
    }
}
