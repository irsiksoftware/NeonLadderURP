# Unity Package Export System

## Overview
Cross-platform automated system for exporting Unity packages and uploading to Google Drive. Works on Windows, macOS, Linux, and CI/CD environments.

## Features
- ✅ **Cross-platform**: Works on Windows, macOS, and Linux
- ✅ **CI/CD Ready**: GitHub Actions workflow included
- ✅ **Batch Processing**: Export multiple packages at once
- ✅ **Smart Detection**: Automatically finds packages with DownloadInstructions.txt
- ✅ **Google Drive Integration**: Upload and generate shareable links
- ✅ **Progress Tracking**: Detailed logging and manifest generation
- ✅ **Safety Checks**: Verifies Unity isn't running before export

## Quick Start

### macOS/Linux
```bash
cd Scripts
./export-packages.sh --dry-run  # Test what would be exported
./export-packages.sh              # Export all packages
```

### Windows (Command Prompt)
```cmd
cd Scripts
export-packages.bat --dry-run    # Test what would be exported
export-packages.bat               # Export all packages
```

### Windows (PowerShell)
```powershell
.\Scripts\Export-UnityPackages.ps1 -DryRun  # Test what would be exported
.\Scripts\Export-UnityPackages.ps1           # Export all packages
```

### Python (All Platforms)
```bash
python3 Scripts/export_unity_packages.py --dry-run  # Test
python3 Scripts/export_unity_packages.py            # Export
```

## Command Line Options

| Option | Description |
|--------|-------------|
| `--dry-run` | Show what would be exported without actually doing it |
| `--skip-upload` | Export packages but skip Google Drive upload |
| `--packages NAME1 NAME2` | Export specific packages only |
| `--unity-path PATH` | Specify Unity installation path (auto-detected by default) |
| `--project-path PATH` | Specify project path (default: current directory) |

## Examples

### Export specific packages
```bash
# Shell/Bash
./export-packages.sh --packages "HeroEditor" "LeartesStudios/CyberpunkCity"

# PowerShell
.\Export-UnityPackages.ps1 -Packages "HeroEditor", "LeartesStudios/CyberpunkCity"
```

### Export without uploading
```bash
./export-packages.sh --skip-upload
```

### Test run (show what would be exported)
```bash
./export-packages.sh --dry-run
```

## Package Discovery

The system automatically discovers packages by:
1. Scanning `Assets/Packages/` directory
2. Finding directories with `DownloadInstructions.txt`
3. Including subdirectories (e.g., LeartesStudios/CyberpunkCity)

## Output Structure

```
NeonLadderURP/
├── PackageExports/
│   ├── HeroEditor.unitypackage
│   ├── LeartesStudios_CyberpunkCity.unitypackage
│   ├── manifest_20250809_143022.json
│   └── export_*.log
```

## Manifest File

Each export generates a JSON manifest with:
- Export date and time
- Unity version used
- Platform information
- List of exported packages with sizes
- Total export size

## CI/CD Integration

### GitHub Actions
The workflow runs:
- **Manual trigger**: Via GitHub Actions UI
- **Weekly**: Sundays at 2 AM UTC
- **On push**: When package files change

```yaml
# Trigger manually with specific packages
workflow_dispatch:
  inputs:
    packages: "HeroEditor ModernUI"
```

### Self-Hosted Runners
For self-hosted runners, ensure:
1. Unity is pre-installed at expected paths
2. Python 3.6+ is available
3. Git LFS is configured for large files

## Unity Installation Paths

The system checks these default locations:

### Windows
- `C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe`
- `C:\Program Files\Unity\Hub\Editor\6000.0.37f1\Editor\Unity.exe`

### macOS
- `/Applications/Unity/Hub/Editor/6000.0.26f1/Unity.app`
- `/Applications/Unity/Hub/Editor/6000.0.37f1/Unity.app`

### Linux
- `/opt/Unity/Editor/Unity`
- `/home/runner/Unity/Hub/Editor/6000.0.26f1/Editor/Unity`

### Custom Path
Set environment variable: `UNITY_PATH=/path/to/unity`

## Configuration

Settings stored in `.claude/package_export_config.json`:

