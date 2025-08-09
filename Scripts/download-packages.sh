#!/bin/bash
# Cross-platform Unity package download script
# Downloads packages from Google Drive links in DownloadInstructions.txt

set -e

# Get the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${GREEN}Unity Package Download Tool${NC}"
echo "================================"
echo -e "${BLUE}Using 5Gbps fiber connection for fast downloads!${NC}"

# Check Python installation
if ! command -v python3 &> /dev/null; then
    if command -v python &> /dev/null; then
        PYTHON_CMD="python"
    else
        echo -e "${RED}Error: Python is not installed${NC}"
        exit 1
    fi
else
    PYTHON_CMD="python3"
fi

# Create download directory
DOWNLOAD_DIR="$PROJECT_ROOT/PackageDownloads"
mkdir -p "$DOWNLOAD_DIR"

# Parse command line arguments
PACKAGES=""
VERIFY_ONLY=""

while [[ $# -gt 0 ]]; do
    case $1 in
        --verify-only)
            VERIFY_ONLY="--verify-only"
            echo -e "${YELLOW}Verify-only mode: checking existing downloads${NC}"
            shift
            ;;
        --packages)
            shift
            PACKAGES="--packages"
            while [[ $# -gt 0 ]] && [[ ! "$1" =~ ^-- ]]; do
                PACKAGES="$PACKAGES $1"
                shift
            done
            ;;
        --help)
            echo "Usage: $0 [options]"
            echo "Options:"
            echo "  --packages NAME1 NAME2  Download specific packages"
            echo "  --verify-only          Verify existing downloads"
            echo "  --help                 Show this help message"
            exit 0
            ;;
        *)
            shift
            ;;
    esac
done

# Run the Python download script
echo -e "${BLUE}Starting package download...${NC}"
echo "Download directory: $DOWNLOAD_DIR"
echo ""

$PYTHON_CMD "$SCRIPT_DIR/download_unity_packages.py" \
    --project-path "$PROJECT_ROOT" \
    $VERIFY_ONLY \
    $PACKAGES

EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}Download completed successfully!${NC}"
    echo -e "${BLUE}Check PackageDownloads/ directory for downloaded packages${NC}"
else
    echo -e "${RED}Download failed with code $EXIT_CODE${NC}"
fi

exit $EXIT_CODE