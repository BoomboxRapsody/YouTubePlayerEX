// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace NekoPlayer.App.Utils
{
    public enum ShutdownOptions
    {
        [Description("Normal app quit")]
        None = 0,

        [Description("Quit and shutdown")]
        Shutdown = 1,

        [Description("Quit and restart system")]
        Restart = 2,
    }
}
