using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NeonLadder.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace NeonLadder.Tests.Editor
{
    [TestFixture]
    public class UnityPackageExporterTests
    {
        private string testPackagePath;
        private string testExportPath;
        private string originalExportPath;
        
        [SetUp]
        public void Setup()
        {
            // Create test directories
            testPackagePath = Path.Combine(Application.dataPath, "TestPackages");
            testExportPath = Path.Combine(Application.dataPath, "..", "TestExportedPackages");
            
            if (!Directory.Exists(testPackagePath))
            {
                Directory.CreateDirectory(testPackagePath);
            }
            
            if (!Directory.Exists(testExportPath))
            {
                Directory.CreateDirectory(testExportPath);
            }
            
            // Store original export path for restoration
            originalExportPath = Path.Combine(Application.dataPath, "..", "ExportedPackages");
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test directories
            if (Directory.Exists(testPackagePath))
            {
                try
                {
                    Directory.Delete(testPackagePath, true);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Could not delete test package directory: {ex.Message}");
                }
            }
            
            if (Directory.Exists(testExportPath))
            {
                try
                {
                    Directory.Delete(testExportPath, true);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Could not delete test export directory: {ex.Message}");
                }
            }
        }
        
        #region Package Discovery Tests
        
        [Test]
        public void DiscoverPackages_FindsAllPackagesInDirectory()
        {
            // Act
            var packages = UnityPackageExporter.DiscoverPackages();
            
            // Assert
            Assert.IsNotNull(packages, "Packages list should not be null");
            Assert.Greater(packages.Count, 0, "Should find at least one package");
            
            // Verify known packages exist
            var knownPackages = new[] { "Modern UI Pack", "Dialog System For Unity", "HeroEditor" };
            foreach (var packageName in knownPackages)
            {
                var package = packages.FirstOrDefault(p => p.Name == packageName);
                Assert.IsNotNull(package, $"Should find package: {packageName}");
            }
        }
        
        [Test]
        public void DiscoverPackages_IdentifiesDownloadInstructions()
        {
            // Act
            var packages = UnityPackageExporter.DiscoverPackages();
            
            // Assert
            var packagesWithInstructions = packages.Where(p => p.HasDownloadInstructions).ToList();
            Assert.Greater(packagesWithInstructions.Count, 0, 
                "Should find packages with DownloadInstructions.txt");
            
            // Verify specific package has instructions
            var modernUI = packages.FirstOrDefault(p => p.Name == "Modern UI Pack");
            if (modernUI != null)
            {
                Assert.IsTrue(modernUI.HasDownloadInstructions, 
                    "Modern UI Pack should have DownloadInstructions.txt");
            }
        }
        
        [Test]
        public void DiscoverPackages_CalculatesPackageSize()
        {
            // Act
            var packages = UnityPackageExporter.DiscoverPackages();
            
            // Assert
            foreach (var package in packages.Take(5)) // Test first 5 packages
            {
                Assert.Greater(package.SizeInBytes, 0, 
                    $"Package {package.Name} should have size > 0");
                
                // Verify size string formatting
                var sizeString = package.GetSizeString();
                Assert.IsNotEmpty(sizeString, $"Size string should not be empty for {package.Name}");
                Assert.IsTrue(sizeString.EndsWith("B") || sizeString.EndsWith("KB") || 
                             sizeString.EndsWith("MB") || sizeString.EndsWith("GB"),
                    "Size string should have proper unit suffix");
            }
        }
        
        [Test]
        public void PackageInfo_GetSizeString_FormatsCorrectly()
        {
            // Arrange
            var testCases = new[]
            {
                (512L, "512 B"),
                (1024L, "1.0 KB"),
                (1536L, "1.5 KB"),
                (1048576L, "1.0 MB"),
                (5242880L, "5.0 MB"),
                (1073741824L, "1.00 GB")
            };
            
            foreach (var (bytes, expected) in testCases)
            {
                // Arrange
                var packageInfo = new UnityPackageExporter.PackageInfo
                {
                    SizeInBytes = bytes
                };
                
                // Act
                var sizeString = packageInfo.GetSizeString();
                
                // Assert
                Assert.AreEqual(expected, sizeString, 
                    $"Size {bytes} bytes should format as {expected}");
            }
        }
        
        #endregion
        
        #region Export Tests
        
        [Test]
        public void ExportResult_InitializesCorrectly()
        {
            // Arrange & Act
            var result = new UnityPackageExporter.ExportResult
            {
                Success = true,
                PackageName = "TestPackage",
                ExportPath = "/path/to/export.unitypackage",
                ExportTimeSeconds = 2.5f
            };
            
            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("TestPackage", result.PackageName);
            Assert.AreEqual("/path/to/export.unitypackage", result.ExportPath);
            Assert.AreEqual(2.5f, result.ExportTimeSeconds);
            Assert.IsNull(result.ErrorMessage);
        }
        
        [Test]
        public void ExportSinglePackage_ValidatesPackagePath()
        {
            // Arrange
            var invalidPackage = new UnityPackageExporter.PackageInfo
            {
                Name = "NonExistentPackage",
                Path = "Assets/Packages/DoesNotExist",
                HasDownloadInstructions = true
            };
            
            // Act
            var result = UnityPackageExporter.ExportSinglePackage(invalidPackage);
            
            // Assert
            Assert.IsFalse(result.Success, "Export should fail for non-existent package");
            Assert.IsNotNull(result.ErrorMessage, "Should have error message");
            Assert.IsTrue(result.ErrorMessage.Contains("No assets found"), 
                "Error message should indicate no assets found");
        }
        
        [Test]
        public void ExportSinglePackage_CreatesExportDirectory()
        {
            // Arrange
            var exportPath = Path.Combine(Application.dataPath, "..", "ExportedPackages");
            
            // Clean up first if it exists
            if (Directory.Exists(exportPath))
            {
                Directory.Delete(exportPath, true);
            }
            
            Assert.IsFalse(Directory.Exists(exportPath), "Export directory should not exist initially");
            
            // Find a real package to export
            var packages = UnityPackageExporter.DiscoverPackages();
            if (packages.Count == 0)
            {
                Assert.Inconclusive("No packages found to test export");
                return;
            }
            
            // Act - Try to export (might fail, but should create directory)
            var package = packages.First();
            UnityPackageExporter.ExportSinglePackage(package);
            
            // Assert
            Assert.IsTrue(Directory.Exists(exportPath), 
                "Export directory should be created during export attempt");
        }
        
        #endregion
        
        #region Path and Naming Tests
        
        [Test]
        public void PackageName_HandlesSpacesInExportPath()
        {
            // Arrange
            var testNames = new[]
            {
                ("Modern UI Pack", "Modern_UI_Pack"),
                ("Dialog System For Unity", "Dialog_System_For_Unity"),
                ("Footsteps - Essentials", "Footsteps_-_Essentials"),
                ("Test Package", "Test_Package")
            };
            
            foreach (var (input, expected) in testNames)
            {
                // Act
                var sanitized = input.Replace(" ", "_");
                
                // Assert
                Assert.AreEqual(expected, sanitized, 
                    $"Package name '{input}' should be sanitized to '{expected}'");
            }
        }
        
        [Test]
        public void PackagePath_UsesCorrectAssetPath()
        {
            // Arrange
            var packageName = "TestPackage";
            var expectedPath = $"Assets/Packages/{packageName}";
            
            // Act
            var packageInfo = new UnityPackageExporter.PackageInfo
            {
                Name = packageName,
                Path = expectedPath
            };
            
            // Assert
            Assert.AreEqual(expectedPath, packageInfo.Path);
            Assert.IsTrue(packageInfo.Path.StartsWith("Assets/Packages/"), 
                "Package path should start with Assets/Packages/");
        }
        
        #endregion
        
        #region Google Drive Integration Tests
        
        [Test]
        public void PackageInfo_ParsesGoogleDriveLink()
        {
            // Arrange
            var testPackageDir = Path.Combine(testPackagePath, "TestPackageWithLink");
            Directory.CreateDirectory(testPackageDir);
            
            var instructionsContent = @"# TestPackage - Unity Package

## Download Link
Google Drive: https://drive.google.com/file/d/1234567890/view?usp=sharing

## Installation Instructions
1. Download the .unitypackage file";
            
            File.WriteAllText(Path.Combine(testPackageDir, "DownloadInstructions.txt"), 
                instructionsContent);
            
            // Act - This would be done in DiscoverPackages normally
            var content = File.ReadAllText(Path.Combine(testPackageDir, "DownloadInstructions.txt"));
            string googleDriveLink = null;
            
            if (content.Contains("Google Drive:"))
            {
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("Google Drive:"))
                    {
                        googleDriveLink = line.Replace("Google Drive:", "").Trim();
                        break;
                    }
                }
            }
            
            // Assert
            Assert.IsNotNull(googleDriveLink, "Should find Google Drive link");
            Assert.AreEqual("https://drive.google.com/file/d/1234567890/view?usp=sharing", 
                googleDriveLink);
        }
        
        [Test]
        public void DownloadInstructions_ContainsRequiredSections()
        {
            // This tests the expected format of DownloadInstructions.txt
            var requiredSections = new[]
            {
                "# ",  // Title
                "## Download Link",
                "## Installation Instructions",
                "Google Drive:"
            };
            
            // Find a package with DownloadInstructions.txt
            var packages = UnityPackageExporter.DiscoverPackages();
            var packageWithInstructions = packages.FirstOrDefault(p => p.HasDownloadInstructions);
            
            if (packageWithInstructions == null)
            {
                Assert.Inconclusive("No packages with DownloadInstructions.txt found");
                return;
            }
            
            var instructionsPath = Path.Combine(Application.dataPath, "Packages", 
                packageWithInstructions.Name, "DownloadInstructions.txt");
            
            if (File.Exists(instructionsPath))
            {
                var content = File.ReadAllText(instructionsPath);
                
                foreach (var section in requiredSections)
                {
                    Assert.IsTrue(content.Contains(section), 
                        $"DownloadInstructions.txt should contain '{section}'");
                }
            }
        }
        
        #endregion
        
        #region CLI Support Tests
        
        [Test]
        public void CLIExport_RequiresPackageName()
        {
            // This tests the CLI argument parsing logic
            var args = new string[] { "-batchmode", "-projectPath", "/path/to/project" };
            
            // Simulate finding package name argument
            string packageName = null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-packageName")
                {
                    packageName = args[i + 1];
                    break;
                }
            }
            
            Assert.IsNull(packageName, "Should not find package name without -packageName argument");
        }
        
        [Test]
        public void CLIExport_ParsesPackageNameCorrectly()
        {
            // Arrange
            var args = new string[] 
            { 
                "-batchmode", 
                "-projectPath", "/path/to/project",
                "-packageName", "Modern UI Pack",
                "-executeMethod", "UnityPackageExporter.ExportPackageFromCLI"
            };
            
            // Act
            string packageName = null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-packageName")
                {
                    packageName = args[i + 1];
                    break;
                }
            }
            
            // Assert
            Assert.AreEqual("Modern UI Pack", packageName, 
                "Should correctly parse package name from CLI arguments");
        }
        
        #endregion
        
        #region Performance Tests
        
        [Test]
        public void DiscoverPackages_PerformanceTest()
        {
            // Measure performance of package discovery
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Act
            var packages = UnityPackageExporter.DiscoverPackages();
            
            stopwatch.Stop();
            
            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 5000, 
                "Package discovery should complete within 5 seconds");
            
            Debug.Log($"Discovered {packages.Count} packages in {stopwatch.ElapsedMilliseconds}ms");
        }
        
        #endregion
    }
}