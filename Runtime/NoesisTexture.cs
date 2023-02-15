using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Noesis
{
    public partial class Texture
    {
        public static void RegisterCallbacks()
        {
            Noesis_SetUnregisterTextureCallback(_unregisterTexture);
        }

        internal static Texture WrapTexture(UnityEngine.Texture tex)
        {
            if (tex == null)
            {
                return null;
            }

            return WrapTexture(tex, EnsureNativePointer(tex), tex.width, tex.height, 1);
        }

        internal static Texture WrapTexture(UnityEngine.Texture2D tex)
        {
            if (tex == null)
            {
                return null;
            }

            return WrapTexture(tex, EnsureNativePointer(tex), tex.width, tex.height, tex.mipmapCount);
        }

        private static Texture WrapTexture(object texture, IntPtr nativePointer,
            int width, int height, int numLevels)
        {
            IntPtr texturePtr = Noesis_WrapTexture(nativePointer, width, height, numLevels);
            Noesis.Texture tex = new Noesis.NativeTexture(texturePtr, true);

            // A texture with the same pointer could be enqueued for release, just remove it
            // now to avoid an exception when adding the new texture to the dictionary
            if (Textures.ContainsKey(texturePtr.ToInt64()))
            {
                Noesis_RemoveEnqueuedTexture(texturePtr);
                Textures.Remove(texturePtr.ToInt64());
            }

            Textures.Add(texturePtr.ToInt64(), texture);

            return tex;
        }

        internal static IntPtr EnsureNativePointer(UnityEngine.Texture tex)
        {
            IntPtr nativeTexturePtr = tex.GetNativeTexturePtr();
            if (nativeTexturePtr == IntPtr.Zero)
            {
                UnityEngine.RenderTexture renderTexture = tex as UnityEngine.RenderTexture;
                if (renderTexture != null)
                {
                    renderTexture.Create();
                }

                nativeTexturePtr = tex.GetNativeTexturePtr();
                if (nativeTexturePtr == IntPtr.Zero)
                {
                    throw new System.Exception("Can't create TextureSource, texture native pointer is null");
                }
            }

            return nativeTexturePtr;
        }

        private static Dictionary<long, object> Textures = new Dictionary<long, object>();

        private delegate void UnregisterTextureCallback(IntPtr texturePtr);
        private static UnregisterTextureCallback _unregisterTexture = UnregisterTexture;
        [MonoPInvokeCallback(typeof(UnregisterTextureCallback))]
        private static void UnregisterTexture(IntPtr texturePtr)
        {
            try
            {
                Textures.Remove(texturePtr.ToInt64());
            }
            catch (Exception exception)
            {
                Error.UnhandledException(exception);
            }
        }

        #region Imports
        [DllImport(Library.Name)]
        static extern void Noesis_RemoveEnqueuedTexture(IntPtr texturePtr);

        [DllImport(Library.Name)]
        static extern IntPtr Noesis_WrapTexture(IntPtr texture, int width, int height,int numLevels);

        [DllImport(Library.Name)]
        static extern void Noesis_SetUnregisterTextureCallback(UnregisterTextureCallback callback);
        #endregion
    }
}
