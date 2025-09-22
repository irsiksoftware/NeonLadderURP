using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace NeonLadder.BuildSystem
{
    [CreateAssetMenu(fileName = "BuildProfileManager", menuName = "NeonLadder/Build System/Build Profile Manager", order = 0)]
    public class BuildProfileManager : ScriptableObject
    {
        private const string MANAGER_PATH = "Assets/BuildProfiles/BuildProfileManager.asset";
        private const string PROFILES_FOLDER = "Assets/BuildProfiles";

        [Header("Profile Management")]
        public BuildProfile activeProfile;
        public List<BuildProfile> profiles = new List<BuildProfile>();

        [Header("Build History")]
        public int lastBuildNumber = 0;
        public string lastBuildDate = "";
        public string lastBuildProfile = "";

        [Header("Global Settings")]
        public bool autoIncrementBuildNumber = true;
        public bool showBuildNotifications = true;
        public bool validateBeforeBuild = true;

        private static BuildProfileManager _instance;
        public static BuildProfileManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<BuildProfileManager>(MANAGER_PATH);
                    if (_instance == null)
                    {
                        _instance = CreateNewManager();
                    }
                }
                return _instance;
            }
        }

        private static BuildProfileManager CreateNewManager()
        {
            if (!AssetDatabase.IsValidFolder("Assets/BuildProfiles"))
            {
                AssetDatabase.CreateFolder("Assets", "BuildProfiles");
            }

            var manager = CreateInstance<BuildProfileManager>();
            AssetDatabase.CreateAsset(manager, MANAGER_PATH);
            AssetDatabase.SaveAssets();

            Debug.Log("[BuildProfileManager] Created new Build Profile Manager at: " + MANAGER_PATH);
            return manager;
        }

        public void RefreshProfileList()
        {
            profiles.Clear();

            string[] guids = AssetDatabase.FindAssets("t:BuildProfile", new[] { PROFILES_FOLDER });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(path);
                if (profile != null)
                {
                    profiles.Add(profile);
                }
            }

            profiles = profiles.OrderBy(p => p.profileName).ToList();
            EditorUtility.SetDirty(this);
        }

        public BuildProfile CreateProfile(string profileName)
        {
            if (!AssetDatabase.IsValidFolder(PROFILES_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets", "BuildProfiles");
            }

            var profile = CreateInstance<BuildProfile>();
            profile.profileName = profileName;
            profile.name = profileName;

            // Set some sensible defaults based on current settings
            profile.companyName = PlayerSettings.companyName;
            profile.productName = PlayerSettings.productName;
            profile.version = PlayerSettings.bundleVersion;
            profile.buildTarget = EditorUserBuildSettings.activeBuildTarget;
            profile.targetGroup = BuildPipeline.GetBuildTargetGroup(profile.buildTarget);

            string assetPath = $"{PROFILES_FOLDER}/{profileName}.asset";
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(profile, assetPath);
            AssetDatabase.SaveAssets();

            profiles.Add(profile);
            EditorUtility.SetDirty(this);

            Debug.Log($"[BuildProfileManager] Created new profile: {profileName}");
            return profile;
        }

        public void DeleteProfile(BuildProfile profile)
        {
            if (profile == null) return;

            if (activeProfile == profile)
            {
                activeProfile = null;
            }

            profiles.Remove(profile);
            string path = AssetDatabase.GetAssetPath(profile);
            AssetDatabase.DeleteAsset(path);
            EditorUtility.SetDirty(this);

            Debug.Log($"[BuildProfileManager] Deleted profile: {profile.profileName}");
        }

        public void ApplyProfile(BuildProfile profile)
        {
            if (profile == null)
            {
                Debug.LogError("[BuildProfileManager] Cannot apply null profile");
                return;
            }

            activeProfile = profile;
            profile.Validate();

            Debug.Log($"[BuildProfileManager] Applying profile: {profile.profileName}");

            // Apply build target
            if (EditorUserBuildSettings.activeBuildTarget != profile.buildTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(profile.targetGroup, profile.buildTarget);
            }

            // Apply player settings
            if (profile.overrideCompanyName)
                PlayerSettings.companyName = profile.companyName;
            if (profile.overrideProductName)
                PlayerSettings.productName = profile.productName;
            if (profile.overrideVersion)
                PlayerSettings.bundleVersion = profile.version;

            // Apply graphics settings
            if (profile.overrideGraphicsAPIs)
            {
                PlayerSettings.SetGraphicsAPIs(profile.buildTarget, profile.graphicsAPIs);
            }
            if (profile.overrideColorSpace)
            {
                PlayerSettings.colorSpace = profile.colorSpace;
            }

            // Apply scripting settings
            PlayerSettings.SetScriptingBackend(profile.targetGroup, profile.scriptingBackend);
            PlayerSettings.SetApiCompatibilityLevel(profile.targetGroup, profile.apiLevel);
            PlayerSettings.SetIncrementalIl2CppBuild(profile.targetGroup, profile.useIncrementalGC);

            if (profile.scriptingBackend == ScriptingImplementation.IL2CPP)
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(profile.targetGroup, profile.il2cppCompilerConfig);
            }

            // Apply scripting defines
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                profile.targetGroup,
                string.Join(";", profile.scriptingDefines)
            );

            // Apply optimization settings
            PlayerSettings.SetManagedStrippingLevel(profile.targetGroup, profile.strippingLevel);
            PlayerSettings.stripEngineCode = profile.stripEngineCode;

            // Apply scene configuration
            ApplySceneConfiguration(profile);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            if (showBuildNotifications)
            {
                EditorUtility.DisplayDialog(
                    "Build Profile Applied",
                    $"Successfully applied build profile: {profile.profileName}",
                    "OK"
                );
            }

            Debug.Log($"[BuildProfileManager] Profile applied successfully: {profile.profileName}");
        }

        private void ApplySceneConfiguration(BuildProfile profile)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

            switch (profile.sceneMode)
            {
                case BuildProfile.SceneListMode.CurrentBuildSettings:
                    return; // Don't change anything

                case BuildProfile.SceneListMode.Custom:
                    foreach (var sceneAsset in profile.customScenes)
                    {
                        if (sceneAsset != null)
                        {
                            string path = AssetDatabase.GetAssetPath(sceneAsset);
                            scenes.Add(new EditorBuildSettingsScene(path, true));
                        }
                    }
                    break;

                case BuildProfile.SceneListMode.Pattern:
                    foreach (var pattern in profile.scenePatterns)
                    {
                        var matchingScenes = AssetDatabase.FindAssets("t:Scene")
                            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                            .Where(path => System.Text.RegularExpressions.Regex.IsMatch(path, pattern))
                            .Select(path => new EditorBuildSettingsScene(path, true));
                        scenes.AddRange(matchingScenes);
                    }
                    break;

                case BuildProfile.SceneListMode.Additive:
                    scenes.AddRange(EditorBuildSettings.scenes);
                    foreach (var sceneAsset in profile.customScenes)
                    {
                        if (sceneAsset != null)
                        {
                            string path = AssetDatabase.GetAssetPath(sceneAsset);
                            if (!scenes.Any(s => s.path == path))
                                scenes.Add(new EditorBuildSettingsScene(path, true));
                        }
                    }
                    break;

                case BuildProfile.SceneListMode.Subtractive:
                    scenes.AddRange(EditorBuildSettings.scenes);
                    foreach (var sceneAsset in profile.excludedScenes)
                    {
                        if (sceneAsset != null)
                        {
                            string path = AssetDatabase.GetAssetPath(sceneAsset);
                            scenes.RemoveAll(s => s.path == path);
                        }
                    }
                    break;

                case BuildProfile.SceneListMode.All:
                    var allScenes = AssetDatabase.FindAssets("t:Scene")
                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                        .Select(path => new EditorBuildSettingsScene(path, true));
                    scenes.AddRange(allScenes);
                    break;

                case BuildProfile.SceneListMode.Generated:
                    var generatedScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Generated" })
                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                        .Select(path => new EditorBuildSettingsScene(path, true));

                    // Always include TitleScreen as first scene
                    var titleScreen = AssetDatabase.FindAssets("t:Scene TitleScreen")
                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(titleScreen))
                    {
                        scenes.Add(new EditorBuildSettingsScene(titleScreen, true));
                    }

                    scenes.AddRange(generatedScenes);
                    break;
            }

            if (scenes.Count > 0)
            {
                EditorBuildSettings.scenes = scenes.ToArray();
                Debug.Log($"[BuildProfileManager] Applied {scenes.Count} scenes to build settings");
            }
        }

        public BuildReport BuildWithProfile(BuildProfile profile, bool switchBackAfterBuild = false)
        {
            if (profile == null)
            {
                Debug.LogError("[BuildProfileManager] Cannot build with null profile");
                return null;
            }

            var previousProfile = activeProfile;
            ApplyProfile(profile);

            if (validateBeforeBuild)
            {
                if (!ValidateBuildSettings(profile))
                {
                    Debug.LogError("[BuildProfileManager] Build validation failed");
                    if (switchBackAfterBuild && previousProfile != null)
                        ApplyProfile(previousProfile);
                    return null;
                }
            }

            // Run pre-build script if specified
            if (profile.runPreBuildScript && !string.IsNullOrEmpty(profile.preBuildScript))
            {
                try
                {
                    var method = Type.GetType(profile.preBuildScript);
                    if (method != null)
                    {
                        method.GetMethod("Execute")?.Invoke(null, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BuildProfileManager] Pre-build script failed: {e.Message}");
                }
            }

            // Prepare build options
            string outputPath = profile.GetFormattedOutputPath();

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            string fileName = profile.executableName;
            if (!fileName.EndsWith(".exe") && profile.buildTarget == BuildTarget.StandaloneWindows64)
            {
                fileName += ".exe";
            }

            outputPath = Path.Combine(outputPath, fileName);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = outputPath,
                target = profile.buildTarget,
                options = profile.buildOptions
            };

            if (profile.developmentBuild)
                options.options |= BuildOptions.Development;
            if (profile.allowDebugging)
                options.options |= BuildOptions.AllowDebugging;
            if (profile.connectProfiler)
                options.options |= BuildOptions.ConnectWithProfiler;
            if (profile.buildScriptsOnly)
                options.options |= BuildOptions.BuildScriptsOnly;
            if (profile.enableHeadlessMode)
                options.options |= BuildOptions.EnableHeadlessMode;

            Debug.Log($"[BuildProfileManager] Starting build: {profile.profileName}");
            Debug.Log($"[BuildProfileManager] Output: {outputPath}");
            Debug.Log($"[BuildProfileManager] Scenes: {options.scenes.Length}");

            BuildReport report = BuildPipeline.BuildPlayer(options);

            // Update build history
            if (report.summary.result == BuildResult.Succeeded)
            {
                lastBuildNumber++;
                lastBuildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                lastBuildProfile = profile.profileName;
                EditorUtility.SetDirty(this);

                Debug.Log($"[BuildProfileManager] Build succeeded: {outputPath}");
                Debug.Log($"[BuildProfileManager] Total time: {report.summary.totalTime.TotalSeconds:F2} seconds");
                Debug.Log($"[BuildProfileManager] Total size: {report.summary.totalSize / 1024 / 1024:F2} MB");

                // Run post-build script if specified
                if (profile.runPostBuildScript && !string.IsNullOrEmpty(profile.postBuildScript))
                {
                    try
                    {
                        var method = Type.GetType(profile.postBuildScript);
                        if (method != null)
                        {
                            method.GetMethod("Execute")?.Invoke(null, new object[] { outputPath });
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[BuildProfileManager] Post-build script failed: {e.Message}");
                    }
                }

                if (profile.openBuildFolderAfterBuild)
                {
                    EditorUtility.RevealInFinder(outputPath);
                }

                if (showBuildNotifications)
                {
                    EditorUtility.DisplayDialog(
                        "Build Complete",
                        $"Build succeeded!\n\nProfile: {profile.profileName}\nOutput: {outputPath}\nTime: {report.summary.totalTime.TotalSeconds:F2}s",
                        "OK"
                    );
                }
            }
            else
            {
                Debug.LogError($"[BuildProfileManager] Build failed: {report.summary.result}");
                if (showBuildNotifications)
                {
                    EditorUtility.DisplayDialog(
                        "Build Failed",
                        $"Build failed with result: {report.summary.result}\n\nCheck console for details.",
                        "OK"
                    );
                }
            }

            if (switchBackAfterBuild && previousProfile != null)
            {
                ApplyProfile(previousProfile);
            }

            return report;
        }

        private bool ValidateBuildSettings(BuildProfile profile)
        {
            bool valid = true;

            // Check if we have scenes
            if (EditorBuildSettings.scenes.Length == 0)
            {
                Debug.LogError("[BuildProfileManager] No scenes in build settings!");
                valid = false;
            }

            // Check if first scene exists
            if (EditorBuildSettings.scenes.Length > 0)
            {
                string firstScene = EditorBuildSettings.scenes[0].path;
                if (!File.Exists(firstScene))
                {
                    Debug.LogError($"[BuildProfileManager] First scene doesn't exist: {firstScene}");
                    valid = false;
                }
            }

            // Validate Steam settings if Steam build
            if (profile.steamBuild)
            {
                // Check for steam_appid.txt file (Steam's standard location)
                string steamAppIdPath = "steam_appid.txt";
                if (!System.IO.File.Exists(steamAppIdPath))
                {
                    Debug.LogWarning("[BuildProfileManager] Steam build enabled but steam_appid.txt file not found in project root!");
                    Debug.LogWarning("[BuildProfileManager] Create steam_appid.txt with your Steam App ID for proper Steam integration");
                }
                else
                {
                    try
                    {
                        string appId = System.IO.File.ReadAllText(steamAppIdPath).Trim();
                        if (string.IsNullOrEmpty(appId))
                        {
                            Debug.LogWarning("[BuildProfileManager] steam_appid.txt exists but is empty!");
                        }
                        else
                        {
                            Debug.Log($"[BuildProfileManager] Steam App ID loaded from steam_appid.txt: {appId}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[BuildProfileManager] Error reading steam_appid.txt: {e.Message}");
                    }
                }
            }

            return valid;
        }

        [MenuItem("NeonLadder/Build/Build Active Profile %#b")]
        public static void BuildActiveProfile()
        {
            var manager = Instance;
            if (manager.activeProfile == null)
            {
                Debug.LogError("[BuildProfileManager] No active profile selected!");
                return;
            }

            manager.BuildWithProfile(manager.activeProfile);
        }

        public static void BuildFromCommandLine()
        {
            string profileName = GetCommandLineArg("-profileName");
            if (string.IsNullOrEmpty(profileName))
            {
                Debug.LogError("[BuildProfileManager] No profile name specified in command line!");
                EditorApplication.Exit(1);
                return;
            }

            var manager = Instance;
            var profile = manager.profiles.FirstOrDefault(p => p.profileName == profileName);

            if (profile == null)
            {
                Debug.LogError($"[BuildProfileManager] Profile not found: {profileName}");
                EditorApplication.Exit(1);
                return;
            }

            var report = manager.BuildWithProfile(profile);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[BuildProfileManager] Command line build succeeded");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[BuildProfileManager] Command line build failed");
                EditorApplication.Exit(1);
            }
        }

        private static string GetCommandLineArg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void CreateDefaultProfiles()
        {
            // Only create if they don't exist
            if (FindExistingProfile("Development") == null)
            {
                CreateDevelopmentProfile();
            }
            if (FindExistingProfile("Release") == null)
            {
                CreateReleaseProfile();
            }
            if (FindExistingProfile("Ultra Fast") == null)
            {
                CreateUltraFastProfile();
            }
            if (FindExistingProfile("Ultra Fast - MVP") == null)
            {
                CreateUltraFastMVPProfile();
            }
            if (FindExistingProfile("Steam Early Access") == null)
            {
                CreateSteamEarlyAccessProfile();
            }
        }

        private static BuildProfile FindExistingProfile(string name)
        {
            string[] guids = AssetDatabase.FindAssets($"t:BuildProfile {name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(path);
                if (profile != null && profile.profileName == name)
                {
                    return profile;
                }
            }
            return null;
        }

        [MenuItem("NeonLadder/Build/Create Default Configurations")]
        public static void CreateDefaultConfigurationsMenu()
        {
            CreateDefaultProfiles();
            Debug.Log("[BuildProfileManager] Default profiles created/verified");
        }

        private static void CreateDevelopmentProfile()
        {
            var manager = Instance;
            var profile = manager.CreateProfile("Development");

            profile.developmentBuild = true;
            profile.allowDebugging = true;
            profile.connectProfiler = false;
            profile.scriptingBackend = ScriptingImplementation.Mono2x;
            profile.il2cppCompilerConfig = Il2CppCompilerConfiguration.Debug;
            profile.strippingLevel = ManagedStrippingLevel.Disabled;
            profile.stripEngineCode = false;
            profile.sceneMode = BuildProfile.SceneListMode.CurrentBuildSettings;
            profile.buildOptions = BuildOptions.Development;
            profile.outputFolder = "Builds/Windows/development-{Date}-{Timestamp}/";
            profile.timestampBuilds = true;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        private static void CreateReleaseProfile()
        {
            var manager = Instance;
            var profile = manager.CreateProfile("Release");

            profile.developmentBuild = false;
            profile.allowDebugging = false;
            profile.connectProfiler = false;
            profile.scriptingBackend = ScriptingImplementation.IL2CPP;
            profile.il2cppCompilerConfig = Il2CppCompilerConfiguration.Release;
            profile.strippingLevel = ManagedStrippingLevel.High;
            profile.stripEngineCode = true;
            profile.sceneMode = BuildProfile.SceneListMode.CurrentBuildSettings;
            profile.buildOptions = BuildOptions.None;
            profile.outputFolder = "Builds/Windows/release-{Date}-{Timestamp}/";
            profile.timestampBuilds = true;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        private static void CreateUltraFastProfile()
        {
            var manager = Instance;
            var profile = manager.CreateProfile("Ultra Fast");

            // Ultra fast development settings
            profile.developmentBuild = true;
            profile.allowDebugging = false;
            profile.connectProfiler = false;
            profile.scriptingBackend = ScriptingImplementation.Mono2x;
            profile.strippingLevel = ManagedStrippingLevel.Disabled;
            profile.stripEngineCode = false;
            profile.buildOptions = BuildOptions.Development;

            // Custom scene list - just the Title scene (first scene)
            profile.sceneMode = BuildProfile.SceneListMode.Custom;

            // Find and add only the Title scene
            string titleScenePath = "Assets/Scenes/Title.unity";
            var titleSceneAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(titleScenePath);
            if (titleSceneAsset != null)
            {
                profile.customScenes = new List<UnityEditor.SceneAsset> { titleSceneAsset };
            }
            else
            {
                // Fallback: use first scene in build settings
                if (EditorBuildSettings.scenes.Length > 0)
                {
                    var firstSceneAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(EditorBuildSettings.scenes[0].path);
                    if (firstSceneAsset != null)
                    {
                        profile.customScenes = new List<UnityEditor.SceneAsset> { firstSceneAsset };
                    }
                }
            }

            // Ultra fast output location
            profile.outputFolder = "Builds/Windows/ultra-fast-{Date}-{Timestamp}/";
            profile.executableName = "NeonLadder";
            profile.timestampBuilds = true;

            // Optimization flags
            profile.openBuildFolderAfterBuild = true;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            Debug.Log("[BuildProfileManager] Created Ultra Fast profile with single scene for fastest iteration");
        }

        private static void CreateUltraFastMVPProfile()
        {
            var manager = Instance;
            var profile = manager.CreateProfile("Ultra Fast - MVP");

            // Ultra fast development settings (same as Ultra Fast)
            profile.developmentBuild = true;
            profile.allowDebugging = false;
            profile.connectProfiler = false;
            profile.scriptingBackend = ScriptingImplementation.Mono2x;
            profile.strippingLevel = ManagedStrippingLevel.Disabled;
            profile.stripEngineCode = false;
            profile.buildOptions = BuildOptions.Development;

            // MVP scene list - includes core gameplay scenes
            profile.sceneMode = BuildProfile.SceneListMode.Custom;
            profile.customScenes = new List<UnityEditor.SceneAsset>();

            // Core scenes
            string[] mvpScenePaths = {
                "Assets/Scenes/Title.unity",
                "Assets/Scenes/Staging.unity",
                "Assets/Scenes/Start.unity",
                "Assets/Scenes/Credits.unity",
                "Assets/Scenes/Cutscenes/BossDefeated.unity",
                "Assets/Scenes/Cutscenes/Death.unity",
                "Assets/Scenes/PackageScenes/URP_SiegeOfPonthus.unity"
            };

            // Add core scenes
            foreach (string scenePath in mvpScenePaths)
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(scenePath);
                if (sceneAsset != null)
                {
                    profile.customScenes.Add(sceneAsset);
                    Debug.Log($"[BuildProfileManager] Added MVP scene: {scenePath}");
                }
                else
                {
                    Debug.LogWarning($"[BuildProfileManager] MVP scene not found: {scenePath}");

                    // Special handling for URP_SiegeOfPonthus - try alternative path
                    if (scenePath.Contains("URP_SiegeOfPonthus"))
                    {
                        string altPath = "Assets/Packages/LeartesStudios/SiegeOfPonthus/Scene/URP_SiegeOfPonthus.unity";
                        var altSceneAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(altPath);
                        if (altSceneAsset != null)
                        {
                            profile.customScenes.Add(altSceneAsset);
                            Debug.Log($"[BuildProfileManager] Added MVP scene from alternative path: {altPath}");
                        }
                        else
                        {
                            Debug.LogError($"[BuildProfileManager] URP_SiegeOfPonthus not found in either location!");
                        }
                    }
                }
            }

            // Add all Generated/Connections scenes
            string[] connectionGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Generated/Connections" });
            foreach (string guid in connectionGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(path);
                if (sceneAsset != null)
                {
                    profile.customScenes.Add(sceneAsset);
                }
            }

            // Add all Generated/BossArenas scenes
            string[] bossArenaGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Generated/BossArenas" });
            foreach (string guid in bossArenaGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(path);
                if (sceneAsset != null)
                {
                    profile.customScenes.Add(sceneAsset);
                }
            }

            // MVP output location
            profile.outputFolder = "Builds/Windows/ultra-fast-mvp-{Date}-{Timestamp}/";
            profile.executableName = "NeonLadder";
            profile.timestampBuilds = true;

            // Optimization flags
            profile.openBuildFolderAfterBuild = true;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            Debug.Log($"[BuildProfileManager] Created Ultra Fast - MVP profile with {profile.customScenes.Count} scenes for complete gameplay testing");
        }

        private static void CreateSteamEarlyAccessProfile()
        {
            var manager = Instance;
            var profile = manager.CreateProfile("Steam Early Access");

            // Release-quality settings for Steam distribution with IL2CPP
            profile.developmentBuild = false;
            profile.allowDebugging = false;
            profile.connectProfiler = false;
            profile.scriptingBackend = ScriptingImplementation.IL2CPP; // Using IL2CPP for production performance
            profile.il2cppCompilerConfig = Il2CppCompilerConfiguration.Release; // Release configuration for optimized builds
            profile.strippingLevel = ManagedStrippingLevel.High; // High stripping for smaller build size
            profile.stripEngineCode = true; // Enable engine code stripping for production
            profile.buildOptions = BuildOptions.None;

            // Same MVP scene list as Ultra Fast MVP
            profile.sceneMode = BuildProfile.SceneListMode.Custom;
            profile.customScenes = new List<UnityEditor.SceneAsset>();

            // Core scenes (same as MVP)
            string[] mvpScenePaths = {
                "Assets/Scenes/Title.unity",
                "Assets/Scenes/Staging.unity",
                "Assets/Scenes/Start.unity",
                "Assets/Scenes/Credits.unity",
                "Assets/Scenes/Cutscenes/BossDefeated.unity",
                "Assets/Scenes/Cutscenes/Death.unity",
                "Assets/Scenes/PackageScenes/URP_SiegeOfPonthus.unity"
            };

            // Add core scenes
            foreach (string scenePath in mvpScenePaths)
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(scenePath);
                if (sceneAsset != null)
                {
                    profile.customScenes.Add(sceneAsset);
                    Debug.Log($"[BuildProfileManager] Added Steam EA scene: {scenePath}");
                }
                else
                {
                    Debug.LogWarning($"[BuildProfileManager] Steam EA scene not found: {scenePath}");

                    // Special handling for URP_SiegeOfPonthus - try alternative path
                    if (scenePath.Contains("URP_SiegeOfPonthus"))
                    {
                        string altPath = "Assets/Packages/LeartesStudios/SiegeOfPonthus/Scene/URP_SiegeOfPonthus.unity";
                        var altSceneAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(altPath);
                        if (altSceneAsset != null)
                        {
                            profile.customScenes.Add(altSceneAsset);
                            Debug.Log($"[BuildProfileManager] Added Steam EA scene from alternative path: {altPath}");
                        }
                        else
                        {
                            Debug.LogError($"[BuildProfileManager] URP_SiegeOfPonthus not found in either location!");
                        }
                    }
                }
            }

            // Add all Generated/Connections scenes
            string[] connectionGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Generated/Connections" });
            foreach (string guid in connectionGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(path);
                if (sceneAsset != null)
                {
                    profile.customScenes.Add(sceneAsset);
                }
            }

            // Add all Generated/BossArenas scenes
            string[] bossArenaGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Generated/BossArenas" });
            foreach (string guid in bossArenaGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(path);
                if (sceneAsset != null)
                {
                    profile.customScenes.Add(sceneAsset);
                }
            }

            // Steam Early Access output location
            profile.outputFolder = "Builds/Windows/steam-early-access-{Date}-{Timestamp}/";
            profile.executableName = "NeonLadder";
            profile.timestampBuilds = true;

            // Steam-specific settings
            profile.steamBuild = true;
            profile.steamAppId = ""; // Set your Steam App ID here

            // Release optimization settings
            profile.overrideCompression = true;
            profile.compressionMethod = BuildProfile.Compression.LZ4HC;
            profile.overrideColorSpace = true;
            profile.colorSpace = ColorSpace.Linear;

            // Quality settings - disable Fast Build Shader Stripping for release
            profile.openBuildFolderAfterBuild = true;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            Debug.Log($"[BuildProfileManager] Created Steam Early Access profile with {profile.customScenes.Count} scenes for release distribution");
            Debug.Log("[BuildProfileManager] Note: Using IL2CPP backend for optimized production performance");
            Debug.Log("[BuildProfileManager] Note: Ensure VC++ build tools are installed for IL2CPP compilation");
            Debug.Log("[BuildProfileManager] Note: Fast Build Shader Stripping should be DISABLED for this profile to include all shaders");
        }
    }
}