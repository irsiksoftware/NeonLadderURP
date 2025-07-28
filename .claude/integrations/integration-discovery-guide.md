# Integration Discovery & Leverage Guide

## 🔍 **Discovery Process for External Tools**

### **Step 1: Tool Detection**
```bash
# Check if tool exists in common locations
where <tool-name>
"C:\tools\<tool-name>" --version
"C:\tools\<tool-name>" --help
```

### **Step 2: Authentication Verification**
```bash
# Check authentication status
<tool-name> about
<tool-name> whoami
<tool-name> status
```

### **Step 3: Basic Command Testing**
```bash
# Test read operations first (safe)
<tool-name> list
<tool-name> info
<tool-name> get
```

### **Step 4: Document Working Commands**
- Store successful commands in `.claude/integrations/<tool>-analysis.md`
- Include exact syntax, required parameters, common use cases
- Note any authentication requirements or setup steps

---

## 🛠️ **Successfully Integrated Tools**

### **✅ Google Drive (gdrive v3.9.1)**
**Location**: `C:\tools\gdrive`  
**Authentication**: ✅ Active (dakotairsik@gmail.com)  
**Key Commands**:
```bash
# File operations
"C:\tools\gdrive" files list --query "name contains 'keyword'" --max 5
"C:\tools\gdrive" files upload "local-file" --parent <folder-id>
"C:\tools\gdrive" permissions share <file-id> --role reader --type anyone

# Common IDs
# Main Folder: 19_CR6EWMjzs5u4o5_ch6UNddoN-LNf7z
```

### **✅ Unity CLI (6000.0.26f1)**
**Location**: `C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe`  
**Authentication**: N/A (Local install)  
**Key Commands**:
```bash
# Test execution
"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\Ender\NeonLadder" -executeMethod CLITestRunner.RunPlayModeTests

# Build operations
"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe" -batchmode -projectPath "path" -buildTarget StandaloneWindows64
```

### **✅ Git (v2.47.1)**
**Location**: System PATH  
**Authentication**: ✅ Configured  
**Key Commands**:
```bash
# Standard operations
git status
git add -A
git commit -m "message"
git push
```

### **✅ PowerShell**
**Location**: System PATH  
**Authentication**: N/A  
**Key Commands**:
```bash
# Process management
powershell -Command "Stop-Process -Name Unity -Force"
powershell -Command "Get-Process Unity -ErrorAction SilentlyContinue"

# Encoding operations
powershell -Command '[System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String("encoded_string"))'
```

---

## 🔍 **Tools to Investigate**

### **🔲 GitHub CLI (gh)**
**Expected Location**: `C:\Program Files\GitHub CLI\gh.exe`  
**Test Commands**:
```bash
# Check installation
"C:\Program Files\GitHub CLI\gh.exe" --version

# Check authentication
"C:\Program Files\GitHub CLI\gh.exe" auth status

# Basic operations
"C:\Program Files\GitHub CLI\gh.exe" repo list
```

### **🔲 Steam CMD**
**Expected Location**: `C:\steamcmd\steamcmd.exe`  
**Purpose**: Steam build deployment automation  
**Test Commands**:
```bash
# Check installation
"C:\steamcmd\steamcmd.exe" +quit

# Login test
"C:\steamcmd\steamcmd.exe" +login <username> +quit
```

### **🔲 Node.js/npm**
**Expected Location**: System PATH  
**Purpose**: Package management, build tools  
**Test Commands**:
```bash
npm --version
node --version
```

### **🔲 Python/pip**
**Expected Location**: System PATH  
**Purpose**: Automation scripts, data processing  
**Test Commands**:
```bash
python --version
pip --version
```

---

## 📋 **Integration Checklist Template**

### **For New Tool Discovery:**
- [ ] Check tool installation: `where <tool>` or `<tool> --version`
- [ ] Verify authentication: `<tool> about` or equivalent
- [ ] Test read operations: `<tool> list` or safe commands
- [ ] Document working syntax in `.claude/integrations/<tool>-analysis.md`
- [ ] Add to `.claude/settings.local.json` allow list if needed
- [ ] Update `CLAUDE.md` with usage examples
- [ ] Test integration with current workflow

### **Documentation Requirements:**
1. **Tool Location**: Exact path to executable
2. **Authentication Status**: How to check/configure
3. **Core Commands**: 3-5 most useful commands with exact syntax
4. **Common Use Cases**: Specific to NeonLadder project needs
5. **Folder IDs/Config**: Any persistent identifiers needed
6. **Error Handling**: Common issues and solutions

---

## 🎯 **Best Practices**

### **Command Documentation**
- Always include full paths in quotes for Windows compatibility
- Test commands before documenting them
- Include expected output examples when helpful
- Note any required parameters or environment setup

### **Authentication Management**
- Document where credentials are stored
- Include how to verify authentication status
- Note any refresh/renewal requirements

### **Error Prevention**
- Start with read-only operations for testing
- Use `--help` extensively to understand syntax
- Test in safe environments before production use

### **Future Claude Models**
- Store working commands in standardized format
- Include context about when/why commands are used
- Document any tool-specific quirks or limitations

---

## 📊 **Integration Status Matrix**

| Tool | Status | Auth | Location | Priority |
|------|--------|------|----------|----------|
| gdrive v3 | ✅ Active | ✅ Verified | `C:\tools\gdrive` | High |
| Unity CLI | ✅ Active | N/A | `C:\Program Files\Unity\Hub\Editor\...` | High |
| Git | ✅ Active | ✅ Configured | System PATH | High |
| PowerShell | ✅ Active | N/A | System PATH | High |
| GitHub CLI | 🔍 Unknown | 🔍 Unknown | `C:\Program Files\GitHub CLI\gh.exe` | Medium |
| Steam CMD | 🔍 Unknown | 🔍 Unknown | TBD | Medium |
| npm | 🔍 Unknown | N/A | System PATH | Low |
| Python | 🔍 Unknown | N/A | System PATH | Low |

**Legend:**
- ✅ Active: Tool verified and working
- 🔍 Unknown: Tool may be available, needs investigation
- ❌ Missing: Tool not available
- 🔄 Partial: Tool available but needs configuration