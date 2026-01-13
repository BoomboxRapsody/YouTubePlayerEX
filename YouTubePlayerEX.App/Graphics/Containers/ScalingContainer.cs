using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using YouTubePlayerEX.App.Config;

namespace YouTubePlayerEX.App.Graphics.Containers
{
    public partial class ScalingContainer : DrawSizePreservingFillContainer
    {
        private Bindable<float>? uiScale;

        protected float CurrentScale { get; private set; } = 1;

        [BackgroundDependencyLoader]
        private void load(YTPlayerEXConfigManager appConfig)
        {
            uiScale = appConfig.GetBindable<float>(YTPlayerEXSetting.UIScale);
            uiScale.BindValueChanged(args => this.TransformTo(nameof(CurrentScale), args.NewValue, 500, Easing.OutQuart), true);
        }

        protected override void Update()
        {
            Scale = new Vector2(CurrentScale);
            Size = new Vector2(1 / CurrentScale);

            base.Update();
        }
    }
}
