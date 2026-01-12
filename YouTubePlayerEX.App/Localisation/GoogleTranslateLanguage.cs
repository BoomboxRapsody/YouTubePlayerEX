using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace YouTubePlayerEX.App.Localisation
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum GoogleTranslateLanguage
    {
        [Description(@"Auto detect")]
        auto,

        [Description(@"English")]
        en,

        [Description(@"日本語")]
        ja,

        [Description(@"한국어")]
        ko,
    }
}
