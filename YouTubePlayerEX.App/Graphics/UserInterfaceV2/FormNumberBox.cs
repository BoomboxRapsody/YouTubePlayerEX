using osu.Framework.Input;

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    public partial class FormNumberBox : FormTextBox
    {
        private readonly bool allowDecimals;

        public FormNumberBox(bool allowDecimals = false)
        {
            this.allowDecimals = allowDecimals;
        }

        internal override InnerTextBox CreateTextBox() => new InnerNumberBox(allowDecimals)
        {
            SelectAllOnFocus = true,
        };

        internal partial class InnerNumberBox : InnerTextBox
        {
            public InnerNumberBox(bool allowDecimals)
            {
                InputProperties = new TextInputProperties(allowDecimals ? TextInputType.Decimal : TextInputType.Number, false);
            }
        }
    }
}
