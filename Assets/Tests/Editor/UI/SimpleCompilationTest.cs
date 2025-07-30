using UnityEngine;
using UnityEditor;
using NUnit.Framework;

namespace NeonLadder.Tests.Editor.UI
{
    /// <summary>
    /// Simple test to verify basic compilation and assembly references work
    /// </summary>
    [TestFixture]
    public class SimpleCompilationTest
    {
        [Test]
        public void SimpleTest_ShouldCompileAndRun()
        {
            // This is just to verify our test assembly compiles correctly
            Assert.AreEqual(2, 1 + 1, "Basic math should work");
        }
        
        [Test] 
        public void UnityEditor_ShouldBeAccessible()
        {
            // Verify we can access Unity Editor APIs
            Assert.IsTrue(EditorApplication.isPlaying == false || EditorApplication.isPlaying == true, 
                "Should be able to access EditorApplication");
        }
    }
}