using UnityEditor.PackageManager;

public class NoesisVersion
{
    public static string Get()
    {
        var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.noesis.noesisgui");
        return info.version;
    }
}