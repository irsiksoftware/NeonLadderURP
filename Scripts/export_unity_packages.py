#!/usr/bin/env python3
"""
Unity Package Export and Google Drive Upload Script
Cross-platform solution for automating Unity package exports
Works on Windows, macOS, and Linux
"""

import os
import sys
import json
import subprocess
import platform
import shutil
import hashlib
import argparse
from pathlib import Path
from datetime import datetime
from typing import List, Dict, Optional, Tuple
import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

class UnityPackageExporter:
    """Handles Unity package export operations across platforms"""
    
    def __init__(self, project_path: Path, unity_path: Optional[Path] = None, dry_run: bool = False):
        self.project_path = Path(project_path).resolve()
        self.packages_path = self.project_path / "Assets" / "Packages"
        self.export_path = self.project_path / "PackageExports"
        self.system = platform.system()
        self.dry_run = dry_run
        
        # Find Unity installation based on platform (skip for dry-run)
        if not dry_run:
            self.unity_path = unity_path or self._find_unity_installation()
        else:
            self.unity_path = None
            logger.info("Dry-run mode: Unity installation check skipped")
        
        # Ensure export directory exists
        self.export_path.mkdir(exist_ok=True)
        
        # Package configuration
        self.config_file = self.project_path / ".claude" / "package_export_config.json"
        self.config = self._load_config()
    
    def _find_unity_installation(self) -> Path:
        """Find Unity installation path based on the operating system"""
        unity_paths = {
            "Windows": [
                Path("C:/Program Files/Unity/Hub/Editor/6000.0.26f1/Editor/Unity.exe"),
                Path("C:/Program Files/Unity/Hub/Editor/6000.0.37f1/Editor/Unity.exe"),
                Path("C:/Program Files/Unity/Editor/Unity.exe"),
            ],
            "Darwin": [  # macOS
                Path("/Applications/Unity/Hub/Editor/6000.0.26f1/Unity.app/Contents/MacOS/Unity"),
                Path("/Applications/Unity/Hub/Editor/6000.0.37f1/Unity.app/Contents/MacOS/Unity"),
                Path("/Applications/Unity/Unity.app/Contents/MacOS/Unity"),
            ],
            "Linux": [
                Path("/opt/Unity/Editor/Unity"),
                Path("/home/runner/Unity/Hub/Editor/6000.0.26f1/Editor/Unity"),
            ]
        }
        
        for unity_path in unity_paths.get(self.system, []):
            if unity_path.exists():
                logger.info(f"Found Unity at: {unity_path}")
                return unity_path
        
        # Try to find Unity via environment variable
        unity_env = os.environ.get("UNITY_PATH")
        if unity_env:
            unity_path = Path(unity_env)
            if unity_path.exists():
                return unity_path
        
        raise FileNotFoundError(
            f"Unity installation not found on {self.system}. "
            "Please set UNITY_PATH environment variable or specify --unity-path"
        )
    
    def _load_config(self) -> Dict:
        """Load package export configuration"""
        default_config = {
            "packages_to_export": [],
            "exclude_patterns": [
                "*.meta",
                ".git",
                "__pycache__",
                "*.pyc",
                ".DS_Store",
                "Thumbs.db"
            ],
            "google_drive_folder_id": None,
            "last_export": None
        }
        
        if self.config_file.exists():
            try:
                with open(self.config_file, 'r') as f:
                    loaded_config = json.load(f)
                    default_config.update(loaded_config)
            except json.JSONDecodeError:
                logger.warning("Invalid config file, using defaults")
        
        return default_config
    
    def _save_config(self):
        """Save current configuration"""
        self.config_file.parent.mkdir(exist_ok=True)
        with open(self.config_file, 'w') as f:
            json.dump(self.config, f, indent=2, default=str)
    
    def discover_packages(self) -> List[Dict]:
        """Discover all packages with DownloadInstructions.txt"""
        packages = []
        
        if not self.packages_path.exists():
            logger.error(f"Packages directory not found: {self.packages_path}")
            return packages
        
        # Scan for packages with DownloadInstructions.txt
        for item in self.packages_path.iterdir():
            if item.is_dir():
                # Check for DownloadInstructions.txt
                download_file = item / "DownloadInstructions.txt"
                if download_file.exists():
                    package_info = {
                        "name": item.name,
                        "path": str(item.relative_to(self.project_path)),
                        "has_download_instructions": True,
                        "size_mb": self._get_directory_size(item) / (1024 * 1024)
                    }
                    packages.append(package_info)
                
                # Check subdirectories (e.g., LeartesStudios has many sub-packages)
                for subitem in item.iterdir():
                    if subitem.is_dir():
                        sub_download_file = subitem / "DownloadInstructions.txt"
                        if sub_download_file.exists():
                            package_info = {
                                "name": f"{item.name}/{subitem.name}",
                                "path": str(subitem.relative_to(self.project_path)),
                                "has_download_instructions": True,
                                "size_mb": self._get_directory_size(subitem) / (1024 * 1024)
                            }
                            packages.append(package_info)
        
        return packages
    
    def _get_directory_size(self, path: Path) -> int:
        """Calculate total size of directory in bytes"""
        total = 0
        try:
            for entry in path.rglob('*'):
                if entry.is_file():
                    total += entry.stat().st_size
        except (OSError, PermissionError):
            pass
        return total
    
    def check_unity_running(self) -> bool:
        """Check if Unity is currently running"""
        if self.system == "Windows":
            result = subprocess.run(
                ["tasklist", "/FI", "IMAGENAME eq Unity.exe"],
                capture_output=True,
                text=True
            )
            return "Unity.exe" in result.stdout
        else:  # macOS and Linux
            result = subprocess.run(
                ["pgrep", "-f", "Unity"],
                capture_output=True
            )
            return result.returncode == 0
    
    def export_package(self, package_path: str, output_name: str) -> Optional[Path]:
        """Export a Unity package using Unity CLI"""
        if self.check_unity_running():
            logger.error("Unity is currently running. Please close Unity before exporting packages.")
            return None
        
        output_file = self.export_path / f"{output_name}.unitypackage"
        
        # Create Unity export method script
        export_script = self.project_path / "Assets" / "Editor" / "PackageExporter.cs"
        export_script.parent.mkdir(exist_ok=True)
        
        script_content = f'''using UnityEngine;
using UnityEditor;
using System.IO;

public class PackageExporter
{{
    public static void ExportPackage()
    {{
        string packagePath = "{package_path.replace(os.sep, "/")}";
        string outputPath = "{str(output_file).replace(os.sep, "/")}";
        
        Debug.Log($"Exporting package from: {{packagePath}}");
        Debug.Log($"Output path: {{outputPath}}");
        
        if (!Directory.Exists(packagePath))
        {{
            Debug.LogError($"Package path does not exist: {{packagePath}}");
            EditorApplication.Exit(1);
            return;
        }}
        
        // Ensure output directory exists
        string outputDir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outputDir))
        {{
            Directory.CreateDirectory(outputDir);
        }}
        
        try
        {{
            AssetDatabase.ExportPackage(
                packagePath,
                outputPath,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
            );
            Debug.Log($"Successfully exported package to: {{outputPath}}");
            EditorApplication.Exit(0);
        }}
        catch (System.Exception e)
        {{
            Debug.LogError($"Failed to export package: {{e.Message}}");
            EditorApplication.Exit(1);
        }}
    }}
}}'''
        
        with open(export_script, 'w') as f:
            f.write(script_content)
        
        # Run Unity in batch mode to export the package
        cmd = [
            str(self.unity_path),
            "-batchmode",
            "-projectPath", str(self.project_path),
            "-executeMethod", "PackageExporter.ExportPackage",
            "-logFile", str(self.export_path / f"export_{output_name}.log"),
            "-quit"
        ]
        
        logger.info(f"Exporting {package_path} to {output_file.name}")
        
        try:
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=300)
            
            if output_file.exists():
                logger.info(f"Successfully exported: {output_file.name} ({output_file.stat().st_size / (1024*1024):.2f} MB)")
                return output_file
            else:
                logger.error(f"Export failed for {package_path}")
                if result.stderr:
                    logger.error(result.stderr)
                return None
                
        except subprocess.TimeoutExpired:
            logger.error(f"Export timed out for {package_path}")
            return None
        except Exception as e:
            logger.error(f"Export failed: {e}")
            return None
        finally:
            # Clean up the export script
            if export_script.exists():
                export_script.unlink()
    
    def generate_manifest(self, exported_packages: List[Dict]) -> Path:
        """Generate a manifest file with export information"""
        manifest = {
            "export_date": datetime.now().isoformat(),
            "unity_version": "6000.0.26f1",
            "platform": self.system,
            "packages": exported_packages,
            "total_size_mb": sum(p.get("size_mb", 0) for p in exported_packages)
        }
        
        manifest_file = self.export_path / f"manifest_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
        with open(manifest_file, 'w') as f:
            json.dump(manifest, f, indent=2, default=str)
        
        logger.info(f"Generated manifest: {manifest_file.name}")
        return manifest_file


