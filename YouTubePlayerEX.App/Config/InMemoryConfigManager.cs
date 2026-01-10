using System;
using osu.Framework.Configuration;

namespace YouTubePlayerEX.App.Config
{
    public class InMemoryConfigManager<TLookup> : ConfigManager<TLookup>
        where TLookup : struct, Enum
    {
        public InMemoryConfigManager()
        {
            InitialiseDefaults();
        }

        protected override void PerformLoad()
        {
        }

        protected override bool PerformSave() => true;
    }
}
