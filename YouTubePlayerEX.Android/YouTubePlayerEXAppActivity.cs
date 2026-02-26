// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using osu.Framework;
using osu.Framework.Android;
using Debug = System.Diagnostics.Debug;

namespace YouTubePlayerEX.Android
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class YouTubePlayerEXAppActivity : AndroidGameActivity
    {
        private readonly YouTubePlayerEXAppAndroid game;

        private bool gameCreated;

        protected override Game CreateGame()
        {
            if (gameCreated)
                throw new InvalidOperationException("Framework tried to create a game twice.");

            gameCreated = true;
            return game;
        }

        public YouTubePlayerEXAppActivity()
        {
            game = new YouTubePlayerEXAppAndroid(this);
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Debug.Assert(Window != null);

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            Debug.Assert(WindowManager?.DefaultDisplay != null);
            Debug.Assert(Resources?.DisplayMetrics != null);

            RequestedOrientation = ScreenOrientation.SensorLandscape;
        }
    }
}
