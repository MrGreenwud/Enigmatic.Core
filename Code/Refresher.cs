using UnityEditor;

namespace Enigmatic.Core
{
    public static class Refresher
    {
        [MenuItem("Tools/Refresh")]
        public static void Refresh()
        {
            AssetDatabase.Refresh();
        }
    }
}