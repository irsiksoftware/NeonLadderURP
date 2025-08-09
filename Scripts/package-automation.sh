#!/bin/bash

# Unity Package Export and Google Drive Backup Automation
# For use with Claude Code and manual execution
# PBI-82: Automate Unity package export and Google Drive backup

set -e  # Exit on error

# Configuration
UNITY_PATH="/Applications/Unity/Hub/Editor/6000.0.26f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="/Users/cameronblair/Documents/Enderwork/NeonLadderURP"
PACKAGES_DIR="$PROJECT_PATH/Assets/Packages"
EXPORT_DIR="$PROJECT_PATH/ExportedPackages"
GDRIVE_FOLDER_ID="1PfhnfbV6jvi-eh46z4s3yzxnd0TfDpj8"  # NeonLadder Google Drive folder
LOG_FILE="$PROJECT_PATH/package-export.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
log_message() {
    echo -e "${GREEN}[$(date '+%Y-%m-%d %H:%M:%S')]${NC} $1" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1" | tee -a "$LOG_FILE"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1" | tee -a "$LOG_FILE"
}

check_unity_running() {
    if pgrep -x "Unity" > /dev/null; then
        log_error "Unity is currently running. Please close Unity before exporting packages."
        echo -e "${YELLOW}To close Unity, run:${NC} pkill -x Unity"
        exit 1
    fi
}

create_export_directory() {
    if [ ! -d "$EXPORT_DIR" ]; then
        mkdir -p "$EXPORT_DIR"
        log_message "Created export directory: $EXPORT_DIR"
    fi
}

# Function to export a single package
export_package() {
    local package_name="$1"
    local package_path="Assets/Packages/$package_name"
    local output_file="$EXPORT_DIR/${package_name// /_}.unitypackage"
    
    log_message "Exporting package: $package_name"
    
    # Create Unity export script
    cat > "$PROJECT_PATH/export_package.cs" << EOF
using UnityEngine;
using UnityEditor;
using System.IO;

public class PackageExporter
{
    static void ExportPackage()
    {
        string packagePath = "$package_path";
        string outputPath = "$output_file";
        
        if (!Directory.Exists(packagePath))
        {
            Debug.LogError($"Package directory not found: {packagePath}");
            EditorApplication.Exit(1);
            return;
        }
        
        // Get all assets in the package directory
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        System.Collections.Generic.List<string> packageAssets = new System.Collections.Generic.List<string>();
        
        foreach (string path in assetPaths)
        {
            if (path.StartsWith(packagePath))
            {
                packageAssets.Add(path);
            }
        }
        
        if (packageAssets.Count == 0)
        {
            Debug.LogError($"No assets found in package: {packagePath}");
            EditorApplication.Exit(1);
            return;
        }
        
        Debug.Log($"Exporting {packageAssets.Count} assets from {packagePath}");
        
        // Export the package
        AssetDatabase.ExportPackage(
            packageAssets.ToArray(),
            outputPath,
            ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
        );
        
        Debug.Log($"Package exported successfully to: {outputPath}");
        EditorApplication.Exit(0);
    }
}
EOF
    
    # Run Unity in batch mode to export the package
    "$UNITY_PATH" -batchmode \
        -projectPath "$PROJECT_PATH" \
        -executeMethod PackageExporter.ExportPackage \
        -logFile "$PROJECT_PATH/unity_export_${package_name// /_}.log" \
        -quit
    
    local exit_code=$?
    
    # Clean up temporary script
    rm -f "$PROJECT_PATH/export_package.cs"
    rm -f "$PROJECT_PATH/export_package.cs.meta"
    
    if [ $exit_code -eq 0 ] && [ -f "$output_file" ]; then
        local file_size=$(du -h "$output_file" | cut -f1)
        log_message "✓ Exported: $package_name ($file_size)"
        return 0
    else
        log_error "Failed to export: $package_name"
        return 1
    fi
}

