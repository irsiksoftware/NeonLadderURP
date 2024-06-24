using UnityEngine;

namespace NeonLadder.Utilities
{
    /// <summary>
    /// This class hides the GameObject it is attached to based on the specified platforms.
    /// </summary>
    public class PlatformVisibilityController : MonoBehaviour
    {
        public bool hideOnWebGL = false;
        public bool hideOnAndroid = false;
        public bool hideOnIOS = false;
        public bool hideOnWindows = false;
        public bool hideOnMacOS = false;
        public bool hideOnLinux = false;
        public bool hideOnEditor = false;

        void Awake()
        {
            if (Application.isEditor && hideOnEditor)
            {
                gameObject.SetActive(false);
            }
            else
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WebGLPlayer:
                        if (hideOnWebGL)
                        {
                            gameObject.SetActive(false);
                        }
                        break;

                    case RuntimePlatform.Android:
                        if (hideOnAndroid)
                        {
                            gameObject.SetActive(false);
                        }
                        break;

                    case RuntimePlatform.IPhonePlayer:
                        if (hideOnIOS)
                        {
                            gameObject.SetActive(false);
                        }
                        break;

                    case RuntimePlatform.WindowsPlayer:
                        if (hideOnWindows)
                        {
                            gameObject.SetActive(false);
                        }
                        break;

                    case RuntimePlatform.OSXPlayer:
                        if (hideOnMacOS)
                        {
                            gameObject.SetActive(false);
                        }
                        break;

                    case RuntimePlatform.LinuxPlayer:
                        if (hideOnLinux)
                        {
                            gameObject.SetActive(false);
                        }
                        break;
                }
            }
        }
    }
}
