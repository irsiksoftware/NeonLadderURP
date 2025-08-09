#!/usr/bin/env python3
"""
Unit tests for Unity package automation system
Tests export, download, and sync functionality
"""

import unittest
import tempfile
import shutil
import json
from pathlib import Path
from unittest.mock import Mock, patch, MagicMock
import sys
import os

# Add Scripts directory to path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

# Import modules to test
import export_unity_packages
import download_unity_packages
import sync_unity_packages


class TestGoogleDriveDownloader(unittest.TestCase):
    """Test Google Drive download functionality"""
    
    def setUp(self):
        self.temp_dir = tempfile.mkdtemp()
        self.download_path = Path(self.temp_dir) / "downloads"
        self.downloader = download_unity_packages.GoogleDriveDownloader(self.download_path)
    
    def tearDown(self):
        shutil.rmtree(self.temp_dir)
    
    def test_extract_file_id_standard_format(self):
        """Test extracting file ID from standard Google Drive URL"""
        url = "https://drive.google.com/file/d/1kEu526WZH449qsZyLNYIadSMYfTf9x_m/view?usp=drive_link"
        file_id = self.downloader.extract_file_id(url)
        self.assertEqual(file_id, "1kEu526WZH449qsZyLNYIadSMYfTf9x_m")
    
    def test_extract_file_id_old_format(self):
        """Test extracting file ID from old format URL"""
        url = "https://drive.google.com/open?id=1kEu526WZH449qsZyLNYIadSMYfTf9x_m"
        file_id = self.downloader.extract_file_id(url)
        self.assertEqual(file_id, "1kEu526WZH449qsZyLNYIadSMYfTf9x_m")
    
    def test_extract_file_id_invalid(self):
        """Test handling invalid URLs"""
        url = "https://example.com/not-a-drive-link"
        file_id = self.downloader.extract_file_id(url)
        self.assertIsNone(file_id)
    
    def test_get_direct_download_url(self):
        """Test generating direct download URL"""
        file_id = "test123"
        url = self.downloader.get_direct_download_url(file_id)
        self.assertEqual(url, "https://drive.google.com/uc?export=download&id=test123")
    
    @patch('urllib.request.urlopen')
    def test_download_file_success(self, mock_urlopen):
        """Test successful file download"""
        mock_response = MagicMock()
        mock_response.read.return_value = b"test content"
        mock_urlopen.return_value.__enter__.return_value = mock_response
        
        url = "https://drive.google.com/file/d/test123/view"
        result = self.downloader.download_file(url, "test.unitypackage", max_size_gb=1.0)
        
        self.assertIsNotNone(result)
        self.assertTrue(result.exists())
        self.assertEqual(result.read_bytes(), b"test content")


class TestUnityPackageExporter(unittest.TestCase):
    """Test Unity package export functionality"""
    
    def setUp(self):
        self.temp_dir = tempfile.mkdtemp()
        self.project_path = Path(self.temp_dir)
        
        # Create mock project structure
        (self.project_path / "Assets" / "Packages").mkdir(parents=True)
        (self.project_path / "Assets" / "Scripts").mkdir(parents=True)
        
    def tearDown(self):
        shutil.rmtree(self.temp_dir)
    
    def test_discover_packages(self):
        """Test package discovery"""
        # Create test packages with DownloadInstructions.txt
        pkg1 = self.project_path / "Assets" / "Packages" / "TestPackage1"
        pkg1.mkdir()
        (pkg1 / "DownloadInstructions.txt").write_text("Test instructions")
        
        pkg2 = self.project_path / "Assets" / "Packages" / "TestPackage2"
        pkg2.mkdir()
        (pkg2 / "DownloadInstructions.txt").write_text("Test instructions")
        
        # Package without instructions (should not be discovered)
        pkg3 = self.project_path / "Assets" / "Packages" / "TestPackage3"
        pkg3.mkdir()
        
        exporter = export_unity_packages.UnityPackageExporter(
            self.project_path, 
            dry_run=True
        )
        packages = exporter.discover_packages()
        
        self.assertEqual(len(packages), 2)
        package_names = [p["name"] for p in packages]
        self.assertIn("TestPackage1", package_names)
        self.assertIn("TestPackage2", package_names)
        self.assertNotIn("TestPackage3", package_names)
    
    def test_check_unity_running_mock(self):
        """Test Unity running check"""
        exporter = export_unity_packages.UnityPackageExporter(
            self.project_path,
            dry_run=True
        )
        
        with patch('subprocess.run') as mock_run:
            # Simulate Unity not running
            mock_run.return_value.returncode = 1
            mock_run.return_value.stdout = ""
            
            is_running = exporter.check_unity_running()
            self.assertFalse(is_running)
    
    def test_config_save_load(self):
        """Test configuration persistence"""
        exporter = export_unity_packages.UnityPackageExporter(
            self.project_path,
            dry_run=True
        )
        
        # Modify config
        exporter.config["test_key"] = "test_value"
        exporter.config["packages_to_export"] = ["Package1", "Package2"]
        exporter._save_config()
        
        # Create new exporter and check config loaded
        exporter2 = export_unity_packages.UnityPackageExporter(
            self.project_path,
            dry_run=True
        )
        
        self.assertEqual(exporter2.config["test_key"], "test_value")
        self.assertEqual(exporter2.config["packages_to_export"], ["Package1", "Package2"])


