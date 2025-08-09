#!/bin/bash

# QA Build Creation Script for NeonLadder
# Creates testable builds with debug info for QA testing

set -e

# Configuration
UNITY_PATH="/Applications/Unity/Hub/Editor/6000.0.26f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="/Users/cameronblair/Documents/Enderwork/NeonLadderURP"
BUILD_PATH="$PROJECT_PATH/QA_Builds"
PR_NUMBER="${1:-}"
PLATFORM="${2:-Mac}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}=== NeonLadder QA Build Creator ===${NC}"
echo ""

# Check if PR number provided
if [ -z "$PR_NUMBER" ]; then
    echo -e "${RED}Error: Please provide PR number${NC}"
    echo "Usage: $0 <PR_NUMBER> [Platform]"
    echo "Example: $0 101 Windows"
    exit 1
fi

# Check if Unity is running
if pgrep -x "Unity" > /dev/null; then
    echo -e "${RED}Unity is running. Please close it first.${NC}"
    exit 1
fi

# Create build directory
BUILD_DIR="$BUILD_PATH/PR-$PR_NUMBER"
mkdir -p "$BUILD_DIR"

echo -e "${YELLOW}Building QA version for PR #$PR_NUMBER...${NC}"

# Get PR info
PR_INFO=$(gh pr view $PR_NUMBER --json title,branch,author)
PR_TITLE=$(echo $PR_INFO | jq -r '.title')
PR_BRANCH=$(echo $PR_INFO | jq -r '.branch')

# Checkout PR branch
echo "Checking out branch: $PR_BRANCH"
git checkout $PR_BRANCH
git pull

# Create build info file
cat > "$BUILD_DIR/BUILD_INFO.txt" << EOF
QA Build Information
====================
PR Number: #$PR_NUMBER
Title: $PR_TITLE
Branch: $PR_BRANCH
Build Date: $(date '+%Y-%m-%d %H:%M:%S')
Unity Version: 6000.0.26f1
Platform: $PLATFORM

Test Focus Areas:
- See QA_TEST_PLAN.md for detailed test cases
- Record any differences in game feel
- Check performance metrics
- Test save/load functionality

How to Report Issues:
1. Note the exact steps to reproduce
2. Include screenshots/video if possible
3. Check if issue exists in main branch
4. File bug report on GitHub with tag 'qa-found'
EOF

# Create test checklist
cat > "$BUILD_DIR/QA_CHECKLIST.txt" << EOF
QA Test Checklist for PR #$PR_NUMBER
=====================================

PLAYER MOVEMENT
[ ] Walk left/right feels responsive
[ ] Sprint has correct speed
[ ] Jump height unchanged
[ ] Double jump timing works
[ ] No input lag detected

COMBAT SYSTEM  
[ ] Melee attacks animate correctly
[ ] Weapon switching works
[ ] Combos register properly
[ ] Attack canceling functions
[ ] Damage numbers display

PERFORMANCE
[ ] Maintains 60 FPS in combat
[ ] No memory leaks detected
[ ] Scene transitions smooth
[ ] No stuttering or hitches

CRITICAL PATHS
[ ] Can complete tutorial
[ ] Can save and load game
[ ] Can defeat enemies
[ ] Can collect items
[ ] Can use abilities

REGRESSION TESTS
[ ] Previous bugs still fixed
[ ] No new crashes
[ ] UI remains functional
[ ] Audio plays correctly

Notes:
_________________________________
_________________________________
_________________________________

Tested By: ________________
Date: ____________________
Verdict: [ ] PASS [ ] FAIL
EOF

# Build the game
echo -e "${YELLOW}Starting Unity build...${NC}"

case "$PLATFORM" in
    "Windows")
        BUILD_TARGET="Win64"
        OUTPUT_FILE="$BUILD_DIR/NeonLadder_PR${PR_NUMBER}.exe"
        ;;
    "Mac")
        BUILD_TARGET="OSXUniversal"
        OUTPUT_FILE="$BUILD_DIR/NeonLadder_PR${PR_NUMBER}.app"
        ;;
    "Linux")
        BUILD_TARGET="Linux64"
        OUTPUT_FILE="$BUILD_DIR/NeonLadder_PR${PR_NUMBER}"
        ;;
    *)
        echo -e "${RED}Unknown platform: $PLATFORM${NC}"
        exit 1
        ;;
esac

# Create Unity build script
cat > "$PROJECT_PATH/qa_build.cs" << EOF
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class QABuilder
{
    public static void BuildQA()
    {
        string[] scenes = new string[]
        {
            "Assets/Scenes/Title.unity",
            "Assets/Scenes/Start.unity",
            "Assets/Scenes/Staging.unity",
            // Add all required scenes
        };
        
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "$OUTPUT_FILE",
            target = BuildTarget.$BUILD_TARGET,
            options = BuildOptions.Development | 
                     BuildOptions.AllowDebugging | 
                     BuildOptions.ShowBuiltPlayer
        };
        
        BuildReport report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("QA Build succeeded: " + report.summary.totalSize + " bytes");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError("QA Build failed");
            EditorApplication.Exit(1);
        }
    }
}
EOF

# Run Unity build
"$UNITY_PATH" -batchmode \
    -projectPath "$PROJECT_PATH" \
    -executeMethod QABuilder.BuildQA \
    -logFile "$BUILD_DIR/build.log" \
    -quit

BUILD_RESULT=$?

# Clean up
rm -f "$PROJECT_PATH/qa_build.cs"
rm -f "$PROJECT_PATH/qa_build.cs.meta"

if [ $BUILD_RESULT -eq 0 ]; then
    echo -e "${GREEN}✓ QA Build successful!${NC}"
    echo -e "${GREEN}Location: $BUILD_DIR${NC}"
    echo ""
    echo "Next steps:"
    echo "1. Share build with QA testers"
    echo "2. Have them fill out QA_CHECKLIST.txt"
    echo "3. Collect feedback and bug reports"
    echo "4. Iterate based on findings"
    
    # Open build folder
    open "$BUILD_DIR"
else
    echo -e "${RED}✗ Build failed. Check $BUILD_DIR/build.log for details${NC}"
    exit 1
fi