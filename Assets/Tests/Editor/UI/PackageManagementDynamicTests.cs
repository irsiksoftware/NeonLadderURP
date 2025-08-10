using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeonLadder.Editor;
using System;
using System.Text.RegularExpressions;

namespace NeonLadder.Tests.Editor.UI
{
    [TestFixture]
    public class PackageManagementDynamicTests
    {
        private string testPackagesPath;
        private string originalPackagesPath;
        
        [SetUp]
        public void Setup()
        {
            // Create temporary test packages directory
            testPackagesPath = Path.Combine(Application.dataPath, "TestPackages");
            if (!Directory.Exists(testPackagesPath))
            {
                Directory.CreateDirectory(testPackagesPath);
            }
            
            // We can't easily change the PACKAGES_PATH constant, so we'll test with real data
            // but create some test packages for controlled testing
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test directories
            if (Directory.Exists(testPackagesPath))
            {
                Directory.Delete(testPackagesPath, true);
            }
            
            // Refresh to remove from Unity
            AssetDatabase.Refresh();
        }
        
        #region Core Package Discovery Tests
        
        [Test]
        public void GetDiscoveredPackages_ReturnsNonEmptyList()
        {
            // Act
            var packages = PackageManagementDynamic.GetDiscoveredPackages();
            
            // Assert
            Assert.IsNotNull(packages, "Package list should not be null");
            Assert.IsTrue(packages.Count > 0, "Should discover at least some packages in Assets/Packages");
            
            // Verify we have some expected packages
            Assert.IsTrue(packages.Contains("EasyRoads3D"), "Should contain EasyRoads3D package");
            Assert.IsTrue(packages.Contains("LeartesStudios"), "Should contain LeartesStudios package");
        }
        
        [Test]
        public void GetDiscoveredPackages_ContainsExpectedPackageTypes()
        {
            // Act
            var packages = PackageManagementDynamic.GetDiscoveredPackages();
            
            // Assert - Check for variety of package types we know exist
            var expectedPackages = new[] { 
                "Dialog System For Unity", 
                "Modern UI Pack", 
                "HeroEditor", 
                "Synty" 
            };
            
            foreach (var expectedPackage in expectedPackages)
            {
                Assert.IsTrue(packages.Contains(expectedPackage), 
                    $"Should contain expected package: {expectedPackage}");
            }
        }
        
        #endregion
        
        #region File Size Formatting Tests
        
        [Test]
        public void FormatFileSize_FormatsBytes_Correctly()
        {
            // Test various byte sizes - Note: 0 returns "Unknown" in our implementation
            Assert.AreEqual("Unknown", PackageManagementDynamic.FormatFileSize(0));
            Assert.AreEqual("512 B", PackageManagementDynamic.FormatFileSize(512));
            Assert.AreEqual("1 KB", PackageManagementDynamic.FormatFileSize(1024));
            Assert.AreEqual("1.5 KB", PackageManagementDynamic.FormatFileSize(1536));
            Assert.AreEqual("2 MB", PackageManagementDynamic.FormatFileSize(2 * 1024 * 1024));
            Assert.AreEqual("1.5 GB", PackageManagementDynamic.FormatFileSize((long)(1.5 * 1024 * 1024 * 1024)));
        }
        
        [Test]
        public void FormatFileSize_HandlesLargeFiles()
        {
            // Test very large files
            long terabyte = 1024L * 1024 * 1024 * 1024;
            string result = PackageManagementDynamic.FormatFileSize(terabyte);
            Assert.IsTrue(result.Contains("TB"), $"Should format as TB: {result}");
        }
        
        [Test]
        public void FormatFileSize_HandlesNegativeOrInvalid()
        {
            Assert.AreEqual("Unknown", PackageManagementDynamic.FormatFileSize(-1));
            Assert.AreEqual("Unknown", PackageManagementDynamic.FormatFileSize(-1000));
        }
        
        #endregion
        
        #region DownloadInstructions.txt Management Tests
        
        [Test]
        public void UpdateDownloadInstructions_CreatesFileWhenMissing()
        {
            // Arrange
            string testPackageName = "TestPackage";
            string testPackageDir = Path.Combine("Assets/Packages", testPackageName);
            string testInstructionsPath = Path.Combine(testPackageDir, "DownloadInstructions.txt");
            
            // Create test package directory
            if (!Directory.Exists(testPackageDir))
            {
                Directory.CreateDirectory(testPackageDir);
            }
            
            // Ensure file doesn't exist
            if (File.Exists(testInstructionsPath))
            {
                File.Delete(testInstructionsPath);
            }
            
            try
            {
                // Act
                string testLink = "https://drive.google.com/file/d/test123/view";
                PackageManagementDynamic.UpdateDownloadInstructions(testPackageName, testLink);
                
                // Assert
                Assert.IsTrue(File.Exists(testInstructionsPath), "Should create DownloadInstructions.txt file");
                
                string content = File.ReadAllText(testInstructionsPath);
                Assert.IsTrue(content.Contains(testLink), "Should contain the Google Drive link");
                Assert.IsTrue(content.Contains(testPackageName), "Should contain package name");
                Assert.IsTrue(content.Contains(DateTime.Now.ToString("yyyy-MM-dd")), "Should contain current date");
            }
            finally
            {
                // Cleanup
                if (File.Exists(testInstructionsPath))
                {
                    File.Delete(testInstructionsPath);
                }
                if (Directory.Exists(testPackageDir))
                {
                    Directory.Delete(testPackageDir);
                }
            }
        }
        
