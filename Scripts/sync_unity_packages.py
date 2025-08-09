#!/usr/bin/env python3
"""
Unity Package Sync System
Complete workflow for exporting packages and syncing with Google Drive
Combines export and upload functionality
"""

import os
import sys
import json
import subprocess
import platform
import shutil
import argparse
from pathlib import Path
from datetime import datetime
from typing import List, Dict, Optional, Tuple
import logging
import time
import re

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


class UnityPackageSync:
    """Complete Unity package sync workflow"""
    
    def __init__(self, project_path: Path):
        self.project_path = Path(project_path).resolve()
        self.packages_path = self.project_path / "Assets" / "Packages"
        self.export_path = self.project_path / "PackageExports"
        self.download_path = self.project_path / "PackageDownloads"
        self.config_file = self.project_path / ".claude" / "package_sync_config.json"
        self.config = self._load_config()
        
        # Ensure directories exist
        self.export_path.mkdir(exist_ok=True)
        self.download_path.mkdir(exist_ok=True)
        
    def _load_config(self) -> Dict:
        """Load sync configuration"""
        default_config = {
            "google_drive_folder": "NeonLadder_Packages",
            "last_sync": None,
            "package_mappings": {},
            "auto_update_instructions": True
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
    
    def find_packages_to_sync(self) -> List[Dict]:
        """Find packages that need syncing"""
        packages = []
        
        # Check both Packages and Audio directories
        search_paths = [
            self.packages_path,
            self.project_path / "Assets" / "Audio"
        ]
        
        for search_path in search_paths:
            if not search_path.exists():
                continue
                
            for txt_file in search_path.rglob("DownloadInstructions.txt"):
                # Determine package info
                if "LeartesStudios" in str(txt_file):
                    parts = txt_file.parts
                    leartes_idx = parts.index("LeartesStudios")
                    if leartes_idx + 1 < len(parts) - 1:
                        package_name = f"LeartesStudios/{parts[leartes_idx + 1]}"
                        package_path = Path(*parts[:leartes_idx+2])
                    else:
                        package_name = "LeartesStudios"
                        package_path = Path(*parts[:leartes_idx+1])
                else:
                    package_name = txt_file.parent.name
                    package_path = txt_file.parent
                
                packages.append({
                    "name": package_name,
                    "path": package_path,
                    "instructions_file": txt_file,
                    "has_link": self._has_drive_link(txt_file)
                })
        
        return packages
    
    def _has_drive_link(self, txt_file: Path) -> bool:
        """Check if file has Google Drive link"""
        try:
            content = txt_file.read_text()
            return "drive.google.com" in content
        except:
            return False
    
    def upload_to_gdrive(self, file_path: Path, package_name: str) -> Optional[str]:
        """Upload file to Google Drive using gdrive CLI"""
        try:
            # Check if gdrive is available
            result = subprocess.run(["which", "gdrive"], capture_output=True)
            if result.returncode != 0:
                logger.error("gdrive CLI not found. Install with: brew install gdrive")
                return None
            
            # Upload file
            logger.info(f"Uploading {file_path.name} to Google Drive...")
            
            cmd = [
                "gdrive", "files", "upload",
                "--parent", self.config.get("google_drive_folder_id", "root"),
                str(file_path)
            ]
            
            result = subprocess.run(cmd, capture_output=True, text=True)
            
            if result.returncode == 0:
                # Extract file ID from output
                output = result.stdout
                # Parse the file ID from gdrive output
                if "Id:" in output:
                    file_id = output.split("Id:")[1].strip().split()[0]
                    share_link = f"https://drive.google.com/file/d/{file_id}/view?usp=sharing"
                    logger.info(f"Uploaded successfully: {share_link}")
                    return share_link
                else:
                    logger.error("Could not extract file ID from gdrive output")
                    return None
            else:
                logger.error(f"Upload failed: {result.stderr}")
                return None
                
        except Exception as e:
            logger.error(f"Upload error: {e}")
            return None
    
    def update_download_instructions(self, txt_file: Path, drive_link: str, package_name: str):
        """Update DownloadInstructions.txt with new Google Drive link"""
        content = f"""Download the necessary file(s) from the following link:

{drive_link}

Instructions:
1. Download the .unitypackage file from the above link.
2. Open Unity and go to Assets > Import Package > Custom Package.
3. Select the downloaded .unitypackage file and import it into your project.

Package: {package_name}
Last Updated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}
Synced with UnityPackageSync
"""
        
        txt_file.write_text(content)
        logger.info(f"Updated: {txt_file.relative_to(self.project_path)}")
    
    def create_placeholder_package(self, package_path: Path, package_name: str) -> Path:
        """Create a placeholder .unitypackage for packages without Unity"""
        output_file = self.export_path / f"{package_name.replace('/', '_')}.unitypackage"
        
        # Create a simple tar file as placeholder (Unity packages are tar.gz)
        logger.info(f"Creating placeholder for {package_name}")
        
        # Write a readme file
        readme = self.export_path / "README.txt"
        readme.write_text(f"Placeholder for {package_name}\nExport with Unity to get actual package")
        
        # Create tar archive
        import tarfile
        with tarfile.open(output_file, "w:gz") as tar:
            tar.add(readme, arcname="README.txt")
        
        readme.unlink()  # Clean up temp file
        
        return output_file
    
    def sync_package(self, package: Dict, use_placeholder: bool = True) -> bool:
        """Sync a single package"""
        package_name = package["name"]
        logger.info(f"\nSyncing: {package_name}")
        
        # Check if package already has a link
        if package["has_link"]:
            logger.info(f"Package already has Google Drive link, skipping")
            return True
        
        # Create export (placeholder for now since Unity isn't installed)
        export_file = self.export_path / f"{package_name.replace('/', '_')}.unitypackage"
        
        if not export_file.exists():
            if use_placeholder:
                export_file = self.create_placeholder_package(package["path"], package_name)
            else:
                logger.warning(f"No export found for {package_name}, skipping")
                return False
        
        # Upload to Google Drive
        drive_link = self.upload_to_gdrive(export_file, package_name)
        
        if drive_link:
            # Update DownloadInstructions.txt
            self.update_download_instructions(
                package["instructions_file"],
                drive_link,
                package_name
            )
            
            # Save mapping
            self.config["package_mappings"][package_name] = {
                "file_id": drive_link.split("/d/")[1].split("/")[0],
                "link": drive_link,
                "updated": datetime.now().isoformat()
            }
            
            return True
        
        return False
    
    def sync_all(self, specific_packages: Optional[List[str]] = None, use_placeholders: bool = True):
        """Sync all packages or specific ones"""
        packages = self.find_packages_to_sync()
        
        if specific_packages:
            packages = [p for p in packages if p["name"] in specific_packages]
        
        logger.info(f"Found {len(packages)} packages to sync")
        
        success_count = 0
        for package in packages:
            if self.sync_package(package, use_placeholders):
                success_count += 1
        
        # Save configuration
        self.config["last_sync"] = datetime.now().isoformat()
        self._save_config()
        
        logger.info(f"\nSync complete: {success_count}/{len(packages)} successful")
        
        return success_count, len(packages)
    
    def generate_package_list(self) -> Path:
        """Generate a markdown file with all package links"""
        packages = self.find_packages_to_sync()
        
        md_content = "# Unity Package Downloads\n\n"
        md_content += f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n\n"
        
        # Group by category
        categories = {}
        for package in packages:
            if package["has_link"]:
                category = "LeartesStudios" if "LeartesStudios" in package["name"] else "Third Party"
                if category not in categories:
                    categories[category] = []
                
                # Read the link from file
                try:
                    content = package["instructions_file"].read_text()
                    links = re.findall(r'https://drive\.google\.com/[^\s]+', content)
                    if links:
                        categories[category].append({
                            "name": package["name"],
                            "link": links[0]
                        })
                except:
                    pass
        
        # Write categories
        for category, items in categories.items():
            md_content += f"\n## {category}\n\n"
            for item in sorted(items, key=lambda x: x["name"]):
                md_content += f"- [{item['name']}]({item['link']})\n"
        
        # Save file
        list_file = self.project_path / "PACKAGE_DOWNLOADS.md"
        list_file.write_text(md_content)
        
        logger.info(f"Generated package list: {list_file.name}")
        return list_file


def main():
    parser = argparse.ArgumentParser(
        description="Sync Unity packages with Google Drive"
    )
    parser.add_argument(
        "--project-path",
        type=Path,
        default=Path.cwd(),
        help="Path to Unity project"
    )
    parser.add_argument(
        "--packages",
        nargs="+",
        help="Specific packages to sync"
    )
    parser.add_argument(
        "--no-placeholders",
        action="store_true",
        help="Don't create placeholder packages"
    )
    parser.add_argument(
        "--list-only",
        action="store_true",
        help="Only generate package list, don't sync"
    )
    
    args = parser.parse_args()
    
    syncer = UnityPackageSync(args.project_path)
    
    if args.list_only:
        syncer.generate_package_list()
        return
    
    # Check gdrive authentication
    result = subprocess.run(["gdrive", "account", "list"], capture_output=True)
    if result.returncode != 0 or b"No accounts" in result.stdout:
        logger.error("gdrive not authenticated. Run: gdrive account add")
        logger.info("\nTo set up gdrive:")
        logger.info("1. Run: gdrive account add")
        logger.info("2. Follow the authentication process")
        logger.info("3. Run this script again")
        sys.exit(1)
    
    # Sync packages
    success, total = syncer.sync_all(
        args.packages,
        not args.no_placeholders
    )
    
    # Generate package list
    if success > 0:
        syncer.generate_package_list()
    
    sys.exit(0 if success == total else 1)


if __name__ == "__main__":
    main()