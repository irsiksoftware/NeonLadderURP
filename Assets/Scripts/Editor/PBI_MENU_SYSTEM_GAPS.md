# Product Backlog Items (PBIs) - NeonLadder Menu System Gaps

## Overview
This document identifies gaps between the documented menu structure and actual implementation, providing PBIs to complete the menu system.

## Current State Analysis

### ✅ Existing Menu Items

#### Top Menu Bar (MenuItem)
- ✅ `NeonLadder/Debug/*` - All 6 items implemented
- ✅ `NeonLadder/Upgrade System/Upgrade Designer` - Implemented
- ✅ `NeonLadder/Examples/Create Example Purchasable Items` - Implemented

#### Asset Creation (CreateAssetMenu)
- ✅ `NeonLadder/Debug/Logging System Config`
- ✅ `NeonLadder/Dialog/Dialog System Config`
- ✅ `NeonLadder/Documentation/Marvel Squad Persona`
- ✅ `NeonLadder/Items/Purchasable Item`
- ✅ `NeonLadder/Progression/Upgrade`
- ✅ `NeonLadder/Procedural Generation/Path Generator Config`
- ❌ `NeonLadder/Save System/*` - Inconsistent paths
- ❌ `NeonLadder/Saves/*` - Missing consolidation

## 🎯 Product Backlog Items

> **Note**: Using NL-MENU-XXX numbering to avoid conflicts with existing PBI numbers

### NL-MENU-001: Consolidate Save System Menu Paths
**Priority**: High  
**Story Points**: 2  
**Description**: Standardize save system asset creation menus under consistent path  

**Acceptance Criteria**:
- Change `NeonLadder/Save System/Save State Configuration` to `NeonLadder/Saves/Save State Config`
- Change `NeonLadder/Data Management/Save System Config` to `NeonLadder/Saves/Save System Config`
- Update documentation to reflect new paths

**Implementation Files**:
- `Assets/Scripts/DataManagement/SaveStateConfiguration.cs` - Update CreateAssetMenu path
- `Assets/Scripts/DataManagement/SaveSystemConfig.cs` - Update CreateAssetMenu path

**Current Code**:
```csharp
// SaveStateConfiguration.cs
[CreateAssetMenu(fileName = "New Save State Config", menuName = "NeonLadder/Save System/Save State Configuration")]

// SaveSystemConfig.cs  
[CreateAssetMenu(fileName = "Save System Config", menuName = "NeonLadder/Data Management/Save System Config")]
```

**Target Code**:
```csharp
// SaveStateConfiguration.cs
[CreateAssetMenu(fileName = "New Save State Config", menuName = "NeonLadder/Saves/Save State Config")]

// SaveSystemConfig.cs
[CreateAssetMenu(fileName = "Save System Config", menuName = "NeonLadder/Saves/Save System Config")]
```

---

### NL-MENU-002: Create Save System Editor Tools Menu
**Priority**: Medium  
**Story Points**: 5  
**Description**: Add editor tools for save system management similar to Debug and Upgrade systems

**Acceptance Criteria**:
- Create `NeonLadder/Save System/Save Manager` menu item
- Opens editor window for viewing/managing save files
- Includes options to backup, restore, and validate saves

**Implementation Plan**:
1. Create `Assets/Scripts/Editor/SaveSystem/SaveSystemEditor.cs`
2. Implement EditorWindow with save file browser
3. Add validation and backup functionality

**Example Structure**:
```csharp
namespace NeonLadder.Editor.SaveSystem
{
    public class SaveSystemEditor : EditorWindow
    {
        [MenuItem("NeonLadder/Save System/Save Manager")]
        public static void ShowWindow()
        {
            GetWindow<SaveSystemEditor>("Save Manager");
        }
    }
}
```

---

### NL-MENU-003: Create Dialog System Designer
**Priority**: Medium  
**Story Points**: 8  
**Description**: Add visual dialog designer similar to Upgrade Designer

**Acceptance Criteria**:
- Create `NeonLadder/Dialog System/Dialog Designer` menu item
- Visual node-based dialog editor
- Import/export dialog trees
- Preview dialog flow

**Implementation Files**:
- Create `Assets/Scripts/Editor/DialogSystem/DialogSystemEditor.cs`
- Integrate with existing `DialogSystemConfig.cs`

**Related Systems**:
- Dialog System Config already exists at `Assets/Scripts/Dialog/DialogSystemConfig.cs`
- Would benefit from visual editing tools

---

### NL-MENU-004: Create Procedural Generation Tools Menu
**Priority**: Low  
**Story Points**: 5  
**Description**: Add editor tools for procedural generation testing and visualization

**Acceptance Criteria**:
- Create `NeonLadder/Procedural/Path Visualizer` menu item
- Shows preview of generated paths
- Allows testing different seeds
- Exports path data for analysis

**Implementation Files**:
- Create `Assets/Scripts/Editor/ProceduralGeneration/PathGeneratorEditor.cs`
- Uses existing `PathGeneratorConfig.cs`

