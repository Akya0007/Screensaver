using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PipeSpawner : MonoBehaviour
{
    public int desiredActivePipeCount = 1; // Number of active pipes to maintain
    public float pipeSpeed = 1f; // Speed of the pipes
    public int maxPipesOnScreen = 7; // Maximum number of pipes allowed on screen at once
    public int minPipeTurns = 100; // Minimum number of turns a pipe must make
    public int maxPipeTurns = 200; // Maximum number of turns a pipe can make
    public float pipeRadius = 0.5f; // Radius of the pipe segments
    private int activePipes = 0; // Counter for active pipes
    private List<Pipe> finishedPipes = new List<Pipe>(); // List to keep track of finished pipes
    private bool paused = false; // Flag to check if the spawner is paused
    private Pipe currentPipe = null; // Reference to the current pipe being generated
    public Vector3 boundarySize = new Vector3(60, 30, 60); // Boundary size for the pipes

    private Dictionary<Vector3, bool> occupiedPositions = new Dictionary<Vector3, bool>(); // Dictionary to keep track of occupied positions

    void Start()
    {
        StartCoroutine(SpawnPipe()); // Start the coroutine to spawn pipes
    }

    IEnumerator SpawnPipe()
    {
        while (!paused) // Continue spawning pipes if not paused
        {
            if (currentPipe == null && activePipes < maxPipesOnScreen) // Check if a new pipe can be spawned
            {
                Vector3 startPosition = GetRandomUnoccupiedPosition(); // Get a random unoccupied position
                if (startPosition == Vector3.zero) // If no positions are available, log a message and exit
                {
                    Debug.Log("No points available to start a new pipe");
                    yield break;
                }

                GameObject pipeObj = new GameObject("Pipe"); // Create a new GameObject for the pipe
                Pipe pipe = pipeObj.AddComponent<Pipe>(); // Add the Pipe component to the GameObject
                pipe.SetSpeed(pipeSpeed); // Set the speed of the pipe
                pipe.SetMaxTurns(Random.Range(minPipeTurns, maxPipeTurns)); // Set the maximum number of turns for the pipe
                pipe.boundarySize = boundarySize; // Set the boundary size for the pipe
                pipe.OnPipeFinished += OnPipeFinished; // Subscribe to the OnPipeFinished event

                pipe.StartGeneration(startPosition, GetRandomInitialDirection()); // Start generating the pipe

                activePipes++; // Increment the active pipes count
                currentPipe = pipe; // Set the current pipe

                // Add the start position to the occupied positions
                if (!occupiedPositions.ContainsKey(startPosition))
                    occupiedPositions.Add(startPosition, true);
            }
            yield return new WaitUntil(() => !currentPipe.isGenerating); // Wait until the current pipe finishes generating
        }
    }

    // Return the paused state
    public bool IsPaused()
    {
        return paused;
    }

    // Set the speed for each active pipe
    void SetSpeed(float speed)
    {
        foreach (Pipe pipe in FindObjectsOfType<Pipe>())
        {
            pipe.SetSpeed(speed);
        }
    }

    // Handle the completion of a pipe
    public void OnPipeFinished(Pipe pipe)
    {
        pipe.OnPipeFinished -= this.OnPipeFinished; // Unsubscribe from the OnPipeFinished event
        activePipes--; // Decrement the active pipes count
        finishedPipes.Add(pipe); // Add the finished pipe to the list

        // Add all pipe segments to the occupied positions
        foreach (Transform segment in pipe.transform)
        {
            if (!occupiedPositions.ContainsKey(segment.position))
                occupiedPositions.Add(segment.position, true);
        }

        currentPipe = null; // Clear the reference to the current pipe

        if (activePipes < desiredActivePipeCount) // Check if more pipes need to be spawned
        {
            StartCoroutine(SpawnPipe()); // Spawn new pipes if needed
        }
    }

    // Reset all pipes and start spawning new pipes
    public void ResetPipes()
    {
        activePipes = 0; // Reset the active pipes count
        finishedPipes.Clear(); // Clear the list of finished pipes
        occupiedPositions.Clear(); // Clear the dictionary of occupied positions

        foreach (Pipe pipe in FindObjectsOfType<Pipe>())
        {
            Destroy(pipe.gameObject); // Destroy all active pipe GameObjects
        }

        StartCoroutine(SpawnPipe()); // Restart spawning pipes
    }

    // Get a random unoccupied position within the boundary
    Vector3 GetRandomUnoccupiedPosition()
    {
        List<Vector3> availablePositions = new List<Vector3>();

        // Iterate through all possible positions within the boundary
        for (float x = -boundarySize.x / 2; x <= boundarySize.x / 2; x += pipeRadius * 2)
        {
            for (float y = -boundarySize.y / 2; y <= boundarySize.y / 2; y += pipeRadius * 2)
            {
                for (float z = -boundarySize.z / 2; z <= boundarySize.z / 2; z += pipeRadius * 2)
                {
                    Vector3 potentialPosition = new Vector3(x, y, z);
                    if (!occupiedPositions.ContainsKey(potentialPosition))
                    {
                        availablePositions.Add(potentialPosition);
                    }
                }
            }
        }

        if (availablePositions.Count == 0)
        {
            return Vector3.zero; // Return Vector3.zero if no available positions are found
        }

        return availablePositions[Random.Range(0, availablePositions.Count)];
    }

    // Get a random initial direction for the pipe
    Vector3 GetRandomInitialDirection()
    {
        List<Vector3> directions = new List<Vector3> {
            Vector3.forward, Vector3.back,
            Vector3.left, Vector3.right,
            Vector3.up, Vector3.down
        };

        return directions[Random.Range(0, directions.Count)];
    }
}
