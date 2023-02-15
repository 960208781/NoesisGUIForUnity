using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

internal class NoesisSettingsBuildProvider : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private NoesisSettings _settingsAdded;

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var wasDirty = IsPlayerSettingsDirty();
        _settingsAdded = null;

        NoesisSettings settings = NoesisSettings.Get();

        // Add NoesisSettings object assets, if it's not in there already
        var preloadedAssets = PlayerSettings.GetPreloadedAssets();
        if (!preloadedAssets.Contains(settings))
        {
            _settingsAdded = settings;
            var preloadedAssets_ = preloadedAssets.ToList();
            preloadedAssets_.Add(settings);
            PlayerSettings.SetPreloadedAssets(preloadedAssets_.ToArray());
        }

        if (!wasDirty)
        {
            ClearPlayerSettingsDirtyFlag();
        }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (_settingsAdded == null)
        {
            return;
        }

        var wasDirty = IsPlayerSettingsDirty();

        // Revert back to original state
        var preloadedAssets = PlayerSettings.GetPreloadedAssets().Where(x => x != _settingsAdded);
        PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());

        _settingsAdded = null;

        if (!wasDirty)
        {
            ClearPlayerSettingsDirtyFlag();
        }
    }

    private static bool IsPlayerSettingsDirty()
    {
        var settings = UnityEngine.Resources.FindObjectsOfTypeAll<PlayerSettings>();
        if (settings != null && settings.Length > 0)
        {
            return EditorUtility.IsDirty(settings[0]);
        }

        return false;
    }

    private static void ClearPlayerSettingsDirtyFlag()
    {
        var settings = UnityEngine.Resources.FindObjectsOfTypeAll<PlayerSettings>();
        if (settings != null && settings.Length > 0)
        {
            EditorUtility.ClearDirty(settings[0]);
        }
    }
}