using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NeonLadder.BuildSystem
{
    public static class SteamVDFGenerator
    {
        public static bool GenerateVDFFiles(BuildProfile profile)
        {
            try
            {
                string latestBuildPath = FindLatestBuildDirectory(profile);
                if (string.IsNullOrEmpty(latestBuildPath))
                {
                    Debug.LogError("No build directory found. Build the project first.");
                    return false;
                }

                string steamFolder = Path.Combine(Application.dataPath, "..", "Steam", "deployment-configurations");
                Directory.CreateDirectory(steamFolder);

                // Generate App Build VDF
                GenerateAppBuildVDF(steamFolder, latestBuildPath, profile);

                // Generate Depot Build VDF
                GenerateDepotBuildVDF(steamFolder, latestBuildPath, profile);

                // Generate Deploy CMD
                GenerateDeployCMD(steamFolder, latestBuildPath, profile);

                Debug.Log($"Steam deployment files generated successfully!");
                Debug.Log($"Latest build: {latestBuildPath}");
                Debug.Log($"Files saved to: {steamFolder}");

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to generate VDF files: {e.Message}");
                return false;
            }
        }

        private static string FindLatestBuildDirectory(BuildProfile profile)
        {
            string buildsRoot = Path.Combine(Application.dataPath, "..", "Builds", "Windows");

            if (!Directory.Exists(buildsRoot))
                return null;

            // Look for steam-demo directories with timestamps
            var buildDirs = Directory.GetDirectories(buildsRoot, "steam-demo-*")
                .Select(dir => new DirectoryInfo(dir))
                .OrderByDescending(d => d.CreationTime)
                .ToArray();

            if (buildDirs.Length == 0)
                return null;

            return buildDirs[0].Name; // Return just the folder name, not full path
        }

        private static void GenerateAppBuildVDF(string steamFolder, string latestBuildPath, BuildProfile profile)
        {
            // Get the absolute path to the build folder
            string absoluteBuildPath = Path.Combine(Application.dataPath, "..", "Builds", "Windows", latestBuildPath);
            absoluteBuildPath = Path.GetFullPath(absoluteBuildPath); // Normalize the path

            string content = "\"AppBuild\"\n" +
                "{\n" +
                "\t\"AppID\" \"3089980\" // NeonLadder Demo App ID\n" +
                "\t\"Desc\" \"NeonLadder Steam Demo Build - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\" // Description for this build\n" +
                "\t\"ContentRoot\" \"" + absoluteBuildPath.Replace(@"\", @"\\") + "\" // Absolute path to the specific build folder\n" +
                "\t\"BuildOutput\" \"..\\..\\output\\\" // Build logs and cache output (one level deeper now)\n" +
                "\t\"Preview\" \"0\" // Set to 1 for testing without uploading\n" +
                "\t// Local key removed - including it forces LCS mode even when set to \"0\"\n" +
                "\t\"SetLive\" \"\" // Don't automatically set live - do it manually in Steamworks\n" +
                "\n" +
                "\t\"Depots\"\n" +
                "\t{\n" +
                "\t\t\"3089981\" // NeonLadder Demo Depot (Windows 64-bit)\n" +
                "\t\t{\n" +
                "\t\t\t\"FileMapping\"\n" +
                "\t\t\t{\n" +
                "\t\t\t\t// Map all files from the build folder\n" +
                "\t\t\t\t\"LocalPath\" \"*\" // All files from this build folder\n" +
                "\t\t\t\t\"DepotPath\" \".\" // Map to root of depot\n" +
                "\t\t\t\t\"Recursive\" \"1\" // Include all subfolders\n" +
                "\t\t\t}\n" +
                "\n" +
                "\t\t\t// Exclude files we don't want in the depot\n" +
                "\t\t\t\"FileExclusion\" \"*.pdb\" // Exclude debug symbols\n" +
                "\t\t\t\"FileExclusion\" \"*.log\" // Exclude log files\n" +
                "\t\t\t\"FileExclusion\" \"*_BurstDebugInformation_*\" // Exclude Unity burst debug info\n" +
                "\t\t\t\"FileExclusion\" \"*_BackUpThisFolder_*\" // Exclude Unity backup folders\n" +
                "\t\t\t\"FileExclusion\" \"steam_appid.txt\" // Don't upload local steam_appid file\n" +
                "\t\t\t\"FileExclusion\" \"Thumbs.db\" // Windows thumbnail cache\n" +
                "\t\t\t\"FileExclusion\" \".DS_Store\" // macOS folder metadata\n" +
                "\t\t}\n" +
                "\t}\n" +
                "}";

            File.WriteAllText(Path.Combine(steamFolder, "app_build_3089980_demo.vdf"), content);
        }

        private static void GenerateDepotBuildVDF(string steamFolder, string latestBuildPath, BuildProfile profile)
        {
            // Get the absolute path to the build folder
            string absoluteBuildPath = Path.Combine(Application.dataPath, "..", "Builds", "Windows", latestBuildPath);
            absoluteBuildPath = Path.GetFullPath(absoluteBuildPath); // Normalize the path

            string content = "\"DepotBuild\"\n" +
                "{\n" +
                "\t\"DepotID\" \"3089981\" // NeonLadder Demo Depot ID\n" +
                "\n" +
                "\t// Set the content root to the specific build folder\n" +
                "\t\"ContentRoot\" \"" + absoluteBuildPath.Replace(@"\", @"\\") + "\"\n" +
                "\n" +
                "\t// Files to include in this depot\n" +
                "\t\"FileMapping\"\n" +
                "\t{\n" +
                "\t\t// Map everything from this build folder\n" +
                "\t\t\"LocalPath\" \"*\"\n" +
                "\t\t\"DepotPath\" \".\"\n" +
                "\t\t\"Recursive\" \"1\"\n" +
                "\t}\n" +
                "\n" +
                "\t// Files to exclude from the depot\n" +
                "\t\"FileExclusion\" \"*.pdb\"\n" +
                "\t\"FileExclusion\" \"*.log\"\n" +
                "\t\"FileExclusion\" \"*_BurstDebugInformation_*\"\n" +
                "\t\"FileExclusion\" \"*_BackUpThisFolder_*\"\n" +
                "\t\"FileExclusion\" \"steam_appid.txt\"\n" +
                "\t\"FileExclusion\" \"Thumbs.db\"\n" +
                "\t\"FileExclusion\" \".DS_Store\"\n" +
                "\n" +
                "\t// File properties\n" +
                "\t\"FileProperties\"\n" +
                "\t{\n" +
                "\t\t// Mark the main executable\n" +
                "\t\t\"" + profile.executableName + ".exe\"\n" +
                "\t\t{\n" +
                "\t\t\t\"oslist\" \"windows\"\n" +
                "\t\t\t\"osarch\" \"64\"\n" +
                "\t\t}\n" +
                "\t}\n" +
                "}";

            File.WriteAllText(Path.Combine(steamFolder, "depot_build_3089981.vdf"), content);
        }

        private static void GenerateDeployCMD(string steamFolder, string latestBuildPath, BuildProfile profile)
        {
            string template = @"@echo off
echo ====================================
echo NeonLadder Steam Deployment Script
echo ====================================
echo.

echo Deploying NeonLadder Demo to Steam...
echo App ID: 3089980
echo Generated: {GENERATION_DATE}
echo Latest Build: {BUILD_FOLDER}
echo.

REM Change to Steam folder
cd /d ""%~dp0""

REM Verify steamcmd exists
if not exist "".\..\steamcmd.exe"" (
    echo ===============================================
    echo ERROR: steamcmd.exe not found in Steam folder
    echo ===============================================
    echo.
    echo Please install SteamCMD:
    echo 1. Download from: https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip
    echo 2. Extract steamcmd.exe to: %~dp0
    echo 3. Run this script again
    echo.
    echo OR if SteamCMD is installed elsewhere:
    echo Copy steamcmd.exe to this Steam folder
    echo.
    pause
    exit /b 1
)

echo Starting Steam upload...
echo Username: {STEAM_USERNAME} (update this in the script if needed)
echo.

"".\..\steamcmd.exe"" +login {STEAM_USERNAME} +run_app_build ""%~dp0app_build_3089980_demo.vdf"" +quit

echo.
echo ====================================
echo Deployment complete!
echo ====================================
echo Check the output above for any errors
echo.
echo If successful, your build is now on Steam!
echo Remember to set it live in Steamworks if needed.
pause";

            string content = template
                .Replace("{GENERATION_DATE}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{BUILD_FOLDER}", latestBuildPath)
                .Replace("{STEAM_USERNAME}", "irsiksoftware");

            File.WriteAllText(Path.Combine(steamFolder, "deploy_steam_demo.cmd"), content);
        }
    }
}