class TestUnityPackageDownloader(unittest.TestCase):
    """Test Unity package download functionality"""
    
    def setUp(self):
        self.temp_dir = tempfile.mkdtemp()
        self.project_path = Path(self.temp_dir)
        
        # Create mock project structure
        (self.project_path / "Assets" / "Packages" / "TestPkg").mkdir(parents=True)
        (self.project_path / "Assets" / "Audio" / "Background").mkdir(parents=True)
        
    def tearDown(self):
        shutil.rmtree(self.temp_dir)
    
    def test_find_download_instructions(self):
        """Test finding DownloadInstructions.txt files with links"""
        # Create test file with Google Drive link
        instructions_file = self.project_path / "Assets" / "Packages" / "TestPkg" / "DownloadInstructions.txt"
        instructions_file.write_text(
            "Download from:\nhttps://drive.google.com/file/d/test123/view?usp=sharing\n"
        )
        
        # Create file without link
        no_link_file = self.project_path / "Assets" / "Audio" / "Background" / "DownloadInstructions.txt"
        no_link_file.write_text("No link here")
        
        downloader = download_unity_packages.UnityPackageDownloader(self.project_path)
        instructions = downloader.find_download_instructions()
        
        self.assertEqual(len(instructions), 1)
        self.assertEqual(instructions[0]["name"], "TestPkg")
        self.assertIn("drive.google.com", instructions[0]["url"])
    
    def test_verify_downloads(self):
        """Test download verification"""
        downloader = download_unity_packages.UnityPackageDownloader(self.project_path)
        
        # Create test packages
        valid_pkg = downloader.download_path / "valid.unitypackage"
        valid_pkg.write_bytes(b"x" * 1000)  # Valid size
        
        invalid_pkg = downloader.download_path / "invalid.unitypackage"
        invalid_pkg.write_bytes(b"x" * 50)  # Too small
        
        valid_packages = downloader.verify_downloads()
        
        self.assertEqual(len(valid_packages), 1)
        self.assertEqual(valid_packages[0].name, "valid.unitypackage")


