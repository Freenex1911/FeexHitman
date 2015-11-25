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

namespace Freenex.Hitman
{
    public class Hitman : RocketPlugin<HitmanConfiguration>
    {
        public DatabaseManager HitmanDatabase;
        public static Hitman Instance;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList(){
                {"hitman_general_chat_created","Kopfgeld ausgesetzt: {0} ({1} €)"},
                {"hitman_general_chat_increased","Kopfgeld um {1} € erhöht: {0} ({2} €)"},
                {"hitman_general_chat_received","{0} hat {1} getötet und {2} € ({3} %) Kopfgeld erhalten."},
                {"hitman_general_not_found","Spieler nicht gefunden."},
                {"hitman_add_self","Du kannst dich nicht selber hinzufügen."},
                {"hitman_add_amount","Dies ist kein gültiger Betrag."},
                {"hitman_add_balance","Du besitzt nicht genug Geld um ein Kopfgeld auszusetzen."},
                {"hitman_add_minimum","Das Kopfgeld muss mindestens {0} € betragen."},
                {"hitman_payout","Das Kopfgeld von {0} wurde dir ausgezahlt. ({1} €)"},
                {"hitman_check_true","Das Kopfgeld von {0} beträgt {1} €."},
                {"hitman_check_false","Auf {0} ist kein Kopfgeld ausgesetzt."}
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            HitmanDatabase = new DatabaseManager();
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
            Logger.Log("Freenex's Hitman has been loaded!");
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            Logger.Log("Freenex's Hitman has been unloaded!");
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            if (Hitman.Instance.HitmanDatabase.CheckExists(player.CSteamID))
            {
                Hitman.Instance.HitmanDatabase.UpdateVictimDisplayName(player.CSteamID, player.DisplayName);
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

            if (UPmurderer.HasPermission("hitman.member"))
            {
				if (Hitman.Instance.HitmanDatabase.CheckExists(player.CSteamID))
                {
                    decimal amount = HitmanDatabase.GetBounty(player.CSteamID);
                    Uconomy.Instance.Database.IncreaseBalance(UPmurderer.Id, amount);
                    Hitman.Instance.HitmanDatabase.RemoveVictimAccount(player.CSteamID);
                    UnturnedChat.Say(Hitman.Instance.Translations.Instance.Translate("hitman_general_chat_received", UPmurderer.DisplayName, player.DisplayName, amount, 100), UnityEngine.Color.yellow);
                }
            }
            else if (UPmurderer.HasPermission("hitman.receive"))
            {
                if (Hitman.Instance.HitmanDatabase.CheckExists(player.CSteamID))
                {
                    decimal amount = HitmanDatabase.GetBounty(player.CSteamID);
                    amount = System.Math.Round(amount * (Hitman.Instance.Configuration.Instance.DefaultPercentage / 100), 2);
                    Uconomy.Instance.Database.IncreaseBalance(UPmurderer.Id, amount);
                    Hitman.Instance.HitmanDatabase.RemoveVictimAccount(player.CSteamID);
                    UnturnedChat.Say(Hitman.Instance.Translations.Instance.Translate("hitman_general_chat_received", UPmurderer.DisplayName, player.DisplayName, amount, Hitman.Instance.Configuration.Instance.DefaultPercentage), UnityEngine.Color.yellow);
                }
            }
        }
    }
}
