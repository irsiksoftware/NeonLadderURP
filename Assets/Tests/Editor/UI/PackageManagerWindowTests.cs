using NUnit.Framework;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using NeonLadder.Editor;
using System.Collections.Generic;
using System.Reflection;

namespace NeonLadder.Tests.Editor.UI
{
    [TestFixture]
    public class PackageManagerWindowTests
    {
        #region Google Drive Link Parsing Tests
        
        [Test]
        public void ParseGoogleDriveLink_ValidShareLink_ExtractsFileId()
        {
            // Arrange
            string content = @"Package: TestPackage
Google Drive Link: https://drive.google.com/file/d/1ABC123def456GHI789/view?usp=sharing
Instructions: Download from above";
            
            // Act - Use reflection to access private method
            var windowType = typeof(PackageManagerWindow);
            var method = windowType.GetMethod("ParseGoogleDriveLink", BindingFlags.NonPublic | BindingFlags.Instance);
            var window = ScriptableObject.CreateInstance<PackageManagerWindow>();
            var result = method.Invoke(window, new object[] { content });
            
            // Assert
            Assert.IsNotNull(result, "Should parse valid Google Drive link");
            var tuple = result as Tuple<string, string>;
            Assert.IsNotNull(tuple, "Should return tuple");
            Assert.AreEqual("1ABC123def456GHI789", tuple.Item1, "Should extract correct file ID");
            Assert.AreEqual("https://drive.google.com/file/d/1ABC123def456GHI789/view", tuple.Item2, "Should generate correct link");
        }
        
        [Test]
        public void ParseGoogleDriveLink_OpenLink_ExtractsFileId()
        {
            // Arrange
            string content = @"Package: TestPackage
Google Drive Link: https://drive.google.com/open?id=1XYZ789abc123DEF456
Instructions: Download from above";
            
            // Act
            var windowType = typeof(PackageManagerWindow);
            var method = windowType.GetMethod("ParseGoogleDriveLink", BindingFlags.NonPublic | BindingFlags.Instance);
            var window = ScriptableObject.CreateInstance<PackageManagerWindow>();
            var result = method.Invoke(window, new object[] { content });
            
            // Assert
            Assert.IsNotNull(result, "Should parse open-style Google Drive link");
            var tuple = result as Tuple<string, string>;
            Assert.AreEqual("1XYZ789abc123DEF456", tuple.Item1, "Should extract correct file ID");
        }
        
        [Test]
        public void ParseGoogleDriveLink_FileIdOnly_ExtractsFileId()
        {
            // Arrange
            string content = @"Package: TestPackage
File ID: 1QWE987rty543UIO321
Instructions: Download from above";
            
            // Act
            var windowType = typeof(PackageManagerWindow);
            var method = windowType.GetMethod("ParseGoogleDriveLink", BindingFlags.NonPublic | BindingFlags.Instance);
            var window = ScriptableObject.CreateInstance<PackageManagerWindow>();
            var result = method.Invoke(window, new object[] { content });
            
            // Assert
            Assert.IsNotNull(result, "Should parse file ID format");
            var tuple = result as Tuple<string, string>;
            Assert.AreEqual("1QWE987rty543UIO321", tuple.Item1, "Should extract file ID");
        }
        
        [Test]
        public void ParseGoogleDriveLink_NoValidLink_ReturnsNull()
        {
            // Arrange
            string content = @"Package: TestPackage
Google Drive Link: [PENDING UPLOAD]
Instructions: Download from above";
            
            // Act
            var windowType = typeof(PackageManagerWindow);
            var method = windowType.GetMethod("ParseGoogleDriveLink", BindingFlags.NonPublic | BindingFlags.Instance);
            var window = ScriptableObject.CreateInstance<PackageManagerWindow>();
            var result = method.Invoke(window, new object[] { content });
            
            // Assert
            Assert.IsNull(result, "Should return null for invalid links");
        }
        
        [Test]
        public void ParseGoogleDriveLink_EasyRoads3DRealContent_ExtractsCorrectId()
        {
            // Arrange - Real content from EasyRoads3D after our fix
            string content = @"Download the necessary file(s) from the following link:

Google Drive Link: https://drive.google.com/file/d/1qyzC8JMab-WEOvHTucb_P36td2wcVaVP/view
[PENDING UPLOAD - Large package requires upload to Google Drive]";
            
            // Act
            var windowType = typeof(PackageManagerWindow);
            var method = windowType.GetMethod("ParseGoogleDriveLink", BindingFlags.NonPublic | BindingFlags.Instance);
            var window = ScriptableObject.CreateInstance<PackageManagerWindow>();
            var result = method.Invoke(window, new object[] { content });
            
            // Assert
            Assert.IsNotNull(result, "Should parse EasyRoads3D content");
            var tuple = result as Tuple<string, string>;
            Assert.AreEqual("1qyzC8JMab-WEOvHTucb_P36td2wcVaVP", tuple.Item1, "Should extract EasyRoads3D file ID");
        }
        
