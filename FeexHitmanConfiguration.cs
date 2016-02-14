using Rocket.API;

namespace Freenex.FeexHitman
{
    public class FeexHitmanConfiguration : IRocketPluginConfiguration
    {
        public string DatabaseTableName;
        public decimal MinimumBounty;
        public decimal CommandListMaximum;

        public void LoadDefaults()
        {
            DatabaseTableName = "hitman";
            MinimumBounty = 100;
            CommandListMaximum = 20;
        }
    }
}
