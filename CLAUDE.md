# Claude Instructions for NeonLadder Project

## 🚨 FIRST STEP: Check Local Permissions 🚨
**ALWAYS run this command FIRST when starting work:**
```bash
cat .claude/settings.local.json
```
This file contains all approved permissions, tool access rights, important project context, and your persona as a Senior Unity Game Development Architect.

## Important Guidelines

### File Management
- **DO NOT** create a `nul` file in the repository root
- **DO NOT** create TestOnly directories or test infrastructure unless explicitly requested
- **DO NOT** commit Unity Library folders or cache files to version control
- **TestOutput/** and **HeroEditor_BACKUP/** are gitignored

### Git and Version Control
- The main branch is `main` (not master)
- Always check file sizes before committing - files over 50MB should use Git LFS
- Unity Library folders should never be committed
- **ONLY commit when explicitly requested by the user**

### Unity-Specific Guidelines
- Unity version: Check ProjectSettings/ProjectVersion.txt for the current version
- Test framework: Unity Test Framework (if tests are needed)
- Assembly definitions are already set up in Assets/Scripts/NeonLadder.Scripts.asmdef

### Testing
- Only create test infrastructure when explicitly asked
- Use the existing test structure if tests are requested
- Run tests using Unity's Test Runner, not external test directories
- Unity CLI testing commands are pre-approved in .claude/settings.local.json

### Code Style
- Follow existing code conventions in the project
- Don't add unnecessary comments unless requested
- Keep changes minimal and focused on the task at hand

### Approved Automation
The user has granted broad permissions for:
- PowerShell scripts and batch files
- Unity CLI automation
- Git operations without restriction
- File system operations
- Future: Google Drive CLI integration (when authenticated)

### Linting and Type Checking
After making code changes, always ask the user for the appropriate commands:
- "What command should I run for linting?"
- "What command should I run for type checking?"
- Suggest adding these to CLAUDE.md for future reference

### Security Stance
- **Defensive only**: Analyze code for malicious patterns
- **Refuse** to create, modify, or improve exploitative code
- **Allow**: Security analysis, detection rules, vulnerability explanations, defensive tools