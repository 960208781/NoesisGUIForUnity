using UnityEngine;
using System.IO;
using System;

public class NoesisXaml: ScriptableObject
{
    void OnDisable()
    {
        UnregisterDependencies();
    }

    public object Load()
    {
        RegisterDependencies();

        NoesisUnity.Init();
        return Noesis.GUI.LoadXaml(new MemoryStream(content), uri);
    }

    public void RegisterDependencies()
    {
        if (!_registered)
        {
            NoesisUnity.InitCore();
            _registered = true;
            _RegisterDependencies();
        }
    }

    public void UnregisterDependencies()
    {
        if (_registered)
        {
            _UnregisterDependencies();
            _registered = false;
        }
    }

    private void _RegisterDependencies()
    {
        if (uri != null)
        {
            // Self-register, we need this for hot-reloading
            NoesisXamlProvider.instance.Register(uri, this);
        }
    
        if (xamls != null)
        {
            foreach (var xaml in xamls)
            {
                if (xaml.uri != null && xaml.xaml != null)
                {
                    NoesisXamlProvider.instance.Register(xaml.uri, xaml.xaml);
                    xaml.xaml.RegisterDependencies();
                }
            }
        }

        if (textures != null)
        {
            foreach (var texture in textures)
            {
                if (texture.uri != null && texture.texture != null)
                {
                    NoesisTextureProvider.instance.Register(texture.uri, texture.texture);
                }
            }
        }

        if (sprites != null)
        {
            foreach (var sprite in sprites)
            {
                if (sprite.uri != null && sprite.sprite != null)
                {
                    NoesisTextureProvider.instance.Register(sprite.uri, sprite.sprite);
                }
            }
        }

        if (fonts != null)
        {
            foreach (var font in fonts)
            {
                if (font.uri != null && font.font != null)
                {
                    NoesisFontProvider.instance.Register(font.uri, font.font);
                }
            }
        }

        if (audios != null)
        {
            foreach (var audio in audios)
            {
                if (audio.uri != null && audio.audio != null)
                {
                    AudioProvider.instance.Register(audio.uri, audio.audio);
                }
            }
        }

        if (videos != null)
        {
            foreach (var video in videos)
            {
                if (video.uri != null && video.video != null)
                {
                    VideoProvider.instance.Register(video.uri, video.video);
                }
            }
        }

        if (shaders != null)
        {
            foreach (var shader in shaders)
            {
                if (shader.uri != null && shader.shader != null)
                {
                    NoesisShaderProvider.instance.Register(shader.uri, shader.shader);
                }
            }
        }
    }

    private void _UnregisterDependencies()
    {
        if (uri != null)
        {
            NoesisXamlProvider.instance.Unregister(uri);
        }

        if (xamls != null)
        {
            foreach (var xaml in xamls)
            {
                if (xaml.uri != null)
                {
                    // Don't call UnregisterDependencies here because this dependency could be active
                    // in more places. OnDisable() will automatically call UnregisterDependencies
                    NoesisXamlProvider.instance.Unregister(xaml.uri);
                }
            }
        }

        if (textures != null)
        {
            foreach (var texture in textures)
            {
                if (texture.uri != null)
                {
                    NoesisTextureProvider.instance.Unregister(texture.uri);
                }
            }
        }

        if (sprites != null)
        {
            foreach (var sprite in sprites)
            {
                if (sprite.uri != null)
                {
                    NoesisTextureProvider.instance.Unregister(sprite.uri);
                }
            }
        }

        if (fonts != null)
        {
            foreach (var font in fonts)
            {
                if (font.uri != null)
                {
                    NoesisFontProvider.instance.Unregister(font.uri);
                }
            }
        }

        if (audios != null)
        {
            foreach (var audio in audios)
            {
                if (audio.uri != null)
                {
                    AudioProvider.instance.Unregister(audio.uri);
                }
            }
        }

        if (videos != null)
        {
            foreach (var video in videos)
            {
                if (video.uri != null)
                {
                    VideoProvider.instance.Unregister(video.uri);
                }
            }
        }

        if (shaders != null)
        {
            foreach (var shader in shaders)
            {
                if (shader.uri != null)
                {
                    NoesisShaderProvider.instance.Unregister(shader.uri);
                }
            }
        }
    }

    public string uri;
    public byte[] content;

    [System.Serializable]
    public struct Xaml
    {
        public string uri;
        public NoesisXaml xaml;
    }

    [System.Serializable]
    public struct Font
    {
        public string uri;
        public NoesisFont font;
    }

    [System.Serializable]
    public struct Texture
    {
        public string uri;
        public UnityEngine.Texture texture;
    }

    [System.Serializable]
    public struct Sprite
    {
        public string uri;
        public UnityEngine.Sprite sprite;
    }

    [System.Serializable]
    public struct Audio
    {
        public string uri;
        public UnityEngine.AudioClip audio;
    }

    [System.Serializable]
    public struct Video
    {
        public string uri;
        public UnityEngine.Video.VideoClip video;
    }

    [System.Serializable]
    public struct Shader
    {
        public string uri;
        public NoesisShader shader;
    }

    public Xaml[] xamls;
    public Font[] fonts;
    public Texture[] textures;
    public Sprite[] sprites;
    public Audio[] audios;
    public Video[] videos;
    public Shader[] shaders;

    private bool _registered = false;
}
