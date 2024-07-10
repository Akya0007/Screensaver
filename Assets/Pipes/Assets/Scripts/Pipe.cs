
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pipe : MonoBehaviour
{
    public int maximumPipeTurns = 200; // Maximum number of turns a pipe can make before stopping
    public float minimumStretchDistance = 3f; // Minimum distance a pipe segment can stretch
    public float maximumStretchDistance = 10f; // Maximum distance a pipe segment can stretch
    public float speed = 0.01f; // Speed at which the pipe grows
    public float pipeRadius = 0.5f; // Radius of the pipe segments
    public float turnSphereRadius = 0.7f; // Radius of the spheres at the turns
    public int turnFrequency = 5; // Number of segments before a turn
    public Color pipeColor; // Color of the pipe
    public bool isGenerating = false; // Flag to check if the pipe is currently generating

    private List<GameObject> pipeSegments = new List<GameObject>(); // List to hold all the pipe segments and spheres
    private bool isPaused = false; // Flag to check if the pipe is paused
    private Vector3 lastDirection; // Store the last direction to avoid moving backwards

    public delegate void PipeFinished(Pipe pipe); // Delegate for when a pipe is finished
    public event PipeFinished OnPipeFinished; // Event triggered when the pipe is finished

    public Vector3 boundarySize; // Boundary size for the pipes

    // Start the generation process for the pipe
    public void StartGeneration(Vector3 startPosition, Vector3 startDirection)
    {
        transform.position = startPosition; // Set the start position
        lastDirection = startDirection; // Set the initial direction
        pipeColor = GetRandomColor(); // Assign a random color to the pipe
        StartCoroutine(GeneratePipes()); // Start generating the pipe
    }

    // Coroutine to generate the pipe segments
    IEnumerator GeneratePipes()
    {
        isGenerating = true; // Set the generating flag to true
        Vector3 currentPosition = transform.position; // Initialize the current position
        Vector3 direction = lastDirection; // Initialize the direction
        int segments = 0; // Counter for the number of segments
        int turns = 0; // Counter for the number of turns

        while (turns < maximumPipeTurns) // Continue until maximum turns are reached
        {
            float distance = Random.Range(minimumStretchDistance, maximumStretchDistance); // Random distance for the next segment
            Vector3 targetPosition = currentPosition + direction * distance; // Calculate the target position

            // Check if the new segment collides with anything or is out of boundary
            if (CheckCollision(currentPosition, targetPosition) || !IsWithinBoundary(targetPosition))
            {
                direction = GetRandomForwardDirection(lastDirection); // Get a new direction
                CreateTurn(ref currentPosition, ref direction); // Create a turn at the current position
                turns++; // Increment the turn counter
                segments = 0; // Reset the segment counter after a turn
                continue; // Skip to the next iteration
            }

            CreateSegment(ref currentPosition, targetPosition); // Create the new segment
            currentPosition = targetPosition; // Update the current position to the target position
            lastDirection = direction; // Update the last direction
            segments++; // Increment the segment counter

            if (segments >= turnFrequency) // Check if it's time to make a turn
            {
                direction = GetRandomForwardDirection(lastDirection); // Get a new direction
                CreateTurn(ref currentPosition, ref direction); // Create a turn at the current position
                segments = 0; // Reset the segment counter after a turn
                turns++; // Increment the turn counter
            }

            yield return new WaitForSeconds(1f / speed); // Wait before generating the next segment
        }

        isGenerating = false; // Set the generating flag to false
        EndPipe(); // End the pipe when maximum turns are reached
    }

    // Get a random direction that is not opposite to the last direction
    Vector3 GetRandomForwardDirection(Vector3 lastDirection)
    {
        // List of possible directions for the pipe to move
        List<Vector3> directions = new List<Vector3> {
            Vector3.forward, Vector3.back,
            Vector3.left, Vector3.right,
            Vector3.up, Vector3.down
        };
        directions.Remove(-lastDirection); // Remove the opposite direction of the last movement
        return directions[Random.Range(0, directions.Count)]; // Return a random direction from the list
    }

    // Check for collision along the path of the pipe segment
    bool CheckCollision(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized; // Calculate the direction of the segment
        float distance = Vector3.Distance(start, end); // Calculate the distance of the segment
        Ray ray = new Ray(start, direction); // Create a ray from the start position in the direction of the segment
        return Physics.Raycast(ray, distance); // Check if the ray hits anything within the distance
    }

    // Create a new segment of the pipe
    void CreateSegment(ref Vector3 start, Vector3 end)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // Create a cylinder to represent the pipe segment
        cylinder.transform.position = (start + end) / 2; // Position the cylinder at the midpoint between start and end
        cylinder.transform.localScale = new Vector3(pipeRadius, Vector3.Distance(start, end) / 2, pipeRadius); // Scale the cylinder to the correct length and thickness
        cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start); // Rotate the cylinder to align with the segment direction
        cylinder.GetComponent<Renderer>().material.color = pipeColor; // Set the color of the cylinder
        cylinder.transform.parent = transform; // Set the pipe object as the parent of the cylinder
        pipeSegments.Add(cylinder); // Add the cylinder to the list of pipe segments
    }

    // Create a turn in the pipe
    void CreateTurn(ref Vector3 position, ref Vector3 direction)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Create a sphere to represent the turn
        sphere.transform.position = position; // Position the sphere at the current position
        sphere.transform.localScale = Vector3.one * turnSphereRadius; // Scale the sphere to the correct size
        sphere.GetComponent<Renderer>().material.color = pipeColor; // Set the color of the sphere
        sphere.transform.parent = transform; // Set the pipe object as the parent of the sphere
        pipeSegments.Add(sphere); // Add the sphere to the list of pipe segments

        direction = GetRandomForwardDirection(lastDirection); // Get a new direction for the next segment
    }

    // Set the speed of the pipe
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    // Set the maximum number of turns for the pipe
    public void SetMaxTurns(int newMaxTurns)
    {
        maximumPipeTurns = newMaxTurns;
    }

    // Return the paused state of the pipe
    public bool IsPaused()
    {
        return isPaused;
    }

    // Trigger the OnPipeFinished event
    public void EndPipe()
    {
        OnPipeFinished?.Invoke(this);
    }

    // Generate a random color for the pipe
    private Color GetRandomColor()
    {
        Color color;
        do
        {
            color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f); // Generate lighter colors by setting hue, saturation, and value ranges
        } while (color.r < 0.2f && color.g < 0.2f && color.b < 0.2f); // Avoid very dark colors

        return color;
    }

    // Check if the position is within the boundary
    bool IsWithinBoundary(Vector3 position)
    {
        return Mathf.Abs(position.x) <= boundarySize.x / 2 &&
               Mathf.Abs(position.y) <= boundarySize.y / 2 &&
               Mathf.Abs(position.z) <= boundarySize.z / 2;
    }

    // Get the end position of the pipe
    public Vector3 GetEndPosition()
    {
        return pipeSegments.Count > 0 ? pipeSegments[pipeSegments.Count - 1].transform.position : transform.position;
    }

    // Get the last direction of the pipe
    public Vector3 GetLastDirection()
    {
        return lastDirection;
    }
}