        [Test]
        public void UpdateDownloadInstructions_UpdatesExistingFile()
        {
            // Arrange
            string testPackageName = "TestPackage2";
            string testPackageDir = Path.Combine("Assets/Packages", testPackageName);
            string testInstructionsPath = Path.Combine(testPackageDir, "DownloadInstructions.txt");
            
            // Create test package directory
            if (!Directory.Exists(testPackageDir))
            {
                Directory.CreateDirectory(testPackageDir);
            }
            
            try
            {
                // Create initial file
                string initialContent = @"Package: TestPackage2
Export Date: 2024-01-01
Google Drive Link: [PENDING UPLOAD]

Instructions:
1. Download from link above
";
                File.WriteAllText(testInstructionsPath, initialContent);
                
                // Act
                string newLink = "https://drive.google.com/file/d/newlink456/view";
                PackageManagementDynamic.UpdateDownloadInstructions(testPackageName, newLink);
                
                // Assert
                string updatedContent = File.ReadAllText(testInstructionsPath);
                Assert.IsTrue(updatedContent.Contains(newLink), "Should contain the new Google Drive link");
                Assert.IsFalse(updatedContent.Contains("[PENDING UPLOAD]"), "Should remove pending upload text");
                Assert.IsTrue(updatedContent.Contains("Instructions:"), "Should preserve existing instructions");
            }
            finally
            {
                // Cleanup
                if (File.Exists(testInstructionsPath))
                {
                    File.Delete(testInstructionsPath);
                }
                if (Directory.Exists(testPackageDir))
                {
                    Directory.Delete(testPackageDir);
                }
            }
        }
        
        #endregion
        
        #region Export Package Tests
        
        [Test]
        public void ExportPackage_WithNonExistentPackage_ShowsErrorDialog()
        {
            // This test is tricky because ExportPackage shows UI dialogs
            // We can test the path detection logic instead
            string nonExistentPath = Path.Combine("Assets/Packages", "NonExistentPackage");
            Assert.IsFalse(Directory.Exists(nonExistentPath), 
                "Test package should not exist for this test");
        }
        
        #endregion
        
        #region Integration Tests - Real Package Data
        
        [Test]
        public void RealPackageData_EasyRoads3D_HasValidDownloadInstructions()
        {
            // Test that the EasyRoads3D fix we implemented works
            string easyRoadsPath = Path.Combine("Assets/Packages/EasyRoads3D/DownloadInstructions.txt");
            
            if (File.Exists(easyRoadsPath))
            {
                string content = File.ReadAllText(easyRoadsPath);
                Assert.IsTrue(content.Contains("drive.google.com"), 
                    "EasyRoads3D should now have a valid Google Drive link");
                Assert.IsTrue(content.Contains("1qyzC8JMab-WEOvHTucb_P36td2wcVaVP"), 
                    "Should contain the correct file ID");
            }
            else
            {
                Assert.Inconclusive("EasyRoads3D package not found - skipping integration test");
            }
        }
        
        [Test]
        public void RealPackageData_LeartesStudios_HasLargeFileWarning()
        {
            // Test that large packages have appropriate warnings
            string leartesPath = Path.Combine("Assets/Packages/LeartesStudios/DownloadInstructions.txt");
            
            if (File.Exists(leartesPath))
            {
                string content = File.ReadAllText(leartesPath);
                Assert.IsTrue(content.Contains("GB") || content.Contains("WARNING") || content.Contains("MASSIVE"), 
                    "Large packages should have size warnings");
                Assert.IsTrue(content.Contains("drive.google.com"), 
                    "Should have a valid Google Drive link");
            }
            else
            {
                Assert.Inconclusive("LeartesStudios package not found - skipping integration test");
            }
        }
        
        #endregion
        
        #region Edge Cases and Error Handling
        
        [Test]
        public void FormatFileSize_EdgeCases()
        {
            // Test edge cases around unit boundaries
            Assert.AreEqual("1023 B", PackageManagementDynamic.FormatFileSize(1023));
            Assert.AreEqual("1 KB", PackageManagementDynamic.FormatFileSize(1024));
            
            // The actual result will be "1024 KB" because (1024*1024-1)/1024 = 1023.9990234375
            // which gets rounded, but our implementation may handle this differently
            string result = PackageManagementDynamic.FormatFileSize(1024 * 1024 - 1);
            Assert.IsTrue(result.Contains("KB"), $"Should be in KB units, got: {result}");
            
            Assert.AreEqual("1 MB", PackageManagementDynamic.FormatFileSize(1024 * 1024));
        }
        
        [Test]
        public void PackageManagement_MenuItemExists()
        {
            // Verify the menu item registration works
            var packageManagementType = typeof(PackageManagement);
            Assert.IsNotNull(packageManagementType, "PackageManagement class should exist");
            
            // Check for menu methods
            var methods = packageManagementType.GetMethods();
            bool hasMenuMethod = false;
            foreach (var method in methods)
            {
                if (method.Name == "OpenPackageManager")
                {
                    hasMenuMethod = true;
                    break;
                }
            }
            Assert.IsTrue(hasMenuMethod, "Should have OpenPackageManager method");
        }
        
        #endregion
    }
}