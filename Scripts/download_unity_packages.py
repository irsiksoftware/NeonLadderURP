#!/usr/bin/env python3
"""
Unity Package Download Script
Downloads Unity packages from Google Drive links in DownloadInstructions.txt files
Cross-platform solution that works without authentication
"""

import os
import sys
import re
import json
import urllib.request
import urllib.parse
import subprocess
import platform
import argparse
from pathlib import Path
from datetime import datetime
from typing import List, Dict, Optional, Tuple
import logging
import hashlib
import time

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


class GoogleDriveDownloader:
    """Downloads files from Google Drive without authentication"""
    
    def __init__(self, download_path: Path):
        self.download_path = Path(download_path)
        self.download_path.mkdir(exist_ok=True)
        self.session_files = []
        
    def extract_file_id(self, url: str) -> Optional[str]:
        """Extract Google Drive file ID from various URL formats"""
        patterns = [
            r'/file/d/([a-zA-Z0-9_-]+)',  # Standard format
            r'id=([a-zA-Z0-9_-]+)',        # Old format
            r'/open\?id=([a-zA-Z0-9_-]+)', # Open format
        ]
        
        for pattern in patterns:
            match = re.search(pattern, url)
            if match:
                return match.group(1)
        return None
    
    def get_direct_download_url(self, file_id: str) -> str:
        """Convert file ID to direct download URL"""
        return f"https://drive.google.com/uc?export=download&id={file_id}"
    
    def download_file(self, url: str, output_name: str, max_size_gb: float = 5.0) -> Optional[Path]:
        """Download file from Google Drive URL"""
        file_id = self.extract_file_id(url)
        if not file_id:
            logger.error(f"Could not extract file ID from URL: {url}")
            return None
        
        output_file = self.download_path / output_name
        
        # Skip if already downloaded
        if output_file.exists():
            logger.info(f"File already exists: {output_file.name}")
            return output_file
        
        direct_url = self.get_direct_download_url(file_id)
        logger.info(f"Downloading {output_name} from Google Drive...")
        logger.info(f"File ID: {file_id}")
        
        try:
            # First request to get the confirmation token if needed
            request = urllib.request.Request(direct_url)
            request.add_header('User-Agent', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36')
            
            with urllib.request.urlopen(request) as response:
                # Check if we need confirmation for large files
                content = response.read()
                
                # Check for virus scan warning (large files)
                if b"confirm=" in content or b"virus scan warning" in content.lower():
                    # Extract confirmation token
                    confirm_match = re.search(b'confirm=([a-zA-Z0-9_-]+)', content)
                    if confirm_match:
                        confirm_token = confirm_match.group(1).decode('utf-8')
                        # Download with confirmation
                        confirmed_url = f"{direct_url}&confirm={confirm_token}"
                        request = urllib.request.Request(confirmed_url)
                        request.add_header('User-Agent', 'Mozilla/5.0')
                        
                        with urllib.request.urlopen(request) as confirmed_response:
                            content = confirmed_response.read()
                
                # Check file size (prevent downloading files that are too large)
                file_size_mb = len(content) / (1024 * 1024)
                max_size_mb = max_size_gb * 1024
                
                if file_size_mb > max_size_mb:
                    logger.error(f"File too large: {file_size_mb:.2f} MB exceeds {max_size_gb} GB limit")
                    return None
                
                # Save the file
                with open(output_file, 'wb') as f:
                    f.write(content)
                
                logger.info(f"Downloaded: {output_file.name} ({file_size_mb:.2f} MB)")
                self.session_files.append(output_file)
                return output_file
                
        except urllib.error.HTTPError as e:
            if e.code == 404:
                logger.error(f"File not found (404): {url}")
            else:
                logger.error(f"HTTP Error {e.code}: {e.reason}")
            return None
        except Exception as e:
            logger.error(f"Download failed: {e}")
            return None
    
    def download_with_curl(self, url: str, output_name: str) -> Optional[Path]:
        """Alternative download method using curl (works on all platforms)"""
        file_id = self.extract_file_id(url)
        if not file_id:
            return None
        
        output_file = self.download_path / output_name
        if output_file.exists():
            logger.info(f"File already exists: {output_file.name}")
            return output_file
        
        # Check if curl is available
        curl_cmd = "curl" if platform.system() != "Windows" else "curl.exe"
        
        try:
            # Build curl command for Google Drive download
            cmd = [
                curl_cmd,
                "-L",  # Follow redirects
                "-o", str(output_file),
                "-H", "User-Agent: Mozilla/5.0",
                f"https://drive.google.com/uc?export=download&id={file_id}"
            ]
            
            logger.info(f"Downloading with curl: {output_name}")
            result = subprocess.run(cmd, capture_output=True, text=True)
            
            if result.returncode == 0 and output_file.exists():
                file_size_mb = output_file.stat().st_size / (1024 * 1024)
                logger.info(f"Downloaded: {output_file.name} ({file_size_mb:.2f} MB)")
                self.session_files.append(output_file)
                return output_file
            else:
                logger.error(f"Curl download failed: {result.stderr}")
                if output_file.exists():
                    output_file.unlink()
                return None
                
        except FileNotFoundError:
            logger.warning("Curl not found, falling back to urllib")
            return self.download_file(url, output_name)
        except Exception as e:
            logger.error(f"Curl download error: {e}")
            return None


class UnityPackageDownloader:
    """Manages Unity package downloads from DownloadInstructions.txt files"""
    
    def __init__(self, project_path: Path):
        self.project_path = Path(project_path).resolve()
        self.packages_path = self.project_path / "Assets" / "Packages"
        self.download_path = self.project_path / "PackageDownloads"
        self.downloader = GoogleDriveDownloader(self.download_path)
        
    def find_download_instructions(self) -> List[Dict]:
        """Find all DownloadInstructions.txt files with Google Drive links"""
        instructions = []
        
        # Check both Packages and Audio directories
        search_paths = [
            self.packages_path,
            self.project_path / "Assets" / "Audio"
        ]
        
        for search_path in search_paths:
            if not search_path.exists():
                continue
                
            for txt_file in search_path.rglob("DownloadInstructions.txt"):
                try:
                    with open(txt_file, 'r', encoding='utf-8') as f:
                        content = f.read()
                    
                    # Look for Google Drive links
                    drive_pattern = r'https://drive\.google\.com/[^\s]+'
                    matches = re.findall(drive_pattern, content)
                    
                    if matches:
                        # Determine package name based on hierarchy
                        if "LeartesStudios" in str(txt_file):
                            # Handle LeartesStudios nested structure
                            parts = txt_file.parts
                            leartes_idx = parts.index("LeartesStudios")
                            if leartes_idx + 1 < len(parts) - 1:  # Has subdirectory
                                package_name = f"LeartesStudios/{parts[leartes_idx + 1]}"
                            else:
                                package_name = "LeartesStudios"
                        else:
                            package_name = txt_file.parent.name
                        
                        instructions.append({
                            "name": package_name,
                            "path": str(txt_file),
                            "url": matches[0],  # Use first Google Drive link found
                            "relative_path": str(txt_file.relative_to(self.project_path))
                        })
                        
                except Exception as e:
                    logger.warning(f"Error reading {txt_file}: {e}")
        
        return instructions
    
    def download_package(self, instruction: Dict) -> Optional[Path]:
        """Download a single package based on instruction"""
        package_name = instruction["name"].replace("/", "_").replace(" ", "_")
        output_name = f"{package_name}.unitypackage"
        
        logger.info(f"\nProcessing: {instruction['name']}")
        logger.info(f"From: {instruction['relative_path']}")
        
        # Try curl first (usually faster), fall back to urllib
        downloaded = self.downloader.download_with_curl(instruction["url"], output_name)
        if not downloaded:
            downloaded = self.downloader.download_file(instruction["url"], output_name)
        
        return downloaded
    
    def download_all(self, specific_packages: Optional[List[str]] = None) -> Dict:
        """Download all packages or specific ones"""
        instructions = self.find_download_instructions()
        
        if not instructions:
            logger.warning("No DownloadInstructions.txt files with Google Drive links found")
            return {"total": 0, "success": 0, "failed": 0, "files": []}
        
        # Filter specific packages if requested
        if specific_packages:
            instructions = [i for i in instructions if i["name"] in specific_packages]
        
        logger.info(f"Found {len(instructions)} packages to download")
        
        results = {
            "total": len(instructions),
            "success": 0,
            "failed": 0,
            "files": [],
            "total_size_mb": 0
        }
        
        for i, instruction in enumerate(instructions, 1):
            logger.info(f"\n[{i}/{len(instructions)}] Downloading {instruction['name']}...")
            
            downloaded = self.download_package(instruction)
            if downloaded:
                file_size_mb = downloaded.stat().st_size / (1024 * 1024)
                results["success"] += 1
                results["files"].append(str(downloaded))
                results["total_size_mb"] += file_size_mb
            else:
                results["failed"] += 1
                logger.error(f"Failed to download: {instruction['name']}")
        
        return results
    
    def verify_downloads(self) -> List[Path]:
        """Verify downloaded files are valid Unity packages"""
        valid_packages = []
        
        for file in self.download_path.glob("*.unitypackage"):
            # Unity packages are basically tar.gz files
            try:
                # Check if file has minimum size (not empty)
                if file.stat().st_size > 100:
                    valid_packages.append(file)
                else:
                    logger.warning(f"Package too small, might be corrupted: {file.name}")
            except Exception as e:
                logger.warning(f"Could not verify {file.name}: {e}")
        
        return valid_packages
    
    def generate_download_report(self, results: Dict) -> Path:
        """Generate a report of downloaded packages"""
        report = {
            "download_date": datetime.now().isoformat(),
            "platform": platform.system(),
            "project_path": str(self.project_path),
            "statistics": {
                "total_packages": results["total"],
                "successful": results["success"],
                "failed": results["failed"],
                "total_size_mb": results.get("total_size_mb", 0)
            },
            "downloaded_files": results["files"]
        }
        
        report_file = self.download_path / f"download_report_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
        with open(report_file, 'w') as f:
            json.dump(report, f, indent=2)
        
        logger.info(f"\nGenerated report: {report_file.name}")
        return report_file


def main():
    parser = argparse.ArgumentParser(
        description="Download Unity packages from Google Drive links in DownloadInstructions.txt files"
    )
    parser.add_argument(
        "--project-path",
        type=Path,
        default=Path.cwd(),
        help="Path to Unity project (default: current directory)"
    )
    parser.add_argument(
        "--packages",
        nargs="+",
        help="Specific packages to download (default: all with Google Drive links)"
    )
    parser.add_argument(
        "--verify-only",
        action="store_true",
        help="Only verify existing downloads, don't download new files"
    )
    parser.add_argument(
        "--max-size",
        type=float,
        default=5.0,
        help="Maximum file size in GB (default: 5.0)"
    )
    
    args = parser.parse_args()
    
    # Initialize downloader
    downloader = UnityPackageDownloader(args.project_path)
    
    if args.verify_only:
        logger.info("Verifying existing downloads...")
        valid = downloader.verify_downloads()
        logger.info(f"Found {len(valid)} valid packages:")
        for package in valid:
            size_mb = package.stat().st_size / (1024 * 1024)
            logger.info(f"  - {package.name} ({size_mb:.2f} MB)")
        return
    
    # Start download process
    start_time = time.time()
    logger.info("=" * 60)
    logger.info("Unity Package Downloader")
    logger.info("=" * 60)
    
    # Download packages
    results = downloader.download_all(args.packages)
    
    # Generate report
    if results["total"] > 0:
        downloader.generate_download_report(results)
        
        # Print summary
        elapsed_time = time.time() - start_time
        logger.info("\n" + "=" * 60)
        logger.info("Download Summary:")
        logger.info(f"  Total packages: {results['total']}")
        logger.info(f"  Successful: {results['success']}")
        logger.info(f"  Failed: {results['failed']}")
        logger.info(f"  Total size: {results['total_size_mb']:.2f} MB")
        logger.info(f"  Time elapsed: {elapsed_time:.2f} seconds")
        
        if results['total_size_mb'] > 0 and elapsed_time > 0:
            speed_mbps = (results['total_size_mb'] * 8) / elapsed_time
            logger.info(f"  Average speed: {speed_mbps:.2f} Mbps")
        
        # Verify downloads
        logger.info("\nVerifying downloads...")
        valid = downloader.verify_downloads()
        logger.info(f"Verified {len(valid)} valid packages")
    else:
        logger.info("No packages to download")
    
    logger.info("\nDownload process completed")


if __name__ == "__main__":
    main()