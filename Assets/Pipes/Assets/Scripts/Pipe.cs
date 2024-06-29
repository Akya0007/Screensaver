/**using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pipe : MonoBehaviour
{
    public int maximumPipeTurns = 200;
    public float minimumStretchDistance = 3f;
    public float maximumStretchDistance = 10f;
    public float speed = 3f;
    public float pipeRadius = 2f; // Increased the pipe radius
    public float turnSphereRadius = 2.5f; // Increased the turn sphere radius
    public int turnFrequency = 5; // Number of segments before a turn
    public Color pipeColor;

    private List<GameObject> pipeSegments = new List<GameObject>();
    private bool isPaused = false;

    public delegate void PipeFinished(Pipe pipe);
    public event PipeFinished OnPipeFinished;

    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();

    void Start()
    {
        pipeColor = GetRandomColor();
        StartCoroutine(GeneratePipes());
    }

    IEnumerator GeneratePipes()
    {
        Vector3 currentPosition = Vector3.zero;
        Vector3 direction = GetRandomDirection();
        int segments = 0;
        int turns = 0;

        while (turns < maximumPipeTurns)
        {
            float distance = Random.Range(minimumStretchDistance, maximumStretchDistance);
            Vector3 targetPosition = currentPosition + direction * distance;

            if (CheckCollision(currentPosition, targetPosition))
            {
                direction = GetRandomDirection();
                CreateTurn(ref currentPosition, ref direction);
                turns++;
                segments = 0; // Reset segments after a turn
                continue;
            }

            CreateSegment(ref currentPosition, targetPosition);
            currentPosition = targetPosition;
            segments++;

            if (segments >= turnFrequency)
            {
                direction = GetRandomDirection();
                CreateTurn(ref currentPosition, ref direction);
                segments = 0; // Reset segments after a turn
            }

            yield return new WaitForSeconds(1f / speed);
        }

        EndPipe();
    }

    Vector3 GetRandomDirection()
    {
        List<Vector3> directions = new List<Vector3> {
            Vector3.forward, Vector3.back,
            Vector3.left, Vector3.right,
            Vector3.up, Vector3.down
        };

        return directions[Random.Range(0, directions.Count)];
    }

    bool CheckCollision(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        Ray ray = new Ray(start, direction);
        return Physics.Raycast(ray, distance);
    }

    void CreateSegment(ref Vector3 start, Vector3 end)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = (start + end) / 2;
        cylinder.transform.localScale = new Vector3(pipeRadius, Vector3.Distance(start, end) / 2, pipeRadius);
        cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start);
        cylinder.GetComponent<Renderer>().material.color = pipeColor;
        cylinder.transform.parent = transform;
        pipeSegments.Add(cylinder);

        occupiedPositions.Add(start);
        occupiedPositions.Add(end);
    }

    void CreateTurn(ref Vector3 position, ref Vector3 direction)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * turnSphereRadius;
        sphere.GetComponent<Renderer>().material.color = pipeColor;
        sphere.transform.parent = transform;
        pipeSegments.Add(sphere);

        occupiedPositions.Add(position);

        direction = GetRandomDirection();
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetMaxTurns(int newMaxTurns)
    {
        maximumPipeTurns = newMaxTurns;
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public void EndPipe()
    {
        OnPipeFinished?.Invoke(this);
    }

    public void FadeOut(float fadeTime, float delay, Material fadeMaterial)
    {
        if (fadeMaterial != null) // Check if fadeMaterial is assigned
        {
            StartCoroutine(FadeOutCoroutine(fadeTime, delay, fadeMaterial));
        }
        else
        {
            Debug.LogWarning("Fade material is not assigned. Skipping fade out.");
        }
    }

    private IEnumerator FadeOutCoroutine(float fadeTime, float delay, Material fadeMaterial)
    {
        yield return new WaitForSeconds(delay);

        foreach (GameObject segment in pipeSegments)
        {
            Renderer renderer = segment.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material originalMaterial = renderer.material;
                renderer.material = fadeMaterial;

                Color color = fadeMaterial.color;
                float startAlpha = color.a;
                float rate = 1.0f / fadeTime;
                float progress = 0.0f;

                while (progress < 1.0f)
                {
                    color.a = Mathf.Lerp(startAlpha, 0, progress);
                    fadeMaterial.color = color;
                    progress += rate * Time.deltaTime;
                    yield return null;
                }

                color.a = 0;
                fadeMaterial.color = color;
                Destroy(segment);
            }
        }
    }

    private Color GetRandomColor()
    {
        Color color;
        do
        {
            color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f); // Generate lighter colors
        } while (color.r < 0.2f && color.g < 0.2f && color.b < 0.2f); // Avoid very dark colors

        return color;
    }
}
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pipe : MonoBehaviour
{
    public int maximumPipeTurns = 200; // Maximum number of turns a pipe can make before stopping
    public float minimumStretchDistance = 3f; // Minimum distance a pipe segment can stretch
    public float maximumStretchDistance = 10f; // Maximum distance a pipe segment can stretch
    public float speed = 3f; // Speed at which the pipe grows
    public float pipeRadius = 2f; // Radius of the pipe segments
    public float turnSphereRadius = 2.5f; // Radius of the spheres at the turns
    public int turnFrequency = 5; // Number of segments before a turn
    public Color pipeColor; // Color of the pipe

    private List<GameObject> pipeSegments = new List<GameObject>(); // List to hold all the pipe segments and spheres
    private bool isPaused = false; // Flag to check if the pipe is paused

    public delegate void PipeFinished(Pipe pipe); // Delegate for when a pipe is finished
    public event PipeFinished OnPipeFinished; // Event triggered when the pipe is finished

    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>(); // Set to keep track of occupied positions to avoid collisions

    void Start()
    {
        pipeColor = GetRandomColor(); // Set a random color for the pipe
        StartCoroutine(GeneratePipes()); // Start generating pipes
    }

    IEnumerator GeneratePipes()
    {
        Vector3 currentPosition = Vector3.zero; // Starting position of the pipe
        Vector3 direction = GetRandomDirection(); // Get a random initial direction
        int segments = 0; // Counter for the number of segments
        int turns = 0; // Counter for the number of turns

        while (turns < maximumPipeTurns)
        {
            float distance = Random.Range(minimumStretchDistance, maximumStretchDistance); // Random distance for the next segment
            Vector3 targetPosition = currentPosition + direction * distance; // Calculate the target position

            if (CheckCollision(currentPosition, targetPosition)) // Check if the new segment collides with anything
            {
                direction = GetRandomDirection(); // Get a new direction
                CreateTurn(ref currentPosition, ref direction); // Create a turn at the current position
                turns++; // Increment the turn counter
                segments = 0; // Reset the segment counter after a turn
                continue; // Skip to the next iteration
            }

            CreateSegment(ref currentPosition, targetPosition); // Create the new segment
            currentPosition = targetPosition; // Update the current position to the target position
            segments++; // Increment the segment counter

            if (segments >= turnFrequency) // Check if it's time to make a turn
            {
                direction = GetRandomDirection(); // Get a new direction
                CreateTurn(ref currentPosition, ref direction); // Create a turn at the current position
                segments = 0; // Reset the segment counter after a turn
            }

            yield return new WaitForSeconds(1f / speed); // Wait before generating the next segment
        }

        EndPipe(); // End the pipe when maximum turns are reached
    }

    Vector3 GetRandomDirection()
    {
        // List of possible directions for the pipe to move
        List<Vector3> directions = new List<Vector3> {
            Vector3.forward, Vector3.back,
            Vector3.left, Vector3.right,
            Vector3.up, Vector3.down
        };

        return directions[Random.Range(0, directions.Count)]; // Return a random direction from the list
    }

    bool CheckCollision(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized; // Calculate the direction of the segment
        float distance = Vector3.Distance(start, end); // Calculate the distance of the segment
        Ray ray = new Ray(start, direction); // Create a ray from the start position in the direction of the segment
        return Physics.Raycast(ray, distance); // Check if the ray hits anything within the distance
    }

    void CreateSegment(ref Vector3 start, Vector3 end)
    {
        // Create a cylinder to represent the pipe segment
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = (start + end) / 2; // Position the cylinder at the midpoint between start and end
        cylinder.transform.localScale = new Vector3(pipeRadius, Vector3.Distance(start, end) / 2, pipeRadius); // Scale the cylinder to the correct length and thickness
        cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start); // Rotate the cylinder to align with the segment direction
        cylinder.GetComponent<Renderer>().material.color = pipeColor; // Set the color of the cylinder
        cylinder.transform.parent = transform; // Set the pipe object as the parent of the cylinder
        pipeSegments.Add(cylinder); // Add the cylinder to the list of pipe segments

        occupiedPositions.Add(start); // Mark the start position as occupied
        occupiedPositions.Add(end); // Mark the end position as occupied
    }

    void CreateTurn(ref Vector3 position, ref Vector3 direction)
    {
        // Create a sphere to represent the turn
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position; // Position the sphere at the current position
        sphere.transform.localScale = Vector3.one * turnSphereRadius; // Scale the sphere to the correct size
        sphere.GetComponent<Renderer>().material.color = pipeColor; // Set the color of the sphere
        sphere.transform.parent = transform; // Set the pipe object as the parent of the sphere
        pipeSegments.Add(sphere); // Add the sphere to the list of pipe segments

        occupiedPositions.Add(position); // Mark the turn position as occupied

        direction = GetRandomDirection(); // Get a new direction for the next segment
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed; // Set the speed of the pipe
    }

    public void SetMaxTurns(int newMaxTurns)
    {
        maximumPipeTurns = newMaxTurns; // Set the maximum number of turns for the pipe
    }

    public bool IsPaused()
    {
        return isPaused; // Return the paused state of the pipe
    }

    public void EndPipe()
    {
        OnPipeFinished?.Invoke(this); // Trigger the OnPipeFinished event
    }

    public void FadeOut(float fadeTime, float delay, Material fadeMaterial)
    {
        if (fadeMaterial != null) // Check if fadeMaterial is assigned
        {
            StartCoroutine(FadeOutCoroutine(fadeTime, delay, fadeMaterial)); // Start the fade-out coroutine
        }
        else
        {
            Debug.LogWarning("Fade material is not assigned. Skipping fade out."); // Log a warning if fadeMaterial is not assigned
        }
    }

    private IEnumerator FadeOutCoroutine(float fadeTime, float delay, Material fadeMaterial)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay

        foreach (GameObject segment in pipeSegments) // Iterate through all the pipe segments
        {
            Renderer renderer = segment.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material originalMaterial = renderer.material; // Store the original material
                renderer.material = fadeMaterial; // Set the fade material

                Color color = fadeMaterial.color;
                float startAlpha = color.a;
                float rate = 1.0f / fadeTime;
                float progress = 0.0f;

                while (progress < 1.0f)
                {
                    color.a = Mathf.Lerp(startAlpha, 0, progress); // Gradually decrease the alpha value
                    fadeMaterial.color = color;
                    progress += rate * Time.deltaTime;
                    yield return null;
                }

                color.a = 0;
                fadeMaterial.color = color; // Ensure the alpha is set to 0
                Destroy(segment); // Destroy the segment after fading out
            }
        }
    }

    private Color GetRandomColor()
    {
        Color color;
        do
        {
            color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f); // Generate lighter colors by setting hue, saturation, and value ranges
        } while (color.r < 0.2f && color.g < 0.2f && color.b < 0.2f); // Avoid very dark colors

        return color; // Return the generated color
    }
}