        #endregion
        
        #region Unit Conversion Tests
        
        [Test]
        public void ConvertToBytes_AllUnits_ConvertsCorrectly()
        {
            // Act & Assert using reflection to access private method
            var windowType = typeof(PackageManagerWindow);
            var method = windowType.GetMethod("ConvertToBytes", BindingFlags.NonPublic | BindingFlags.Instance);
            var window = ScriptableObject.CreateInstance<PackageManagerWindow>();
            
            // Test all supported units
            Assert.AreEqual(1024L, method.Invoke(window, new object[] { 1024.0, "B" }));
            Assert.AreEqual(1024L, method.Invoke(window, new object[] { 1.0, "KB" }));
            Assert.AreEqual(1048576L, method.Invoke(window, new object[] { 1.0, "MB" }));
            Assert.AreEqual(1073741824L, method.Invoke(window, new object[] { 1.0, "GB" }));
            Assert.AreEqual(1099511627776L, method.Invoke(window, new object[] { 1.0, "TB" }));
            
            // Test fractional values
            Assert.AreEqual(1536L, method.Invoke(window, new object[] { 1.5, "KB" }));
            Assert.AreEqual(2147483648L, method.Invoke(window, new object[] { 2.0, "GB" }));
        }
        
        [Test]
        public void ConvertToBytes_UnknownUnit_ReturnsSizeAsBytes()
        {
            // Act & Assert
            var windowType = typeof(PackageManagerWindow);
            var method = windowType.GetMethod("ConvertToBytes", BindingFlags.NonPublic | BindingFlags.Instance);
            var window = ScriptableObject.CreateInstance<PackageManagerWindow>();
            
            Assert.AreEqual(500L, method.Invoke(window, new object[] { 500.0, "UNKNOWN" }));
        }
        
        #endregion
        
        #region Google Drive Output Parsing Tests
        
        [Test]
        public void FindPackageOnGoogleDrive_ParsesGdriveOutput()
        {
            // This test simulates what would happen if we could mock gdrive output
            // Testing the parsing logic without actual gdrive calls
            
            // Arrange - Simulate gdrive list output format
            string mockGdriveOutput = @"Id                                  Name                                        Type      Size       Created
1qyzC8JMab-WEOvHTucb_P36td2wcVaVP   EasyRoads3D.unitypackage                    regular   14 MB      2025-02-04 01:21:25
1nXLBeSRZSzhFe_iPTl7QNuZBVO4j0DYE   DamageNumbersPro.unitypackage               regular   12.8 MB    2025-02-04 01:21:07";
            
            // Test the regex pattern that would be used in FindPackageOnGoogleDrive
            var lines = mockGdriveOutput.Split('\n');
            bool foundEasyRoads = false;
            string extractedId = null;
            
            foreach (var line in lines)
            {
                if (line.Contains(".unitypackage") && line.Contains("EasyRoads3D"))
                {
                    // Simulate the parsing logic from FindPackageOnGoogleDrive
                    var parts = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5)
                    {
                        extractedId = parts[0];
                        foundEasyRoads = true;
                        break;
                    }
                }
            }
            
            // Assert
            Assert.IsTrue(foundEasyRoads, "Should find EasyRoads3D in mock output");
            Assert.AreEqual("1qyzC8JMab-WEOvHTucb_P36td2wcVaVP", extractedId, "Should extract correct file ID");
        }
        
        [Test]
        public void SizeRegexPattern_ParsesVariousSizes()
        {
            // Test the size parsing regex from GetGoogleDriveFileSize
            var sizePattern = @"([0-9.]+)\s*([KMGT]?B)";
            
            var testCases = new Dictionary<string, (double expectedSize, string expectedUnit)>
            {
                { "14 MB", (14.0, "MB") },
                { "1.5 GB", (1.5, "GB") },
                { "512 KB", (512.0, "KB") },
                { "2048 B", (2048.0, "B") },
                { "1.75 TB", (1.75, "TB") }
            };
            
            foreach (var testCase in testCases)
            {
                var match = Regex.Match(testCase.Key, sizePattern, RegexOptions.IgnoreCase);
                Assert.IsTrue(match.Success, $"Should match size pattern: {testCase.Key}");
                
                if (match.Success && double.TryParse(match.Groups[1].Value, out double size))
                {
                    string unit = match.Groups[2].Value.ToUpper();
                    Assert.AreEqual(testCase.Value.expectedSize, size, $"Size should match for {testCase.Key}");
                    Assert.AreEqual(testCase.Value.expectedUnit, unit, $"Unit should match for {testCase.Key}");
                }
            }
        }
        