---

### NL-MENU-005: Create Performance Profiling Menu
**Priority**: Medium  
**Story Points**: 3  
**Description**: Add quick access to performance profiling tools

**Acceptance Criteria**:
- Create `NeonLadder/Performance/Profile Current Scene` menu item
- Create `NeonLadder/Performance/Clear Profiler Data` menu item
- Create `NeonLadder/Performance/Export Performance Report` menu item

**Implementation Files**:
- Create `Assets/Scripts/Editor/Performance/PerformanceMenuItems.cs`
- Integrate with existing `PerformanceProfiler.cs`

---

### NL-MENU-006: Create Build & Deploy Menu Category
**Priority**: High  
**Story Points**: 5  
**Description**: Centralize build and deployment operations

**Acceptance Criteria**:
- Create `NeonLadder/Build & Deploy/Build for Steam` menu item
- Create `NeonLadder/Build & Deploy/Build for itch.io` menu item
- Create `NeonLadder/Build & Deploy/Run All Tests` menu item

**Implementation Files**:
- Create `Assets/Scripts/Editor/BuildSystem/BuildMenuItems.cs`
- Integrate with Unity's BuildPipeline API

---

### NL-MENU-007: Add Documentation Generator
**Priority**: Low  
**Story Points**: 3  
**Description**: Auto-generate documentation from code

**Acceptance Criteria**:
- Create `NeonLadder/Documentation/Generate API Docs` menu item
- Create `NeonLadder/Documentation/Generate Menu Map` menu item
- Exports markdown documentation

**Implementation Files**:
- Create `Assets/Scripts/Editor/Documentation/DocumentationGenerator.cs`

---

### NL-MENU-008: Create Marvel Team Management Menu
**Priority**: Low  
**Story Points**: 2  
**Description**: Fun tools for Marvel persona system

**Acceptance Criteria**:
- Create `NeonLadder/Marvel Team/Assemble Team` menu item
- Create `NeonLadder/Marvel Team/Generate Team Report` menu item
- Shows current personas and their specialties

**Implementation Files**:
- Create `Assets/Scripts/Editor/MarvelTeam/MarvelTeamEditor.cs`
- Uses existing `MarvelSquadPersona.cs`

---

## 📊 Implementation Priority Matrix

| Priority | Effort | Items |
|----------|--------|------|
| High | Low | NL-MENU-001 (Save Path Consolidation) |
| High | Medium | NL-MENU-006 (Build & Deploy) |
| Medium | Low | NL-MENU-005 (Performance Menu) |
| Medium | Medium | NL-MENU-002 (Save Manager) |
| Medium | High | NL-MENU-003 (Dialog Designer) |
| Low | Low | NL-MENU-008 (Marvel Team) |
| Low | Medium | NL-MENU-004 (Procedural Tools), NL-MENU-007 (Documentation) |

## 🔧 Technical Notes

### Menu Organization Best Practices
1. Use consistent naming: "System" for runtime, "Designer/Manager" for tools
2. Group related items with priority ranges (100-199, 200-299, etc.)
3. Add validation methods for context-sensitive items
4. Include keyboard shortcuts for frequently used tools

### File Structure Pattern
```
Assets/Scripts/
├── Editor/
│   ├── [SystemName]/
│   │   ├── [SystemName]Editor.cs      # Main editor window
│   │   └── [SystemName]MenuItems.cs   # Additional menu items
│   └── README_NEONLADDER_MENU_SYSTEM.md
└── [Feature]/
    ├── [RuntimeClasses].cs
    └── Editor/
        └── [FeatureSpecificEditor].cs
```

### Integration Points
- **Logging System**: Fully integrated with menu system ✅
- **Upgrade System**: Has designer window ✅
- **Save System**: Needs editor tools 🔧
- **Dialog System**: Needs visual designer 🔧
- **Procedural Generation**: Needs visualization tools 🔧
- **Performance**: Needs quick access menus 🔧
- **Build System**: Needs centralized menu 🔧

## 🎯 Sprint Planning Recommendation

### Sprint 1 (Quick Wins)
- NL-MENU-001: Save Path Consolidation (2 SP)
- NL-MENU-005: Performance Menu (3 SP)
- **Total: 5 Story Points**

### Sprint 2 (Core Tools)
- NL-MENU-006: Build & Deploy Menu (5 SP)
- NL-MENU-002: Save Manager (5 SP)
- **Total: 10 Story Points**

### Sprint 3 (Advanced Tools)
- NL-MENU-003: Dialog Designer (8 SP)
- **Total: 8 Story Points**

### Backlog (Nice to Have)
- NL-MENU-004: Procedural Tools (5 SP)
- NL-MENU-007: Documentation Generator (3 SP)
- NL-MENU-008: Marvel Team Tools (2 SP)

---

*These PBIs will complete the NeonLadder menu system, providing comprehensive editor tools for all major systems.*