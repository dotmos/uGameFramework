using UnityEngine;

namespace UserInterface {
    /// <summary>
    /// Helper class for our menu names
    /// </summary>
    public class NamingHelper {
        public const string BaseName = "Gentlymad UI";
        public const string Separator = "/";

        /// <summary>
        /// menu names for everything scrollbar specific
        /// </summary>
        public static class Scrollbar {
            public const string Name = BaseName + Separator + nameof(Scrollbar);
            public const string HierachyName = nameof(GameObject) + Separator + Name;
        }

        public static class Button {
            public const string Name = BaseName + Separator + nameof(Button);
            public const string HierachyName = nameof(Button) + Separator + Name;
        }

        public static class Toggle {
            public const string Name = BaseName + Separator + nameof(Toggle);
            public const string HierachyName = nameof(Toggle) + Separator + Name;
        }

        public static class ResizableLayoutElement {
            public const string Name = BaseName + Separator + nameof(ResizableLayoutElement);
            public const string HierachyName = nameof(ResizableLayoutElement) + Separator + Name;
        }

        public static class ResizeHandler {
            public const string Name = BaseName + Separator + nameof(ResizeHandler);
            public const string HierachyName = nameof(ResizeHandler) + Separator + Name;
        }

        public static class Tab {
            public const string Name = BaseName + Separator + nameof(Tab);
            public const string HierachyName = nameof(Tab) + Separator + Name;
        }

        public static class Tabbar {
            public const string Name = BaseName + Separator + nameof(Tabbar);
            public const string HierachyName = nameof(Tabbar) + Separator + Name;
        }

        public static class ScrollRect {
            public const string Name = BaseName + Separator + nameof(ScrollRect);
            public const string HierachyName = nameof(ScrollRect) + Separator + Name;
        }

        public static class ScrollRectNavigation {
            public const string Name = BaseName + Separator + nameof(ScrollRectNavigation);
            public const string HierachyName = nameof(ScrollRectNavigation) + Separator + Name;
        }
    }
}
