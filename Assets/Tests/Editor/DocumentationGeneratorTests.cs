using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using NeonLadder.Editor.Documentation;
using System;
using System.Linq;

namespace NeonLadder.Tests.Editor
{
    /// <summary>
    /// Unit tests for the Documentation Generator system
    /// Tests API doc generation, menu scanning, architecture analysis, and quality reporting
    /// </summary>
    [TestFixture]
    public class DocumentationGeneratorTests
    {
        private DocumentationGenerator generator;
        private string testOutputPath;
        
        [SetUp]
        public void Setup()
        {
            generator = new DocumentationGenerator();
            testOutputPath = Path.Combine(Application.dataPath, "..", "Documentation");
            
            // Ensure documentation directory exists
            if (!Directory.Exists(testOutputPath))
            {
                Directory.CreateDirectory(testOutputPath);
            }
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up is optional - keep generated docs for review
        }
        
        [Test]
        public void DocumentationGenerator_CanBeInstantiated()
        {
            // Arrange & Act
            var testGenerator = new DocumentationGenerator();
            
            // Assert
            Assert.IsNotNull(testGenerator, "DocumentationGenerator should be instantiated");
        }
        
        [Test]
        public void GenerateAPIDocumentation_CreatesOutputDirectory()
        {
            // Arrange
            string apiPath = Path.Combine(testOutputPath, "API");
            
            // Act
            generator.GenerateAPIDocumentation();
            
            // Assert
            Assert.IsTrue(Directory.Exists(apiPath), "API documentation directory should be created");
        }
        
        [Test]
        public void GenerateAPIDocumentation_CreatesIndexFile()
        {
            // Act
            generator.GenerateAPIDocumentation();
            
            // Assert
            string indexPath = Path.Combine(testOutputPath, "API", "README.md");
            Assert.IsTrue(File.Exists(indexPath), "API index file should be created");
            
            if (File.Exists(indexPath))
            {
                string content = File.ReadAllText(indexPath);
                Assert.IsTrue(content.Contains("NeonLadder API Documentation"), 
                    "Index should contain API documentation title");
                Assert.IsTrue(content.Contains("Table of Contents"), 
                    "Index should contain table of contents");
            }
        }
        
        [Test]
        public void GenerateMenuDocumentation_CreatesMenuFiles()
        {
            // Act
            generator.GenerateMenuDocumentation();
            
            // Assert
            string menuPath = Path.Combine(testOutputPath, "Menus");
            Assert.IsTrue(Directory.Exists(menuPath), "Menu documentation directory should be created");
            
            string hierarchyPath = Path.Combine(menuPath, "MenuHierarchy.md");
            Assert.IsTrue(File.Exists(hierarchyPath), "Menu hierarchy file should be created");
            
            string shortcutsPath = Path.Combine(menuPath, "KeyboardShortcuts.md");
            Assert.IsTrue(File.Exists(shortcutsPath), "Keyboard shortcuts file should be created");
        }
        
        [Test]
        public void GenerateMenuDocumentation_FindsNeonLadderMenus()
        {
            // Act
            generator.GenerateMenuDocumentation();
            
            // Assert
            string hierarchyPath = Path.Combine(testOutputPath, "Menus", "MenuHierarchy.md");
            if (File.Exists(hierarchyPath))
            {
                string content = File.ReadAllText(hierarchyPath);
                Assert.IsTrue(content.Contains("NeonLadder"), 
                    "Menu documentation should contain NeonLadder menus");
                Assert.IsTrue(content.Contains("Menu Hierarchy"), 
                    "Should contain menu hierarchy section");
            }
        }
        
