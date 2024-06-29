using UnityEngine;

/**
 *	Simple script to rotate the scene camera around the pipe bounds.
 */
[RequireComponent(typeof(Camera))]
public class RotateCamera : MonoBehaviour
{
    const float MIN_CAM_DISTANCE = 60f; // Minimum camera distance from the target
    const float MAX_CAM_DISTANCE = 200f; // Maximum camera distance from the target
    public float scrollModifier = 50f; // Modifier for the scroll wheel zoom speed

    public PipeSpawner pipeSpawner; // Reference to the PipeSpawner script
    public float distanceFromPivot = 80f; // Initial distance from the pivot point (target)
    Vector3 target = Vector3.zero; // Target position to orbit around

    public float orbitSpeed = 100f; // Speed of camera orbit when using the mouse
    public float idleSpeed = 5f; // Speed of camera orbit when idle (no mouse input)

    float sign = 1f; // Direction of idle rotation

    void Start()
    {
        if (pipeSpawner == null) // If the PipeSpawner reference is not set
        {
            PipeSpawner[] spawners = FindObjectsOfType<PipeSpawner>(); // Find all PipeSpawner instances
            if (spawners.Length > 0)
                pipeSpawner = spawners[0]; // Use the first PipeSpawner found
            else
                Debug.LogError("Please create a GameObject with a PipeSpawner component."); // Log an error if no PipeSpawner is found
        }
        target = pipeSpawner.transform.position; // Set the target to the PipeSpawner's position
    }

    Vector2 mouse = Vector2.zero; // Store the mouse position
    Vector3 eulerRotation = Vector3.zero; // Store the camera's rotation
    bool ignore = false; // Flag to ignore mouse input when interacting with the settings window

    void LateUpdate()
    {
        eulerRotation = transform.localRotation.eulerAngles; // Get the current rotation of the camera

        // Toggle accepting mouse input to avoid accidental camera movement when dragging the settings window
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mpos = Input.mousePosition; // Get the current mouse position
            mpos.y = Screen.height - mpos.y; // Adjust the y position for screen space
            if (pipeSpawner.settingsWindowRect.Contains(mpos)) // Check if the mouse is over the settings window
                ignore = true; // Ignore mouse input if over the settings window
        }

        if (Input.GetMouseButtonUp(0))
            ignore = false; // Stop ignoring mouse input when the mouse button is released

        if (Input.GetMouseButton(0) && !ignore) // Rotate the camera when the left mouse button is held down and not over the settings window
        {
            mouse.x = Input.GetAxis("Mouse X"); // Get horizontal mouse movement
            mouse.y = -Input.GetAxis("Mouse Y"); // Get vertical mouse movement (inverted)

            eulerRotation.x += mouse.y * orbitSpeed * Time.deltaTime; // Adjust x rotation based on vertical mouse movement
            eulerRotation.y += mouse.x * orbitSpeed * Time.deltaTime; // Adjust y rotation based on horizontal mouse movement
            eulerRotation.z = 0f; // Keep z rotation at zero

            float x = eulerRotation.x > 180f ? -(360 - eulerRotation.x) : eulerRotation.x; // Normalize the x rotation

            eulerRotation.x = Mathf.Clamp(x, -30f, 30f); // Clamp the x rotation to prevent flipping

            sign = mouse.x >= 0f ? 1f : -1f; // Set the rotation direction based on mouse movement
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0f) // Zoom the camera in and out using the mouse scroll wheel
        {
            distanceFromPivot -= Input.GetAxis("Mouse ScrollWheel") * (distanceFromPivot / MAX_CAM_DISTANCE) * scrollModifier; // Adjust the distance from the target
            distanceFromPivot = Mathf.Clamp(distanceFromPivot, MIN_CAM_DISTANCE, MAX_CAM_DISTANCE); // Clamp the distance to prevent getting too close or too far
        }

        if (!pipeSpawner.IsPaused()) // Rotate the camera idly if the spawner is not paused
            eulerRotation.y += sign * idleSpeed * Time.deltaTime; // Adjust y rotation based on idle speed and direction

        transform.localRotation = Quaternion.Euler(eulerRotation); // Apply the new rotation to the camera

        transform.position = transform.localRotation * (Vector3.forward * -distanceFromPivot) + target; // Update the camera's position based on the new rotation and distance from the target
    }
}
