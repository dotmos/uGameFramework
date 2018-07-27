using UnityEngine;

namespace UserInterface {
    /// <summary>
    /// Helper class for our menu names.
    /// Avoid written out strings where possible.
    /// </summary>
    public class NamingHelper {
        public const string BasePrefix = "GM";
        public const string BaseName = "Gentlymad UI";
        public const string Separator = "/";
        public const string TypeSeparator = "_";
        public const string DisplaySpace = " ";

        /// <summary>
        /// menu names for everything scrollbar specific.
        /// </summary>
        public static class Scrollbar {
            public const string Name = BaseName + Separator + nameof(Scrollbar);
            public const string HierachyName = nameof(GameObject) + Separator + Name;
        }

        /// <summary>
        /// menu names for everything Button specific.
        /// </summary>
        public static class Button {
            public const string Name = BaseName + Separator + nameof(Button);
            public const string HierachyName = nameof(GameObject) + Separator + Name;
            public static class Variants {
                public const string Icon = "Icon";
                public const string Label = "Label";
                public const string IconLabel = Icon + Label;
            }
        }

        /// <summary>
        /// menu names for everything Toggle specific.
        /// </summary>
        public static class Toggle {
            public const string Name = BaseName + Separator + nameof(Toggle);
            public const string HierachyName = nameof(GameObject) + Separator + Name;
        }

        /// <summary>
        /// menu names for everything ResizableLayout specific.
        /// </summary>
        public static class ResizableLayout {
            public const string PrefabName = BasePrefix + nameof(ResizableLayout);
            public const string Name = BaseName + Separator + nameof(ResizableLayout);
            public const string HierachyName = nameof(GameObject) + Separator + Name;
        }

        /// <summary>
        /// menu names for everything ResizeHandler specific.
        /// </summary>
        public static class ResizeHandler {
            public const string Name = BaseName + Separator + nameof(ResizeHandler);
            public const string HierachyName = nameof(GameObject) + Separator + Name;
        }

        /// <summary>
        /// menu names for everything Tab specific.
        /// </summary>
        public static class Tab {
            public const string Name = BaseName + Separator + nameof(Tab);
            public const string HierachyName = nameof(GameObject) + Separator + Name;
        }

        /// <summary>
        /// menu names for everything Tabbar specific.
        /// </summary>
        public static class Tabbar {
            public const string Name = BaseName + Separator + nameof(Tabbar);
            public const string HierachyName = nameof(GameObject) + Separator + Name;
        }

        /// <summary>
        /// menu names for everything ScrollRect specific.
        /// </summary>
        public static class ScrollRect {
            public const string PrefabName = "ScrollView";
            public const string Name = BaseName + Separator + nameof(ScrollRect);
            public const string HierachyName = nameof(GameObject) + Separator + BaseName + Separator + PrefabName;
            public static class Variants {
                public const string Vertical = "Vertical";
            }
        }

        /// <summary>
        /// menu names for everything ScrollRectNavigation specific.
        /// </summary>
        public static class ScrollRectNavigation {
            public const string Name = BaseName + Separator + nameof(ScrollRectNavigation);
            public const string HierachyName = nameof(GameObject) + Separator + Name;
        }
    }
}
