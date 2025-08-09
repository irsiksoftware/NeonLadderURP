using UnityEditor;
using UnityEngine;

namespace NeonLadder.Editor
{
    public static class PackageManagement
    {
        [MenuItem("NeonLadder/Package Management/Open Package Manager", priority = 60)]
        public static void OpenPackageManager()
        {
            PackageManagerWindow.ShowWindow();
        }
    }
}