using NeonLadder.Debugging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace NeonLadder.Editor
{
    [InitializeOnLoad]
    public static class PackageManagementDynamic
    {
        private const string PACKAGES_PATH = "Assets/Packages";
        private const int BASE_PRIORITY = 60;
        private static List<string> discoveredPackages = new List<string>();
        private static LogCategory logCategory = LogCategory.Packages;

        static PackageManagementDynamic()
        {
            // Discover packages on load
            DiscoverPackages();
            
            // Unity's MenuItem attributes must be compile-time constants,
            // so we'll use a different approach with reflection and method generation
            GenerateMenuItems();
        }
        
        private static void DiscoverPackages()
        {
            discoveredPackages.Clear();
            
            if (!Directory.Exists(PACKAGES_PATH))
            {
                throw new DirectoryNotFoundException($"Packages directory not found at path: {PACKAGES_PATH}");
            }
            
            var directories = Directory.GetDirectories(PACKAGES_PATH);
            foreach (var dir in directories)
            {
                var packageName = Path.GetFileName(dir);
                discoveredPackages.Add(packageName);
            }

        }
        
        private static void GenerateMenuItems()
        {
            // Unfortunately, Unity's MenuItem attribute requires compile-time constants
            // We'll need to use a different approach - create a context menu or custom window
            // For now, we'll create a single menu item that opens a package manager window
        }
        
        
        public static List<string> GetDiscoveredPackages()
        {
            if (discoveredPackages.Count == 0)
            {
                DiscoverPackages();
            }
            return new List<string>(discoveredPackages);
        }
        
        public static void ExportPackage(string packageName)
        {
            string packagePath = Path.Combine(PACKAGES_PATH, packageName);
            
            // Get all assets in the package folder
            string[] assetPaths = GetAllAssetsAtPath(packagePath);
            
            if (assetPaths.Length > 0)
            {
                // Ensure the export directory exists
                string exportDir = Path.GetFullPath("PackageExports");
                if (!Directory.Exists(exportDir))
                {
                    Directory.CreateDirectory(exportDir);
                }
                
                // Open save file dialog for the user to choose export location
                string defaultFileName = $"{packageName}.unitypackage";
                string savePath = EditorUtility.SaveFilePanel(
                    $"Export {packageName} Package",
                    exportDir,
                    defaultFileName,
                    "unitypackage"
                );
                
                // Check if user cancelled
                if (string.IsNullOrEmpty(savePath))
                {
                    Debug.Log($"Export cancelled for {packageName}");
                    return;
                }
                
                Debug.Log($"Exporting {packageName}: {assetPaths.Length} assets to {savePath}");
                
                // Show progress bar
                EditorUtility.DisplayProgressBar(
                    $"Exporting {packageName}",
                    "Preparing package for export...",
                    0.5f
                );
                
                try
                {
                    // Export with dependencies and recursive
                    AssetDatabase.ExportPackage(
                        assetPaths,
                        savePath,
                        ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
                    );
                    
                    Debug.Log($"Successfully exported {packageName} to: {savePath}");
                    
                    // Show success dialog
                    if (EditorUtility.DisplayDialog(
                        "Export Successful",
                        $"{packageName} has been exported successfully!\n\nLocation: {savePath}\n\nWould you like to show the file in Explorer?",
                        "Show in Explorer",
                        "Close"))
                    {
                        // Open folder and select the exported file
                        EditorUtility.RevealInFinder(savePath);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to export {packageName}: {e.Message}");
                    EditorUtility.DisplayDialog(
                        "Export Failed",
                        $"Failed to export {packageName}.\n\nError: {e.Message}",
                        "OK"
                    );
                }
                finally
                {
                    // Clear progress bar
                    EditorUtility.ClearProgressBar();
                }
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "No Assets Found",
                    $"No assets found in package: {packageName}\nPath: {packagePath}",
                    "OK"
                );
            }
        }
        
        private static string[] GetAllAssetsAtPath(string path)
        {
            var assets = new List<string>();
            
            if (Directory.Exists(path))
            {
                // Get all files recursively
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    // Convert to Unity asset path format (forward slashes)
                    var assetPath = file.Replace('\\', '/');
                    
                    // Skip meta files - Unity will handle them automatically
                    if (!assetPath.EndsWith(".meta"))
                    {
                        assets.Add(assetPath);
                    }
                }
                
                // Also include the directories themselves for proper structure
                var directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                foreach (var dir in directories)
                {
                    var dirPath = dir.Replace('\\', '/');
                    // Unity needs directory paths for proper export structure
                    assets.Add(dirPath);
                }
            }
            else
            {
                Debug.LogWarning($"Package directory not found: {path}");
            }
            
            return assets.ToArray();
        }
        
        public static void UploadPackageToGoogleDrive(string packageName)
        {
            // First, check if the package has been exported
            string exportPath = Path.Combine("PackageExports", $"{packageName}.unitypackage");
            
            // If not in default location, ask user to locate it
            if (!File.Exists(exportPath))
            {
                exportPath = EditorUtility.OpenFilePanel(
                    $"Locate {packageName} Package",
                    "PackageExports",
                    "unitypackage"
                );
                
                if (string.IsNullOrEmpty(exportPath) || !File.Exists(exportPath))
                {
                    Debug.LogError($"Package file not found. Please export {packageName} first.");
                    return;
                }
            }
            
            // Get file size for progress estimation
            FileInfo fileInfo = new FileInfo(exportPath);
            long fileSize = fileInfo.Length;
            string fileSizeFormatted = FormatFileSize(fileSize);
            
            // Confirm upload for large files
            if (fileSize > 1024 * 1024 * 100) // > 100MB
            {
                if (!EditorUtility.DisplayDialog(
                    "Large File Upload",
                    $"The package {packageName} is {fileSizeFormatted}.\n\n" +
                    "Large uploads may take several minutes.\n" +
                    "Unity will be unresponsive during upload.\n\n" +
                    "Continue with upload?",
                    "Upload",
                    "Cancel"))
                {
                    return;
                }
            }
            
            Debug.Log($"Starting upload of {packageName} ({fileSizeFormatted}) to Google Drive...");
            
            // Show initial progress
            EditorUtility.DisplayProgressBar(
                $"Uploading {packageName}",
                $"Starting upload ({fileSizeFormatted})...",
                0.1f
            );
            
            try
            {
                // Use gdrive CLI to upload
                // Note: gdrive doesn't provide real-time progress via stdout, so we'll use a background process
                var startInfo = new ProcessStartInfo
                {
                    FileName = @"C:\tools\gdrive",
                    Arguments = $"files upload \"{exportPath}\" --parent 1GfaSFj_VU4jTXigOtQX9Ws8dknXGBISY", // Upload to specific folder
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(startInfo))
                {
                    // Update progress bar periodically
                    float progress = 0.1f;
                    System.DateTime startTime = System.DateTime.Now;
                    
                    while (!process.HasExited)
                    {
                        // Estimate progress based on time (for large files)
                        var elapsed = (System.DateTime.Now - startTime).TotalSeconds;
                        progress = Mathf.Min(0.9f, 0.1f + (float)(elapsed / 60.0) * 0.8f); // Assume ~1 min per GB
                        
                        EditorUtility.DisplayProgressBar(
                            $"Uploading {packageName}",
                            $"Uploading {fileSizeFormatted} to Google Drive... ({elapsed:F0}s)",
                            progress
                        );
                        
                        // Check every 100ms
                        System.Threading.Thread.Sleep(100);
                        
                        // Allow cancellation
                        if (EditorUtility.DisplayCancelableProgressBar(
                            $"Uploading {packageName}",
                            $"Uploading {fileSizeFormatted} to Google Drive... ({elapsed:F0}s)",
                            progress))
                        {
                            process.Kill();
                            Debug.Log("Upload cancelled by user");
                            EditorUtility.ClearProgressBar();
                            return;
                        }
                    }
                    
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    
                    EditorUtility.ClearProgressBar();
                    
                    if (process.ExitCode == 0)
                    {
                        // Parse the file ID from output
                        // gdrive returns: "Uploaded FILE_ID at XX MB/s, total YY"
                        string fileId = ExtractFileIdFromUploadOutput(output);
                        
                        if (!string.IsNullOrEmpty(fileId))
                        {
                            string driveLink = $"https://drive.google.com/file/d/{fileId}/view?usp=sharing";
                            
                            Debug.Log($"Successfully uploaded {packageName} to Google Drive: {driveLink}");
                            
                            // Update or create DownloadInstructions.txt
                            UpdateDownloadInstructions(packageName, driveLink);
                            
                            // Show success with link
                            if (EditorUtility.DisplayDialog(
                                "Upload Successful",
                                $"{packageName} uploaded successfully!\n\n" +
                                $"File ID: {fileId}\n\n" +
                                "The Google Drive link has been saved to DownloadInstructions.txt\n\n" +
                                "Copy link to clipboard?",
                                "Copy Link",
                                "Close"))
                            {
                                EditorGUIUtility.systemCopyBuffer = driveLink;
                                Debug.Log($"Google Drive link copied to clipboard: {driveLink}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Upload completed but couldn't extract file ID from output: {output}");
                            EditorUtility.DisplayDialog(
                                "Upload Complete",
                                $"{packageName} was uploaded but the file ID couldn't be extracted.\n\n" +
                                "Check Google Drive manually for the uploaded file.",
                                "OK"
                            );
                        }
                    }
                    else
                    {
                        Debug.LogError($"Upload failed: {error}");
                        EditorUtility.DisplayDialog(
                            "Upload Failed",
                            $"Failed to upload {packageName}.\n\nError: {error}",
                            "OK"
                        );
                    }
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"Upload error: {e.Message}");
                EditorUtility.DisplayDialog(
                    "Upload Error",
                    $"Error uploading {packageName}:\n{e.Message}",
                    "OK"
                );
            }
        }
        
        private static string ExtractFileIdFromUploadOutput(string output)
        {
            // gdrive output format: "Uploaded FILE_ID at XX MB/s, total YY"
            // or "Id: FILE_ID"
            var patterns = new[]
            {
                @"Uploaded\s+([a-zA-Z0-9_-]+)",
                @"Id:\s*([a-zA-Z0-9_-]+)",
                @"^([a-zA-Z0-9_-]{20,})"
            };
            
            foreach (var pattern in patterns)
            {
                var match = Regex.Match(output, pattern, RegexOptions.Multiline);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            
            return null;
        }
        
        public static string FormatFileSize(long bytes)
        {
            if (bytes <= 0) return "Unknown";
            
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }
        
        public static void UpdateDownloadInstructions(string packageName, string googleDriveLink = null)
        {
            string instructionsPath = Path.Combine(PACKAGES_PATH, packageName, "DownloadInstructions.txt");
            
            if (!File.Exists(instructionsPath))
            {
                // Create new instructions file
                string defaultContent = $@"Package: {packageName}
Export Date: {DateTime.Now:yyyy-MM-dd}
Google Drive Link: {googleDriveLink ?? "[Pending upload]"}

Instructions:
1. Download the package from the Google Drive link above
2. Import into Unity via Assets > Import Package > Custom Package
3. Select the downloaded .unitypackage file
";
                File.WriteAllText(instructionsPath, defaultContent);
                Debug.Log($"Created DownloadInstructions.txt for {packageName}");
            }
            else if (!string.IsNullOrEmpty(googleDriveLink))
            {
                // Update existing file with new link
                var lines = File.ReadAllLines(instructionsPath).ToList();
                bool updated = false;
                
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("Google Drive Link:"))
                    {
                        lines[i] = $"Google Drive Link: {googleDriveLink}";
                        updated = true;
                        break;
                    }
                }
                
                if (!updated)
                {
                    lines.Insert(2, $"Google Drive Link: {googleDriveLink}");
                }
                
                File.WriteAllLines(instructionsPath, lines);
                Debug.Log($"Updated download instructions for {packageName}");
            }
        }
    }
    
    public class PackageManagerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private Dictionary<string, bool> packageFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, PackageInfo> packageInfoCache = new Dictionary<string, PackageInfo>();
        private float lastRefreshTime = 0;
        private const float CACHE_DURATION = 60f; // Refresh cache every 60 seconds
        
        private class PackageInfo
        {
            public string GoogleDriveId;
            public string GoogleDriveLink;
            public long FileSize;
            public string FileSizeFormatted;
            public bool HasDownloadInstructions;
            public DateTime LastChecked;
            public bool HasBackupIssues;
            public string BackupIssueReason;
        }
        
        public static void ShowWindow()
        {
            var window = GetWindow<PackageManagerWindow>("Package Manager");
            window.minSize = new Vector2(500, 400);
        }
        
        private void OnGUI()
        {
            // Store original color at the top so it's available throughout the method
            Color originalColor = GUI.contentColor;
            
            EditorGUILayout.LabelField("Third-Party Package Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Refresh cache if needed
            if (Time.realtimeSinceStartup - lastRefreshTime > CACHE_DURATION)
            {
                RefreshPackageInfoCache();
                lastRefreshTime = Time.realtimeSinceStartup;
            }
            
            var packages = PackageManagementDynamic.GetDiscoveredPackages();
            
            if (packages.Count == 0)
            {
                EditorGUILayout.HelpBox("No packages found in Assets/Packages", MessageType.Warning);
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var package in packages.OrderBy(p => p))
            {
                if (!packageFoldouts.ContainsKey(package))
                {
                    packageFoldouts[package] = false;
                }
                
                // Get cached package info
                PackageInfo info = GetPackageInfo(package);
                
                // Build foldout label with file size if available
                string foldoutLabel = package;
                bool hasIssues = false;
                
                if (info != null)
                {
                    hasIssues = info.HasBackupIssues;
                    
                    if (!hasIssues && info.FileSize > 0)
                    {
                        foldoutLabel += $" ({info.FileSizeFormatted})";
                    }
                    else if (hasIssues)
                    {
                        foldoutLabel += " ⚠️";
                    }
                }
                else
                {
                    // If info is null, that's also a backup issue
                    hasIssues = true;
                    foldoutLabel += " ⚠️";
                }
                
                // Set text color based on backup status
                if (hasIssues)
                {
                    GUI.contentColor = Color.red;
                }
                
                packageFoldouts[package] = EditorGUILayout.Foldout(packageFoldouts[package], foldoutLabel, true);
                
                // Restore original color
                GUI.contentColor = originalColor;
                
                if (packageFoldouts[package])
                {
                    EditorGUI.indentLevel++;
                    
                    // First row - Export/Upload
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button("Export", GUILayout.Width(100)))
                    {
                        PackageManagementDynamic.ExportPackage(package);
                    }
                    
                    if (GUILayout.Button("Upload to Drive", GUILayout.Width(120)))
                    {
                        PackageManagementDynamic.UploadPackageToGoogleDrive(package);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // Second row - Download button if Google Drive link exists
                    if (info != null && !string.IsNullOrEmpty(info.GoogleDriveId))
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        if (GUILayout.Button($"Download Package ({info.FileSizeFormatted})", GUILayout.Width(200)))
                        {
                            DownloadPackageFromGoogleDrive(package, info.GoogleDriveId);
                        }
                        
                        if (GUILayout.Button("Copy Link", GUILayout.Width(80)))
                        {
                            EditorGUIUtility.systemCopyBuffer = info.GoogleDriveLink;
                            Debug.Log($"Copied Google Drive link for {package}");
                        }
                        
                        // Add button to create/update DownloadInstructions.txt
                        if (!info.HasDownloadInstructions)
                        {
                            if (GUILayout.Button("Create Instructions", GUILayout.Width(120)))
                            {
                                CreateDownloadInstructions(package, info.GoogleDriveLink, info.FileSizeFormatted);
                                RefreshPackageInfoCache(); // Refresh to update UI
                            }
                        }
                        else if (info.HasBackupIssues)
                        {
                            // Debug: Show what conditions we're checking
                            Debug.Log($"EasyRoads3D debug - HasBackupIssues: {info.HasBackupIssues}, Reason: '{info.BackupIssueReason}'");
                            
                            if (GUILayout.Button("Update Instructions", GUILayout.Width(120)))
                            {
                                PackageManagementDynamic.UpdateDownloadInstructions(package, info.GoogleDriveLink);
                                RefreshPackageInfoCache(); // Refresh to update UI
                            }
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    // Status info with color coding
                    if (info != null && info.HasBackupIssues)
                    {
                        // Show backup issue in red
                        GUI.contentColor = Color.red;
                        EditorGUILayout.LabelField($"⚠️ BACKUP ISSUE: {info.BackupIssueReason}", EditorStyles.miniLabel);
                        GUI.contentColor = originalColor;
                        
                        // Show help text
                        EditorGUILayout.HelpBox("This package has backup issues. Use the buttons above to export, upload, or update download instructions.", MessageType.Warning);
                    }
                    else
                    {
                        // Show normal status
                        string instructionsPath = Path.Combine("Assets/Packages", package, "DownloadInstructions.txt");
                        if (File.Exists(instructionsPath))
                        {
                            if (info != null && !string.IsNullOrEmpty(info.GoogleDriveId))
                            {
                                GUI.contentColor = Color.green;
                                EditorGUILayout.LabelField($"✓ Backup OK - Has DownloadInstructions.txt with Google Drive link", EditorStyles.miniLabel);
                                GUI.contentColor = originalColor;
                            }
                            else
                            {
                                GUI.contentColor = Color.yellow;
                                EditorGUILayout.LabelField("⚠️ Has DownloadInstructions.txt but no Drive link found", EditorStyles.miniLabel);
                                GUI.contentColor = originalColor;
                            }
                        }
                        else
                        {
                            GUI.contentColor = Color.red;
                            EditorGUILayout.LabelField("✗ No DownloadInstructions.txt", EditorStyles.miniLabel);
                            GUI.contentColor = originalColor;
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            // Package summary with backup status
            int totalPackages = packages.Count;
            int packagesWithIssues = 0;
            int packagesWithBackups = 0;
            int packagesWithInstructions = 0;
            
            foreach (var package in packages)
            {
                var info = GetPackageInfo(package);
                if (info != null)
                {
                    if (info.HasBackupIssues)
                        packagesWithIssues++;
                    else if (!string.IsNullOrEmpty(info.GoogleDriveId))
                        packagesWithBackups++;
                    
                    if (info.HasDownloadInstructions)
                        packagesWithInstructions++;
                }
            }
            
            EditorGUILayout.LabelField($"Total Packages: {totalPackages}", EditorStyles.boldLabel);
            
            // Color-coded status summary
            EditorGUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            EditorGUILayout.LabelField($"✓ With Backups: {packagesWithBackups}", EditorStyles.miniLabel);
            GUI.contentColor = Color.red;
            EditorGUILayout.LabelField($"⚠️ With Issues: {packagesWithIssues}", EditorStyles.miniLabel);
            GUI.contentColor = Color.gray;
            EditorGUILayout.LabelField($"📄 With Instructions: {packagesWithInstructions}", EditorStyles.miniLabel);
            GUI.contentColor = originalColor;
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Refresh Package Info"))
            {
                RefreshPackageInfoCache();
            }
        }
        
        private PackageInfo GetPackageInfo(string packageName)
        {
            if (!packageInfoCache.ContainsKey(packageName))
            {
                UpdatePackageInfo(packageName);
            }
            
            return packageInfoCache.ContainsKey(packageName) ? packageInfoCache[packageName] : null;
        }
        
        private void UpdatePackageInfo(string packageName)
        {
            var info = new PackageInfo();
            info.LastChecked = DateTime.Now;
            info.HasBackupIssues = false;
            info.BackupIssueReason = "";
            
            string instructionsPath = Path.Combine("Assets/Packages", packageName, "DownloadInstructions.txt");
            if (File.Exists(instructionsPath))
            {
                info.HasDownloadInstructions = true;
                
                // Parse Google Drive link from instructions
                string content = File.ReadAllText(instructionsPath);
                var driveInfo = ParseGoogleDriveLink(content);
                
                if (driveInfo != null)
                {
                    info.GoogleDriveId = driveInfo.Item1;
                    info.GoogleDriveLink = driveInfo.Item2;
                    
                    // Get file size from Google Drive
                    long fileSize = GetGoogleDriveFileSize(info.GoogleDriveId);
                    if (fileSize > 0)
                    {
                        info.FileSize = fileSize;
                        info.FileSizeFormatted = PackageManagementDynamic.FormatFileSize(fileSize);
                    }
                    else
                    {
                        // Could not get file size - backup issue
                        info.HasBackupIssues = true;
                        info.BackupIssueReason = "Cannot retrieve file size from Google Drive";
                        info.FileSizeFormatted = "Unknown";
                    }
                }
                else
                {
                    // DownloadInstructions.txt exists but no Google Drive link found
                    // Check if package exists on Google Drive anyway
                    var drivePackageInfo = FindPackageOnGoogleDrive(packageName);
                    if (drivePackageInfo != null)
                    {
                        info.GoogleDriveId = drivePackageInfo.Item1;
                        info.GoogleDriveLink = drivePackageInfo.Item2;
                        info.FileSize = drivePackageInfo.Item3;
                        info.FileSizeFormatted = PackageManagementDynamic.FormatFileSize(info.FileSize);
                        
                        // Has instructions but link is outdated/missing
                        info.HasBackupIssues = true;
                        info.BackupIssueReason = "DownloadInstructions.txt exists but Google Drive link is missing/invalid";
                    }
                    else
                    {
                        // No Google Drive link in file and no package found on Drive
                        info.HasBackupIssues = true;
                        info.BackupIssueReason = "No Google Drive link found in DownloadInstructions.txt";
                    }
                }
            }
            else
            {
                // No DownloadInstructions.txt file - check if package exists on Google Drive
                info.HasDownloadInstructions = false;
                
                // Check for existing package on Google Drive
                var drivePackageInfo = FindPackageOnGoogleDrive(packageName);
                if (drivePackageInfo != null)
                {
                    info.GoogleDriveId = drivePackageInfo.Item1;
                    info.GoogleDriveLink = drivePackageInfo.Item2;
                    info.FileSize = drivePackageInfo.Item3;
                    info.FileSizeFormatted = PackageManagementDynamic.FormatFileSize(info.FileSize);
                    
                    // Package exists on Drive but no local instructions - minor issue
                    info.HasBackupIssues = true;
                    info.BackupIssueReason = "Package exists on Google Drive but missing local DownloadInstructions.txt";
                }
                else
                {
                    // No DownloadInstructions.txt and no package on Drive
                    info.HasBackupIssues = true;
                    info.BackupIssueReason = "Missing DownloadInstructions.txt file and no backup found on Google Drive";
                }
            }
            
            packageInfoCache[packageName] = info;
        }
        
        private void RefreshPackageInfoCache()
        {
            packageInfoCache.Clear();
            var packages = PackageManagementDynamic.GetDiscoveredPackages();
            foreach (var package in packages)
            {
                UpdatePackageInfo(package);
            }
        }
        
        private Tuple<string, string> ParseGoogleDriveLink(string content)
        {
            // Handle null or empty content
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }
            
            // Try to find Google Drive link patterns
            // Pattern 1: https://drive.google.com/file/d/FILE_ID/view
            // Pattern 2: https://drive.google.com/open?id=FILE_ID
            // Pattern 3: Just the file ID
            
            var patterns = new[]
            {
                @"https://drive\.google\.com/file/d/([a-zA-Z0-9_-]+)",
                @"https://drive\.google\.com/open\?id=([a-zA-Z0-9_-]+)",
                @"Google Drive ID:\s*([a-zA-Z0-9_-]+)",
                @"File ID:\s*([a-zA-Z0-9_-]+)",
                @"^([a-zA-Z0-9_-]{20,})$" // Just an ID on its own line
            };
            
            foreach (var pattern in patterns)
            {
                var match = Regex.Match(content, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string fileId = match.Groups[1].Value;
                    string link = $"https://drive.google.com/file/d/{fileId}/view";
                    return Tuple.Create(fileId, link);
                }
            }
            
            return null;
        }
        
        private long GetGoogleDriveFileSize(string fileId)
        {
            try
            {
                // Use gdrive CLI to get file info
                var startInfo = new ProcessStartInfo
                {
                    FileName = @"C:\tools\gdrive",
                    Arguments = $"files info {fileId}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    
                    // Parse human-readable size format: "Size: 33.1 GB"
                    var sizePattern = @"Size:\s*([0-9.]+)\s*([KMGT]?B)";
                    var match = Regex.Match(output, sizePattern, RegexOptions.IgnoreCase);
                    
                    if (match.Success)
                    {
                        if (double.TryParse(match.Groups[1].Value, out double size))
                        {
                            string unit = match.Groups[2].Value.ToUpper();
                            long bytes = ConvertToBytes(size, unit);
                            return bytes;
                        }
                    }
                    
                    // Fallback: try to parse raw bytes (in case format changes)
                    var bytesPattern = @"Size:\s*(\d+)(?:\s*bytes?)?";
                    var bytesMatch = Regex.Match(output, bytesPattern, RegexOptions.IgnoreCase);
                    if (bytesMatch.Success)
                    {
                        if (long.TryParse(bytesMatch.Groups[1].Value, out long bytes))
                        {
                            return bytes;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to get file size for {fileId}: {e.Message}");
            }
            
            return 0;
        }
        
        private long ConvertToBytes(double size, string unit)
        {
            switch (unit.ToUpper())
            {
                case "B":
                    return (long)size;
                case "KB":
                    return (long)(size * 1024);
                case "MB":
                    return (long)(size * 1024 * 1024);
                case "GB":
                    return (long)(size * 1024 * 1024 * 1024);
                case "TB":
                    return (long)(size * 1024L * 1024 * 1024 * 1024);
                default:
                    return (long)size; // Assume bytes if unknown unit
            }
        }
        
        private Tuple<string, string, long> FindPackageOnGoogleDrive(string packageName)
        {
            try
            {
                // Search for packages matching this name pattern
                var startInfo = new ProcessStartInfo
                {
                    FileName = @"C:\tools\gdrive",
                    Arguments = $"files list --parent 1GfaSFj_VU4jTXigOtQX9Ws8dknXGBISY --query \"name contains '{packageName}.unitypackage'\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    
                    if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        // Parse the output to find matching files
                        var lines = output.Split('\n');
                        foreach (var line in lines)
                        {
                            if (line.Contains(".unitypackage") && line.Contains(packageName))
                            {
                                // Parse gdrive list format: Id Name Type Size Created
                                var parts = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length >= 5)
                                {
                                    string fileId = parts[0];
                                    string fileName = parts[1];
                                    string sizeStr = parts[3] + " " + parts[4]; // "33.1 MB"
                                    
                                    // Parse size
                                    var sizeMatch = Regex.Match(sizeStr, @"([0-9.]+)\s*([KMGT]?B)", RegexOptions.IgnoreCase);
                                    long fileSize = 0;
                                    if (sizeMatch.Success)
                                    {
                                        if (double.TryParse(sizeMatch.Groups[1].Value, out double size))
                                        {
                                            string unit = sizeMatch.Groups[2].Value.ToUpper();
                                            fileSize = ConvertToBytes(size, unit);
                                        }
                                    }
                                    
                                    string driveLink = $"https://drive.google.com/file/d/{fileId}/view";
                                    return Tuple.Create(fileId, driveLink, fileSize);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to search Google Drive for {packageName}: {e.Message}");
            }
            
            return null;
        }
        
        private void CreateDownloadInstructions(string packageName, string googleDriveLink, string fileSize)
        {
            string instructionsPath = Path.Combine("Assets/Packages", packageName, "DownloadInstructions.txt");
            string packageDir = Path.GetDirectoryName(instructionsPath);
            
            // Ensure directory exists
            if (!Directory.Exists(packageDir))
            {
                Debug.LogWarning($"Package directory doesn't exist: {packageDir}");
                return;
            }
            
            string content = $@"Package: {packageName}
Export Date: {DateTime.Now:yyyy-MM-dd}
Google Drive Link: {googleDriveLink}
File Size: {fileSize}

Instructions:
1. Download the package from the Google Drive link above
2. Import into Unity via Assets > Import Package > Custom Package
3. Select the downloaded .unitypackage file

Note: This package was found on Google Drive and instructions were auto-generated.
";
            
            try
            {
                File.WriteAllText(instructionsPath, content);
                AssetDatabase.Refresh(); // Refresh to show new file in Unity
                Debug.Log($"Created DownloadInstructions.txt for {packageName}");
                
                EditorUtility.DisplayDialog(
                    "Instructions Created",
                    $"DownloadInstructions.txt has been created for {packageName}\n\nLocation: {instructionsPath}",
                    "OK"
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create DownloadInstructions.txt for {packageName}: {e.Message}");
                EditorUtility.DisplayDialog(
                    "Error",
                    $"Failed to create DownloadInstructions.txt for {packageName}\n\nError: {e.Message}",
                    "OK"
                );
            }
        }
        
        private void DownloadPackageFromGoogleDrive(string packageName, string fileId)
        {
            string downloadDir = "PackageDownloads";
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(downloadDir))
            {
                Directory.CreateDirectory(downloadDir);
            }
            
            // gdrive downloads with original filename, so we need to determine what that will be
            string expectedDownloadPath = Path.Combine(downloadDir, $"{packageName}.unitypackage");
            
            // Get package info for file size
            PackageInfo info = GetPackageInfo(packageName);
            string fileSizeFormatted = info?.FileSizeFormatted ?? "Unknown size";
            
            // Confirm download for large files
            if (info != null && info.FileSize > 1024 * 1024 * 100) // > 100MB
            {
                if (!EditorUtility.DisplayDialog(
                    "Large File Download",
                    $"The package {packageName} is {fileSizeFormatted}.\n\n" +
                    "Large downloads may take several minutes.\n" +
                    "Unity will be unresponsive during download.\n\n" +
                    "Continue with download?",
                    "Download",
                    "Cancel"))
                {
                    return;
                }
            }
            
            Debug.Log($"Starting download of {packageName} ({fileSizeFormatted}) from Google Drive...");
            
            // Show initial progress
            EditorUtility.DisplayProgressBar(
                $"Downloading {packageName}",
                $"Starting download ({fileSizeFormatted})...",
                0.1f
            );
            
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = @"C:\tools\gdrive",
                    Arguments = $"files download {fileId} --destination \"{downloadDir}\" --overwrite",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(startInfo))
                {
                    // Update progress bar periodically
                    float progress = 0.1f;
                    System.DateTime startTime = System.DateTime.Now;
                    
                    while (!process.HasExited)
                    {
                        // Estimate progress based on time (for large files)
                        var elapsed = (System.DateTime.Now - startTime).TotalSeconds;
                        progress = Mathf.Min(0.9f, 0.1f + (float)(elapsed / 60.0) * 0.8f); // Assume ~1 min per GB
                        
                        EditorUtility.DisplayProgressBar(
                            $"Downloading {packageName}",
                            $"Downloading {fileSizeFormatted} from Google Drive... ({elapsed:F0}s)",
                            progress
                        );
                        
                        // Check every 100ms
                        System.Threading.Thread.Sleep(100);
                        
                        // Allow cancellation
                        if (EditorUtility.DisplayCancelableProgressBar(
                            $"Downloading {packageName}",
                            $"Downloading {fileSizeFormatted} from Google Drive... ({elapsed:F0}s)",
                            progress))
                        {
                            process.Kill();
                            Debug.Log("Download cancelled by user");
                            EditorUtility.ClearProgressBar();
                            return;
                        }
                    }
                    
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    
                    EditorUtility.ClearProgressBar();
                    
                    if (process.ExitCode == 0)
                    {
                        // Find the actual downloaded file (gdrive uses original filename)
                        string actualDownloadPath = expectedDownloadPath;
                        var downloadedFiles = Directory.GetFiles(downloadDir, "*.unitypackage")
                            .Where(f => Path.GetFileName(f).Contains(packageName))
                            .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                            .ToArray();
                        
                        if (downloadedFiles.Length > 0)
                        {
                            actualDownloadPath = downloadedFiles[0];
                        }
                        
                        Debug.Log($"Successfully downloaded {packageName} to {actualDownloadPath}");
                        
                        // Show success dialog with option to reveal in finder
                        if (EditorUtility.DisplayDialog(
                            "Download Complete",
                            $"{packageName} has been downloaded successfully!\n\nLocation: {actualDownloadPath}\n\nWould you like to show the file in Explorer?",
                            "Show in Explorer",
                            "Close"))
                        {
                            EditorUtility.RevealInFinder(actualDownloadPath);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Failed to download {packageName}: {error}");
                        EditorUtility.DisplayDialog("Download Failed", 
                            $"Failed to download {packageName}.\nError: {error}", 
                            "OK");
                    }
                }
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"Error downloading {packageName}: {e.Message}");
                EditorUtility.DisplayDialog("Download Error", 
                    $"Error downloading {packageName}:\n{e.Message}", 
                    "OK");
            }
        }
        
    }
}