class GoogleDriveUploader:
    """Handles Google Drive uploads (placeholder for implementation)"""
    
    def __init__(self):
        self.authenticated = False
    
    def authenticate(self) -> bool:
        """Authenticate with Google Drive"""
        # This would use either gdrive CLI or Google API
        logger.info("Google Drive authentication placeholder")
        return True
    
    def upload_file(self, file_path: Path, folder_id: Optional[str] = None) -> Optional[str]:
        """Upload file to Google Drive and return shareable link"""
        # Placeholder implementation
        logger.info(f"Would upload {file_path.name} to Google Drive")
        return f"https://drive.google.com/file/d/PLACEHOLDER_{file_path.stem}/view"
    
    def update_download_instructions(self, package_path: Path, drive_link: str):
        """Update DownloadInstructions.txt with new Google Drive link"""
        instructions_file = package_path / "DownloadInstructions.txt"
        
        content = f"""Download the necessary file(s) from the following link:

{drive_link}

Instructions:
1. Download the .unitypackage file from the above link.
2. Open Unity and go to Assets > Import Package > Custom Package.
3. Select the downloaded .unitypackage file and import it into your project.

Last Updated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}
"""
        
        with open(instructions_file, 'w') as f:
            f.write(content)
        
        logger.info(f"Updated {instructions_file}")


