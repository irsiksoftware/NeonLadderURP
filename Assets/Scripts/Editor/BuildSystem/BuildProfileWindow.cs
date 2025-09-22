using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace NeonLadder.BuildSystem
{
    public class BuildProfileWindow : EditorWindow
    {
        private BuildProfileManager manager;
        private Vector2 scrollPosLeft;
        private Vector2 scrollPosRight;
        private BuildProfile selectedProfile;
        private UnityEditor.Editor profileEditor;
        private bool showSettings = false;

        private GUIStyle profileButtonStyle;
        private GUIStyle activeProfileButtonStyle;
        private GUIStyle headerStyle;

        [MenuItem("NeonLadder/Build/Open Build Profile Manager", priority = 1)]
        [MenuItem("Window/NeonLadder/Build Profile Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildProfileWindow>("Build Profiles");
            window.minSize = new Vector2(800, 400);
        }

        private void OnEnable()
        {
            manager = BuildProfileManager.Instance;
            manager.RefreshProfileList();
        }

        private void InitStyles()
        {
            if (profileButtonStyle == null)
            {
                profileButtonStyle = new GUIStyle(GUI.skin.button);
                profileButtonStyle.alignment = TextAnchor.MiddleLeft;
                profileButtonStyle.padding.left = 10;
            }

            if (activeProfileButtonStyle == null)
            {
                activeProfileButtonStyle = new GUIStyle(profileButtonStyle);
                activeProfileButtonStyle.fontStyle = FontStyle.Bold;
            }

            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.fontSize = 14;
                headerStyle.alignment = TextAnchor.MiddleCenter;
            }
        }

        private void OnGUI()
        {
            InitStyles();

            if (manager == null)
            {
                EditorGUILayout.HelpBox("Initializing Build Profile Manager...", MessageType.Info);
                manager = BuildProfileManager.Instance;
                return;
            }

            DrawToolbar();
            DrawMainContent();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("New Profile", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                CreateNewProfile();
            }

            if (GUILayout.Button("Duplicate", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                DuplicateProfile(selectedProfile);
            }

            if (GUILayout.Button("Delete", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                DeleteProfile(selectedProfile);
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                manager.RefreshProfileList();
                Repaint();
            }

            GUILayout.FlexibleSpace();

            if (manager.activeProfile != null)
            {
                EditorGUILayout.LabelField($"Active: {manager.activeProfile.profileName}", EditorStyles.toolbarButton);
            }

            if (GUILayout.Button("Settings", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                showSettings = !showSettings;
            }

            if (GUILayout.Button("Build Active", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                if (manager.activeProfile != null)
                {
                    manager.BuildWithProfile(manager.activeProfile);
                }
                else
                {
                    EditorUtility.DisplayDialog("No Active Profile", "Please select and apply a profile first.", "OK");
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawMainContent()
        {
            if (showSettings)
            {
                DrawSettingsPanel();
                return;
            }

            EditorGUILayout.BeginHorizontal();

            // Left Panel - Profile List
            DrawProfileListPanel();

            // Right Panel - Profile Details
            DrawProfileDetailsPanel();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawProfileListPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));

            EditorGUILayout.LabelField("Build Profiles", headerStyle, GUILayout.Height(25));
            EditorGUILayout.Separator();

            scrollPosLeft = EditorGUILayout.BeginScrollView(scrollPosLeft);

            if (manager.profiles.Count == 0)
            {
                EditorGUILayout.HelpBox("No profiles found.\nClick 'New Profile' to create one.", MessageType.Info);
            }
            else
            {
                foreach (var profile in manager.profiles)
                {
                    if (profile == null) continue;

                    DrawProfileListItem(profile);
                }
            }

            EditorGUILayout.EndScrollView();

            DrawQuickCreateButtons();

            EditorGUILayout.EndVertical();
        }

        private void DrawProfileListItem(BuildProfile profile)
        {
            EditorGUILayout.BeginHorizontal();

            // Active indicator
            bool isActive = manager.activeProfile == profile;
            if (isActive)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("►", GUILayout.Width(20));
                GUI.color = Color.white;
            }
            else
            {
                EditorGUILayout.LabelField("", GUILayout.Width(20));
            }

            // Profile button
            GUI.backgroundColor = profile.profileColor;
            bool isSelected = selectedProfile == profile;

            GUIStyle buttonStyle = isActive ? activeProfileButtonStyle : profileButtonStyle;
            if (isSelected)
            {
                GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f, 1f);
            }

            if (GUILayout.Button(profile.profileName, buttonStyle, GUILayout.Height(25)))
            {
                SelectProfile(profile);
            }

            GUI.backgroundColor = Color.white;

            // Quick actions
            if (GUILayout.Button("▶", GUILayout.Width(25), GUILayout.Height(25)))
            {
                manager.ApplyProfile(profile);
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            if (isActive || isSelected)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(20));

                string info = $"{profile.buildTarget} | {profile.scriptingBackend}";
                if (profile.developmentBuild) info += " | Dev";

                EditorGUILayout.LabelField(info, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawQuickCreateButtons()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Quick Create", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Dev"))
            {
                CreateDevelopmentProfile();
            }
            if (GUILayout.Button("Release"))
            {
                CreateReleaseProfile();
            }
            if (GUILayout.Button("Demo"))
            {
                CreateDemoProfile();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawProfileDetailsPanel()
        {
            EditorGUILayout.BeginVertical();

            if (selectedProfile == null)
            {
                EditorGUILayout.HelpBox("Select a profile to view and edit its settings.", MessageType.Info);
            }
            else
            {
                DrawProfileHeader();
                DrawProfileActions();

                EditorGUILayout.Separator();

                scrollPosRight = EditorGUILayout.BeginScrollView(scrollPosRight);

                // Create or update profile editor
                if (profileEditor == null || profileEditor.target != selectedProfile)
                {
                    if (profileEditor != null)
                        DestroyImmediate(profileEditor);

                    profileEditor = UnityEditor.Editor.CreateEditor(selectedProfile);
                }

                if (profileEditor != null)
                {
                    profileEditor.OnInspectorGUI();
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawProfileHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Profile: {selectedProfile.profileName}", headerStyle, GUILayout.Height(25));

            if (manager.activeProfile == selectedProfile)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("[ACTIVE]", GUILayout.Width(60));
                GUI.color = Color.white;
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(selectedProfile.description))
            {
                EditorGUILayout.LabelField(selectedProfile.description, EditorStyles.wordWrappedMiniLabel);
            }
        }

        private void DrawProfileActions()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
            if (GUILayout.Button("Apply Profile", GUILayout.Height(30)))
            {
                manager.ApplyProfile(selectedProfile);
                Repaint();
            }

            GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f);
            if (GUILayout.Button("Build This Profile", GUILayout.Height(30)))
            {
                manager.BuildWithProfile(selectedProfile);
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save Current Settings to Profile"))
            {
                SaveCurrentSettingsToProfile(selectedProfile);
            }

            if (GUILayout.Button("Preview Scenes"))
            {
                PreviewScenes(selectedProfile);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSettingsPanel()
        {
            EditorGUILayout.LabelField("Build Manager Settings", headerStyle, GUILayout.Height(25));
            EditorGUILayout.Separator();

            manager.autoIncrementBuildNumber = EditorGUILayout.Toggle("Auto Increment Build Number", manager.autoIncrementBuildNumber);
            manager.showBuildNotifications = EditorGUILayout.Toggle("Show Build Notifications", manager.showBuildNotifications);
            manager.validateBeforeBuild = EditorGUILayout.Toggle("Validate Before Build", manager.validateBeforeBuild);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Build History", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Last Build Number: {manager.lastBuildNumber}");
            EditorGUILayout.LabelField($"Last Build Date: {manager.lastBuildDate}");
            EditorGUILayout.LabelField($"Last Build Profile: {manager.lastBuildProfile}");

            EditorGUILayout.Separator();
            if (GUILayout.Button("Reset Build Number"))
            {
                manager.lastBuildNumber = 0;
                EditorUtility.SetDirty(manager);
            }
        }

        private void SelectProfile(BuildProfile profile)
        {
            selectedProfile = profile;

            if (profileEditor != null && profileEditor.target != profile)
            {
                DestroyImmediate(profileEditor);
                profileEditor = null;
            }

            Repaint();
        }

        private void CreateNewProfile()
        {
            string profileName = EditorInputDialog.Show("New Profile", "Enter profile name:", "New Profile");

            if (!string.IsNullOrEmpty(profileName))
            {
                var newProfile = manager.CreateProfile(profileName);
                SelectProfile(newProfile);
                manager.RefreshProfileList();
                Repaint();
            }
        }

        private void DuplicateProfile(BuildProfile profile)
        {
            if (profile == null)
            {
                EditorUtility.DisplayDialog("No Profile Selected", "Please select a profile to duplicate.", "OK");
                return;
            }

            string newName = EditorInputDialog.Show("Duplicate Profile", "Enter new profile name:", $"{profile.profileName} (Copy)");

            if (!string.IsNullOrEmpty(newName))
            {
                var clone = profile.Clone();
                clone.profileName = newName;
                clone.name = newName;

                string path = $"Assets/BuildProfiles/{newName}.asset";
                path = AssetDatabase.GenerateUniqueAssetPath(path);

                AssetDatabase.CreateAsset(clone, path);
                AssetDatabase.SaveAssets();

                manager.RefreshProfileList();
                SelectProfile(clone);
                Repaint();
            }
        }

        private void DeleteProfile(BuildProfile profile)
        {
            if (profile == null)
            {
                EditorUtility.DisplayDialog("No Profile Selected", "Please select a profile to delete.", "OK");
                return;
            }

            if (EditorUtility.DisplayDialog(
                "Delete Profile",
                $"Are you sure you want to delete the profile '{profile.profileName}'?",
                "Delete",
                "Cancel"))
            {
                manager.DeleteProfile(profile);
                selectedProfile = null;

                if (profileEditor != null)
                {
                    DestroyImmediate(profileEditor);
                    profileEditor = null;
                }

                Repaint();
            }
        }

        private void SaveCurrentSettingsToProfile(BuildProfile profile)
        {
            if (profile == null) return;

            profile.companyName = PlayerSettings.companyName;
            profile.productName = PlayerSettings.productName;
            profile.version = PlayerSettings.bundleVersion;
            profile.buildTarget = EditorUserBuildSettings.activeBuildTarget;
            profile.targetGroup = BuildPipeline.GetBuildTargetGroup(profile.buildTarget);

            // Save current scene list
            profile.customScenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path))
                .Where(s => s != null)
                .ToList();

            profile.sceneMode = BuildProfile.SceneListMode.Custom;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Settings Saved", $"Current settings saved to profile: {profile.profileName}", "OK");
        }

        private void PreviewScenes(BuildProfile profile)
        {
            if (profile == null) return;

            var tempActive = manager.activeProfile;
            manager.ApplyProfile(profile);

            var scenes = EditorBuildSettings.scenes;

            string sceneList = $"Scenes for profile '{profile.profileName}':\n\n";
            sceneList += $"Total: {scenes.Count(s => s.enabled)} enabled scenes\n\n";

            int index = 0;
            foreach (var scene in scenes)
            {
                if (scene.enabled)
                {
                    sceneList += $"{index}: {System.IO.Path.GetFileNameWithoutExtension(scene.path)}\n";
                    index++;
                }
            }

            EditorUtility.DisplayDialog("Scene Preview", sceneList, "OK");

            if (tempActive != null && tempActive != profile)
            {
                manager.ApplyProfile(tempActive);
            }
        }

        private void CreateDevelopmentProfile()
        {
            var profile = manager.CreateProfile("Development");
            profile.profileColor = Color.cyan;
            profile.description = "Development build with debug features enabled";
            profile.developmentBuild = true;
            profile.allowDebugging = true;
            profile.connectProfiler = false;
            profile.buildOptions = BuildOptions.Development | BuildOptions.AllowDebugging;
            profile.scriptingDefines = new[] { "DEVELOPMENT_BUILD", "DEBUG", "ENABLE_LOGS" };
            profile.strippingLevel = ManagedStrippingLevel.Disabled;
            profile.sceneMode = BuildProfile.SceneListMode.CurrentBuildSettings;
            profile.outputFolder = "Builds/Development/{Date}/";

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            SelectProfile(profile);
            manager.RefreshProfileList();
            Repaint();
        }

        private void CreateReleaseProfile()
        {
            var profile = manager.CreateProfile("Release");
            profile.profileColor = Color.green;
            profile.description = "Optimized release build for distribution";
            profile.developmentBuild = false;
            profile.buildOptions = BuildOptions.None;
            profile.scriptingDefines = new[] { "RELEASE", "DISABLE_LOGS" };
            profile.scriptingBackend = ScriptingImplementation.IL2CPP;
            profile.strippingLevel = ManagedStrippingLevel.High;
            profile.stripEngineCode = true;
            profile.sceneMode = BuildProfile.SceneListMode.CurrentBuildSettings;
            profile.outputFolder = "Builds/Release/{Version}/";
            profile.openBuildFolderAfterBuild = true;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            SelectProfile(profile);
            manager.RefreshProfileList();
            Repaint();
        }

        private void CreateDemoProfile()
        {
            var profile = manager.CreateProfile("Demo");
            profile.profileColor = Color.yellow;
            profile.description = "Limited demo build with restricted content";
            profile.developmentBuild = false;
            profile.buildOptions = BuildOptions.None;
            profile.scriptingDefines = new[] { "DEMO_BUILD", "LIMITED_CONTENT", "WATERMARK" };
            profile.sceneMode = BuildProfile.SceneListMode.Custom;
            profile.outputFolder = "Builds/Demo/{Version}/";

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            SelectProfile(profile);
            manager.RefreshProfileList();
            Repaint();
        }

        private void OnDestroy()
        {
            if (profileEditor != null)
            {
                DestroyImmediate(profileEditor);
            }
        }
    }

    // Simple input dialog helper
    public class EditorInputDialog
    {
        public static string Show(string title, string message, string defaultValue = "")
        {
            string result = defaultValue;

            var window = EditorWindow.CreateInstance<InputDialogWindow>();
            window.titleContent = new GUIContent(title);
            window.message = message;
            window.inputValue = defaultValue;
            window.ShowModalUtility();

            if (window.confirmed)
            {
                result = window.inputValue;
            }

            return result;
        }

        private class InputDialogWindow : EditorWindow
        {
            public string message;
            public string inputValue;
            public bool confirmed = false;

            void OnGUI()
            {
                EditorGUILayout.LabelField(message);
                inputValue = EditorGUILayout.TextField(inputValue);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("OK"))
                {
                    confirmed = true;
                    Close();
                }
                if (GUILayout.Button("Cancel"))
                {
                    confirmed = false;
                    Close();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}