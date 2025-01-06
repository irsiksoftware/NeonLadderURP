param (
    [bool]$ShowMetaFiles = $false,         # Default to false
    [bool]$AlsoDisplay = $true,            # Default to true
    [bool]$WriteScriptsToFile = $true      # New parameter (default = true)
)

# ================================
# PRIORITIZED LIST OF SCRIPTS
# ================================
# The order here reflects importance from top to bottom.
$filesGPTwantsTheContentsOf = @(
    'Player.cs',               # 1
    'Enemy.cs',                # 2
    'Boss.cs',                 # 3
    'Game.cs',                 # 4
    'GameControllerManager.cs',# 5
    'SceneChangeController.cs',# 6
    'SaveSystem.cs',           # 7
    'AudioManager.cs',         # 8
    'ManagerController.cs',    # 9
    'TitleScreen.cs'           # 10
    'CollisionController.cs'   # 11
    'SceneChangeController.cs' # 12
    'PortalSpawnController.cs'  # 13
    'BaseAction.cs'            # 14
    'KinematicObject.cs'	# 15
    'BaseStat.cs'		# 16
    'Simulation.cs'		# 17
    'Simulation.Event.cs'	# 18
    'PlayerAction.cs'           # 19
    'BaseGameEvent.cs'       	# 20 Base class for custom game events
    'EventManager.cs'		# 21 Central event/observer hub
)

# Define the root path and exclusions
$rootPath = "."
$excludedDirs = @("Packages", "AnotherDirToExclude") 
$excludedExtensions = if ($ShowMetaFiles) { @() } else { @(".meta") }

# ================================
# RECURSIVE DIRECTORY TREE FUNCTION
# ================================
function Get-DirectoryTree {
    param (
        [string]$Path,
        [string[]]$Exclusions,
        [string[]]$ExcludedExtensions,
        [int]$Level = 0
    )

    $items = Get-ChildItem -Path $Path -Force | Where-Object {
        -not ($_.PSIsContainer -and $_.Name -in $Exclusions)
    }

    $output = @()

    foreach ($item in $items) {
        $indent = " " * ($Level * 2)

        if ($item.PSIsContainer) {
            $output += "$indent+-- $($item.Name)"
            $output += Get-DirectoryTree -Path $item.FullName `
                                         -Exclusions $Exclusions `
                                         -ExcludedExtensions $ExcludedExtensions `
                                         -Level ($Level + 1)
        }
        else {
            if ($item.Extension -notin $ExcludedExtensions) {
                $output += "$indent|   $($item.Name)"
            }
        }
    }

    return $output
}

# ================================
# MAIN SCRIPT EXECUTION
# ================================

# 1) Generate the directory tree
$output = Get-DirectoryTree -Path $rootPath `
                           -Exclusions $excludedDirs `
                           -ExcludedExtensions $excludedExtensions

# 2) Write the directory tree to a file
$output | Out-File -FilePath solution_structure.txt -Encoding UTF8

# 3) Optionally display the directory tree in the console
if ($AlsoDisplay) {
    $output | Out-Host
}

# 4) Print the contents of the prioritized files
Write-Host "`n===== Printing Requested Script Contents =====`n"

# If $WriteScriptsToFile is true, append a similar header in the file
if ($WriteScriptsToFile) {
    Add-Content -Path solution_structure.txt -Value "`r`n===== Printing Requested Script Contents =====`r`n"
}

foreach ($scriptName in $filesGPTwantsTheContentsOf) {
    # Find all matches of this script name in the project
    $matchingFiles = Get-ChildItem -Path $rootPath -Recurse -Force -Include $scriptName -File |
                     Where-Object {
                         $parentDirName = Split-Path $_.FullName -Parent | Split-Path -Leaf
                         -not ($parentDirName -in $excludedDirs)
                     }

    foreach ($file in $matchingFiles) {
        # Print to console
        Write-Host "`n/***********************/"
        Write-Host "/** $($file.Name) **/"
        Write-Host "/***********************/`n"
        Get-Content -Path $file.FullName | Out-Host

        # Also write to solution_structure.txt if requested
        if ($WriteScriptsToFile) {
            Add-Content -Path solution_structure.txt -Value "`r`n/***********************/"
            Add-Content -Path solution_structure.txt -Value "/** $($file.Name) **/"
            Add-Content -Path solution_structure.txt -Value "/***********************/`r`n"

            # Append file contents
            Get-Content -Path $file.FullName | Add-Content -Path solution_structure.txt
        }
    }
}