def main():
    parser = argparse.ArgumentParser(description="Export Unity packages and upload to Google Drive")
    parser.add_argument(
        "--project-path",
        type=Path,
        default=Path.cwd(),
        help="Path to Unity project (default: current directory)"
    )
    parser.add_argument(
        "--unity-path",
        type=Path,
        help="Path to Unity executable (auto-detected if not specified)"
    )
    parser.add_argument(
        "--packages",
        nargs="+",
        help="Specific packages to export (default: all with DownloadInstructions.txt)"
    )
    parser.add_argument(
        "--skip-upload",
        action="store_true",
        help="Skip Google Drive upload step"
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Show what would be exported without actually doing it"
    )
    
    args = parser.parse_args()
    
    # Initialize exporter
    try:
        exporter = UnityPackageExporter(args.project_path, args.unity_path, args.dry_run)
    except FileNotFoundError as e:
        logger.error(e)
        sys.exit(1)
    
    # Discover packages
    packages = exporter.discover_packages()
    
    if not packages:
        logger.warning("No packages found with DownloadInstructions.txt")
        sys.exit(0)
    
    logger.info(f"Found {len(packages)} packages to export")
    
    # Filter packages if specific ones requested
    if args.packages:
        packages = [p for p in packages if p["name"] in args.packages]
    
    # Show packages and sizes
    total_size = sum(p["size_mb"] for p in packages)
    logger.info(f"Total size to export: {total_size:.2f} MB")
    
    if args.dry_run:
        logger.info("Dry run - packages that would be exported:")
        for pkg in packages:
            logger.info(f"  - {pkg['name']} ({pkg['size_mb']:.2f} MB)")
        return
    
    # Export packages
    exported = []
    for pkg in packages:
        output_name = pkg["name"].replace("/", "_").replace(" ", "_")
        exported_file = exporter.export_package(
            str(exporter.project_path / pkg["path"]),
            output_name
        )
        
        if exported_file:
            pkg["exported_file"] = str(exported_file)
            pkg["export_size_mb"] = exported_file.stat().st_size / (1024 * 1024)
            exported.append(pkg)
    
    # Generate manifest
    if exported:
        manifest = exporter.generate_manifest(exported)
        logger.info(f"Exported {len(exported)}/{len(packages)} packages successfully")
    
    # Upload to Google Drive if not skipped
    if not args.skip_upload and exported:
        uploader = GoogleDriveUploader()
        if uploader.authenticate():
            for pkg in exported:
                exported_file = Path(pkg["exported_file"])
                drive_link = uploader.upload_file(exported_file)
                if drive_link:
                    package_path = exporter.project_path / pkg["path"]
                    uploader.update_download_instructions(package_path, drive_link)
    
    # Save configuration
    exporter.config["last_export"] = datetime.now().isoformat()
    exporter.config["packages_to_export"] = [p["name"] for p in packages]
    exporter._save_config()
    
    logger.info("Export process completed")


if __name__ == "__main__":
    main()