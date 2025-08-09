#!/bin/bash
# Complete Unity Package Automation Script
# Handles export, upload, and download workflows

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

show_help() {
    echo -e "${GREEN}Unity Package Automation System${NC}"
    echo "=================================="
    echo ""
    echo "Commands:"
    echo "  export    - Export Unity packages to .unitypackage files"
    echo "  upload    - Upload packages to Google Drive"  
    echo "  download  - Download packages from Google Drive"
    echo "  sync      - Complete sync workflow (export + upload)"
    echo "  list      - Generate markdown list of all packages"
    echo "  verify    - Verify downloaded packages"
    echo ""
    echo "Usage:"
    echo "  $0 export [--dry-run] [--packages NAME1 NAME2]"
    echo "  $0 upload [--packages NAME1 NAME2]"
    echo "  $0 download [--packages NAME1 NAME2]"
    echo "  $0 sync [--packages NAME1 NAME2]"
    echo "  $0 list"
    echo "  $0 verify"
    echo ""
    echo "Examples:"
    echo "  $0 download                    # Download all packages"
    echo "  $0 download --packages HeroEditor Synty"
    echo "  $0 sync                        # Full sync workflow"
    echo "  $0 export --dry-run            # Test export without running"
}

check_python() {
    if ! command -v python3 &> /dev/null; then
        echo -e "${RED}Error: Python 3 is required${NC}"
        exit 1
    fi
}

check_gdrive() {
    if ! command -v gdrive &> /dev/null; then
        echo -e "${YELLOW}Warning: gdrive not found${NC}"
        echo "Install with: brew install gdrive"
        echo "Then run: gdrive account add"
        return 1
    fi
    
    # Check if authenticated
    if ! gdrive account list | grep -q "@"; then
        echo -e "${YELLOW}Warning: gdrive not authenticated${NC}"
        echo "Run: gdrive account add"
        return 1
    fi
    
    return 0
}

export_packages() {
    echo -e "${BLUE}Exporting Unity packages...${NC}"
    check_python
    
    cd "$PROJECT_ROOT"
    python3 Scripts/export_unity_packages.py "$@"
}

upload_packages() {
    echo -e "${BLUE}Uploading packages to Google Drive...${NC}"
    check_python
    
    if ! check_gdrive; then
        echo -e "${RED}Cannot upload without gdrive authentication${NC}"
        exit 1
    fi
    
    cd "$PROJECT_ROOT"
    python3 Scripts/sync_unity_packages.py "$@"
}

download_packages() {
    echo -e "${BLUE}Downloading packages from Google Drive...${NC}"
    check_python
    
    cd "$PROJECT_ROOT"
    python3 Scripts/download_unity_packages.py "$@"
    
    echo -e "${GREEN}Downloads complete!${NC}"
    echo -e "${CYAN}Location: PackageDownloads/${NC}"
}

sync_packages() {
    echo -e "${BLUE}Starting full sync workflow...${NC}"
    check_python
    
    if ! check_gdrive; then
        echo -e "${YELLOW}Skipping upload step - gdrive not configured${NC}"
        export_packages "$@"
    else
        cd "$PROJECT_ROOT"
        python3 Scripts/sync_unity_packages.py "$@"
    fi
}

list_packages() {
    echo -e "${BLUE}Generating package list...${NC}"
    check_python
    
    cd "$PROJECT_ROOT"
    python3 Scripts/sync_unity_packages.py --list-only
    
    if [ -f "PACKAGE_DOWNLOADS.md" ]; then
        echo -e "${GREEN}Package list generated: PACKAGE_DOWNLOADS.md${NC}"
        echo ""
        echo "Preview:"
        head -20 PACKAGE_DOWNLOADS.md
    fi
}

verify_packages() {
    echo -e "${BLUE}Verifying downloaded packages...${NC}"
    check_python
    
    cd "$PROJECT_ROOT"
    python3 Scripts/download_unity_packages.py --verify-only
}

# Main command dispatch
case "$1" in
    export)
        shift
        export_packages "$@"
        ;;
    upload)
        shift
        upload_packages "$@"
        ;;
    download)
        shift
        download_packages "$@"
        ;;
    sync)
        shift
        sync_packages "$@"
        ;;
    list)
        list_packages
        ;;
    verify)
        verify_packages
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        if [ -z "$1" ]; then
            show_help
        else
            echo -e "${RED}Unknown command: $1${NC}"
            echo ""
            show_help
            exit 1
        fi
        ;;
esac