        [Test]
        public void GenerateArchitectureDocumentation_CreatesArchitectureFiles()
        {
            // Act
            generator.GenerateArchitectureDocumentation();
            
            // Assert
            string archPath = Path.Combine(testOutputPath, "Architecture");
            Assert.IsTrue(Directory.Exists(archPath), "Architecture documentation directory should be created");
            
            string managerPath = Path.Combine(archPath, "ManagerArchitecture.md");
            Assert.IsTrue(File.Exists(managerPath), "Manager architecture file should be created");
            
            string soPath = Path.Combine(archPath, "ScriptableObjects.md");
            Assert.IsTrue(File.Exists(soPath), "ScriptableObjects documentation should be created");
            
            string depsPath = Path.Combine(archPath, "ComponentDependencies.md");
            Assert.IsTrue(File.Exists(depsPath), "Component dependencies file should be created");
        }
        
        [Test]
        public void GenerateArchitectureDocumentation_IncludesManagerClasses()
        {
            // Act
            generator.GenerateArchitectureDocumentation();
            
            // Assert
            string managerPath = Path.Combine(testOutputPath, "Architecture", "ManagerArchitecture.md");
            if (File.Exists(managerPath))
            {
                string content = File.ReadAllText(managerPath);
                Assert.IsTrue(content.Contains("Manager"), 
                    "Architecture docs should reference Manager classes");
                Assert.IsTrue(content.Contains("graph TD") || content.Contains("Manager System Architecture"), 
                    "Should contain architecture diagram or description");
            }
        }
        
        [Test]
        public void GenerateCodeQualityReport_CreatesReportFile()
        {
            // Act
            generator.GenerateCodeQualityReport();
            
            // Assert
            string reportsPath = Path.Combine(testOutputPath, "Reports");
            Assert.IsTrue(Directory.Exists(reportsPath), "Reports directory should be created");
            
            var reportFiles = Directory.GetFiles(reportsPath, "QualityReport_*.md");
            Assert.Greater(reportFiles.Length, 0, "At least one quality report should be created");
            
            if (reportFiles.Length > 0)
            {
                string content = File.ReadAllText(reportFiles[0]);
                Assert.IsTrue(content.Contains("Code Quality Report"), 
                    "Report should contain quality report title");
                Assert.IsTrue(content.Contains("Documentation Coverage"), 
                    "Report should contain documentation coverage section");
                Assert.IsTrue(content.Contains("Recommendations"), 
                    "Report should contain recommendations section");
            }
        }
        
        [Test]
        public void MenuItemAttributes_AreProperlyConfigured()
        {
            // Arrange
            var generatorType = typeof(DocumentationGenerator);
            var methods = generatorType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            
            // Act
            var menuMethods = methods.Where(m => m.GetCustomAttribute<MenuItem>() != null).ToList();
            
            // Assert
            Assert.Greater(menuMethods.Count, 0, "Should have at least one menu item method");
            
            // Check specific menu items exist
            Assert.IsTrue(menuMethods.Any(m => m.Name == "GenerateAPIDocs"), 
                "Should have GenerateAPIDocs menu method");
            Assert.IsTrue(menuMethods.Any(m => m.Name == "GenerateMenuMap"), 
                "Should have GenerateMenuMap menu method");
            Assert.IsTrue(menuMethods.Any(m => m.Name == "GenerateArchitectureDocs"), 
                "Should have GenerateArchitectureDocs menu method");
            Assert.IsTrue(menuMethods.Any(m => m.Name == "GenerateQualityReport"), 
                "Should have GenerateQualityReport menu method");
            Assert.IsTrue(menuMethods.Any(m => m.Name == "GenerateAllDocumentation"), 
                "Should have GenerateAllDocumentation menu method");
        }
        
