using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.ProceduralGeneration;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class SceneRouterTests
    {
        private SceneRouter sceneRouter;
        private SceneNameMapper sceneMapper;
        private SceneRoutingContext routingContext;
        
        [SetUp]
        public void SetUp()
        {
            GameObject routerGO = new GameObject("TestSceneRouter");
            sceneRouter = routerGO.AddComponent<SceneRouter>();
            sceneMapper = new SceneNameMapper();
            
            GameObject contextGO = new GameObject("TestSceneRoutingContext");
            routingContext = contextGO.AddComponent<SceneRoutingContext>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (sceneRouter != null)
            {
                Object.DestroyImmediate(sceneRouter.gameObject);
            }
            
            if (routingContext != null)
            {
                Object.DestroyImmediate(routingContext.gameObject);
            }
            
            // Clean up any test context objects
            var testContexts = GameObject.FindGameObjectsWithTag("Untagged");
            foreach (var go in testContexts)
            {
                if (go.name.StartsWith("TestContext"))
                {
                    Object.DestroyImmediate(go);
                }
            }
        }
        
        #region SceneRouter Basic Functionality Tests
        
        [Test]
        public void SceneRouter_Singleton_WorksCorrectly()
        {
            var instance1 = SceneRouter.Instance;
            var instance2 = SceneRouter.Instance;
            
            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2);
        }
        
        [Test]
        public void GetSceneNameFromMapNode_StartNode_ReturnsMainCityHub()
        {
            var startNode = new MapNode
            {
                Id = "START",
                Type = NodeType.Start,
                Properties = new Dictionary<string, object>()
            };
            
            string sceneName = sceneRouter.GetSceneNameFromMapNode(startNode);
            
            Assert.AreEqual("MainCityHub", sceneName);
        }
        
        [Test]
        public void GetSceneNameFromMapNode_BossNode_ReturnsBossScene()
        {
            var bossNode = new MapNode
            {
                Id = "BOSS_CATHEDRAL",
                Type = NodeType.Boss,
                Properties = new Dictionary<string, object>
                {
                    ["BossName"] = "cathedral"
                }
            };
            
            string sceneName = sceneRouter.GetSceneNameFromMapNode(bossNode);
            
            Assert.AreEqual("cathedral", sceneName);
        }
        
        [Test]
        public void GetSceneNameFromMapNode_RestShopNode_ReturnsShopOrRestArea()
        {
            var restShopNode = new MapNode
            {
                Id = "RESTSHOP_1",
                Type = NodeType.RestShop,
                Properties = new Dictionary<string, object>()
            };
            
            string sceneName = sceneRouter.GetSceneNameFromMapNode(restShopNode);
            
            Assert.IsTrue(sceneName == "Shop" || sceneName == "RestArea", 
                $"Expected Shop or RestArea, got {sceneName}");
        }
        
        [Test]
        public void GetSceneNameFromMapNode_EventNode_ReturnsServiceScene()
        {
            var eventNode = new MapNode
            {
                Id = "EVENT_PORTAL",
                Type = NodeType.Event,
                Properties = new Dictionary<string, object>
                {
                    ["EventType"] = "Portal"
                }
            };
            
            string sceneName = sceneRouter.GetSceneNameFromMapNode(eventNode);
            
            Assert.AreEqual("Portal", sceneName);
        }
        
        [Test]
        public void GetSceneNameFromMapNode_ConnectionNode_ReturnsConnectionScene()
        {
            var path = new List<MapNode>
            {
                new MapNode { Type = NodeType.Boss, Properties = new Dictionary<string, object> { ["BossName"] = "cathedral" } }
            };
            
            sceneRouter.SetCurrentPath(path);
            
            var connectionNode = new MapNode
            {
                Id = "CONNECTION_1",
                Type = NodeType.Connection,
                Properties = new Dictionary<string, object>
                {
                    ["ConnectionIndex"] = 1
                }
            };
            
            string sceneName = sceneRouter.GetSceneNameFromMapNode(connectionNode);
            
            Assert.AreEqual("cathedral_Connection1", sceneName);
        }
        
        [Test]
        public void GetSceneNameFromMapNode_NullNode_ReturnsNull()
        {
            string sceneName = sceneRouter.GetSceneNameFromMapNode(null);
            
            Assert.IsNull(sceneName);
        }
        
        #endregion
        
        #region SceneNameMapper Tests
        
        [Test]
        public void SceneNameMapper_GetBossSceneFromIdentifier_WorksCorrectly()
        {
            string sceneName = sceneMapper.GetBossSceneFromIdentifier("cathedral");
            Assert.AreEqual("cathedral", sceneName);
            
            sceneName = sceneMapper.GetBossSceneFromIdentifier("banquet");
            Assert.AreEqual("banquet", sceneName);
        }
        
        [Test]
        public void SceneNameMapper_GetServiceSceneFromEventType_AllEventsMap()
        {
            var expectedMappings = new Dictionary<string, string>
            {
                { "Portal", "Portal" },
                { "Shrine", "Shrine" },
                { "TreasureChest", "TreasureRoom" },
                { "MysteriousAltar", "MysteriousAltar" },
                { "Merchant", "Merchant" },
                { "Riddle", "RiddleRoom" }
            };
            
            foreach (var mapping in expectedMappings)
            {
                string sceneName = sceneMapper.GetServiceSceneFromEventType(mapping.Key);
                Assert.AreEqual(mapping.Value, sceneName, $"Event {mapping.Key} should map to {mapping.Value}");
            }
        }
        
        [Test]
        public void SceneNameMapper_GetConnectionSceneName_WorksCorrectly()
        {
            string connectionScene1 = sceneMapper.GetConnectionSceneName("cathedral", 1);
            string connectionScene2 = sceneMapper.GetConnectionSceneName("cathedral", 2);
            
            Assert.AreEqual("cathedral_Connection1", connectionScene1);
            Assert.AreEqual("cathedral_Connection2", connectionScene2);
        }
        
        [Test]
        public void SceneNameMapper_IsServiceScene_IdentifiesCorrectly()
        {
            Assert.IsTrue(sceneMapper.IsServiceScene("Shop"));
            Assert.IsTrue(sceneMapper.IsServiceScene("RestArea"));
            Assert.IsTrue(sceneMapper.IsServiceScene("Portal"));
            Assert.IsFalse(sceneMapper.IsServiceScene("Cathedral"));
            Assert.IsFalse(sceneMapper.IsServiceScene("MainCityHub"));
        }
        
        [Test]
        public void SceneNameMapper_IsBossScene_IdentifiesCorrectly()
        {
            Assert.IsTrue(sceneMapper.IsBossScene("cathedral"));
            Assert.IsTrue(sceneMapper.IsBossScene("finale"));
            Assert.IsFalse(sceneMapper.IsBossScene("Shop"));
            Assert.IsFalse(sceneMapper.IsBossScene("MainCityHub"));
        }
        
        [Test]
        public void SceneNameMapper_IsConnectionScene_IdentifiesCorrectly()
        {
            Assert.IsTrue(sceneMapper.IsConnectionScene("cathedral_Connection1"));
            Assert.IsTrue(sceneMapper.IsConnectionScene("banquet_Connection2"));
            Assert.IsFalse(sceneMapper.IsConnectionScene("cathedral"));
            Assert.IsFalse(sceneMapper.IsConnectionScene("Shop"));
        }
        
        #endregion
        
        #region SceneRoutingContext Tests
        
        [Test]
        public void SceneRoutingContext_InitializesCorrectly()
        {
            GameObject contextGO = new GameObject("TestContext");
            var context = contextGO.AddComponent<SceneRoutingContext>();
            
            Assert.AreEqual("", context.CurrentScene);
            Assert.AreEqual("", context.PreviousScene);
            Assert.IsNull(context.CurrentNode);
            Assert.IsNotNull(context.CurrentPath);
            Assert.IsNotNull(context.VisitedScenes);
            Assert.IsNotNull(context.PersistentData);
            Assert.AreEqual(0, context.CurrentPathIndex);
        }
        
        [Test]
        public void SceneRoutingContext_HasVisitedScene_WorksCorrectly()
        {
            GameObject contextGO = new GameObject("TestContext");
            var context = contextGO.AddComponent<SceneRoutingContext>();
            
            Assert.IsFalse(context.HasVisitedScene("Cathedral"));
            
            context.AddVisitedScene("Cathedral");
            Assert.IsTrue(context.HasVisitedScene("Cathedral"));
        }
        
        [Test]
        public void SceneRoutingContext_PersistentData_WorksCorrectly()
        {
            GameObject contextGO = new GameObject("TestContext");
            var context = contextGO.AddComponent<SceneRoutingContext>();
            
            context.SetPersistentData("TestKey", "TestValue");
            Assert.IsTrue(context.HasPersistentData("TestKey"));
            
            string value = context.GetPersistentData<string>("TestKey");
            Assert.AreEqual("TestValue", value);
            
            context.RemovePersistentData("TestKey");
            Assert.IsFalse(context.HasPersistentData("TestKey"));
        }
        
        [Test]
        public void SceneRoutingContext_PathNavigation_WorksCorrectly()
        {
            GameObject contextGO = new GameObject("TestContext");
            var context = contextGO.AddComponent<SceneRoutingContext>();
            var path = new List<MapNode>
            {
                new MapNode { Id = "Node1", Type = NodeType.Start },
                new MapNode { Id = "Node2", Type = NodeType.Connection },
                new MapNode { Id = "Node3", Type = NodeType.Boss }
            };
            
            context.CurrentPath = path;
            context.CurrentPathIndex = 1;
            
            Assert.AreEqual("Node2", context.GetNodeAtIndex(1).Id);
            Assert.AreEqual("Node3", context.GetNextNode().Id);
            Assert.AreEqual("Node1", context.GetPreviousNode().Id);
            
            Assert.IsFalse(context.IsAtPathStart());
            Assert.IsFalse(context.IsAtPathEnd());
            
            context.CurrentPathIndex = 0;
            Assert.IsTrue(context.IsAtPathStart());
            
            context.CurrentPathIndex = 2;
            Assert.IsTrue(context.IsAtPathEnd());
        }
        
        [Test]
        public void SceneRoutingContext_GetPathProgress_CalculatesCorrectly()
        {
            GameObject contextGO = new GameObject("TestContext");
            var context = contextGO.AddComponent<SceneRoutingContext>();
            var path = new List<MapNode>
            {
                new MapNode { Id = "Node1" },
                new MapNode { Id = "Node2" },
                new MapNode { Id = "Node3" },
                new MapNode { Id = "Node4" },
                new MapNode { Id = "Node5" }
            };
            
            context.CurrentPath = path;
            
            context.CurrentPathIndex = 0;
            Assert.AreEqual(0, context.GetPathProgress());
            
            context.CurrentPathIndex = 2;
            Assert.AreEqual(50, context.GetPathProgress());
            
            context.CurrentPathIndex = 4;
            Assert.AreEqual(100, context.GetPathProgress());
        }
        
        [Test]
        public void SceneRoutingContext_CreateAndRestoreSnapshot_WorksCorrectly()
        {
            GameObject contextGO = new GameObject("TestContext");
            var context = contextGO.AddComponent<SceneRoutingContext>();
            context.CurrentScene = "TestScene";
            context.PreviousScene = "PreviousScene";
            context.AddVisitedScene("Scene1");
            context.AddVisitedScene("Scene2");
            
            var snapshot = context.CreateSnapshot();
            
            Assert.AreEqual("TestScene", snapshot.CurrentScene);
            Assert.AreEqual("PreviousScene", snapshot.PreviousScene);
            Assert.AreEqual(2, snapshot.VisitedScenes.Count);
            
            context.Reset();
            context.RestoreFromSnapshot(snapshot);
            
            Assert.AreEqual("TestScene", context.CurrentScene);
            Assert.AreEqual("PreviousScene", context.PreviousScene);
            Assert.AreEqual(2, context.VisitedScenes.Count);
        }
        
        #endregion
        
        #region Deterministic Behavior Tests
        
        [Test]
        public void SceneRouter_DeterministicMapping_SameSeedProducesSameResults()
        {
            // Set a fixed seed for Random to ensure deterministic behavior
            Random.InitState(12345);
            
            var node1 = new MapNode
            {
                Type = NodeType.RestShop,
                Properties = new Dictionary<string, object>()
            };
            
            string result1 = sceneRouter.GetSceneNameFromMapNode(node1);
            
            // Reset with same seed
            Random.InitState(12345);
            
            string result2 = sceneRouter.GetSceneNameFromMapNode(node1);
            
            Assert.AreEqual(result1, result2, "Same seed should produce same scene selection");
        }
        
        [Test]
        public void SceneRouter_PathProgression_MaintainsCorrectOrder()
        {
            var path = new List<MapNode>
            {
                new MapNode { Id = "START", Type = NodeType.Start },
                new MapNode { Id = "CONN1", Type = NodeType.Connection },
                new MapNode { Id = "CONN2", Type = NodeType.Connection },
                new MapNode { Id = "BOSS", Type = NodeType.Boss, Properties = new Dictionary<string, object> { ["BossName"] = "cathedral" } }
            };
            
            sceneRouter.SetCurrentPath(path);
            
            var nextNode = sceneRouter.GetNextNodeInPath();
            Assert.AreEqual("CONN1", nextNode.Id);
            
            nextNode = sceneRouter.GetNextNodeInPath();
            Assert.AreEqual("CONN2", nextNode.Id);
            
            nextNode = sceneRouter.GetNextNodeInPath();
            Assert.AreEqual("BOSS", nextNode.Id);
            
            nextNode = sceneRouter.GetNextNodeInPath();
            Assert.IsNull(nextNode, "Should return null when at end of path");
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void SceneNameMapper_InvalidBossIdentifier_ReturnsNull()
        {
            string sceneName = sceneMapper.GetBossSceneFromIdentifier("NonexistentBoss");
            Assert.IsNull(sceneName);
        }
        
        [Test]
        public void SceneNameMapper_InvalidEventType_ReturnsNull()
        {
            string sceneName = sceneMapper.GetServiceSceneFromEventType("NonexistentEvent");
            Assert.IsNull(sceneName);
        }
        
        [Test]
        public void SceneNameMapper_EmptyInput_ReturnsNull()
        {
            Assert.IsNull(sceneMapper.GetBossSceneFromIdentifier(""));
            Assert.IsNull(sceneMapper.GetBossSceneFromIdentifier(null));
            Assert.IsNull(sceneMapper.GetServiceSceneFromEventType(""));
        }
        
        [Test]
        public void SceneRoutingContext_InvalidPathIndex_HandledGracefully()
        {
            GameObject contextGO = new GameObject("TestContext");
            var context = contextGO.AddComponent<SceneRoutingContext>();
            var path = new List<MapNode> { new MapNode { Id = "Test" } };
            context.CurrentPath = path;
            
            // Test out of bounds access
            Assert.IsNull(context.GetNodeAtIndex(-1));
            Assert.IsNull(context.GetNodeAtIndex(10));
            
            // Test setting invalid index
            context.CurrentPathIndex = -5;
            Assert.AreEqual(0, context.CurrentPathIndex);
        }
        
        #endregion
    }
}