class TestPackageSync(unittest.TestCase):
    """Test package sync functionality"""
    
    def setUp(self):
        self.temp_dir = tempfile.mkdtemp()
        self.project_path = Path(self.temp_dir)
        
        # Create mock project structure
        (self.project_path / "Assets" / "Packages" / "TestPackage").mkdir(parents=True)
        (self.project_path / ".claude").mkdir()
        
    def tearDown(self):
        shutil.rmtree(self.temp_dir)
    
    def test_find_packages_to_sync(self):
        """Test finding packages that need syncing"""
        # Package without link (needs sync)
        pkg1 = self.project_path / "Assets" / "Packages" / "NeedsSync"
        pkg1.mkdir()
        (pkg1 / "DownloadInstructions.txt").write_text("No link yet")
        
        # Package with link (already synced)
        pkg2 = self.project_path / "Assets" / "Packages" / "AlreadySynced"
        pkg2.mkdir()
        (pkg2 / "DownloadInstructions.txt").write_text(
            "https://drive.google.com/file/d/existing/view"
        )
        
        syncer = sync_unity_packages.UnityPackageSync(self.project_path)
        packages = syncer.find_packages_to_sync()
        
        self.assertEqual(len(packages), 2)
        
        # Check sync status
        needs_sync = [p for p in packages if p["name"] == "NeedsSync"][0]
        already_synced = [p for p in packages if p["name"] == "AlreadySynced"][0]
        
        self.assertFalse(needs_sync["has_link"])
        self.assertTrue(already_synced["has_link"])
    
    def test_create_placeholder_package(self):
        """Test placeholder package creation"""
        syncer = sync_unity_packages.UnityPackageSync(self.project_path)
        
        package_path = self.project_path / "Assets" / "Packages" / "TestPackage"
        result = syncer.create_placeholder_package(package_path, "TestPackage")
        
        self.assertTrue(result.exists())
        self.assertTrue(result.name.endswith(".unitypackage"))
        self.assertGreater(result.stat().st_size, 0)
    
    def test_update_download_instructions(self):
        """Test updating DownloadInstructions.txt"""
        syncer = sync_unity_packages.UnityPackageSync(self.project_path)
        
        txt_file = self.project_path / "Assets" / "Packages" / "TestPackage" / "DownloadInstructions.txt"
        txt_file.write_text("Old content")
        
        syncer.update_download_instructions(
            txt_file,
            "https://drive.google.com/file/d/new123/view",
            "TestPackage"
        )
        
        content = txt_file.read_text()
        self.assertIn("https://drive.google.com/file/d/new123/view", content)
        self.assertIn("TestPackage", content)
        self.assertIn("Last Updated:", content)


class TestIntegration(unittest.TestCase):
    """Integration tests for the complete workflow"""
    
    def setUp(self):
        self.temp_dir = tempfile.mkdtemp()
        self.project_path = Path(self.temp_dir)
        
        # Create realistic project structure
        self._create_project_structure()
        
    def tearDown(self):
        shutil.rmtree(self.temp_dir)
    
    def _create_project_structure(self):
        """Create a realistic Unity project structure for testing"""
        # Create multiple packages
        packages = [
            "Assets/Packages/HeroEditor",
            "Assets/Packages/Synty",
            "Assets/Packages/LeartesStudios/CyberpunkCity",
            "Assets/Audio/Background"
        ]
        
        for pkg_path in packages:
            full_path = self.project_path / pkg_path
            full_path.mkdir(parents=True)
            
            # Add DownloadInstructions.txt
            instructions = full_path / "DownloadInstructions.txt"
            if "HeroEditor" in pkg_path:
                instructions.write_text(
                    "Download from:\nhttps://drive.google.com/file/d/hero123/view\n"
                )
            else:
                instructions.write_text("No link yet")
    
    def test_full_discovery_workflow(self):
        """Test complete package discovery across export and download"""
        # Test export discovery
        exporter = export_unity_packages.UnityPackageExporter(
            self.project_path,
            dry_run=True
        )
        export_packages = exporter.discover_packages()
        
        # Test download discovery
        downloader = download_unity_packages.UnityPackageDownloader(self.project_path)
        download_instructions = downloader.find_download_instructions()
        
        # Test sync discovery
        syncer = sync_unity_packages.UnityPackageSync(self.project_path)
        sync_packages = syncer.find_packages_to_sync()
        
        # Verify discoveries
        self.assertGreater(len(export_packages), 0)
        self.assertEqual(len(download_instructions), 1)  # Only HeroEditor has link
        self.assertGreater(len(sync_packages), 0)
        
        # Verify HeroEditor is found correctly
        hero_download = [d for d in download_instructions if "HeroEditor" in d["name"]]
        self.assertEqual(len(hero_download), 1)


def run_tests():
    """Run all tests with verbose output"""
    loader = unittest.TestLoader()
    suite = unittest.TestSuite()
    
    # Add all test classes
    suite.addTests(loader.loadTestsFromTestCase(TestGoogleDriveDownloader))
    suite.addTests(loader.loadTestsFromTestCase(TestUnityPackageExporter))
    suite.addTests(loader.loadTestsFromTestCase(TestUnityPackageDownloader))
    suite.addTests(loader.loadTestsFromTestCase(TestPackageSync))
    suite.addTests(loader.loadTestsFromTestCase(TestIntegration))
    
    runner = unittest.TextTestRunner(verbosity=2)
    result = runner.run(suite)
    
    return result.wasSuccessful()


if __name__ == "__main__":
    success = run_tests()
    sys.exit(0 if success else 1)