```json
{
  "packages_to_export": ["HeroEditor", "ModernUI"],
  "exclude_patterns": ["*.meta", ".git"],
  "google_drive_folder_id": "folder_id_here",
  "last_export": "2025-08-09T14:30:22"
}
```

## Troubleshooting

### Unity is running
- **Error**: "Unity is currently running"
- **Solution**: Close Unity before exporting

### Unity not found
- **Error**: "Unity installation not found"
- **Solution**: Set `UNITY_PATH` environment variable or use `--unity-path`

### Python not found
- **Error**: "Python is not installed"
- **Solution**: Install Python 3.6+ and add to PATH

### Export timeout
- **Error**: "Export timed out"
- **Solution**: Large packages may need more time. Check logs in `PackageExports/`

### Permission denied (macOS/Linux)
- **Error**: "Permission denied"
- **Solution**: `chmod +x Scripts/export-packages.sh`

## Complete Package Automation System

### Quick Start - Main Command
```bash
./Scripts/package-automation.sh [command] [options]
```

### Available Commands

| Command | Description | Example |
|---------|-------------|------|
| `download` | Download packages from Google Drive | `./Scripts/package-automation.sh download` |
| `export` | Export Unity packages | `./Scripts/package-automation.sh export --dry-run` |
| `upload` | Upload to Google Drive | `./Scripts/package-automation.sh upload` |
| `sync` | Complete workflow (export + upload) | `./Scripts/package-automation.sh sync` |
| `list` | Generate markdown package list | `./Scripts/package-automation.sh list` |
| `verify` | Verify downloaded packages | `./Scripts/package-automation.sh verify` |

## Download Packages (No Authentication Required)

### Download All Packages
```bash
# Download all 35+ packages with Google Drive links
./Scripts/package-automation.sh download
```

### Download Specific Packages
```bash
# Download specific packages by name
./Scripts/package-automation.sh download --packages "HeroEditor" "Synty" "Modern UI Pack"

# Download LeartesStudios packages
./Scripts/package-automation.sh download --packages "LeartesStudios/Cyberpunk City"
```

### Direct Python Usage
```bash
# Download with progress tracking
python3 Scripts/download_unity_packages.py

# Verify existing downloads
python3 Scripts/download_unity_packages.py --verify-only
```

## Google Drive Integration

### Setup (First Time)
```bash
# macOS/Linux
brew install gdrive
gdrive account add

# Follow browser authentication
```

### Upload Workflow
1. Export packages: `./Scripts/package-automation.sh export`
2. Upload to Drive: `./Scripts/package-automation.sh upload`
3. Links auto-update in DownloadInstructions.txt
4. Generate list: `./Scripts/package-automation.sh list`

### Complete Sync
```bash
# Full workflow: export → upload → update links
./Scripts/package-automation.sh sync
```

## Performance

### Download Performance (Tested)
- **5Gbps Fiber**: Downloads limited by Google Drive (~10-50 Mbps)
- **Small packages (<10MB)**: 1-2 seconds
- **Medium packages (10-100MB)**: 5-30 seconds
- **Large packages (>100MB)**: 30-120 seconds

### Export Times (With Unity)
- Small package (<10MB): 30-60 seconds
- Medium package (10-100MB): 1-3 minutes
- Large package (>100MB): 3-10 minutes

### Package Statistics
- **Total Packages**: 35+ with Google Drive links
- **Categories**: LeartesStudios (20+), Third Party (15+)
- **Total Size**: Varies by package selection

## Security Notes

- Unity must be closed during export (prevents file locks)
- Exported packages include all dependencies
- Google Drive links are public (anyone with link can view)
- Consider private sharing for sensitive packages

## Development

### Adding New Package
1. Create directory in `Assets/Packages/`
2. Add `DownloadInstructions.txt`
3. Run export script

### Modifying Export Behavior
Edit `Scripts/export_unity_packages.py`:
- `discover_packages()`: Change package detection
- `export_package()`: Modify Unity export settings
- `GoogleDriveUploader`: Implement actual upload logic

## Support

For issues or improvements:
1. Check logs in `PackageExports/`
2. Run with `--dry-run` to test
3. Create GitHub issue with:
   - Platform (Windows/macOS/Linux)
   - Unity version
   - Error messages
   - Export logs