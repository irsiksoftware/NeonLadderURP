using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NeonLadder.Editor.Tools
{
    /// <summary>
    /// Automated Unity package export and Google Drive backup system.
    /// PBI-82: Provides menu items and CLI support for package management.
    /// </summary>
    public static class UnityPackageExporter
    {
        private const string PACKAGES_PATH = "Assets/Packages";
        private const string EXPORT_PATH = "ExportedPackages";
        private const string GDRIVE_FOLDER_ID = "1PfhnfbV6jvi-eh46z4s3yzxnd0TfDpj8";
        private const string MENU_PREFIX = "Tools/Package Export/";
        
        #region Data Structures
        
        [Serializable]
        public class PackageInfo
        {
            public string Name;
            public string Path;
            public bool HasDownloadInstructions;
            public long SizeInBytes;
            public DateTime LastModified;
            public string GoogleDriveLink;
            
            public string GetSizeString()
            {
                if (SizeInBytes < 1024) return $"{SizeInBytes} B";
                if (SizeInBytes < 1024 * 1024) return $"{SizeInBytes / 1024f:F1} KB";
                if (SizeInBytes < 1024 * 1024 * 1024) return $"{SizeInBytes / (1024f * 1024f):F1} MB";
                return $"{SizeInBytes / (1024f * 1024f * 1024f):F2} GB";
            }
        }
        
        [Serializable]
        public class ExportResult
        {
            public bool Success;
            public string PackageName;
            public string ExportPath;
            public string ErrorMessage;
            public float ExportTimeSeconds;
        }
        
        #endregion
        
        #region Menu Items
        
        [MenuItem(MENU_PREFIX + "Export All Packages", priority = 1)]
        public static void ExportAllPackagesMenu()
        {
            if (EditorUtility.DisplayDialog("Export All Packages",
                "This will export all packages with DownloadInstructions.txt files.\n\n" +
                "This process may take several minutes.\n\n" +
                "Continue?",
                "Export All", "Cancel"))
            {
                ExportAllPackages();
            }
        }
        
        [MenuItem(MENU_PREFIX + "Export Selected Package", priority = 2)]
        public static void ExportSelectedPackageMenu()
        {
            var packages = DiscoverPackages();
            var packageNames = packages.Select(p => p.Name).ToArray();
            
            if (packageNames.Length == 0)
            {
                EditorUtility.DisplayDialog("No Packages Found",
                    "No packages with DownloadInstructions.txt were found.",
                    "OK");
                return;
            }
            
            // Show selection dialog
            var selectedIndex = 0;
            var window = ScriptableObject.CreateInstance<PackageSelectionWindow>();
            window.Initialize(packages.ToArray(), (package) =>
            {
                ExportSinglePackage(package);
            });
            window.ShowUtility();
        }
        
        [MenuItem(MENU_PREFIX + "Upload to Google Drive", priority = 3)]
        public static void UploadToGoogleDriveMenu()
        {
            var exportPath = Path.Combine(Application.dataPath, "..", EXPORT_PATH);
            
            if (!Directory.Exists(exportPath))
            {
                EditorUtility.DisplayDialog("No Exported Packages",
                    "No exported packages found. Please export packages first.",
                    "OK");
                return;
            }
            
            var unityPackages = Directory.GetFiles(exportPath, "*.unitypackage");
            
            if (unityPackages.Length == 0)
            {
                EditorUtility.DisplayDialog("No Packages to Upload",
                    "No .unitypackage files found in the export directory.",
                    "OK");
                return;
            }
            
            if (EditorUtility.DisplayDialog("Upload to Google Drive",
                $"Found {unityPackages.Length} package(s) to upload.\n\n" +
                "This will use the gdrive CLI tool.\n\n" +
                "Continue?",
                "Upload", "Cancel"))
            {
                UploadPackagesToGoogleDrive(unityPackages);
            }
        }
        
        [MenuItem(MENU_PREFIX + "Verify Package Status", priority = 10)]
        public static void VerifyPackageStatusMenu()
        {
            var packages = DiscoverPackages();
            var exportPath = Path.Combine(Application.dataPath, "..", EXPORT_PATH);
            
            var report = "Package Status Report\n";
            report += "====================\n\n";
            report += $"Total Packages: {packages.Count}\n";
            report += $"Export Directory: {exportPath}\n\n";
            
            foreach (var package in packages)
            {
                var exportFile = Path.Combine(exportPath, $"{package.Name.Replace(" ", "_")}.unitypackage");
                var isExported = File.Exists(exportFile);
                
                report += $"• {package.Name}\n";
                report += $"  Size: {package.GetSizeString()}\n";
                report += $"  Exported: {(isExported ? "✓" : "✗")}\n";
                
                if (isExported)
                {
                    var fileInfo = new FileInfo(exportFile);
                    report += $"  Export Size: {GetFileSizeString(fileInfo.Length)}\n";
                    report += $"  Export Date: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm}\n";
                }
                
                if (!string.IsNullOrEmpty(package.GoogleDriveLink))
                {
                    report += $"  Google Drive: ✓\n";
                }
                
                report += "\n";
            }
            
            // Show in console and dialog
            Debug.Log(report);
            EditorUtility.DisplayDialog("Package Status", report, "OK");
        }
        
        [MenuItem(MENU_PREFIX + "Open Export Folder", priority = 20)]
        public static void OpenExportFolder()
        {
            var exportPath = Path.Combine(Application.dataPath, "..", EXPORT_PATH);
            
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
            
            EditorUtility.RevealInFinder(exportPath);
        }
        
        #endregion
        
        #region Core Functions
        
        /// <summary>
        /// Discovers all packages in the Packages directory
        /// </summary>
        public static List<PackageInfo> DiscoverPackages()
        {
            var packages = new List<PackageInfo>();
            var packagesPath = Path.Combine(Application.dataPath, "Packages");
            
            if (!Directory.Exists(packagesPath))
            {
                Debug.LogError($"Packages directory not found: {packagesPath}");
                return packages;
            }
            
            foreach (var dir in Directory.GetDirectories(packagesPath))
            {
                var dirInfo = new DirectoryInfo(dir);
                var packageInfo = new PackageInfo
                {
                    Name = dirInfo.Name,
                    Path = $"Assets/Packages/{dirInfo.Name}",
                    HasDownloadInstructions = File.Exists(Path.Combine(dir, "DownloadInstructions.txt")),
                    LastModified = dirInfo.LastWriteTime,
                    SizeInBytes = GetDirectorySize(dir)
                };
                
                // Check for existing Google Drive link
                var instructionsFile = Path.Combine(dir, "DownloadInstructions.txt");
                if (File.Exists(instructionsFile))
                {
                    var content = File.ReadAllText(instructionsFile);
                    if (content.Contains("Google Drive:"))
                    {
                        var lines = content.Split('\n');
                        foreach (var line in lines)
                        {
                            if (line.StartsWith("Google Drive:"))
                            {
                                packageInfo.GoogleDriveLink = line.Replace("Google Drive:", "").Trim();
                                break;
                            }
                        }
                    }
                }
                
                packages.Add(packageInfo);
            }
            
            return packages.OrderBy(p => p.Name).ToList();
        }
        
        /// <summary>
        /// Exports a single package to .unitypackage format
        /// </summary>
        public static ExportResult ExportSinglePackage(PackageInfo package)
        {
            var result = new ExportResult
            {
                PackageName = package.Name
            };
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // Create export directory if it doesn't exist
                var exportPath = Path.Combine(Application.dataPath, "..", EXPORT_PATH);
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }
                
                // Get all assets in the package
                var assetPaths = AssetDatabase.GetAllAssetPaths()
                    .Where(path => path.StartsWith(package.Path))
                    .ToArray();
                
                if (assetPaths.Length == 0)
                {
                    result.ErrorMessage = $"No assets found in package: {package.Path}";
                    Debug.LogError(result.ErrorMessage);
                    return result;
                }
                
                // Export the package
                var outputFile = Path.Combine(exportPath, $"{package.Name.Replace(" ", "_")}.unitypackage");
                result.ExportPath = outputFile;
                
                Debug.Log($"Exporting {assetPaths.Length} assets from {package.Name}...");
                
                AssetDatabase.ExportPackage(
                    assetPaths,
                    outputFile,
                    ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
                );
                
                stopwatch.Stop();
                result.ExportTimeSeconds = (float)stopwatch.Elapsed.TotalSeconds;
                result.Success = true;
                
                var fileInfo = new FileInfo(outputFile);
                Debug.Log($"✓ Exported {package.Name} ({GetFileSizeString(fileInfo.Length)}) in {result.ExportTimeSeconds:F1}s");
                
                // Show progress
                EditorUtility.DisplayDialog("Export Complete",
                    $"Successfully exported {package.Name}\n\n" +
                    $"Size: {GetFileSizeString(fileInfo.Length)}\n" +
                    $"Time: {result.ExportTimeSeconds:F1} seconds\n" +
                    $"Location: {outputFile}",
                    "OK");
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                Debug.LogError($"Failed to export {package.Name}: {ex.Message}");
                
                EditorUtility.DisplayDialog("Export Failed",
                    $"Failed to export {package.Name}\n\n" +
                    $"Error: {ex.Message}",
                    "OK");
            }
            
            return result;
        }
        
        /// <summary>
        /// Exports all packages with DownloadInstructions.txt
        /// </summary>
        public static List<ExportResult> ExportAllPackages()
        {
            var results = new List<ExportResult>();
            var packages = DiscoverPackages().Where(p => p.HasDownloadInstructions).ToList();
            
            if (packages.Count == 0)
            {
                EditorUtility.DisplayDialog("No Packages to Export",
                    "No packages with DownloadInstructions.txt were found.",
                    "OK");
                return results;
            }
            
            var progressTitle = "Exporting Packages";
            var totalPackages = packages.Count;
            var currentPackage = 0;
            
            foreach (var package in packages)
            {
                currentPackage++;
                var progress = (float)currentPackage / totalPackages;
                
                if (EditorUtility.DisplayCancelableProgressBar(
                    progressTitle,
                    $"Exporting {package.Name} ({currentPackage}/{totalPackages})",
                    progress))
                {
                    Debug.Log("Export cancelled by user");
                    break;
                }
                
                var result = ExportSinglePackage(package);
                results.Add(result);
            }
            
            EditorUtility.ClearProgressBar();
            
            // Show summary
            var successCount = results.Count(r => r.Success);
            var failCount = results.Count(r => !r.Success);
            var totalTime = results.Sum(r => r.ExportTimeSeconds);
            
            var summary = $"Export Complete!\n\n";
            summary += $"Successful: {successCount}\n";
            summary += $"Failed: {failCount}\n";
            summary += $"Total Time: {totalTime:F1} seconds\n";
            
            if (failCount > 0)
            {
                summary += "\nFailed packages:\n";
                foreach (var failed in results.Where(r => !r.Success))
                {
                    summary += $"• {failed.PackageName}: {failed.ErrorMessage}\n";
                }
            }
            
            EditorUtility.DisplayDialog("Export Summary", summary, "OK");
            
            return results;
        }
        
        /// <summary>
        /// Uploads packages to Google Drive using gdrive CLI
        /// </summary>
        public static void UploadPackagesToGoogleDrive(string[] packageFiles)
        {
            // This would need to call out to the shell script
            // For now, we'll provide instructions
            
            var instructions = "To upload packages to Google Drive:\n\n";
            instructions += "1. Open Terminal\n";
            instructions += "2. Navigate to the project directory\n";
            instructions += "3. Run the following command:\n\n";
            instructions += "   ./Scripts/package-automation.sh upload-all\n\n";
            instructions += "This will upload all exported packages and update DownloadInstructions.txt files.";
            
            EditorUtility.DisplayDialog("Upload Instructions", instructions, "OK");
            
            // Copy command to clipboard
            EditorGUIUtility.systemCopyBuffer = "./Scripts/package-automation.sh upload-all";
            Debug.Log("Upload command copied to clipboard: ./Scripts/package-automation.sh upload-all");
        }
        
        #endregion
        
        #region CLI Support
        
        /// <summary>
        /// CLI entry point for package export (called from command line)
        /// </summary>
        public static void ExportPackageFromCLI()
        {
            var args = Environment.GetCommandLineArgs();
            string packageName = null;
            
            // Find the package name argument
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-packageName")
                {
                    packageName = args[i + 1];
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(packageName))
            {
                Debug.LogError("Package name not specified. Use -packageName <name>");
                EditorApplication.Exit(1);
                return;
            }
            
            // Find the package
            var packages = DiscoverPackages();
            var package = packages.FirstOrDefault(p => p.Name == packageName);
            
            if (package == null)
            {
                Debug.LogError($"Package not found: {packageName}");
                EditorApplication.Exit(1);
                return;
            }
            
            // Export the package
            var result = ExportSinglePackage(package);
            
            if (result.Success)
            {
                Debug.Log($"Successfully exported {packageName}");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"Failed to export {packageName}: {result.ErrorMessage}");
                EditorApplication.Exit(1);
            }
        }
        
        /// <summary>
        /// CLI entry point for exporting all packages
        /// </summary>
        public static void ExportAllPackagesFromCLI()
        {
            var results = ExportAllPackages();
            var successCount = results.Count(r => r.Success);
            var totalCount = results.Count;
            
            if (successCount == totalCount)
            {
                Debug.Log($"Successfully exported all {totalCount} packages");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"Exported {successCount}/{totalCount} packages. Some exports failed.");
                EditorApplication.Exit(1);
            }
        }
        
        #endregion
        
        #region Helper Functions
        
        private static long GetDirectorySize(string path)
        {
            long size = 0;
            
            try
            {
                var dirInfo = new DirectoryInfo(path);
                
                foreach (FileInfo file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    size += file.Length;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not calculate directory size for {path}: {ex.Message}");
            }
            
            return size;
        }
        
        private static string GetFileSizeString(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024f:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024f * 1024f):F1} MB";
            return $"{bytes / (1024f * 1024f * 1024f):F2} GB";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Package selection window for interactive export
    /// </summary>
    public class PackageSelectionWindow : EditorWindow
    {
        private UnityPackageExporter.PackageInfo[] packages;
        private Action<UnityPackageExporter.PackageInfo> onPackageSelected;
        private Vector2 scrollPosition;
        private string searchFilter = "";
        
        public void Initialize(UnityPackageExporter.PackageInfo[] packages, 
            Action<UnityPackageExporter.PackageInfo> onPackageSelected)
        {
            this.packages = packages;
            this.onPackageSelected = onPackageSelected;
            
            titleContent = new GUIContent("Select Package to Export");
            minSize = new Vector2(500, 400);
            maxSize = new Vector2(800, 600);
        }
        
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Search bar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Search:", GUILayout.Width(50));
            searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                searchFilter = "";
            }
            EditorGUILayout.EndHorizontal();
            
            // Package list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            var filteredPackages = string.IsNullOrEmpty(searchFilter) 
                ? packages 
                : packages.Where(p => p.Name.ToLower().Contains(searchFilter.ToLower())).ToArray();
            
            foreach (var package in filteredPackages)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                
                // Package info
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(package.Name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Size: {package.GetSizeString()}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Path: {package.Path}", EditorStyles.miniLabel);
                
                if (!package.HasDownloadInstructions)
                {
                    EditorGUILayout.LabelField("⚠ No DownloadInstructions.txt", EditorStyles.miniLabel);
                }
                
                if (!string.IsNullOrEmpty(package.GoogleDriveLink))
                {
                    EditorGUILayout.LabelField("✓ Has Google Drive link", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndVertical();
                
                // Export button
                if (GUILayout.Button("Export", GUILayout.Width(80), GUILayout.Height(40)))
                {
                    onPackageSelected?.Invoke(package);
                    Close();
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            
            EditorGUILayout.EndScrollView();
            
            // Status bar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label($"Showing {filteredPackages.Length} of {packages.Length} packages");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", EditorStyles.toolbarButton))
            {
                Close();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
    }
}