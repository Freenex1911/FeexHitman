using Rocket.API;

namespace Freenex.FeexHitman
{
    public class FeexHitmanConfiguration : IRocketPluginConfiguration
    {
        public classDatabase FeexHitmanDatabase;
        public decimal MinimumBounty;
        public decimal CommandListMaximum;

        public void LoadDefaults()
        {
            FeexHitmanDatabase = new classDatabase()
            {
                DatabaseAddress = "127.0.0.1",
                DatabaseUsername = "unturned",
                DatabasePassword = "password",
                DatabaseName = "unturned",
                DatabaseTableName = "hitman",
                DatabasePort = 3306
            };

            MinimumBounty = 100;
            CommandListMaximum = 20;
        }
    }
}
