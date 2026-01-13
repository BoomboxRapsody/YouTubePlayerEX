using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class RoundedButtonContainer : Container
    {
        public Action<RoundedButtonContainer>? ClickAction { get; set; }

        private void trigger()
        {
            ClickAction?.Invoke(this);
        }

        protected override bool OnClick(ClickEvent e)
        {
            trigger();

            return base.OnClick(e);
        }
    }
}
