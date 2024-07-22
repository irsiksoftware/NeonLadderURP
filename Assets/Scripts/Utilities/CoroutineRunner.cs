using System.Collections;
using UnityEngine;

namespace NeonLadder.Utilities
{
    public static class CoroutineRunner
    {
        private class CoroutineRunnerBehaviour : MonoBehaviour { }

        private static CoroutineRunnerBehaviour runner;

        private static void Initialize()
        {
            if (runner == null)
            {
                GameObject runnerObject = new GameObject("CoroutineRunner");
                runner = runnerObject.AddComponent<CoroutineRunnerBehaviour>();
                Object.DontDestroyOnLoad(runnerObject);
            }
        }

        public static void RunCoroutine(IEnumerator coroutine)
        {
            Initialize();
            runner.StartCoroutine(coroutine);
        }
    }
}
