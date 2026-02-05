// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace YouTubePlayerEX.App.Localisation
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum Language
    {
        [Description(@"繁體中文（台灣）")]
        zh_hant,

        [Description(@"Nederlands")]
        nl,

        [Description(@"Español")]
        es,

        [Description(@"Italiano")]
        it,

        [Description(@"Português")]
        pt,

        [Description(@"English")]
        en,

        [Description(@"日本語")]
        ja,

        [Description(@"한국어")]
        ko,

        [Description(@"Français")]
        fr,

        [Description(@"Русский")]
        ru,

        [Description(@"Türkçe")]
        tr,

        [Description(@"Tiếng Việt")]
        vi,

#if DEBUG
        [Description(@"Debug (show raw keys)")]
        debug
#endif
    }
}