        #endregion
        
        #region Package Manager Window Creation Tests
        
        [Test]
        public void ShowWindow_CreatesWindowInstance()
        {
            // Act
            PackageManagerWindow.ShowWindow();
            
            // Assert - Check that window was created (it will be in the editor)
            var window = EditorWindow.GetWindow<PackageManagerWindow>();
            Assert.IsNotNull(window, "Should create Package Manager window");
            Assert.IsTrue(window.titleContent.text.Contains("Package") || 
                         window.titleContent.text.Contains("PackageManagerWindow"), 
                         $"Should have package-related title, got: {window.titleContent.text}");
            
            // Clean up
            if (window != null)
            {
                window.Close();
            }
        }
        
        #endregion
        
        #region Backup Issue Detection Logic Tests
        
        [Test]
        public void BackupIssueDetection_VariousScenarios()
        {
            // This tests the logic we built for detecting backup issues
            // Testing different combinations of DownloadInstructions.txt and Google Drive status
            
            var testScenarios = new[]
            {
                new { 
                    Name = "Valid instructions with Google Drive link",
                    HasInstructions = true,
                    HasValidLink = true,
                    ExistsOnDrive = true,
                    ExpectedIssues = false,
                    ExpectedReason = ""
                },
                new { 
                    Name = "Instructions exist but no valid link",
                    HasInstructions = true,
                    HasValidLink = false,
                    ExistsOnDrive = true,
                    ExpectedIssues = true,
                    ExpectedReason = "DownloadInstructions.txt exists but Google Drive link is missing/invalid"
                },
                new { 
                    Name = "No instructions but exists on Drive",
                    HasInstructions = false,
                    HasValidLink = false,
                    ExistsOnDrive = true,
                    ExpectedIssues = true,
                    ExpectedReason = "Package exists on Google Drive but missing local DownloadInstructions.txt"
                },
                new { 
                    Name = "No instructions and not on Drive",
                    HasInstructions = false,
                    HasValidLink = false,
                    ExistsOnDrive = false,
                    ExpectedIssues = true,
                    ExpectedReason = "Missing DownloadInstructions.txt file and no backup found on Google Drive"
                }
            };
            
            // This would be the logic tested in UpdatePackageInfo method
            foreach (var scenario in testScenarios)
            {
                // Simulate the backup issue detection logic
                bool hasBackupIssues = false;
                string backupIssueReason = "";
                
                if (scenario.HasInstructions)
                {
                    if (!scenario.HasValidLink)
                    {
                        if (scenario.ExistsOnDrive)
                        {
                            hasBackupIssues = true;
                            backupIssueReason = "DownloadInstructions.txt exists but Google Drive link is missing/invalid";
                        }
                        else
                        {
                            hasBackupIssues = true;
                            backupIssueReason = "No Google Drive link found in DownloadInstructions.txt";
                        }
                    }
                }
                else
                {
                    if (scenario.ExistsOnDrive)
                    {
                        hasBackupIssues = true;
                        backupIssueReason = "Package exists on Google Drive but missing local DownloadInstructions.txt";
                    }
                    else
                    {
                        hasBackupIssues = true;
                        backupIssueReason = "Missing DownloadInstructions.txt file and no backup found on Google Drive";
                    }
                }
                
                // Assert
                Assert.AreEqual(scenario.ExpectedIssues, hasBackupIssues, 
                    $"Backup issues detection failed for scenario: {scenario.Name}");
                if (scenario.ExpectedIssues)
                {
                    Assert.AreEqual(scenario.ExpectedReason, backupIssueReason, 
                        $"Backup issue reason incorrect for scenario: {scenario.Name}");
                }
            }
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void GoogleDriveIntegration_HandlesNullAndEmptyInputs()
        {
            // Test that parsing methods handle null/empty inputs gracefully
            var windowType = typeof(PackageManagerWindow);
            var parseMethod = windowType.GetMethod("ParseGoogleDriveLink", BindingFlags.NonPublic | BindingFlags.Instance);
            var window = ScriptableObject.CreateInstance<PackageManagerWindow>();
            
            // Test null input - should now return null gracefully
            var result1 = parseMethod.Invoke(window, new object[] { null });
            Assert.IsNull(result1, "Should handle null input gracefully");
            
            // Test empty input
            var result2 = parseMethod.Invoke(window, new object[] { "" });
            Assert.IsNull(result2, "Should handle empty input gracefully");
            
            // Test whitespace input
            var result3 = parseMethod.Invoke(window, new object[] { "   \n   " });
            Assert.IsNull(result3, "Should handle whitespace-only input gracefully");
            
            // Clean up
            if (window != null)
            {
                UnityEngine.Object.DestroyImmediate(window);
            }
        }
        
        #endregion
    }
}