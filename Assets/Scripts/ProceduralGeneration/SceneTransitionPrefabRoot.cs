using UnityEngine;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Component for the root SceneTransition prefab that provides configuration warnings and errors
    /// </summary>
    public class SceneTransitionPrefabRoot : MonoBehaviour
    {
        public enum Orientation
        {
            North,
            East,
            South,
            West
        }

        [Header("Scene Transition Configuration")]
        [SerializeField] private Orientation orientation = Orientation.North;
        [SerializeField] private bool showConfigurationHelp = true;
        
        [Space]
        [TextArea(3, 5)]
        [SerializeField] private string configurationNotes = "This is the root SceneTransition GameObject. Configure the SceneTransitionTrigger child component for direction and destination settings.";

        // Public property to access orientation
        public Orientation CurrentOrientation => orientation;

        private void Awake()
        {
            // Ensure this component stays on the root
            if (transform.parent != null)
            {
                Debug.LogWarning($"[SceneTransitionPrefabRoot] {gameObject.name} should be at root level of scene hierarchy!", this);
            }
            
            UpdateSpawnPointPosition();
        }

        private void OnValidate()
        {
            // Update spawn point position when orientation changes in editor
            if (!Application.isPlaying)
            {
                UpdateSpawnPointPosition();
            }
        }

        private void UpdateSpawnPointPosition()
        {
            // Find the spawn point child object
            Transform spawnPoint = transform.Find("Spawn Point");
            if (spawnPoint == null)
            {
                Debug.LogWarning($"[SceneTransitionPrefabRoot] No 'Spawn Point' child found in {gameObject.name}", this);
                return;
            }

            // Set position based on orientation
            // Player walks OVER the spawn point to reach the exit
            Vector3 newPosition = Vector3.zero;
            
            switch (orientation)
            {
                case Orientation.North:
                    // Exit is to the north, spawn point is south of exit
                    newPosition = new Vector3(0, 0, -1.5f);
                    break;
                case Orientation.East:
                    // Exit is to the east, spawn point is west of exit
                    newPosition = new Vector3(-1.5f, 0, 0);
                    break;
                case Orientation.South:
                    // Exit is to the south, spawn point is north of exit
                    newPosition = new Vector3(0, 0, 1.5f);
                    break;
                case Orientation.West:
                    // Exit is to the west, spawn point is east of exit
                    newPosition = new Vector3(1.5f, 0, 0);
                    break;
            }
            
            spawnPoint.localPosition = newPosition;
        }

        /// <summary>
        /// Validate the scene transition configuration
        /// </summary>
        public bool ValidateConfiguration(out string[] warnings, out string[] errors)
        {
            var warningsList = new System.Collections.Generic.List<string>();
            var errorsList = new System.Collections.Generic.List<string>();

            // Check for SceneTransitionTrigger component in children
            var sceneTransition = GetComponentInChildren<SceneTransitionTrigger>();
            if (sceneTransition == null)
            {
                errorsList.Add("No SceneTransitionTrigger component found in children!");
            }
            else
            {
                // Check if trigger collider is assigned
                if (sceneTransition.GetTriggerColliderObject() == null)
                {
                    errorsList.Add("SceneTransitionTrigger has no trigger collider object assigned!");
                }

                // Check if both exit and spawn are disabled
                if (!sceneTransition.CanExitHere && !sceneTransition.CanSpawnHere)
                {
                    warningsList.Add("Neither exit nor spawn functionality is enabled. This transition won't do anything!");
                }
            }

            // Check for REPLACE-ME object
            Transform replaceMeObject = transform.Find("REPLACE-ME");
            if (replaceMeObject != null)
            {
                warningsList.Add("REPLACE-ME object still exists. Replace with appropriate visual representation!");
            }

            // Check for proper naming
            if (gameObject.name.Contains("(Clone)"))
            {
                warningsList.Add("GameObject name contains '(Clone)'. Consider renaming for clarity.");
            }

            warnings = warningsList.ToArray();
            errors = errorsList.ToArray();

            return errorsList.Count == 0;
        }

        private void OnDrawGizmos()
        {
            // Find the spawn point
            Transform spawnPoint = transform.Find("Spawn Point");
            if (spawnPoint == null) return;

            // Set gizmo color - cyan with some transparency
            Color gizmoColor = new Color(0f, 1f, 1f, 0.8f);
            Gizmos.color = gizmoColor;

            Vector3 spawnWorldPos = spawnPoint.position;
            float radius = 0.5f;

            // Draw a wire sphere as the base
            Gizmos.DrawWireSphere(spawnWorldPos, radius);

            // Draw a semi-transparent solid hemisphere (top half)
            // We'll approximate this with multiple disc slices
            Color semiTransparent = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.color = semiTransparent;
            
            int segments = 8;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i / (float)segments) * 90f; // 0 to 90 degrees for hemisphere
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                float discRadius = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                
                if (discRadius > 0.01f)
                {
                    DrawGizmoDisc(spawnWorldPos + Vector3.up * y, discRadius, 16);
                }
            }

            // Draw directional arrow based on orientation
            Gizmos.color = new Color(1f, 1f, 0f, 0.9f); // Yellow for direction
            Vector3 arrowStart = spawnWorldPos + Vector3.up * 0.2f;
            Vector3 arrowEnd = arrowStart;
            
            switch (orientation)
            {
                case Orientation.North:
                    arrowEnd += Vector3.forward * 0.8f;
                    break;
                case Orientation.East:
                    arrowEnd += Vector3.right * 0.8f;
                    break;
                case Orientation.South:
                    arrowEnd += Vector3.back * 0.8f;
                    break;
                case Orientation.West:
                    arrowEnd += Vector3.left * 0.8f;
                    break;
            }
            
            Gizmos.DrawLine(arrowStart, arrowEnd);
            
            // Draw arrowhead
            Vector3 arrowDir = (arrowEnd - arrowStart).normalized;
            Vector3 arrowRight = Vector3.Cross(arrowDir, Vector3.up).normalized;
            
            Gizmos.DrawLine(arrowEnd, arrowEnd - arrowDir * 0.2f + arrowRight * 0.1f);
            Gizmos.DrawLine(arrowEnd, arrowEnd - arrowDir * 0.2f - arrowRight * 0.1f);

            // Draw spawn point label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(spawnWorldPos + Vector3.up * 1f, "SPAWN", new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.cyan },
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            });
            #endif
        }

        private void DrawGizmoDisc(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + Vector3.forward * radius;
            
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}