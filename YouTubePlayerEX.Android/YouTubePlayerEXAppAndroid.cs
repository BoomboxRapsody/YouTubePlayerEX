// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Android.App;
using Android.Content.PM;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Extensions.ObjectExtensions;
using YouTubePlayerEX.App;
using YouTubePlayerEX.App.Updater;

namespace YouTubePlayerEX.Android
{
    public partial class YouTubePlayerEXAppAndroid : YouTubePlayerEXApp
    {
        [Cached]
        private readonly YouTubePlayerEXAppActivity gameActivity;

        private readonly PackageInfo packageInfo;

        public YouTubePlayerEXAppAndroid(YouTubePlayerEXAppActivity activity)
            : base()
        {
            gameActivity = activity;
            packageInfo = Application.Context.ApplicationContext!.PackageManager!.GetPackageInfo(Application.Context.ApplicationContext.PackageName!, 0).AsNonNull();
        }

        public override string Version
        {
            get
            {
                if (!IsDeployedBuild)
                    return @"local " + (DebugUtils.IsDebugBuild ? @"debug" : @"release");

                return packageInfo.VersionName.AsNonNull();
            }
        }

        public override Version AssemblyVersion => new Version(packageInfo.VersionName.AsNonNull().Split('-').First());

        protected override UpdateManager CreateUpdateManager() => new NoActionUpdateManager();

        public override MediaSession CreateMediaSession() => new AndroidMediaSessionHandler(gameActivity);
    }
}
