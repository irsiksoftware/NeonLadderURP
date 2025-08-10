using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.Core.ServiceContainer;
using NeonLadder.Core.ServiceContainer.Services;
using NeonLadder.Managers;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class ServiceContainerTests
    {
        private ServiceLocator serviceContainer;
        
        [SetUp]
        public void Setup()
        {
            // Reset the global instance for each test
            ServiceLocator.Reset();
            serviceContainer = ServiceLocator.CreateNew();
        }
        
        [TearDown]
        public void TearDown()
        {
            serviceContainer?.Clear();
            ServiceLocator.Reset();
        }
        
        [Test]
        public void ServiceLocator_RegisterAndRetrieveService_Success()
        {
            // Arrange
            var testService = new TestGameService();
            
            // Act
            serviceContainer.Register<ITestService>(testService);
            var retrievedService = serviceContainer.Get<ITestService>();
            
            // Assert
            Assert.NotNull(retrievedService);
            Assert.AreEqual(testService, retrievedService);
        }
        
        [Test]
        public void ServiceLocator_TryGetUnregisteredService_ReturnsFalse()
        {
            // Act
            bool result = serviceContainer.TryGet<ITestService>(out var service);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(service);
        }
        
        [Test]
        public void ServiceLocator_RegisterMultipleServices_AllRetrievable()
        {
            // Arrange
            var service1 = new TestGameService();
            var service2 = new AnotherTestService();
            
            // Act
            serviceContainer.Register<ITestService>(service1);
            serviceContainer.Register<IAnotherTestService>(service2);
            
            // Assert
            Assert.NotNull(serviceContainer.Get<ITestService>());
            Assert.NotNull(serviceContainer.Get<IAnotherTestService>());
            Assert.AreEqual(service1, serviceContainer.Get<ITestService>());
            Assert.AreEqual(service2, serviceContainer.Get<IAnotherTestService>());
        }
        
        [Test]
        public void ServiceLocator_UnregisterService_NotRetrievable()
        {
            // Arrange
            var testService = new TestGameService();
            serviceContainer.Register<ITestService>(testService);
            
            // Act
            serviceContainer.Unregister<ITestService>();
            bool result = serviceContainer.TryGet<ITestService>(out var service);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(service);
        }
        
        [Test]
        public void ServiceLocator_ClearServices_AllServicesRemoved()
        {
            // Arrange
            serviceContainer.Register<ITestService>(new TestGameService());
            serviceContainer.Register<IAnotherTestService>(new AnotherTestService());
            
            // Act
            serviceContainer.Clear();
            
            // Assert
            Assert.IsFalse(serviceContainer.IsRegistered<ITestService>());
            Assert.IsFalse(serviceContainer.IsRegistered<IAnotherTestService>());
        }
        
        [Test]
        public void ServiceLocator_RegisterNull_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => 
                serviceContainer.Register<ITestService>(null));
        }
        
        [Test]
        public void ServiceLocator_GetUnregisteredService_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<System.InvalidOperationException>(() => 
                serviceContainer.Get<ITestService>());
        }
        
        [Test]
        public void ServiceLocator_OverwriteRegisteredService_NewServiceRetrieved()
        {
            // Arrange
            var service1 = new TestGameService { TestValue = "First" };
            var service2 = new TestGameService { TestValue = "Second" };
            
            // Act
            serviceContainer.Register<ITestService>(service1);
            serviceContainer.Register<ITestService>(service2); // Overwrite
            var retrievedService = serviceContainer.Get<ITestService>() as TestGameService;
            
            // Assert
            Assert.NotNull(retrievedService);
            Assert.AreEqual("Second", retrievedService.TestValue);
        }
        
        [Test]
        public void ServiceLocator_GlobalInstance_SharedAcrossContexts()
        {
            // Arrange
            var globalInstance1 = ServiceLocator.Instance;
            var globalInstance2 = ServiceLocator.Instance;
            
            // Act
            globalInstance1.Register<ITestService>(new TestGameService());
            
            // Assert
            Assert.AreEqual(globalInstance1, globalInstance2);
            Assert.IsTrue(globalInstance2.IsRegistered<ITestService>());
        }
        
        [Test]
        public void GameService_InitializeAndShutdown_ProperStateTransitions()
        {
            // Arrange
            var service = new TestGameService();
            
            // Act & Assert - Initial state
            Assert.IsFalse(service.IsInitialized);
            
            // Act & Assert - After initialization
            service.Initialize();
            Assert.IsTrue(service.IsInitialized);
            
            // Act & Assert - After shutdown
            service.Shutdown();
            Assert.IsFalse(service.IsInitialized);
        }
        
        [UnityTest]
        public IEnumerator GameServiceManager_Integration_RegistersServices()
        {
            // Arrange
            var gameObject = new GameObject("GameServiceManager");
            var serviceManager = gameObject.AddComponent<GameServiceManager>();
            
            // Wait for Awake to be called
            yield return null;
            
            // Assert - GameServiceManager should be registered
            Assert.IsTrue(ServiceLocator.Instance.IsRegistered<GameServiceManager>());
            
            // Cleanup
            Object.Destroy(gameObject);
        }
        
        [Test]
        public void MigrationHelper_GetEventManager_FallbackBehavior()
        {
            // Arrange
            var mockEventManager = new GameObject("EventManager").AddComponent<EventManager>();
            ServiceLocator.Instance.Register<EventManager>(mockEventManager);
            
            // Act
            var retrievedManager = ManagerControllerMigration.GetEventManager();
            
            // Assert
            Assert.NotNull(retrievedManager);
            Assert.AreEqual(mockEventManager, retrievedManager);
            
            // Cleanup
            Object.Destroy(mockEventManager.gameObject);
        }
        
        // Test interfaces and implementations
        private interface ITestService : IGameService
        {
            string TestValue { get; set; }
        }
        
        private interface IAnotherTestService : IGameService
        {
            int TestNumber { get; set; }
        }
        
        private class TestGameService : ITestService
        {
            public string TestValue { get; set; }
            public bool IsInitialized { get; private set; }
            
            public void Initialize()
            {
                IsInitialized = true;
            }
            
            public void Shutdown()
            {
                IsInitialized = false;
            }
        }
        
        private class AnotherTestService : IAnotherTestService
        {
            public int TestNumber { get; set; }
            public bool IsInitialized { get; private set; }
            
            public void Initialize()
            {
                IsInitialized = true;
            }
            
            public void Shutdown()
            {
                IsInitialized = false;
            }
        }
    }
}