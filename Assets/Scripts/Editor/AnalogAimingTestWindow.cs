using UnityEngine;
using UnityEditor;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Editor
{
    /// <summary>
    /// Editor window to help test and validate the 180-degree analog aiming implementation.
    /// Provides real-time debugging and validation of the aiming system.
    /// </summary>
    public class AnalogAimingTestWindow : EditorWindow
    {
        private AnalogAim180 analogAim;
        private RangedAttackController rangedController;
        private Player player;

        [MenuItem("NeonLadder/Tools/Analog Aiming Test Window")]
        public static void ShowWindow()
        {
            GetWindow<AnalogAimingTestWindow>("Analog Aiming Test");
        }

        void OnGUI()
        {
            GUILayout.Label("180-Degree Analog Aiming Test", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Component references
            EditorGUILayout.LabelField("Component References", EditorStyles.boldLabel);
            analogAim = EditorGUILayout.ObjectField("Analog Aim Component", analogAim, typeof(AnalogAim180), true) as AnalogAim180;
            rangedController = EditorGUILayout.ObjectField("Ranged Attack Controller", rangedController, typeof(RangedAttackController), true) as RangedAttackController;
            player = EditorGUILayout.ObjectField("Player", player, typeof(Player), true) as Player;

            EditorGUILayout.Space();

            // Auto-find components
            if (GUILayout.Button("Auto-Find Components in Scene"))
            {
                AutoFindComponents();
            }

            EditorGUILayout.Space();

            // Status display
            if (analogAim != null)
            {
                DisplayAnalogAimStatus();
            }

            if (rangedController != null)
            {
                DisplayRangedControllerStatus();
            }

            if (player != null)
            {
                DisplayPlayerStatus();
            }

            EditorGUILayout.Space();

            // Integration validation
            EditorGUILayout.LabelField("Integration Validation", EditorStyles.boldLabel);
            ValidateIntegration();

            // Setup instructions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Setup Instructions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "To set up 180-degree analog aiming:\n\n" +
                "1. Add AnalogAim180 component to your Player GameObject\n" +
                "2. Create an empty GameObject as child of Player named 'AimPivot'\n" +
                "3. Position AimPivot at weapon/arm location\n" +
                "4. Assign AimPivot to the AnalogAim180 component\n" +
                "5. Set up Input Action reference to existing 'Aim' action\n" +
                "6. The RangedAttackController will auto-detect the AnalogAim180 component",
                MessageType.Info
            );
        }

        private void AutoFindComponents()
        {
            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }

            if (player != null)
            {
                if (analogAim == null)
                {
                    analogAim = player.GetComponent<AnalogAim180>();
                }

                if (rangedController == null)
                {
                    rangedController = player.GetComponent<RangedAttackController>();
                }
            }
        }

        private void DisplayAnalogAimStatus()
        {
            EditorGUILayout.LabelField("Analog Aim Status", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see runtime status", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Is Active:", GUILayout.Width(80));
            EditorGUILayout.LabelField(analogAim.IsAnalogActive.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Aim Direction:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"({analogAim.CurrentAimDirection.x:F2}, {analogAim.CurrentAimDirection.y:F2})");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stick Magnitude:", GUILayout.Width(80));
            EditorGUILayout.LabelField(analogAim.CurrentStickMagnitude.ToString("F2"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Normalized Angle:", GUILayout.Width(80));
            EditorGUILayout.LabelField(analogAim.CurrentAimAngleNormalized.ToString("F2"));
            EditorGUILayout.EndHorizontal();
        }

        private void DisplayRangedControllerStatus()
        {
            EditorGUILayout.LabelField("Ranged Controller Status", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Analog Override:", GUILayout.Width(100));
            EditorGUILayout.LabelField(rangedController.analogOverridesMouse.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mouse Fallback:", GUILayout.Width(100));
            EditorGUILayout.LabelField(rangedController.enableMouseFallback.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Analog Component:", GUILayout.Width(100));
            EditorGUILayout.LabelField(rangedController.analogAim != null ? "Connected" : "Missing");
            EditorGUILayout.EndHorizontal();
        }

        private void DisplayPlayerStatus()
        {
            EditorGUILayout.LabelField("Player Status", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Using Melee:", GUILayout.Width(80));
            EditorGUILayout.LabelField(player.IsUsingMelee.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Position:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"({player.transform.position.x:F1}, {player.transform.position.y:F1})");
            EditorGUILayout.EndHorizontal();
        }

        private void ValidateIntegration()
        {
            bool hasAnalogAim = analogAim != null;
            bool hasRangedController = rangedController != null;
            bool hasPlayer = player != null;

            // Check AnalogAim180 setup
            if (hasAnalogAim)
            {
                bool hasAimPivot = analogAim.aimPivot != null;
                bool hasInputAction = analogAim.aimStick.action != null;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("✓", GUILayout.Width(20));
                EditorGUILayout.LabelField("AnalogAim180 component found");
                EditorGUILayout.EndHorizontal();

                if (!hasAimPivot)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("✗", GUILayout.Width(20));
                    EditorGUILayout.LabelField("AimPivot not assigned!");
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("✓", GUILayout.Width(20));
                    EditorGUILayout.LabelField("AimPivot assigned");
                    EditorGUILayout.EndHorizontal();
                }

                if (!hasInputAction)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("✗", GUILayout.Width(20));
                    EditorGUILayout.LabelField("Input Action not assigned!");
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("✓", GUILayout.Width(20));
                    EditorGUILayout.LabelField($"Input Action: {analogAim.aimStick.action.name}");
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("✗", GUILayout.Width(20));
                EditorGUILayout.LabelField("AnalogAim180 component missing");
                EditorGUILayout.EndHorizontal();
            }

            // Check RangedAttackController integration
            if (hasRangedController)
            {
                bool hasAnalogReference = rangedController.analogAim != null;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("✓", GUILayout.Width(20));
                EditorGUILayout.LabelField("RangedAttackController found");
                EditorGUILayout.EndHorizontal();

                if (hasAnalogReference)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("✓", GUILayout.Width(20));
                    EditorGUILayout.LabelField("Analog aiming integration active");
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("⚠", GUILayout.Width(20));
                    EditorGUILayout.LabelField("Analog aiming not connected (will auto-detect)");
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("✗", GUILayout.Width(20));
                EditorGUILayout.LabelField("RangedAttackController missing");
                EditorGUILayout.EndHorizontal();
            }
        }

        void Update()
        {
            // Refresh the window during play mode
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}