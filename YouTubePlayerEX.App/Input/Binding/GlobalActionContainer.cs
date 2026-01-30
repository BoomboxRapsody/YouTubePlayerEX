// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;

namespace YouTubePlayerEX.App.Input.Binding
{
    public partial class GlobalActionContainer : KeyBindingContainer<GlobalAction>, IHandleGlobalKeyboardInput, IKeyBindingHandler<GlobalAction>
    {
        protected override bool Prioritised => true;

        private readonly IKeyBindingHandler<GlobalAction>? handler;

        public GlobalActionContainer(YouTubePlayerEXAppBase? game)
            : base(matchingMode: KeyCombinationMatchingMode.Modifiers)
        {
            if (game is IKeyBindingHandler<GlobalAction> h)
                handler = h;
        }

        /// <summary>
        /// All default key bindings across all categories, ordered with highest priority first.
        /// </summary>
        /// <remarks>
        /// IMPORTANT: Take care when changing order of the items in the enumerable.
        /// It is used to decide the order of precedence, with the earlier items having higher precedence.
        /// </remarks>
        public override IEnumerable<IKeyBinding> DefaultKeyBindings => globalKeyBindings;

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e) => handler?.OnPressed(e) == true;

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e) => handler?.OnReleased(e);

        private static IEnumerable<KeyBinding> globalKeyBindings => new[]
        {
            new KeyBinding(InputKey.Enter, GlobalAction.Select),
            new KeyBinding(InputKey.KeypadEnter, GlobalAction.Select),

            new KeyBinding(InputKey.Escape, GlobalAction.Back),
            new KeyBinding(InputKey.ExtraMouseButton1, GlobalAction.Back),

            new KeyBinding(InputKey.Space, GlobalAction.PlayPause),
            new KeyBinding(InputKey.K, GlobalAction.PlayPause),

            new KeyBinding(InputKey.Right, GlobalAction.FastForward_10sec),
            new KeyBinding(InputKey.Left, GlobalAction.FastRewind_10sec),
            new KeyBinding(InputKey.L, GlobalAction.FastForward_10sec),
            new KeyBinding(InputKey.J, GlobalAction.FastRewind_10sec),

            new KeyBinding(InputKey.A, GlobalAction.DecreasePlaybackSpeed),
            new KeyBinding(InputKey.D, GlobalAction.IncreasePlaybackSpeed),

            new KeyBinding(new[] { InputKey.Shift, InputKey.C }, GlobalAction.CycleCaptionLanguage),
            new KeyBinding(new[] { InputKey.Control, InputKey.F6 }, GlobalAction.CycleAspectRatio),

            new KeyBinding(new[] { InputKey.Control, InputKey.O }, GlobalAction.OpenSettings),

            new KeyBinding(new[] { InputKey.Shift, InputKey.P }, GlobalAction.ToggleAdjustPitchOnSpeedChange),

            new KeyBinding(new[] { InputKey.Control, InputKey.P }, GlobalAction.ToggleFPSDisplay),
        };
    }

    /// <remarks>
    /// IMPORTANT: New entries should always be added at the end of the enum, as key bindings are stored using the enum's numeric value and
    /// changes in order would cause key bindings to get associated with the wrong action.
    /// </remarks>
    public enum GlobalAction
    {
        Back,
        Select,
        PlayPause,
        FastForward_10sec,
        FastRewind_10sec,
        OpenSettings,
        ToggleAdjustPitchOnSpeedChange,
        ToggleFPSDisplay,
        CycleCaptionLanguage,
        CycleAspectRatio,

        DecreasePlaybackSpeed,
        IncreasePlaybackSpeed,
    }
}
