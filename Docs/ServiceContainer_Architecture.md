# Service Container Architecture

## Overview
The Service Container replaces the singleton ManagerController pattern with a dependency injection system that provides better testability, loose coupling, and maintainability.

## Architecture Components

### Core Components
- **ServiceContainer.cs** - Main container managing service lifecycle
- **IService** - Base interface for all services
- **IUpdatableService** - Services requiring Update() calls
- **ISceneAwareService** - Services with scene-specific behavior

### Benefits
1. **Testability** - Services can be mocked and tested in isolation
2. **Thread Safety** - All operations are thread-safe with proper locking
3. **Loose Coupling** - Components depend on interfaces, not implementations
4. **Lifecycle Management** - Automatic initialization and cleanup
5. **Scene Awareness** - Services automatically respond to scene changes

## Migration Guide

### Step 1: Enable Service Container
In your scene, replace `ManagerController` with `RefactoredManagerController` and set `useServiceContainer = true`.

### Step 2: Update Component References
Replace direct ManagerController references:

**Before:**
```csharp
ManagerController.Instance.eventManager.TriggerEvent("OnTriggerEnter", gameObject, other);
```

**After (Option 1 - Direct Service Access):**
```csharp
var eventService = ServiceContainer.Instance.Get<IEventService>();
eventService.TriggerEvent("OnTriggerEnter", other);
```

**After (Option 2 - Migration Helper):**
```csharp
var eventService = ServiceMigrationHelper.GetManagerService<IEventService>();
eventService.TriggerEvent("OnTriggerEnter", other);
```

**After (Option 3 - Extension Method):**
```csharp
var eventService = this.GetService<IEventService>();
eventService.TriggerEvent("OnTriggerEnter", other);
```

### Step 3: Mark Migrated Components
Add the attribute to indicate migration:
```csharp
[ServiceContainerMigrated("2025-08-10", "Claude")]
public class MyComponent : MonoBehaviour
{
    // Component code
}
```

## Service Interfaces

### IEventService
- `TriggerEvent(string eventName, object data)`
- `RegisterListener(string eventName, Action<object> listener)`
- `UnregisterListener(string eventName, Action<object> listener)`

### IDialogueService
- `StartDialogue(string dialogueId)`
- `AdvanceDialogue()`
- `IsDialogueActive()`

### ILootService
- `DropLoot(Vector3 position, int amount)`
- `PurchaseItem(string itemId)`

### ICameraService
- `SetCameraPosition(Vector3 position)`
- `ResetCameraPosition()`
- `EmptySceneStates()`

### IGameControllerService
- `SetControllerEnabled(bool enabled)`

### ISceneChangeService
- `ChangeScene(string sceneName)`
- `AssignSceneExit(string exitId, string targetScene)`
- `CycleToNextScene()`

### IMonsterGroupService
- `ActivateMonsterGroup(string groupId)`
- `DeactivateAllGroups()`

### IEnemyDefeatedService
- `RegisterEnemyDefeat(string enemyId)`

## Testing

### Unit Test Example
```csharp
[Test]
public void MyComponent_UsesServiceContainer()
{
    // Arrange
    var container = new ServiceContainer();
    var mockEventService = new Mock<IEventService>();
    container.Register<IEventService>(mockEventService.Object);
    
    // Act
    var component = new MyComponent();
    component.DoSomething();
    
    // Assert
    mockEventService.Verify(x => x.TriggerEvent(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
}
```

### Integration Test Example
```csharp
[UnityTest]
public IEnumerator ServiceContainer_IntegrationTest()
{
    // Arrange
    var managerController = GameObject.Instantiate(managerPrefab);
    var refactored = managerController.GetComponent<RefactoredManagerController>();
    
    // Act
    yield return null; // Wait for initialization
    
    // Assert
    Assert.IsTrue(ServiceContainer.Instance.Has<IEventService>());
}
```

## Performance Considerations
- Service lookups are O(1) using dictionary
- Thread-safe operations have minimal overhead
- Update calls only made to active services
- Scene change notifications are batched

## Backward Compatibility
The system maintains backward compatibility during migration:
1. ServiceMigrationHelper provides fallback to ManagerController.Instance
2. RefactoredManagerController can operate in legacy mode
3. Components can be migrated incrementally

## Future Enhancements
- Async service initialization
- Service dependencies and ordering
- Service pooling for frequently created/destroyed services
- Performance metrics and diagnostics
- Editor tools for service visualization