# Function to upload to Google Drive
upload_to_gdrive() {
    local file_path="$1"
    local file_name=$(basename "$file_path")
    
    log_message "Uploading to Google Drive: $file_name"
    
    # Check if gdrive is available
    if ! command -v gdrive &> /dev/null; then
        log_error "gdrive CLI not found. Please install it first."
        return 1
    fi
    
    # Upload file and get the ID
    local upload_result=$(gdrive files upload --parent "$GDRIVE_FOLDER_ID" "$file_path" 2>&1)
    
    if echo "$upload_result" | grep -q "Uploaded"; then
        # Extract file ID from output
        local file_id=$(echo "$upload_result" | grep -oP 'Id: \K[a-zA-Z0-9_-]+' || echo "")
        
        if [ ! -z "$file_id" ]; then
            # Create shareable link
            gdrive permissions create "$file_id" --type anyone --role reader > /dev/null 2>&1
            local share_link="https://drive.google.com/file/d/$file_id/view?usp=sharing"
            
            log_message "✓ Uploaded: $file_name"
            log_message "  Share link: $share_link"
            echo "$file_name|$file_id|$share_link" >> "$EXPORT_DIR/upload_manifest.txt"
            return 0
        fi
    fi
    
    log_error "Failed to upload: $file_name"
    return 1
}

# Function to update DownloadInstructions.txt
update_download_instructions() {
    local package_name="$1"
    local gdrive_link="$2"
    local instructions_file="$PACKAGES_DIR/$package_name/DownloadInstructions.txt"
    
    if [ -f "$instructions_file" ]; then
        # Backup original
        cp "$instructions_file" "$instructions_file.backup"
        
        # Update with new Google Drive link
        cat > "$instructions_file" << EOF
# $package_name - Unity Package

## Download Link
Google Drive: $gdrive_link

## Installation Instructions
1. Download the .unitypackage file from the link above
2. In Unity, go to Assets > Import Package > Custom Package
3. Select the downloaded file
4. Click Import

## Notes
- Package exported on: $(date '+%Y-%m-%d')
- Unity version: 6000.0.26f1
- Original package preserved in Google Drive for backup

## Alternative Download
If the link above doesn't work, check the main project backup:
https://drive.google.com/drive/folders/$GDRIVE_FOLDER_ID
EOF
        
        log_message "✓ Updated DownloadInstructions.txt for: $package_name"
    fi
}

