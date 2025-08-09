#!/bin/bash
# Monitor Unity installation progress

echo "Monitoring Unity Installation..."
echo "================================"
echo "This script will check every 30 seconds for Unity 6000.0.26f1"
echo "Press Ctrl+C to stop monitoring"
echo ""

UNITY_PATH="/Applications/Unity/Hub/Editor/6000.0.26f1/Unity.app"
CHECK_INTERVAL=30
COUNTER=0

while true; do
    COUNTER=$((COUNTER + 1))
    echo -n "Check #$COUNTER ($(date '+%H:%M:%S')): "
    
    if [ -d "$UNITY_PATH" ]; then
        echo "✅ Unity 6000.0.26f1 is installed!"
        echo ""
        echo "Unity is ready! You can now:"
        echo "1. Open the project in Unity Hub"
        echo "2. Run tests with: ./Scripts/run-unity-tests.sh"
        echo "3. Export packages with: ./Scripts/package-automation.sh export"
        
        # Optional: Open project automatically
        read -p "Would you like to open the project in Unity now? (y/n) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            open -a "$UNITY_PATH" --args -projectPath "$(dirname "$(dirname "$0")")"
        fi
        
        exit 0
    else
        echo "⏳ Still waiting... (Unity Hub should be downloading)"
    fi
    
    sleep $CHECK_INTERVAL
done