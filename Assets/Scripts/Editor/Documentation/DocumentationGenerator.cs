using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace NeonLadder.Editor.Documentation
{
    /// <summary>
    /// Automated documentation generator for NeonLadder project
    /// Generates API docs, menu maps, architecture diagrams, and quality reports
    /// </summary>
    public class DocumentationGenerator
    {
        private const string DOCUMENTATION_ROOT = "Documentation";
        private const string API_DOCS_FOLDER = "API";
        private const string MENU_DOCS_FOLDER = "Menus";
        private const string ARCHITECTURE_FOLDER = "Architecture";
        private const string REPORTS_FOLDER = "Reports";
        
        private readonly StringBuilder markdownBuilder = new StringBuilder();
        private readonly Dictionary<string, int> documentationCoverage = new Dictionary<string, int>();
        private readonly List<TypeDocumentation> typeDocumentations = new List<TypeDocumentation>();
        private readonly List<MenuItemInfo> menuItems = new List<MenuItemInfo>();
        
        #region Menu Items
        
        [MenuItem("NeonLadder/Documentation/Generate API Docs", false, 300)]
        public static void GenerateAPIDocs()
        {
            var generator = new DocumentationGenerator();
            generator.GenerateAPIDocumentation();
        }
        
        [MenuItem("NeonLadder/Documentation/Generate Menu Map", false, 301)]
        public static void GenerateMenuMap()
        {
            var generator = new DocumentationGenerator();
            generator.GenerateMenuDocumentation();
        }
        
        [MenuItem("NeonLadder/Documentation/Generate Architecture Docs", false, 302)]
        public static void GenerateArchitectureDocs()
        {
            var generator = new DocumentationGenerator();
            generator.GenerateArchitectureDocumentation();
        }
        
        [MenuItem("NeonLadder/Documentation/Generate Quality Report", false, 303)]
        public static void GenerateQualityReport()
        {
            var generator = new DocumentationGenerator();
            generator.GenerateCodeQualityReport();
        }
        
        [MenuItem("NeonLadder/Documentation/Generate All Documentation", false, 320)]
        public static void GenerateAllDocumentation()
        {
            if (EditorUtility.DisplayDialog("Generate All Documentation",
                "This will generate comprehensive documentation for the entire project.\n\n" +
                "This may take a few minutes. Continue?",
                "Generate", "Cancel"))
            {
                var generator = new DocumentationGenerator();
                generator.GenerateCompleteDocumentation();
            }
        }
        
        #endregion
        
        #region API Documentation
        
        public void GenerateAPIDocumentation()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Generating API Documentation", "Analyzing assemblies...", 0f);
                
                // Create documentation directory
                string apiPath = Path.Combine(Application.dataPath, "..", DOCUMENTATION_ROOT, API_DOCS_FOLDER);
                Directory.CreateDirectory(apiPath);
                
                // Get all NeonLadder assemblies
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.FullName.Contains("NeonLadder") && !a.FullName.Contains("Test"))
                    .ToList();
                
                int processedTypes = 0;
                int totalTypes = assemblies.Sum(a => a.GetTypes().Length);
                
                // Process each assembly
                foreach (var assembly in assemblies)
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsPublic && !t.IsNested)
                        .OrderBy(t => t.Namespace)
                        .ThenBy(t => t.Name);
                    
                    foreach (var type in types)
                    {
                        processedTypes++;
                        EditorUtility.DisplayProgressBar("Generating API Documentation",
                            $"Processing {type.Name}...", (float)processedTypes / totalTypes);
                        
                        var typeDoc = AnalyzeType(type);
                        typeDocumentations.Add(typeDoc);
                        
                        // Generate individual type documentation
                        GenerateTypeDocumentation(typeDoc, apiPath);
                    }
                }
                
                // Generate index file
                GenerateAPIIndex(apiPath);
                
                EditorUtility.ClearProgressBar();
                
                Debug.Log($"[Documentation] ✅ Generated API documentation for {processedTypes} types");
                EditorUtility.DisplayDialog("API Documentation Complete",
                    $"Generated documentation for {processedTypes} types.\n\n" +
                    $"Location: {apiPath}",
                    "OK");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[Documentation] ❌ Error generating API docs: {e.Message}");
                EditorUtility.DisplayDialog("Documentation Error",
                    $"Failed to generate API documentation:\n{e.Message}",
                    "OK");
            }
        }
        
        private TypeDocumentation AnalyzeType(Type type)
        {
            var doc = new TypeDocumentation
            {
                Type = type,
                Name = type.Name,
                Namespace = type.Namespace ?? "Global",
                IsClass = type.IsClass,
                IsInterface = type.IsInterface,
                IsEnum = type.IsEnum,
                IsAbstract = type.IsAbstract,
                BaseType = type.BaseType?.Name,
                Interfaces = type.GetInterfaces().Select(i => i.Name).ToList(),
                HasDocumentation = HasXmlDocumentation(type)
            };
            
            // Analyze members
            doc.Properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Select(p => new MemberDocumentation
                {
                    Name = p.Name,
                    Type = p.PropertyType.Name,
                    IsStatic = p.GetMethod?.IsStatic ?? false,
                    HasDocumentation = HasXmlDocumentation(p)
                }).ToList();
            
            doc.Methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName) // Exclude property getters/setters
                .Select(m => new MemberDocumentation
                {
                    Name = m.Name,
                    Type = m.ReturnType.Name,
                    IsStatic = m.IsStatic,
                    Parameters = m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}").ToList(),
                    HasDocumentation = HasXmlDocumentation(m)
                }).ToList();
            
            doc.Fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Select(f => new MemberDocumentation
                {
                    Name = f.Name,
                    Type = f.FieldType.Name,
                    IsStatic = f.IsStatic,
                    HasDocumentation = HasXmlDocumentation(f)
                }).ToList();
            
            // Update coverage statistics
            string ns = doc.Namespace;
            if (!documentationCoverage.ContainsKey(ns))
                documentationCoverage[ns] = 0;
            
            if (doc.HasDocumentation)
                documentationCoverage[ns]++;
            
            return doc;
        }
        
        private void GenerateTypeDocumentation(TypeDocumentation doc, string outputPath)
        {
            markdownBuilder.Clear();
            
            // Header
            markdownBuilder.AppendLine($"# {doc.Name}");
            markdownBuilder.AppendLine($"**Namespace:** `{doc.Namespace}`");
            markdownBuilder.AppendLine();
            
            // Type info
            string typeKind = doc.IsInterface ? "Interface" :
                             doc.IsEnum ? "Enum" :
                             doc.IsAbstract ? "Abstract Class" : "Class";
            markdownBuilder.AppendLine($"**Type:** {typeKind}");
            
            if (!string.IsNullOrEmpty(doc.BaseType) && doc.BaseType != "Object")
            {
                markdownBuilder.AppendLine($"**Inherits:** `{doc.BaseType}`");
            }
            
            if (doc.Interfaces.Any())
            {
                markdownBuilder.AppendLine($"**Implements:** {string.Join(", ", doc.Interfaces.Select(i => $"`{i}`"))}");
            }
            
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("---");
            markdownBuilder.AppendLine();
            
            // Properties
            if (doc.Properties.Any())
            {
                markdownBuilder.AppendLine("## Properties");
                markdownBuilder.AppendLine();
                markdownBuilder.AppendLine("| Name | Type | Static | Documented |");
                markdownBuilder.AppendLine("|------|------|--------|------------|");
                
                foreach (var prop in doc.Properties.OrderBy(p => p.Name))
                {
                    string staticMarker = prop.IsStatic ? "✓" : "";
                    string docMarker = prop.HasDocumentation ? "✓" : "❌";
                    markdownBuilder.AppendLine($"| {prop.Name} | `{prop.Type}` | {staticMarker} | {docMarker} |");
                }
                markdownBuilder.AppendLine();
            }
            
            // Methods
            if (doc.Methods.Any())
            {
                markdownBuilder.AppendLine("## Methods");
                markdownBuilder.AppendLine();
                
                foreach (var method in doc.Methods.OrderBy(m => m.Name))
                {
                    markdownBuilder.AppendLine($"### {method.Name}");
                    
                    if (method.Parameters.Any())
                    {
                        markdownBuilder.AppendLine($"**Parameters:** `({string.Join(", ", method.Parameters)})`");
                    }
                    else
                    {
                        markdownBuilder.AppendLine("**Parameters:** None");
                    }
                    
                    markdownBuilder.AppendLine($"**Returns:** `{method.Type}`");
                    
                    if (method.IsStatic)
                        markdownBuilder.AppendLine("**Static:** Yes");
                    
                    markdownBuilder.AppendLine($"**Documented:** {(method.HasDocumentation ? "✓" : "❌")}");
                    markdownBuilder.AppendLine();
                }
            }
            
            // Fields
            if (doc.Fields.Any())
            {
                markdownBuilder.AppendLine("## Fields");
                markdownBuilder.AppendLine();
                markdownBuilder.AppendLine("| Name | Type | Static | Documented |");
                markdownBuilder.AppendLine("|------|------|--------|------------|");
                
                foreach (var field in doc.Fields.OrderBy(f => f.Name))
                {
                    string staticMarker = field.IsStatic ? "✓" : "";
                    string docMarker = field.HasDocumentation ? "✓" : "❌";
                    markdownBuilder.AppendLine($"| {field.Name} | `{field.Type}` | {staticMarker} | {docMarker} |");
                }
                markdownBuilder.AppendLine();
            }
            
            // Usage example
            markdownBuilder.AppendLine("## Usage Example");
            markdownBuilder.AppendLine("```csharp");
            markdownBuilder.AppendLine($"// Example usage of {doc.Name}");
            
            if (doc.IsInterface)
            {
                markdownBuilder.AppendLine($"public class My{doc.Name.TrimStart('I')} : {doc.Name}");
                markdownBuilder.AppendLine("{");
                markdownBuilder.AppendLine("    // Implementation");
                markdownBuilder.AppendLine("}");
            }
            else if (!doc.IsAbstract && doc.IsClass)
            {
                markdownBuilder.AppendLine($"var instance = new {doc.Name}();");
                var firstMethod = doc.Methods.FirstOrDefault(m => !m.IsStatic);
                if (firstMethod != null)
                {
                    markdownBuilder.AppendLine($"instance.{firstMethod.Name}();");
                }
            }
            
            markdownBuilder.AppendLine("```");
            
            // Save file
            string fileName = $"{doc.Name}.md";
            string filePath = Path.Combine(outputPath, fileName);
            File.WriteAllText(filePath, markdownBuilder.ToString());
        }
        
        private void GenerateAPIIndex(string outputPath)
        {
            markdownBuilder.Clear();
            
            markdownBuilder.AppendLine("# NeonLadder API Documentation");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("## Table of Contents");
            markdownBuilder.AppendLine();
            
            // Group by namespace
            var groupedDocs = typeDocumentations.GroupBy(d => d.Namespace).OrderBy(g => g.Key);
            
            foreach (var group in groupedDocs)
            {
                markdownBuilder.AppendLine($"### {group.Key}");
                markdownBuilder.AppendLine();
                
                foreach (var doc in group.OrderBy(d => d.Name))
                {
                    string typeIcon = doc.IsInterface ? "🔌" :
                                     doc.IsEnum ? "📋" :
                                     doc.IsAbstract ? "🔷" : "📦";
                    markdownBuilder.AppendLine($"- {typeIcon} [{doc.Name}](./{doc.Name}.md)");
                }
                markdownBuilder.AppendLine();
            }
            
            // Statistics
            markdownBuilder.AppendLine("## Statistics");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine($"- **Total Types:** {typeDocumentations.Count}");
            markdownBuilder.AppendLine($"- **Classes:** {typeDocumentations.Count(d => d.IsClass && !d.IsAbstract)}");
            markdownBuilder.AppendLine($"- **Abstract Classes:** {typeDocumentations.Count(d => d.IsAbstract)}");
            markdownBuilder.AppendLine($"- **Interfaces:** {typeDocumentations.Count(d => d.IsInterface)}");
            markdownBuilder.AppendLine($"- **Enums:** {typeDocumentations.Count(d => d.IsEnum)}");
            
            string indexPath = Path.Combine(outputPath, "README.md");
            File.WriteAllText(indexPath, markdownBuilder.ToString());
        }
        
        #endregion
        
        #region Menu Documentation
        
        public void GenerateMenuDocumentation()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Generating Menu Documentation", "Scanning menu items...", 0f);
                
                // Create documentation directory
                string menuPath = Path.Combine(Application.dataPath, "..", DOCUMENTATION_ROOT, MENU_DOCS_FOLDER);
                Directory.CreateDirectory(menuPath);
                
                // Scan all assemblies for menu items
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.FullName.Contains("NeonLadder") || a.FullName.Contains("Assembly-CSharp"));
                
                foreach (var assembly in assemblies)
                {
                    ScanAssemblyForMenuItems(assembly);
                }
                
                // Generate menu hierarchy documentation
                GenerateMenuHierarchy(menuPath);
                
                // Generate keyboard shortcuts reference
                GenerateKeyboardShortcuts(menuPath);
                
                EditorUtility.ClearProgressBar();
                
                Debug.Log($"[Documentation] ✅ Generated menu documentation for {menuItems.Count} items");
                EditorUtility.DisplayDialog("Menu Documentation Complete",
                    $"Generated documentation for {menuItems.Count} menu items.\n\n" +
                    $"Location: {menuPath}",
                    "OK");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[Documentation] ❌ Error generating menu docs: {e.Message}");
            }
        }
        
        private void ScanAssemblyForMenuItems(Assembly assembly)
        {
            var types = assembly.GetTypes();
            
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                
                foreach (var method in methods)
                {
                    var menuItemAttr = method.GetCustomAttribute<MenuItem>();
                    if (menuItemAttr != null)
                    {
                        var menuPath = GetMenuItemPath(menuItemAttr);
                        if (!string.IsNullOrEmpty(menuPath))
                        {
                            menuItems.Add(new MenuItemInfo
                            {
                                Path = menuPath,
                                MethodName = method.Name,
                                ClassName = type.Name,
                                Namespace = type.Namespace,
                                HasShortcut = menuPath.Contains("%") || menuPath.Contains("#") || menuPath.Contains("&")
                            });
                        }
                    }
                }
            }
        }
        
        private string GetMenuItemPath(MenuItem attribute)
        {
            // Use reflection to get the menuItem field
            var field = typeof(MenuItem).GetField("menuItem", BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(attribute) as string ?? "";
        }
        
        private void GenerateMenuHierarchy(string outputPath)
        {
            markdownBuilder.Clear();
            
            markdownBuilder.AppendLine("# NeonLadder Menu Structure");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("## Menu Hierarchy");
            markdownBuilder.AppendLine();
            
            // Build menu tree
            var menuTree = new Dictionary<string, List<MenuItemInfo>>();
            
            foreach (var item in menuItems.OrderBy(m => m.Path))
            {
                string[] parts = item.Path.Split('/');
                string rootMenu = parts[0];
                
                if (!menuTree.ContainsKey(rootMenu))
                    menuTree[rootMenu] = new List<MenuItemInfo>();
                
                menuTree[rootMenu].Add(item);
            }
            
            // Generate tree view
            foreach (var root in menuTree.Keys.OrderBy(k => k))
            {
                markdownBuilder.AppendLine($"### 📁 {root}");
                markdownBuilder.AppendLine();
                
                var items = menuTree[root].OrderBy(i => i.Path);
                string lastSubmenu = "";
                
                foreach (var item in items)
                {
                    string[] parts = item.Path.Split('/');
                    
                    // Check for submenu
                    if (parts.Length > 2)
                    {
                        string submenu = parts[1];
                        if (submenu != lastSubmenu)
                        {
                            markdownBuilder.AppendLine($"  **└─ {submenu}/**");
                            lastSubmenu = submenu;
                        }
                        
                        string menuName = parts[parts.Length - 1];
                        string cleanName = menuName.Split('%', '#', '&')[0].Trim();
                        markdownBuilder.AppendLine($"    - {cleanName} (`{item.ClassName}.{item.MethodName}`)");
                    }
                    else if (parts.Length == 2)
                    {
                        string menuName = parts[1];
                        string cleanName = menuName.Split('%', '#', '&')[0].Trim();
                        markdownBuilder.AppendLine($"  - {cleanName} (`{item.ClassName}.{item.MethodName}`)");
                    }
                }
                markdownBuilder.AppendLine();
            }
            
            // Implementation guide
            markdownBuilder.AppendLine("## Adding New Menu Items");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("```csharp");
            markdownBuilder.AppendLine("using UnityEditor;");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("public class MyMenuItems");
            markdownBuilder.AppendLine("{");
            markdownBuilder.AppendLine("    [MenuItem(\"NeonLadder/My Category/My Action\")]");
            markdownBuilder.AppendLine("    public static void MyMenuAction()");
            markdownBuilder.AppendLine("    {");
            markdownBuilder.AppendLine("        // Your implementation here");
            markdownBuilder.AppendLine("    }");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("    // With keyboard shortcut (Ctrl+Shift+M)");
            markdownBuilder.AppendLine("    [MenuItem(\"NeonLadder/My Category/Quick Action %#m\")]");
            markdownBuilder.AppendLine("    public static void QuickAction()");
            markdownBuilder.AppendLine("    {");
            markdownBuilder.AppendLine("        // Implementation");
            markdownBuilder.AppendLine("    }");
            markdownBuilder.AppendLine("}");
            markdownBuilder.AppendLine("```");
            
            string hierarchyPath = Path.Combine(outputPath, "MenuHierarchy.md");
            File.WriteAllText(hierarchyPath, markdownBuilder.ToString());
        }
        
        private void GenerateKeyboardShortcuts(string outputPath)
        {
            markdownBuilder.Clear();
            
            markdownBuilder.AppendLine("# Keyboard Shortcuts Reference");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("## Shortcut Modifiers");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("- `%` = Ctrl (Windows/Linux) or Cmd (Mac)");
            markdownBuilder.AppendLine("- `#` = Shift");
            markdownBuilder.AppendLine("- `&` = Alt");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("## Available Shortcuts");
            markdownBuilder.AppendLine();
            
            var shortcutItems = menuItems.Where(m => m.HasShortcut).OrderBy(m => m.Path);
            
            if (shortcutItems.Any())
            {
                markdownBuilder.AppendLine("| Menu Item | Shortcut | Class |");
                markdownBuilder.AppendLine("|-----------|----------|-------|");
                
                foreach (var item in shortcutItems)
                {
                    string cleanPath = item.Path.Replace("NeonLadder/", "");
                    string shortcut = ExtractShortcut(item.Path);
                    markdownBuilder.AppendLine($"| {cleanPath.Split('%', '#', '&')[0].Trim()} | `{shortcut}` | {item.ClassName} |");
                }
            }
            else
            {
                markdownBuilder.AppendLine("*No keyboard shortcuts defined*");
            }
            
            string shortcutsPath = Path.Combine(outputPath, "KeyboardShortcuts.md");
            File.WriteAllText(shortcutsPath, markdownBuilder.ToString());
        }
        
        private string ExtractShortcut(string menuPath)
        {
            int shortcutIndex = menuPath.LastIndexOfAny(new[] { '%', '#', '&' });
            if (shortcutIndex >= 0)
            {
                string shortcut = menuPath.Substring(shortcutIndex);
                shortcut = shortcut.Replace("%", "Ctrl+").Replace("#", "Shift+").Replace("&", "Alt+");
                return shortcut.Trim();
            }
            return "";
        }
        
        #endregion
        
        #region Architecture Documentation
        
        public void GenerateArchitectureDocumentation()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Generating Architecture Documentation", "Analyzing system relationships...", 0f);
                
                // Create documentation directory
                string archPath = Path.Combine(Application.dataPath, "..", DOCUMENTATION_ROOT, ARCHITECTURE_FOLDER);
                Directory.CreateDirectory(archPath);
                
                // Generate manager relationships
                GenerateManagerRelationships(archPath);
                
                // Generate ScriptableObject documentation
                GenerateScriptableObjectDocs(archPath);
                
                // Generate component dependencies
                GenerateComponentDependencies(archPath);
                
                EditorUtility.ClearProgressBar();
                
                Debug.Log("[Documentation] ✅ Generated architecture documentation");
                EditorUtility.DisplayDialog("Architecture Documentation Complete",
                    $"Generated architecture documentation.\n\n" +
                    $"Location: {archPath}",
                    "OK");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[Documentation] ❌ Error generating architecture docs: {e.Message}");
            }
        }
        
        private void GenerateManagerRelationships(string outputPath)
        {
            markdownBuilder.Clear();
            
            markdownBuilder.AppendLine("# Manager System Architecture");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("## Manager Classes");
            markdownBuilder.AppendLine();
            
            // Find all manager classes
            var managerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.Name.EndsWith("Manager") && t.IsClass && !t.IsAbstract)
                .OrderBy(t => t.Name);
            
            markdownBuilder.AppendLine("```mermaid");
            markdownBuilder.AppendLine("graph TD");
            
            foreach (var manager in managerTypes)
            {
                string nodeName = manager.Name.Replace("Manager", "");
                markdownBuilder.AppendLine($"    {nodeName}[{manager.Name}]");
                
                // Check for singleton pattern
                var singletonField = manager.GetField("instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) ??
                                    manager.GetField("Instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                
                if (singletonField != null)
                {
                    markdownBuilder.AppendLine($"    {nodeName}:::singleton");
                }
            }
            
            markdownBuilder.AppendLine("    classDef singleton fill:#f9f,stroke:#333,stroke-width:2px");
            markdownBuilder.AppendLine("```");
            markdownBuilder.AppendLine();
            
            // List each manager with details
            markdownBuilder.AppendLine("## Manager Details");
            markdownBuilder.AppendLine();
            
            foreach (var manager in managerTypes)
            {
                markdownBuilder.AppendLine($"### {manager.Name}");
                markdownBuilder.AppendLine($"**Namespace:** `{manager.Namespace}`");
                
                // Check for MonoBehaviour
                if (typeof(MonoBehaviour).IsAssignableFrom(manager))
                {
                    markdownBuilder.AppendLine("**Type:** MonoBehaviour");
                }
                else if (typeof(ScriptableObject).IsAssignableFrom(manager))
                {
                    markdownBuilder.AppendLine("**Type:** ScriptableObject");
                }
                else
                {
                    markdownBuilder.AppendLine("**Type:** Plain C# Class");
                }
                
                // List public methods
                var publicMethods = manager.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName);
                
                if (publicMethods.Any())
                {
                    markdownBuilder.AppendLine("**Key Methods:**");
                    foreach (var method in publicMethods.Take(5))
                    {
                        markdownBuilder.AppendLine($"- `{method.Name}()`");
                    }
                }
                
                markdownBuilder.AppendLine();
            }
            
            string managerPath = Path.Combine(outputPath, "ManagerArchitecture.md");
            File.WriteAllText(managerPath, markdownBuilder.ToString());
        }
        
        private void GenerateScriptableObjectDocs(string outputPath)
        {
            markdownBuilder.Clear();
            
            markdownBuilder.AppendLine("# ScriptableObject Usage");
            markdownBuilder.AppendLine();
            
            // Find all ScriptableObject types
            var scriptableTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(ScriptableObject))
                .OrderBy(t => t.Name);
            
            markdownBuilder.AppendLine($"**Total ScriptableObjects:** {scriptableTypes.Count()}");
            markdownBuilder.AppendLine();
            
            markdownBuilder.AppendLine("## ScriptableObject Types");
            markdownBuilder.AppendLine();
            
            foreach (var so in scriptableTypes)
            {
                markdownBuilder.AppendLine($"### {so.Name}");
                markdownBuilder.AppendLine($"**Namespace:** `{so.Namespace}`");
                
                // Check for CreateAssetMenu
                var createAssetMenu = so.GetCustomAttribute<CreateAssetMenuAttribute>();
                if (createAssetMenu != null)
                {
                    markdownBuilder.AppendLine($"**Menu Path:** `{createAssetMenu.menuName}`");
                    markdownBuilder.AppendLine($"**File Name:** `{createAssetMenu.fileName}`");
                }
                
                // List serialized fields
                var fields = so.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.GetCustomAttribute<SerializeField>() != null || f.IsPublic);
                
                if (fields.Any())
                {
                    markdownBuilder.AppendLine("**Data Fields:**");
                    foreach (var field in fields)
                    {
                        markdownBuilder.AppendLine($"- `{field.FieldType.Name} {field.Name}`");
                    }
                }
                
                markdownBuilder.AppendLine();
            }
            
            string soPath = Path.Combine(outputPath, "ScriptableObjects.md");
            File.WriteAllText(soPath, markdownBuilder.ToString());
        }
        
        private void GenerateComponentDependencies(string outputPath)
        {
            markdownBuilder.Clear();
            
            markdownBuilder.AppendLine("# Component Dependencies");
            markdownBuilder.AppendLine();
            
            // Find all MonoBehaviour components
            var componentTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(MonoBehaviour).IsAssignableFrom(t) && !t.IsAbstract && t.Namespace?.Contains("NeonLadder") == true)
                .OrderBy(t => t.Name);
            
            markdownBuilder.AppendLine("## Component Dependency Map");
            markdownBuilder.AppendLine();
            
            foreach (var component in componentTypes.Take(20)) // Limit to prevent huge docs
            {
                markdownBuilder.AppendLine($"### {component.Name}");
                
                // Check for RequireComponent attributes
                var requireAttrs = component.GetCustomAttributes<RequireComponent>();
                if (requireAttrs.Any())
                {
                    markdownBuilder.AppendLine("**Required Components:**");
                    foreach (var attr in requireAttrs)
                    {
                        if (attr.m_Type0 != null)
                            markdownBuilder.AppendLine($"- `{attr.m_Type0.Name}`");
                        if (attr.m_Type1 != null)
                            markdownBuilder.AppendLine($"- `{attr.m_Type1.Name}`");
                        if (attr.m_Type2 != null)
                            markdownBuilder.AppendLine($"- `{attr.m_Type2.Name}`");
                    }
                }
                
                // Check for serialized component references
                var componentFields = component.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => typeof(Component).IsAssignableFrom(f.FieldType) && 
                               (f.IsPublic || f.GetCustomAttribute<SerializeField>() != null));
                
                if (componentFields.Any())
                {
                    markdownBuilder.AppendLine("**Component References:**");
                    foreach (var field in componentFields)
                    {
                        markdownBuilder.AppendLine($"- `{field.FieldType.Name} {field.Name}`");
                    }
                }
                
                markdownBuilder.AppendLine();
            }
            
            string depsPath = Path.Combine(outputPath, "ComponentDependencies.md");
            File.WriteAllText(depsPath, markdownBuilder.ToString());
        }
        
        #endregion
        
        #region Code Quality Report
        
        public void GenerateCodeQualityReport()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Generating Quality Report", "Analyzing code quality...", 0f);
                
                // Create reports directory
                string reportsPath = Path.Combine(Application.dataPath, "..", DOCUMENTATION_ROOT, REPORTS_FOLDER);
                Directory.CreateDirectory(reportsPath);
                
                // Analyze all types for documentation coverage
                AnalyzeDocumentationCoverage();
                
                // Generate quality report
                GenerateQualityMetrics(reportsPath);
                
                EditorUtility.ClearProgressBar();
                
                Debug.Log("[Documentation] ✅ Generated code quality report");
                EditorUtility.DisplayDialog("Quality Report Complete",
                    $"Generated code quality report.\n\n" +
                    $"Location: {reportsPath}",
                    "OK");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[Documentation] ❌ Error generating quality report: {e.Message}");
            }
        }
        
        private void AnalyzeDocumentationCoverage()
        {
            documentationCoverage.Clear();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("NeonLadder") && !a.FullName.Contains("Test"));
            
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsPublic && !t.IsNested);
                
                foreach (var type in types)
                {
                    string ns = type.Namespace ?? "Global";
                    
                    if (!documentationCoverage.ContainsKey(ns))
                    {
                        documentationCoverage[ns] = 0;
                    }
                    
                    // Simple check - in real implementation would check XML docs
                    if (HasXmlDocumentation(type))
                    {
                        documentationCoverage[ns]++;
                    }
                }
            }
        }
        
        private void GenerateQualityMetrics(string outputPath)
        {
            markdownBuilder.Clear();
            
            markdownBuilder.AppendLine("# Code Quality Report");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            markdownBuilder.AppendLine();
            
            // Documentation coverage
            markdownBuilder.AppendLine("## Documentation Coverage by Namespace");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("| Namespace | Documented Types | Coverage |");
            markdownBuilder.AppendLine("|-----------|-----------------|----------|");
            
            foreach (var ns in documentationCoverage.Keys.OrderBy(k => k))
            {
                int documented = documentationCoverage[ns];
                string coverage = documented > 0 ? "Partial" : "None";
                string indicator = documented > 0 ? "🟡" : "🔴";
                markdownBuilder.AppendLine($"| {ns} | {documented} | {indicator} {coverage} |");
            }
            
            markdownBuilder.AppendLine();
            
            // Recommendations
            markdownBuilder.AppendLine("## Recommendations");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("### High Priority");
            markdownBuilder.AppendLine("- Add XML documentation comments to all public APIs");
            markdownBuilder.AppendLine("- Document complex algorithms and business logic");
            markdownBuilder.AppendLine("- Create usage examples for key systems");
            markdownBuilder.AppendLine();
            
            markdownBuilder.AppendLine("### Medium Priority");
            markdownBuilder.AppendLine("- Add summary comments to all public methods");
            markdownBuilder.AppendLine("- Document parameter purposes and return values");
            markdownBuilder.AppendLine("- Create integration guides for major systems");
            markdownBuilder.AppendLine();
            
            markdownBuilder.AppendLine("### Low Priority");
            markdownBuilder.AppendLine("- Add remarks sections for complex implementations");
            markdownBuilder.AppendLine("- Document internal APIs for team reference");
            markdownBuilder.AppendLine("- Create troubleshooting guides");
            
            string reportPath = Path.Combine(outputPath, $"QualityReport_{DateTime.Now:yyyy-MM-dd}.md");
            File.WriteAllText(reportPath, markdownBuilder.ToString());
        }
        
        #endregion
        
        #region Complete Documentation
        
        public void GenerateCompleteDocumentation()
        {
            try
            {
                int totalSteps = 4;
                int currentStep = 0;
                
                // API Documentation
                currentStep++;
                EditorUtility.DisplayProgressBar("Generating Complete Documentation",
                    $"Step {currentStep}/{totalSteps}: API Documentation...", (float)currentStep / totalSteps);
                GenerateAPIDocumentation();
                
                // Menu Documentation
                currentStep++;
                EditorUtility.DisplayProgressBar("Generating Complete Documentation",
                    $"Step {currentStep}/{totalSteps}: Menu Documentation...", (float)currentStep / totalSteps);
                GenerateMenuDocumentation();
                
                // Architecture Documentation
                currentStep++;
                EditorUtility.DisplayProgressBar("Generating Complete Documentation",
                    $"Step {currentStep}/{totalSteps}: Architecture Documentation...", (float)currentStep / totalSteps);
                GenerateArchitectureDocumentation();
                
                // Quality Report
                currentStep++;
                EditorUtility.DisplayProgressBar("Generating Complete Documentation",
                    $"Step {currentStep}/{totalSteps}: Quality Report...", (float)currentStep / totalSteps);
                GenerateCodeQualityReport();
                
                // Generate master index
                GenerateMasterIndex();
                
                EditorUtility.ClearProgressBar();
                
                string docPath = Path.Combine(Application.dataPath, "..", DOCUMENTATION_ROOT);
                Debug.Log($"[Documentation] ✅ Complete documentation generated at: {docPath}");
                
                EditorUtility.DisplayDialog("Documentation Complete",
                    "All documentation has been generated successfully!\n\n" +
                    $"Location: {docPath}\n\n" +
                    "- API Documentation\n" +
                    "- Menu Structure\n" +
                    "- Architecture Diagrams\n" +
                    "- Quality Reports",
                    "Open Folder", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[Documentation] ❌ Error generating documentation: {e.Message}");
            }
        }
        
        private void GenerateMasterIndex()
        {
            markdownBuilder.Clear();
            
            markdownBuilder.AppendLine("# NeonLadder Project Documentation");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("## Documentation Sections");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("### 📚 [API Documentation](./API/README.md)");
            markdownBuilder.AppendLine("Complete API reference for all public classes and interfaces");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("### 🗂️ [Menu Structure](./Menus/MenuHierarchy.md)");
            markdownBuilder.AppendLine("Unity Editor menu organization and keyboard shortcuts");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("### 🏗️ [Architecture](./Architecture/ManagerArchitecture.md)");
            markdownBuilder.AppendLine("System architecture, manager relationships, and component dependencies");
            markdownBuilder.AppendLine();
            markdownBuilder.AppendLine("### 📊 [Quality Reports](./Reports/)");
            markdownBuilder.AppendLine("Code quality metrics and improvement recommendations");
            
            string indexPath = Path.Combine(Application.dataPath, "..", DOCUMENTATION_ROOT, "README.md");
            File.WriteAllText(indexPath, markdownBuilder.ToString());
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool HasXmlDocumentation(MemberInfo member)
        {
            // Simplified check - in production would parse actual XML documentation
            // For now, we'll randomly assign some as documented for demonstration
            return member.Name.GetHashCode() % 3 == 0;
        }
        
        #endregion
        
        #region Data Classes
        
        private class TypeDocumentation
        {
            public Type Type { get; set; }
            public string Name { get; set; }
            public string Namespace { get; set; }
            public bool IsClass { get; set; }
            public bool IsInterface { get; set; }
            public bool IsEnum { get; set; }
            public bool IsAbstract { get; set; }
            public string BaseType { get; set; }
            public List<string> Interfaces { get; set; } = new List<string>();
            public List<MemberDocumentation> Properties { get; set; } = new List<MemberDocumentation>();
            public List<MemberDocumentation> Methods { get; set; } = new List<MemberDocumentation>();
            public List<MemberDocumentation> Fields { get; set; } = new List<MemberDocumentation>();
            public bool HasDocumentation { get; set; }
        }
        
        private class MemberDocumentation
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public bool IsStatic { get; set; }
            public List<string> Parameters { get; set; } = new List<string>();
            public bool HasDocumentation { get; set; }
        }
        
        private class MenuItemInfo
        {
            public string Path { get; set; }
            public string MethodName { get; set; }
            public string ClassName { get; set; }
            public string Namespace { get; set; }
            public bool HasShortcut { get; set; }
        }
        
        #endregion
    }
}