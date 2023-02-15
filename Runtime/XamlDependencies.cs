using Noesis;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Helper classes to inject dependencies to XAMLs
/// </summary>
namespace NoesisGUIExtensions
{
    public class Dependency
    {
        private Uri _uri;
        public Uri Source
        {
            get { return _uri; }
            set
            {
                _uri = value;
#if UNITY_EDITOR
                string path = _uri?.GetPath();
                if (!string.IsNullOrEmpty(path) && !File.Exists(path))
                {
                    UnityEngine.Debug.LogError("[noesis] Dependency not found '" + path +
                        "'. Make sure absolute paths start with '/'.");
                }
#endif
            }
        }
    }

    public static class Xaml
    {
        public static readonly DependencyProperty DependenciesProperty = DependencyProperty.Register(
            "Dependencies", typeof(List<Dependency>), typeof(Xaml), new PropertyMetadata(null));
    }
}