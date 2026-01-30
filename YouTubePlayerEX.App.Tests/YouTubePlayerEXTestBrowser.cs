// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Tests
{
    public partial class YouTubePlayerEXTestBrowser : YouTubePlayerEXAppBase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            var languages = Enum.GetValues<Language>();

            var mappings = languages.Select(language =>
            {
                string cultureCode = language.ToCultureCode();

                try
                {
                    return new LocaleMapping(new ResourceManagerLocalisationStore(cultureCode));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Could not load localisations for language \"{cultureCode}\"");
                    return null;
                }
            }).Where(m => m != null);

            Localisation.AddLocaleMappings(mappings);

            AddRange(new Drawable[]
            {
                new TestBrowser("YouTubePlayerEX"),
            });
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            host.Window.CursorState |= CursorState.Hidden;
        }
    }
}
