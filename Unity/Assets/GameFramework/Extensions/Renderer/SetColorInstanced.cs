﻿using System.Collections;
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

        /// <summary>
        /// Returns the instanced color value using MaterialBlocks
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Color GetColorInstanced(this Renderer renderer, string name) {
            MaterialPropertyBlock mBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mBlock);
            return mBlock.GetColor(name);
        }

        /// <summary>
        /// Sets an instanced rendering compatible float for this renderer using MaterialBlocks. If you want to set multiple floats at once, do not use this as it has a little overhead.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetFloatInstanced(this Renderer renderer, string name, float value) {
            MaterialPropertyBlock mBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mBlock);
            mBlock.SetFloat(name, value);
            renderer.SetPropertyBlock(mBlock);
        }

        /// <summary>
        /// Returns the instanced float value using MaterialBlocks.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static float GetFloatInstanced(this Renderer renderer, string name) {
            MaterialPropertyBlock mBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mBlock);
            return mBlock.GetFloat(name);
        }

        /// <summary>
        /// Sets an instanced rendering compatible vector for this renderer using MaterialBlocks. If you want to set multiple vectors at once, do not use this as it has a little overhead.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetVectorInstanced(this Renderer renderer, string name, Vector4 value) {
            MaterialPropertyBlock mBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mBlock);
            mBlock.SetVector(name, value);
            renderer.SetPropertyBlock(mBlock);
        }

        /// <summary>
        /// Returns the instanced vector using MaterialBlocks.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Vector4 GetVectorInstanced(this Renderer renderer, string name) {
            MaterialPropertyBlock mBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mBlock);
            return mBlock.GetVector(name);
        }
    }
}