# Main execution modes
case "${1:-}" in
    "list")
        echo -e "${BLUE}Available packages to export:${NC}"
        echo "----------------------------------------"
        for dir in "$PACKAGES_DIR"/*; do
            if [ -d "$dir" ]; then
                package_name=$(basename "$dir")
                if [ -f "$dir/DownloadInstructions.txt" ]; then
                    echo "✓ $package_name"
                else
                    echo "  $package_name (no DownloadInstructions.txt)"
                fi
            fi
        done
        ;;
        
    "export")
        package_name="${2:-}"
        if [ -z "$package_name" ]; then
            log_error "Usage: $0 export <package_name>"
            exit 1
        fi
        
        check_unity_running
        create_export_directory
        
        if [ -d "$PACKAGES_DIR/$package_name" ]; then
            export_package "$package_name"
        else
            log_error "Package not found: $package_name"
            exit 1
        fi
        ;;
        
    "upload")
        package_name="${2:-}"
        if [ -z "$package_name" ]; then
            log_error "Usage: $0 upload <package_name>"
            exit 1
        fi
        
        package_file="$EXPORT_DIR/${package_name// /_}.unitypackage"
        if [ -f "$package_file" ]; then
            upload_to_gdrive "$package_file"
            
            # Get the share link from manifest
            if [ -f "$EXPORT_DIR/upload_manifest.txt" ]; then
                share_link=$(grep "^${package_name// /_}.unitypackage" "$EXPORT_DIR/upload_manifest.txt" | cut -d'|' -f3)
                if [ ! -z "$share_link" ]; then
                    update_download_instructions "$package_name" "$share_link"
                fi
            fi
        else
            log_error "Exported package not found: $package_file"
            exit 1
        fi
        ;;
        
    "export-all")
        check_unity_running
        create_export_directory
        
        log_message "Starting bulk export of all packages..."
        
        for dir in "$PACKAGES_DIR"/*; do
            if [ -d "$dir" ] && [ -f "$dir/DownloadInstructions.txt" ]; then
                package_name=$(basename "$dir")
                export_package "$package_name" || true  # Continue on error
            fi
        done
        
        log_message "Bulk export complete!"
        ;;
        
    "upload-all")
        if [ ! -d "$EXPORT_DIR" ]; then
            log_error "No exported packages found. Run 'export-all' first."
            exit 1
        fi
        
        log_message "Starting bulk upload to Google Drive..."
        
        for package_file in "$EXPORT_DIR"/*.unitypackage; do
            if [ -f "$package_file" ]; then
                upload_to_gdrive "$package_file" || true  # Continue on error
            fi
        done
        
        log_message "Bulk upload complete!"
        
        # Update all DownloadInstructions.txt files
        if [ -f "$EXPORT_DIR/upload_manifest.txt" ]; then
            while IFS='|' read -r filename file_id share_link; do
                package_name="${filename%.unitypackage}"
                package_name="${package_name//_/ }"
                update_download_instructions "$package_name" "$share_link"
            done < "$EXPORT_DIR/upload_manifest.txt"
        fi
        ;;
        
    "verify")
        log_message "Verifying package export setup..."
        
        # Check Unity installation
        if [ -f "$UNITY_PATH" ]; then
            echo -e "${GREEN}✓${NC} Unity found at: $UNITY_PATH"
        else
            echo -e "${RED}✗${NC} Unity not found at: $UNITY_PATH"
        fi
        
        # Check gdrive installation
        if command -v gdrive &> /dev/null; then
            echo -e "${GREEN}✓${NC} gdrive CLI installed"
            gdrive about 2>&1 | head -n 1
        else
            echo -e "${RED}✗${NC} gdrive CLI not installed"
        fi
        
        # Check packages directory
        if [ -d "$PACKAGES_DIR" ]; then
            package_count=$(find "$PACKAGES_DIR" -maxdepth 1 -type d | wc -l)
            echo -e "${GREEN}✓${NC} Packages directory found: $((package_count - 1)) packages"
        else
            echo -e "${RED}✗${NC} Packages directory not found"
        fi
        
        # Check export directory
        if [ -d "$EXPORT_DIR" ]; then
            export_count=$(find "$EXPORT_DIR" -name "*.unitypackage" | wc -l)
            echo -e "${GREEN}✓${NC} Export directory exists: $export_count exported packages"
        else
            echo -e "${YELLOW}!${NC} Export directory not yet created"
        fi
        ;;
        
    "download")
        # Download a specific package from Google Drive
        package_name="${2:-}"
        if [ -z "$package_name" ]; then
            log_error "Usage: $0 download <package_name>"
            exit 1
        fi
        
        instructions_file="$PACKAGES_DIR/$package_name/DownloadInstructions.txt"
        if [ -f "$instructions_file" ]; then
            gdrive_link=$(grep "Google Drive:" "$instructions_file" | cut -d' ' -f3)
            if [ ! -z "$gdrive_link" ]; then
                file_id=$(echo "$gdrive_link" | grep -oP '/d/\K[a-zA-Z0-9_-]+')
                if [ ! -z "$file_id" ]; then
                    log_message "Downloading package: $package_name"
                    gdrive files download "$file_id" --path "$EXPORT_DIR"
                else
                    log_error "Could not extract file ID from link"
                fi
            else
                log_error "No Google Drive link found in DownloadInstructions.txt"
            fi
        else
            log_error "DownloadInstructions.txt not found for: $package_name"
        fi
        ;;
        
    *)
        echo -e "${BLUE}Unity Package Automation Tool${NC}"
        echo "=============================="
        echo ""
        echo "Usage: $0 <command> [options]"
        echo ""
        echo "Commands:"
        echo "  list          - List all available packages"
        echo "  verify        - Verify setup and dependencies"
        echo "  export <pkg>  - Export a single package to .unitypackage"
        echo "  upload <pkg>  - Upload exported package to Google Drive"
        echo "  export-all    - Export all packages with DownloadInstructions.txt"
        echo "  upload-all    - Upload all exported packages to Google Drive"
        echo "  download <pkg>- Download package from Google Drive"
        echo ""
        echo "Examples:"
        echo "  $0 list"
        echo "  $0 export \"Modern UI Pack\""
        echo "  $0 upload \"Modern UI Pack\""
        echo "  $0 export-all"
        echo ""
        echo "Note: Unity must be closed before exporting packages"
        ;;
esac