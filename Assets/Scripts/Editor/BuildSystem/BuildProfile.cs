using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace NeonLadder.BuildSystem
{
    [CreateAssetMenu(fileName = "BuildProfile", menuName = "NeonLadder/Build System/Build Profile", order = 1)]
    public class BuildProfile : ScriptableObject
    {
        [Header("Profile Identity")]
        public string profileName = "New Profile";
        public Color profileColor = Color.white;
        [TextArea(2, 4)]
        public string description = "";

        [Header("Build Settings")]
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
        public BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;
        public BuildOptions buildOptions = BuildOptions.None;

        [Header("Output Configuration")]
        public string outputFolder = "Builds/{ProfileName}/{Version}/";
        public string executableName = "NeonLadder";
        public bool timestampBuilds = false;

        [Header("Scene Configuration")]
        public SceneListMode sceneMode = SceneListMode.CurrentBuildSettings;
        public List<SceneAsset> customScenes = new List<SceneAsset>();
        public List<string> scenePatterns = new List<string>();
        public List<SceneAsset> excludedScenes = new List<SceneAsset>();

        [Header("Player Settings Override")]
        public bool overrideCompanyName = false;
        public string companyName = "ShorelineGames, LLC";
        public bool overrideProductName = false;
        public string productName = "NeonLadder";
        public bool overrideVersion = false;
        public string version = "1.0.0";
        public bool overrideIcon = false;
        public Texture2D icon;

        [Header("Graphics Settings")]
        public bool overrideGraphicsAPIs = false;
        public UnityEngine.Rendering.GraphicsDeviceType[] graphicsAPIs = new UnityEngine.Rendering.GraphicsDeviceType[]
        {
            UnityEngine.Rendering.GraphicsDeviceType.Direct3D11,
            UnityEngine.Rendering.GraphicsDeviceType.Direct3D12,
            UnityEngine.Rendering.GraphicsDeviceType.Vulkan
        };
        public bool overrideColorSpace = false;
        public ColorSpace colorSpace = ColorSpace.Linear;

        [Header("Scripting Configuration")]
        public string[] scriptingDefines = new string[0];
        public ScriptingImplementation scriptingBackend = ScriptingImplementation.Mono2x;
        public Il2CppCompilerConfiguration il2cppCompilerConfig = Il2CppCompilerConfiguration.Release;
        public ApiCompatibilityLevel apiLevel = ApiCompatibilityLevel.NET_Standard;
        public bool useIncrementalGC = true;

        [Header("Optimization")]
        public ManagedStrippingLevel strippingLevel = ManagedStrippingLevel.Minimal;
        public bool stripEngineCode = false;
        public StrippingLevel stripLevel = StrippingLevel.Disabled;

        [Header("Compression")]
        public bool overrideCompression = false;
        public Compression compressionMethod = Compression.LZ4;

        [Header("Development Options")]
        public bool allowDebugging = false;
        public bool enableHeadlessMode = false;
        public bool developmentBuild = false;
        public bool connectProfiler = false;
        public bool buildScriptsOnly = false;

        [Header("Steam Integration")]
        public bool steamBuild = false;
        public string steamAppId = "";

        [Header("Pre/Post Build Actions")]
        public bool runPreBuildScript = false;
        public string preBuildScript = "";
        public bool runPostBuildScript = false;
        public string postBuildScript = "";
        public bool openBuildFolderAfterBuild = true;

        public enum SceneListMode
        {
            CurrentBuildSettings,  // Use whatever's in Build Settings
            Custom,                 // Specific list from customScenes
            Pattern,               // Pattern matching from scenePatterns
            Additive,              // Current + additional scenes
            Subtractive,           // Current - excluded scenes
            All,                   // All scenes in project
            Generated              // All procedurally generated scenes
        }

        public enum Compression
        {
            None,
            LZ4,
            LZ4HC
        }

        public string GetFormattedOutputPath()
        {
            string path = outputFolder;
            path = path.Replace("{ProfileName}", profileName);
            path = path.Replace("{Version}", version);
            path = path.Replace("{Platform}", buildTarget.ToString());
            path = path.Replace("{Date}", DateTime.Now.ToString("yyyy-MM-dd"));

            if (timestampBuilds)
            {
                path = path.Replace("{Timestamp}", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            }

            return path;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(profileName))
                profileName = name;

            if (string.IsNullOrEmpty(executableName))
                executableName = "NeonLadder";

            if (steamBuild && string.IsNullOrEmpty(steamAppId))
            {
                Debug.LogWarning($"[BuildProfile] {profileName}: Steam build enabled but no App ID specified");
            }
        }

        public BuildProfile Clone()
        {
            var clone = CreateInstance<BuildProfile>();
            EditorUtility.CopySerialized(this, clone);
            clone.profileName = $"{profileName} (Copy)";
            return clone;
        }
    }
}