using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using YouTubePlayerEX.App.Audio.Effects;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Graphics.Containers;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Screens;
using YouTubePlayerEX.App.Updater;

namespace YouTubePlayerEX.App
{
    [Cached(typeof(YouTubePlayerEXApp))]
    public partial class YouTubePlayerEXApp : YouTubePlayerEXAppBase
    {
        private ScreenStack screenStack;

        public static FontUsage DefaultFont = FontUsage.Default.With("Pretendard", 16, "Regular");

        private BindableNumber<double> sampleVolume = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
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

            UpdateManager updateManager;
            dependencies.Cache(updateManager = CreateUpdateManager());
            Add(updateManager);

            // Add your top-level game components here.
            // A screen stack and sample screen has been provided for convenience, but you can replace it if you don't want to use screens.
            AddRange(new Drawable[]
            {
                new ScalingContainer
                {
                    Child = screenStack = new ScreenStack
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                }
            });
        }

        private DependencyContainer dependencies;

        protected virtual UpdateManager CreateUpdateManager() => new UpdateManager();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(audioDuckFilter = new AudioFilter(Audio.TrackMixer));
            Audio.Tracks.AddAdjustment(AdjustableProperty.Volume, audioDuckVolume);
            sampleVolume = Audio.VolumeSample.GetBoundCopy();

            screenStack.Push(new Loader());
        }

        private readonly List<DuckParameters> duckOperations = new List<DuckParameters>();
        private readonly BindableDouble audioDuckVolume = new BindableDouble(1);
        private AudioFilter audioDuckFilter = null!;

        /// <summary>
        /// Applies ducking, attenuating the volume and/or low-pass cutoff of the currently playing track to make headroom for effects (or just to apply an effect).
        /// </summary>
        /// <returns>A <see cref="IDisposable"/> which will restore the duck operation when disposed.</returns>
        public IDisposable Duck(DuckParameters? parameters = null)
        {
            // Don't duck if samples have no volume, it sounds weird.
            if (sampleVolume.Value == 0)
                return new InvokeOnDisposal(() => { });

            parameters ??= new DuckParameters();

            duckOperations.Add(parameters);

            DuckParameters volumeOperation = duckOperations.MinBy(p => p.DuckVolumeTo)!;
            DuckParameters lowPassOperation = duckOperations.MinBy(p => p.DuckCutoffTo)!;

            audioDuckFilter.CutoffTo(lowPassOperation.DuckCutoffTo, lowPassOperation.DuckDuration, lowPassOperation.DuckEasing);
            this.TransformBindableTo(audioDuckVolume, volumeOperation.DuckVolumeTo, volumeOperation.DuckDuration, volumeOperation.DuckEasing);

            return new InvokeOnDisposal(restoreDucking);

            void restoreDucking() => Schedule(() =>
            {
                if (!duckOperations.Remove(parameters))
                    return;

                DuckParameters? restoreVolumeOperation = duckOperations.MinBy(p => p.DuckVolumeTo);
                DuckParameters? restoreLowPassOperation = duckOperations.MinBy(p => p.DuckCutoffTo);

                // If another duck operation is in the list, restore ducking to its level, else reset back to defaults.
                audioDuckFilter.CutoffTo(restoreLowPassOperation?.DuckCutoffTo ?? AudioFilter.MAX_LOWPASS_CUTOFF, parameters.RestoreDuration, parameters.RestoreEasing);
                this.TransformBindableTo(audioDuckVolume, restoreVolumeOperation?.DuckVolumeTo ?? 1, parameters.RestoreDuration, parameters.RestoreEasing);
            });
        }

        public class DuckParameters
        {
            /// <summary>
            /// The duration of the ducking transition in milliseconds.
            /// Defaults to 100 ms.
            /// </summary>
            public double DuckDuration = 100;

            /// <summary>
            /// The final volume which should be reached during ducking, when 0 is silent and 1 is original volume.
            /// Defaults to 25%.
            /// </summary>
            public double DuckVolumeTo = 0.25;

            /// <summary>
            /// The low-pass cutoff frequency which should be reached during ducking. If not required, set to <see cref="AudioFilter.MAX_LOWPASS_CUTOFF"/>.
            /// Defaults to 300 Hz.
            /// </summary>
            public int DuckCutoffTo = 300;

            /// <summary>
            /// The easing curve to be applied during ducking.
            /// Defaults to <see cref="Easing.Out"/>.
            /// </summary>
            public Easing DuckEasing = Easing.Out;

            /// <summary>
            /// The duration of the restoration transition in milliseconds.
            /// Defaults to 500 ms.
            /// </summary>
            public double RestoreDuration = 500;

            /// <summary>
            /// The easing curve to be applied during restoration.
            /// Defaults to <see cref="Easing.In"/>.
            /// </summary>
            public Easing RestoreEasing = Easing.In;
        }
    }
}