        [Test]
        public void DocumentationPaths_UseCorrectStructure()
        {
            // Arrange
            var expectedFolders = new[] { "API", "Menus", "Architecture", "Reports" };
            
            // Act
            // Paths are created when methods are called, so we check the constants
            var generatorType = typeof(DocumentationGenerator);
            var apiFolder = generatorType.GetField("API_DOCS_FOLDER", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as string;
            var menuFolder = generatorType.GetField("MENU_DOCS_FOLDER", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as string;
            var archFolder = generatorType.GetField("ARCHITECTURE_FOLDER", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as string;
            var reportsFolder = generatorType.GetField("REPORTS_FOLDER", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as string;
            
            // Assert
            Assert.AreEqual("API", apiFolder, "API folder constant should be 'API'");
            Assert.AreEqual("Menus", menuFolder, "Menu folder constant should be 'Menus'");
            Assert.AreEqual("Architecture", archFolder, "Architecture folder constant should be 'Architecture'");
            Assert.AreEqual("Reports", reportsFolder, "Reports folder constant should be 'Reports'");
        }
        
        [Test]
        public void GenerateCompleteDocumentation_CreatesAllSections()
        {
            // Note: This test might take longer due to comprehensive generation
            // Act
            generator.GenerateCompleteDocumentation();
            
            // Assert
            Assert.IsTrue(Directory.Exists(Path.Combine(testOutputPath, "API")), 
                "API directory should exist after complete generation");
            Assert.IsTrue(Directory.Exists(Path.Combine(testOutputPath, "Menus")), 
                "Menus directory should exist after complete generation");
            Assert.IsTrue(Directory.Exists(Path.Combine(testOutputPath, "Architecture")), 
                "Architecture directory should exist after complete generation");
            Assert.IsTrue(Directory.Exists(Path.Combine(testOutputPath, "Reports")), 
                "Reports directory should exist after complete generation");
            
            // Check for master index
            string masterIndexPath = Path.Combine(testOutputPath, "README.md");
            Assert.IsTrue(File.Exists(masterIndexPath), "Master index should be created");
            
            if (File.Exists(masterIndexPath))
            {
                string content = File.ReadAllText(masterIndexPath);
                Assert.IsTrue(content.Contains("NeonLadder Project Documentation"), 
                    "Master index should contain project title");
                Assert.IsTrue(content.Contains("API Documentation"), 
                    "Master index should link to API docs");
                Assert.IsTrue(content.Contains("Menu Structure"), 
                    "Master index should link to menu docs");
                Assert.IsTrue(content.Contains("Architecture"), 
                    "Master index should link to architecture docs");
                Assert.IsTrue(content.Contains("Quality Reports"), 
                    "Master index should link to quality reports");
            }
        }
        
        [Test]
        public void MarkdownGeneration_ProducesValidMarkdown()
        {
            // Act
            generator.GenerateAPIDocumentation();
            
            // Assert
            string indexPath = Path.Combine(testOutputPath, "API", "README.md");
            if (File.Exists(indexPath))
            {
                string content = File.ReadAllText(indexPath);
                
                // Check for valid markdown elements
                Assert.IsTrue(content.Contains("#"), "Should contain markdown headers");
                Assert.IsTrue(content.Contains("##") || content.Contains("###"), 
                    "Should contain subsection headers");
                
                // Check for markdown formatting
                Assert.IsTrue(content.Contains("**") || content.Contains("`"), 
                    "Should use markdown formatting (bold or code)");
            }
        }
        
        [Test]
        public void TypeAnalysis_IdentifiesPublicAPIs()
        {
            // This test verifies that the generator can identify and analyze types
            // Act
            generator.GenerateAPIDocumentation();
            
            // Assert
            var apiFiles = Directory.GetFiles(Path.Combine(testOutputPath, "API"), "*.md");
            Assert.Greater(apiFiles.Length, 1, "Should generate documentation for multiple types (more than just index)");
            
            // Check that at least one type documentation was created
            var typeFiles = apiFiles.Where(f => !f.EndsWith("README.md")).ToArray();
            if (typeFiles.Length > 0)
            {
                string content = File.ReadAllText(typeFiles[0]);
                Assert.IsTrue(content.Contains("Namespace:"), "Type documentation should include namespace");
                Assert.IsTrue(content.Contains("Type:"), "Type documentation should include type kind");
            }
        }
    }
}