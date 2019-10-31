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
                DatabaseAddress = "localhost",
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