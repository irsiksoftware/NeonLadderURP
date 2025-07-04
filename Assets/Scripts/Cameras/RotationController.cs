using UnityEngine;
using Unity.Cinemachine;

public class RotationController : MonoBehaviour
{
    public float TimeToWait = 1f; // Time to wait before starting the animation
    public string AxisToShiftOn = "x"; // Axis to shift on (x, y, or z)
    public float DegreesShift = 45f; // Degrees to shift the camera
    public float AnimationDuration = 2f; // Duration of the animation
    public float ZoomLevel = 5f; // The lens zoom level to set before the animation

    private CinemachineCamera _virtualCamera;
    private CinemachinePositionComposer _framingTransposer;
    private Vector3 _initialRotation;
    private bool _isAnimating = false;

    void Awake()
    {
        _virtualCamera = GetComponent<CinemachineCamera>();

        if (_virtualCamera == null)
        {
            Debug.LogError("CinemachineCamera component not found on the GameObject.");
            return;
        }

        // Retrieve the Framing Transposer component
        _framingTransposer = _virtualCamera.GetComponent<CinemachinePositionComposer>();

        if (_framingTransposer == null)
        {
            Debug.LogError("CinemachinePositionComposer component not found on the Virtual Camera.");
            return;
        }

        // Set the initial rotation
        _initialRotation = _virtualCamera.transform.eulerAngles;

        // Set the initial zoom level
        _framingTransposer.CameraDistance = ZoomLevel;
    }

    public void StartAnimationManually()
    {
        if (_isAnimating || _virtualCamera == null)
            return;

        // Delay the animation start
        Invoke(nameof(StartAnimation), TimeToWait);
    }

    private void StartAnimation()
    {
        _isAnimating = true;
        StartCoroutine(AnimateCameraSequence());
    }

    private System.Collections.IEnumerator AnimateCameraSequence()
    {
        float elapsedTime = 0f;

        // Determine the axis to shift on
        Vector3 rotationDelta = Vector3.zero;
        if (AxisToShiftOn.ToLower() == "x") rotationDelta = new Vector3(DegreesShift, 0f, 0f);
        else if (AxisToShiftOn.ToLower() == "y") rotationDelta = new Vector3(0f, DegreesShift, 0f);
        else if (AxisToShiftOn.ToLower() == "z") rotationDelta = new Vector3(0f, 0f, DegreesShift);

        Vector3 targetRotation = _initialRotation + rotationDelta;
        Debug.Log("Beginning to track Camera Sequence time");
        while (elapsedTime < AnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / AnimationDuration;
            //Debug.Log(elapsedTime);

            // Interpolate rotation
            _virtualCamera.transform.eulerAngles = Vector3.Lerp(_initialRotation, targetRotation, t);

            yield return null;
        }

        // Ensure the final rotation is applied
        _virtualCamera.transform.eulerAngles = targetRotation;

        // Disable the Cinemachine Virtual Camera component
        Debug.Log("Disabling Cinemachine Virtual Camera component.");
        _virtualCamera.enabled = false;

        if (!_virtualCamera.enabled)
        {
            Debug.Log("Cinemachine Virtual Camera successfully disabled.");
        }
        else
        {
            Debug.LogError("Failed to disable the Cinemachine Virtual Camera component.");
        }

        _isAnimating = false;
    }
}
