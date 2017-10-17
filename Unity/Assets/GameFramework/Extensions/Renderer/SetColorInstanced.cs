using System.Collections;
using System.Collections.Generic;

namespace UnityEngine { 
    public static partial class RendererExtensions {

        /// <summary>
        /// Sets an instanced rendering compatible color for this renderer using MaterialBlocks. If you want to set multiple colors at once, do not use this as it has a little overhead.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="name"></param>
        /// <param name="color"></param>
        public static void SetColorInstanced(this Renderer renderer, string name, Color color) {
            MaterialPropertyBlock mBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mBlock);
            mBlock.SetColor(name, color);
            renderer.SetPropertyBlock(mBlock);
        }
    }
}
