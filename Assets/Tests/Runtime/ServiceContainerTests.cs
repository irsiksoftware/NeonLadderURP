using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System;
using NeonLadder.Core.ServiceContainer;
using NeonLadder.Core.ServiceContainer.Services;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class ServiceContainerTests
    {
        private ServiceContainer container;
        private GameObject containerObject;
        
        [SetUp]
        public void Setup()
        {
            // Create service container
            containerObject = new GameObject("TestServiceContainer");
            container = containerObject.AddComponent<ServiceContainer>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (container != null)
            {
                container.Shutdown();
            }
            
            if (containerObject != null)
            {
                Object.DestroyImmediate(containerObject);
            }
        }
        
        #region Basic Registration Tests
        
        [Test]
        public void ServiceContainer_RegisterService_Success()
        {
            // Arrange
            var testService = new TestService();
            
            // Act
            container.Register<TestService>(testService);
            
            // Assert
            Assert.IsTrue(container.Has<TestService>());
            Assert.IsTrue(testService.IsInitialized);
        }
        
        [Test]
        public void ServiceContainer_RegisterByInterface_Success()
        {
            // Arrange
            var testService = new TestServiceImplementation();
            
            // Act
            container.Register<ITestServiceInterface, TestServiceImplementation>(testService);
            
            // Assert
            Assert.IsTrue(container.Has<ITestServiceInterface>());
            var retrieved = container.Get<ITestServiceInterface>();
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(testService, retrieved);
        }
        
        [Test]
        public void ServiceContainer_GetService_ReturnsCorrectInstance()
        {
            // Arrange
            var testService = new TestService();
            container.Register<TestService>(testService);
            
            // Act
            var retrieved = container.Get<TestService>();
            
            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(testService, retrieved);
        }
        
        [Test]
        public void ServiceContainer_GetUnregisteredService_ReturnsNull()
        {
            // Act
            var retrieved = container.Get<TestService>();
            
            // Assert
            Assert.IsNull(retrieved);
        }
        
        [Test]
        public void ServiceContainer_TryGetService_Success()
        {
            // Arrange
            var testService = new TestService();
            container.Register<TestService>(testService);
            
            // Act
            bool result = container.TryGet<TestService>(out var retrieved);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(testService, retrieved);
        }
        
        [Test]
        public void ServiceContainer_TryGetUnregisteredService_ReturnsFalse()
        {
            // Act
            bool result = container.TryGet<TestService>(out var retrieved);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(retrieved);
        }
        
        #endregion
        
        #region Service Lifecycle Tests
        
        [Test]
        public void ServiceContainer_InitializeServices_CallsOnServicesReady()
        {
            // Arrange
            var testService = new TestService();
            container.Register<TestService>(testService);
            
            // Act
            container.InitializeServices();
            
            // Assert
            Assert.IsTrue(testService.ServicesReady);
        }
        
        [Test]
        public void ServiceContainer_RegisterAfterInit_CallsOnServicesReady()
        {
            // Arrange
            container.InitializeServices();
            var testService = new TestService();
            
            // Act
            container.Register<TestService>(testService);
            
            // Assert
            Assert.IsTrue(testService.ServicesReady);
        }
        
        [Test]
        public void ServiceContainer_Unregister_RemovesService()
        {
            // Arrange
            var testService = new TestService();
            container.Register<TestService>(testService);
            
            // Act
            container.Unregister<TestService>();
            
            // Assert
            Assert.IsFalse(container.Has<TestService>());
            Assert.IsTrue(testService.IsShutdown);
        }
        
        [Test]
        public void ServiceContainer_Shutdown_ClearsAllServices()
        {
            // Arrange
            var service1 = new TestService();
            var service2 = new TestService();
            container.Register<TestService>(service1);
            container.Register<ITestServiceInterface, TestServiceImplementation>(new TestServiceImplementation());
            
            // Act
            container.Shutdown();
            
            // Assert
            Assert.IsFalse(container.Has<TestService>());
            Assert.IsFalse(container.Has<ITestServiceInterface>());
            Assert.IsTrue(service1.IsShutdown);
        }
        
        #endregion
        
        #region Updatable Service Tests
        
        [UnityTest]
        public IEnumerator ServiceContainer_UpdatableService_ReceivesUpdates()
        {
            // Arrange
            var updatableService = new TestUpdatableService();
            container.Register<TestUpdatableService>(updatableService);
            updatableService.IsActive = true;
            
            int initialUpdateCount = updatableService.UpdateCount;
            
            // Act - Wait for a few frames
            yield return null;
            yield return null;
            yield return null;
            
            // Assert
            Assert.Greater(updatableService.UpdateCount, initialUpdateCount);
        }
        
        [UnityTest]
        public IEnumerator ServiceContainer_InactiveUpdatableService_NoUpdates()
        {
            // Arrange
            var updatableService = new TestUpdatableService();
            container.Register<TestUpdatableService>(updatableService);
            updatableService.IsActive = false;
            
            int initialUpdateCount = updatableService.UpdateCount;
            
            // Act - Wait for a few frames
            yield return null;
            yield return null;
            yield return null;
            
            // Assert
            Assert.AreEqual(initialUpdateCount, updatableService.UpdateCount);
        }
        
        #endregion
        
        #region Scene-Aware Service Tests
        
        [Test]
        public void ServiceContainer_SceneAwareService_ReceivesSceneChanges()
        {
            // Arrange
            var sceneService = new TestSceneAwareService();
            container.Register<TestSceneAwareService>(sceneService);
            
            // Act
            container.NotifySceneChange("Scene1", "Scene2");
            
            // Assert
            Assert.AreEqual("Scene1", sceneService.LastPreviousScene);
            Assert.AreEqual("Scene2", sceneService.LastNewScene);
            Assert.AreEqual(1, sceneService.SceneChangeCount);
        }
        
        [Test]
        public void ServiceContainer_SceneAwareUpdatable_UpdatesActiveState()
        {
            // Arrange
            var service = new TestSceneAwareUpdatableService();
            service.SetActiveScenes("Scene2", "Scene3");
            container.Register<TestSceneAwareUpdatableService>(service);
            
            // Act & Assert
            container.NotifySceneChange("Scene1", "Scene2");
            Assert.IsTrue(service.IsActive);
            
            container.NotifySceneChange("Scene2", "Scene4");
            Assert.IsFalse(service.IsActive);
            
            container.NotifySceneChange("Scene4", "Scene3");
            Assert.IsTrue(service.IsActive);
        }
        
        #endregion
        
        #region Thread Safety Tests
        
        [Test]
        public void ServiceContainer_ConcurrentRegistration_ThreadSafe()
        {
            // Arrange
            var services = new TestService[10];
            for (int i = 0; i < services.Length; i++)
            {
                services[i] = new TestService();
            }
            
            // Act - Register services concurrently
            var tasks = new System.Threading.Tasks.Task[services.Length];
            for (int i = 0; i < services.Length; i++)
            {
                int index = i;
                tasks[i] = System.Threading.Tasks.Task.Run(() =>
                {
                    container.Register<IService>(services[index]);
                });
            }
            
            System.Threading.Tasks.Task.WaitAll(tasks);
            
            // Assert - Last registered service should be retrievable
            var retrieved = container.Get<IService>();
            Assert.IsNotNull(retrieved);
            Assert.Contains(retrieved, services);
        }
        
        #endregion
        
        #region Diagnostics Tests
        
        [Test]
        public void ServiceContainer_GetDiagnostics_ReturnsInfo()
        {
            // Arrange
            container.Register<TestService>(new TestService());
            container.Register<TestUpdatableService>(new TestUpdatableService());
            container.Register<TestSceneAwareService>(new TestSceneAwareService());
            
            // Act
            string diagnostics = container.GetDiagnostics();
            
            // Assert
            Assert.IsNotNull(diagnostics);
            Assert.IsTrue(diagnostics.Contains("Total Services: 3"));
            Assert.IsTrue(diagnostics.Contains("Updatable Services: 1"));
            Assert.IsTrue(diagnostics.Contains("Scene-Aware Services: 1"));
        }
        
        #endregion
        
        #region Test Service Classes
        
        private class TestService : IService
        {
            public bool IsInitialized { get; private set; }
            public bool ServicesReady { get; private set; }
            public bool IsShutdown { get; private set; }
            
            public void Initialize()
            {
                IsInitialized = true;
            }
            
            public void OnServicesReady()
            {
                ServicesReady = true;
            }
            
            public void Shutdown()
            {
                IsShutdown = true;
            }
        }
        
        private interface ITestServiceInterface : IService
        {
            string GetValue();
        }
        
        private class TestServiceImplementation : ITestServiceInterface
        {
            public void Initialize() { }
            public void OnServicesReady() { }
            public void Shutdown() { }
            public string GetValue() => "TestValue";
        }
        
        private class TestUpdatableService : IUpdatableService
        {
            public bool IsActive { get; set; }
            public int UpdateCount { get; private set; }
            
            public void Initialize() { }
            public void OnServicesReady() { }
            public void Shutdown() { }
            
            public void Update()
            {
                UpdateCount++;
            }
        }
        
        private class TestSceneAwareService : ISceneAwareService
        {
            public string LastPreviousScene { get; private set; }
            public string LastNewScene { get; private set; }
            public int SceneChangeCount { get; private set; }
            
            public void Initialize() { }
            public void OnServicesReady() { }
            public void Shutdown() { }
            
            public void OnSceneChanged(string previousScene, string newScene)
            {
                LastPreviousScene = previousScene;
                LastNewScene = newScene;
                SceneChangeCount++;
            }
            
            public bool IsActiveInScene(string sceneName)
            {
                return true;
            }
        }
        
        private class TestSceneAwareUpdatableService : ISceneAwareService, IUpdatableService
        {
            private string[] activeScenes = new string[0];
            
            public bool IsActive { get; set; }
            
            public void SetActiveScenes(params string[] scenes)
            {
                activeScenes = scenes;
            }
            
            public void Initialize() { }
            public void OnServicesReady() { }
            public void Shutdown() { }
            public void Update() { }
            
            public void OnSceneChanged(string previousScene, string newScene)
            {
                // Update handled by container
            }
            
            public bool IsActiveInScene(string sceneName)
            {
                return Array.IndexOf(activeScenes, sceneName) >= 0;
            }
        }
        
        #endregion
    }
}