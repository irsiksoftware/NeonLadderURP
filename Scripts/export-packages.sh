#!/bin/bash
# Cross-platform Unity package export script
# Works on macOS, Linux, and Git Bash on Windows

set -e

# Get the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"

# Colors for output (works on all platforms)
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}Unity Package Export Tool${NC}"
echo "================================"

# Check Python installation
if ! command -v python3 &> /dev/null; then
    if command -v python &> /dev/null; then
        PYTHON_CMD="python"
    else
        echo -e "${RED}Error: Python is not installed${NC}"
        echo "Please install Python 3.6+ to continue"
        exit 1
    fi
else
    PYTHON_CMD="python3"
fi

# Verify Python version
PYTHON_VERSION=$($PYTHON_CMD -c 'import sys; print(".".join(map(str, sys.version_info[:2])))')
echo "Using Python $PYTHON_VERSION"

# Check if Unity is running
check_unity_running() {
    case "$(uname -s)" in
        Darwin*)
            pgrep -f Unity > /dev/null 2>&1
            ;;
        Linux*)
            pgrep -f Unity > /dev/null 2>&1
            ;;
        MINGW*|MSYS*|CYGWIN*|Windows_NT)
            tasklist | grep -i unity.exe > /dev/null 2>&1
            ;;
        *)
            return 1
            ;;
    esac
}

if check_unity_running; then
    echo -e "${YELLOW}Warning: Unity appears to be running${NC}"
    echo "Please close Unity before exporting packages"
    read -p "Press Enter to continue anyway, or Ctrl+C to cancel..."
fi

# Run the Python export script
cd "$PROJECT_ROOT"

# Parse command line arguments
ARGS=""
DRY_RUN=""
SKIP_UPLOAD=""

while [[ $# -gt 0 ]]; do
    case $1 in
        --dry-run)
            DRY_RUN="--dry-run"
            echo -e "${YELLOW}Running in dry-run mode (no actual exports)${NC}"
            shift
            ;;
        --skip-upload)
            SKIP_UPLOAD="--skip-upload"
            echo -e "${YELLOW}Skipping Google Drive upload${NC}"
            shift
            ;;
        --packages)
            shift
            PACKAGES="--packages"
            while [[ $# -gt 0 ]] && [[ ! "$1" =~ ^-- ]]; do
                PACKAGES="$PACKAGES $1"
                shift
            done
            ARGS="$ARGS $PACKAGES"
            ;;
        *)
            ARGS="$ARGS $1"
            shift
            ;;
    esac
done

# Execute the Python script
echo "Starting package export..."
$PYTHON_CMD "$SCRIPT_DIR/export_unity_packages.py" \
    --project-path "$PROJECT_ROOT" \
    $DRY_RUN \
    $SKIP_UPLOAD \
    $ARGS

EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}Export completed successfully!${NC}"
else
    echo -e "${RED}Export failed with code $EXIT_CODE${NC}"
fi

exit $EXIT_CODE