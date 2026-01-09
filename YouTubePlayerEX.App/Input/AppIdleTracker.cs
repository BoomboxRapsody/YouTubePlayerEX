using osu.Framework.Input;

namespace YouTubePlayerEX.App.Input
{
    public partial class AppIdleTracker : IdleTracker
    {
        private InputManager inputManager;

        public AppIdleTracker(int time)
            : base(time)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();
        }

        protected override bool AllowIdle => inputManager.FocusedDrawable == null;
    }
}
