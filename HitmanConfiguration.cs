using Rocket.API;

namespace Freenex.Hitman
{
    public class HitmanConfiguration : IRocketPluginConfiguration
    {
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public int DatabasePort;

        public decimal MinimumBounty;
        public decimal CommandListMaximum;

        public void LoadDefaults()
        {
            DatabaseAddress = "localhost";
            DatabaseUsername = "unturned";
            DatabasePassword = "password";
            DatabaseName = "unturned";
            DatabaseTableName = "hitman";
            DatabasePort = 3306;
            MinimumBounty = 100;
            CommandListMaximum = 20;
        }
    }
}
