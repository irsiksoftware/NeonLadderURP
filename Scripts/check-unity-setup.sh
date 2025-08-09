#!/bin/bash
# Check Unity installation and setup status

echo "Unity Setup Check"
echo "================="
echo ""

# Check Unity Hub
if pgrep -x "Unity Hub" > /dev/null; then
    echo "✅ Unity Hub is running"
else
    echo "⚠️  Unity Hub is not running"
    echo "   Run: open -a 'Unity Hub'"
fi

# Check for Unity installations
echo ""
echo "Looking for Unity installations..."

# Common Unity paths on macOS
UNITY_PATHS=(
    "/Applications/Unity/Hub/Editor/6000.0.26f1/Unity.app"
    "/Applications/Unity/Hub/Editor/6000.0.37f1/Unity.app"
    "$HOME/Applications/Unity/Hub/Editor/6000.0.26f1/Unity.app"
)

FOUND_UNITY=false
for path in "${UNITY_PATHS[@]}"; do
    if [ -d "$path" ]; then
        echo "✅ Found Unity at: $path"
        FOUND_UNITY=true
        UNITY_EXEC="$path/Contents/MacOS/Unity"
        break
    fi
done

if [ "$FOUND_UNITY" = false ]; then
    echo "⚠️  Unity 6000.0.26f1 not found"
    echo "   Please install it from Unity Hub"
    exit 1
fi

# Check project
echo ""
echo "Project Information:"
echo "-------------------"
PROJECT_PATH="$(dirname "$(dirname "$0")")"
echo "Project Path: $PROJECT_PATH"

if [ -f "$PROJECT_PATH/ProjectSettings/ProjectVersion.txt" ]; then
    VERSION=$(grep m_EditorVersion "$PROJECT_PATH/ProjectSettings/ProjectVersion.txt" | cut -d' ' -f2)
    echo "Required Unity Version: $VERSION"
else
    echo "⚠️  ProjectVersion.txt not found"
fi

# Check if Unity is running
if pgrep -x "Unity" > /dev/null; then
    echo ""
    echo "✅ Unity Editor is running"
    echo "   Project should be open or opening..."
else
    echo ""
    echo "📝 Next Steps:"
    echo "1. Open Unity Hub"
    echo "2. Add project: $PROJECT_PATH"
    echo "3. Open the project with Unity $VERSION"
fi

echo ""
echo "Test Commands:"
echo "-------------"
echo "# Run Unity tests from CLI (once Unity is installed):"
echo "$UNITY_EXEC -batchmode -projectPath \"$PROJECT_PATH\" -runTests -testResults \"$PROJECT_PATH/TestResults.xml\""