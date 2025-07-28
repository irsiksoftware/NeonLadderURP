# 📚 NeonLadder Documentation Hub

*Consolidated documentation for the NeonLadder Marvel Squad development team*

## 📁 Documentation Structure

### 🔧 **technical-specifications/**
Technical documentation created by development team personas

- **[procedural-generation-system.md](technical-specifications/procedural-generation-system.md)** - *By @stephen-strange*  
  Comprehensive guide to the Mystical PathGenerator system for roguelite level generation

### 💼 **business-analysis/**
Business intelligence and project analysis reports

- **[acquisition-audit-report.md](business-analysis/acquisition-audit-report.md)** - *By @nick-fury*  
  Complete technical acquisition audit with valuation and risk assessment

### 🛠️ **integrations/**
System integration guides and status reports

- **[integration-status-summary.md](../integrations/integration-status-summary.md)** - Current status of all external integrations
- **[google-drive-analysis.md](../integrations/google-drive-analysis.md)** - Google Drive CLI integration setup
- **[claude-gpt-collaboration-plan.md](../integrations/claude-gpt-collaboration-plan.md)** - AI collaboration workflows

### 📋 **backlogs/**
Product backlog items and sprint planning

- **[product-backlog.json](../backlogs/product-backlog.json)** - Complete product backlog with story points
- **[sprint-1-steam-readiness.json](../backlogs/sprint-1-steam-readiness.json)** - Steam launch readiness sprint
- **[dialog-system-pbi.json](../backlog/dialog-system-pbi.json)** - Disco Elysium dialog system epic

### 🧠 **team-memory/**
Marvel Squad persona memory and context

- **[shared-context.json](../team-memory/shared-context.json)** - Shared team knowledge and decisions
- **[technical-decisions.json](../team-memory/technical-decisions.json)** - Architecture decision records
- **[personas/](../team-memory/personas/)** - Individual persona memory files

## 🎯 Quick Access Links

### **For Developers**
- [Procedural Generation Implementation](technical-specifications/procedural-generation-system.md#-implementation-guide)
- [Testing Infrastructure](technical-specifications/procedural-generation-system.md#-testing--verification)
- [Controller Mapping Tests](../../Assets/Tests/Runtime/ControllerMappingTests.cs)

### **For Product Managers**
- [Business Valuation Report](business-analysis/acquisition-audit-report.md)
- [Current Sprint Status](../backlogs/sprint-1-steam-readiness.json)
- [Revenue Projections](business-analysis/acquisition-audit-report.md#executive-summary)

### **For QA Engineers**
- [CLI Test Execution Guide](../../CLAUDE.md#cli-testing--automation)
- [Unit Test Coverage](technical-specifications/procedural-generation-system.md#unit-tests-integrated-with-neonladder-test-suite)
- [Platform Compatibility Testing](../../Assets/Tests/Runtime/ControllerMappingTests.cs)

### **For Marvel Squad AI Team**
- [Persona Memory Files](../team-memory/personas/)
- [Team Collaboration Protocols](../team-memory/conflict-resolution-protocols.json)
- [Active Sprint Planning](../team-memory/active-sprints.json)

## 📊 Documentation Standards

### **File Naming Convention**
- Technical specs: `kebab-case-system-name.md`
- Business docs: `kebab-case-report-name.md`
- JSON configs: `kebab-case-config.json`

### **Persona Attribution**
All documentation should include author attribution:
```markdown
*By @persona-name*
```

### **Cross-References**
Use relative paths for internal links:
```markdown
[Link Text](../folder/filename.md)
[Section Link](filename.md#section-header)
```

## 🔄 Update Process

1. **Create/Update**: Add new documentation in appropriate folder
2. **Index**: Update this README.md with new entries
3. **Cross-Link**: Add relevant links to other documentation
4. **Team Memory**: Update persona memory files if relevant

## 🎮 Integration with Main README

The main project [README.md](../../README.md) contains:
- Project overview and setup instructions
- Marvel Squad persona system explanation
- Current sprint status and todos
- Development workflow guides

This documentation hub provides detailed technical and business documentation referenced from the main README.

---

*"Documentation is the foundation upon which great software is built."*  
**- The NeonLadder Marvel Squad Development Team**