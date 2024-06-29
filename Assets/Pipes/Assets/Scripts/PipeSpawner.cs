/**using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PipeSpawner : MonoBehaviour
{
    public int desiredActivePipeCount = 1;
    public float pipeSpeed = 13f;
    public int maxPipesOnScreen = 7;
    public int minPipeTurns = 100;
    public int maxPipeTurns = 200;
    public Material material;
    public Material fadeMaterial;
    public float fadeTime = 2f;
    public Texture2D pausedImage;

    private int activePipes = 0;
    private List<Pipe> finishedPipes = new List<Pipe>();
    private RotateCamera rotateCam;
    private bool paused = false;
    private bool showSettings = false;
    public Rect settingsWindowRect = new Rect(10, 20, 200, 200);

    void Awake()
    {
        rotateCam = Camera.main.transform.GetComponent<RotateCamera>();
    }

    void Start()
    {
        StartCoroutine(SpawnPipes());
    }

    IEnumerator SpawnPipes()
    {
        while (activePipes < desiredActivePipeCount && !paused)
        {
            SpawnPipe();
            yield return new WaitForSeconds(Random.Range(.5f, 1f));
        }
    }

    public bool IsPaused()
    {
        return paused;
    }

    void SetSpeed(float speed)
    {
        foreach (Pipe pipe in FindObjectsOfType<Pipe>())
        {
            pipe.SetSpeed(speed);
        }
    }

    void OnGUI()
    {
        showSettings = GUILayout.Toggle(showSettings, "Settings");

        if (showSettings)
        {
            settingsWindowRect = GUILayout.Window(0, settingsWindowRect, SettingsWindow, "Settings");
            settingsWindowRect.x = Mathf.Clamp(settingsWindowRect.x, 0, Screen.width - settingsWindowRect.width);
            settingsWindowRect.y = Mathf.Clamp(settingsWindowRect.y, 0, Screen.height - 20);
        }

        if (paused)
        {
            if (pausedImage != null)
            {
                if (GUI.Button(new Rect(Screen.width / 2f - pausedImage.width / 2f, Screen.height - pausedImage.height - 20, pausedImage.width, pausedImage.height), pausedImage, GUIStyle.none))
                    TogglePause();
            }
            else
                GUI.Label(new Rect(Screen.width / 2f - 60, Screen.height - 40, 120, 24), "PAUSED");
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        paused = !paused;

        if (!paused && activePipes < desiredActivePipeCount)
        {
            SetSpeed(pipeSpeed);
            StartCoroutine(SpawnPipes());
        }

        SetSpeed(paused ? 0f : pipeSpeed);
    }

    void SettingsWindow(int id)
    {
        int tmp = 0;
        bool doUpdate = false;

        GUILayout.Label("\"Escape\" key to Quit\n\"Space\" key to Pause");

        GUILayout.Label("Active Pipes: " + desiredActivePipeCount);
        {
            tmp = desiredActivePipeCount;
            desiredActivePipeCount = (int)GUILayout.HorizontalSlider(desiredActivePipeCount, 1, 8);

            if (desiredActivePipeCount != tmp)
            {
                if (activePipes > desiredActivePipeCount)
                {
                    int end = activePipes - desiredActivePipeCount, i = 0;

                    foreach (Pipe p in FindObjectsOfType<Pipe>())
                    {
                        if (!p.IsPaused())
                        {
                            p.EndPipe();

                            if (++i > end)
                                break;
                        }
                    }
                }
                else
                {
                    if (!paused)
                        doUpdate = true;
                }
            }
        }

        GUILayout.Label("Max Pipes on Screen: " + maxPipesOnScreen);
        {
            tmp = maxPipesOnScreen;
            maxPipesOnScreen = (int)GUILayout.HorizontalSlider(maxPipesOnScreen, desiredActivePipeCount, 12);
            if (tmp != maxPipesOnScreen)
                doUpdate = true;
        }

        GUILayout.Label("Speed: " + pipeSpeed);
        {
            tmp = (int)pipeSpeed;
            pipeSpeed = GUILayout.HorizontalSlider(pipeSpeed, 1f, 40f);

            if ((int)pipeSpeed != tmp && !paused)
                SetSpeed(pipeSpeed);
        }

        GUILayout.Space(6);

        GUILayout.Label("Camera Orbit Speed: " + rotateCam.idleSpeed.ToString("F2"));
        {
            rotateCam.idleSpeed = GUILayout.HorizontalSlider(rotateCam.idleSpeed, .01f, 10f);
        }

        if (GUILayout.Button("Reset"))
        {
            ResetPipes(); // Call the new ResetPipes method
        }

        if (doUpdate)
        {
            ClearPipes();
            if (activePipes < desiredActivePipeCount)
                StartCoroutine(SpawnPipes());
        }

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    public void OnPipeFinished(Pipe pipe)
    {
        pipe.OnPipeFinished -= this.OnPipeFinished;

        activePipes--;

        finishedPipes.Add(pipe);

        ClearPipes();

        if (activePipes < desiredActivePipeCount)
            StartCoroutine(SpawnPipes());
    }

    void SpawnPipe()
    {
        GameObject pipeObj = new GameObject("Pipe");
        Pipe pipe = pipeObj.AddComponent<Pipe>();
        pipe.SetSpeed(pipeSpeed);
        pipe.SetMaxTurns((int)Random.Range(minPipeTurns, maxPipeTurns));
        pipe.pipeColor = Random.ColorHSV(); // Set a new random color for the pipe

        pipe.OnPipeFinished += OnPipeFinished;

        activePipes++;
    }

    void ClearPipes()
    {
        int totalPipeCount = activePipes + finishedPipes.Count;

        while (totalPipeCount > maxPipesOnScreen && finishedPipes.Count > 0)
        {
            finishedPipes[0].GetComponent<Pipe>().FadeOut(fadeTime, 0f, fadeMaterial);
            finishedPipes.RemoveAt(0);
            totalPipeCount--;
        }
    }

    public void ResetPipes()
    {
        activePipes = 0;
        finishedPipes.Clear();

        foreach (Pipe pipe in FindObjectsOfType<Pipe>())
        {
            Destroy(pipe.gameObject);
        }

        StartCoroutine(SpawnPipes());
    }

    Vector3 GetStartPosition()
    {
        return Vector3.zero; // Start at the origin
    }

    void OnDrawGizmos()
    {
        // Optionally, you can draw gizmos for visualization if needed.
    }
}
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PipeSpawner : MonoBehaviour
{
    public int desiredActivePipeCount = 1; // Number of active pipes to maintain
    public float pipeSpeed = 13f; // Speed of the pipes
    public int maxPipesOnScreen = 7; // Maximum number of pipes allowed on screen at once
    public int minPipeTurns = 100; // Minimum number of turns a pipe must make
    public int maxPipeTurns = 200; // Maximum number of turns a pipe can make
    public Material material; // Material for the pipes
    public Material fadeMaterial; // Material for fading the pipes
    public float fadeTime = 2f; // Duration of the fade out effect
    public Texture2D pausedImage; // Image to display when paused

    private int activePipes = 0; // Counter for active pipes
    private List<Pipe> finishedPipes = new List<Pipe>(); // List to keep track of finished pipes
    private RotateCamera rotateCam; // Reference to the RotateCamera script
    private bool paused = false; // Flag to check if the spawner is paused
    private bool showSettings = false; // Flag to toggle settings display
    public Rect settingsWindowRect = new Rect(10, 20, 200, 200); // Position and size of the settings window

    void Awake()
    {
        rotateCam = Camera.main.GetComponent<RotateCamera>(); // Get the RotateCamera component from the main camera
    }

    void Start()
    {
        StartCoroutine(SpawnPipes()); // Start the coroutine to spawn pipes
    }

    IEnumerator SpawnPipes()
    {
        while (activePipes < desiredActivePipeCount && !paused)
        {
            SpawnPipe(); // Spawn a new pipe
            yield return new WaitForSeconds(Random.Range(.5f, 1f)); // Wait for a random duration before spawning the next pipe
        }
    }

    public bool IsPaused()
    {
        return paused; // Return the paused state
    }

    void SetSpeed(float speed)
    {
        foreach (Pipe pipe in FindObjectsOfType<Pipe>())
        {
            pipe.SetSpeed(speed); // Set the speed for each active pipe
        }
    }

    void OnGUI()
    {
        showSettings = GUILayout.Toggle(showSettings, "Settings"); // Toggle the settings display

        if (showSettings)
        {
            settingsWindowRect = GUILayout.Window(0, settingsWindowRect, SettingsWindow, "Settings"); // Display the settings window
            settingsWindowRect.x = Mathf.Clamp(settingsWindowRect.x, 0, Screen.width - settingsWindowRect.width); // Clamp the window position within the screen
            settingsWindowRect.y = Mathf.Clamp(settingsWindowRect.y, 0, Screen.height - 20);
        }

        if (paused)
        {
            if (pausedImage != null)
            {
                if (GUI.Button(new Rect(Screen.width / 2f - pausedImage.width / 2f, Screen.height - pausedImage.height - 20, pausedImage.width, pausedImage.height), pausedImage, GUIStyle.none))
                    TogglePause(); // Toggle pause state when the image button is clicked
            }
            else
                GUI.Label(new Rect(Screen.width / 2f - 60, Screen.height - 40, 120, 24), "PAUSED"); // Display paused text if no image is set
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            TogglePause(); // Toggle pause state when the space key is pressed
        }
    }

    public void TogglePause()
    {
        paused = !paused;

        if (!paused && activePipes < desiredActivePipeCount)
        {
            SetSpeed(pipeSpeed);
            StartCoroutine(SpawnPipes());
        }

        SetSpeed(paused ? 0f : pipeSpeed); // Set speed to 0 if paused, else set to pipeSpeed
    }

    void SettingsWindow(int id)
    {
        int tmp = 0;
        bool doUpdate = false;

        GUILayout.Label("\"Escape\" key to Quit\n\"Space\" key to Pause");

        GUILayout.Label("Active Pipes: " + desiredActivePipeCount);
        {
            tmp = desiredActivePipeCount;
            desiredActivePipeCount = (int)GUILayout.HorizontalSlider(desiredActivePipeCount, 1, 8);

            if (desiredActivePipeCount != tmp)
            {
                if (activePipes > desiredActivePipeCount)
                {
                    int end = activePipes - desiredActivePipeCount, i = 0;

                    foreach (Pipe p in FindObjectsOfType<Pipe>())
                    {
                        if (!p.IsPaused())
                        {
                            p.EndPipe(); // End the pipe if it's not paused

                            if (++i > end)
                                break;
                        }
                    }
                }
                else
                {
                    if (!paused)
                        doUpdate = true;
                }
            }
        }

        GUILayout.Label("Max Pipes on Screen: " + maxPipesOnScreen);
        {
            tmp = maxPipesOnScreen;
            maxPipesOnScreen = (int)GUILayout.HorizontalSlider(maxPipesOnScreen, desiredActivePipeCount, 12);
            if (tmp != maxPipesOnScreen)
                doUpdate = true;
        }

        GUILayout.Label("Speed: " + pipeSpeed);
        {
            tmp = (int)pipeSpeed;
            pipeSpeed = GUILayout.HorizontalSlider(pipeSpeed, 1f, 40f);

            if ((int)pipeSpeed != tmp && !paused)
                SetSpeed(pipeSpeed); // Update the speed of the pipes if it has changed and not paused
        }

        GUILayout.Space(6);

        GUILayout.Label("Camera Orbit Speed: " + rotateCam.idleSpeed.ToString("F2"));
        {
            rotateCam.idleSpeed = GUILayout.HorizontalSlider(rotateCam.idleSpeed, .01f, 10f); // Update the camera orbit speed
        }

        if (GUILayout.Button("Reset"))
        {
            ResetPipes(); // Call the ResetPipes method to reset the pipe system
        }

        if (doUpdate)
        {
            ClearPipes(); // Clear finished pipes if necessary
            if (activePipes < desiredActivePipeCount)
                StartCoroutine(SpawnPipes());
        }

        GUI.DragWindow(new Rect(0, 0, 10000, 20)); // Allow the settings window to be dragged
    }

    public void OnPipeFinished(Pipe pipe)
    {
        pipe.OnPipeFinished -= this.OnPipeFinished; // Unsubscribe from the OnPipeFinished event

        activePipes--; // Decrement the active pipes count

        finishedPipes.Add(pipe); // Add the finished pipe to the list

        ClearPipes(); // Clear finished pipes if necessary

        if (activePipes < desiredActivePipeCount)
            StartCoroutine(SpawnPipes()); // Spawn new pipes if needed
    }

    void SpawnPipe()
    {
        GameObject pipeObj = new GameObject("Pipe"); // Create a new GameObject for the pipe
        Pipe pipe = pipeObj.AddComponent<Pipe>(); // Add the Pipe component to the GameObject
        pipe.SetSpeed(pipeSpeed); // Set the speed of the pipe
        pipe.SetMaxTurns((int)Random.Range(minPipeTurns, maxPipeTurns)); // Set the maximum number of turns for the pipe
        pipe.pipeColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f); // Set a new random lighter color for the pipe

        pipe.OnPipeFinished += OnPipeFinished; // Subscribe to the OnPipeFinished event

        activePipes++; // Increment the active pipes count
    }

    void ClearPipes()
    {
        int totalPipeCount = activePipes + finishedPipes.Count; // Calculate the total pipe count

        while (totalPipeCount > maxPipesOnScreen && finishedPipes.Count > 0)
        {
            finishedPipes[0].GetComponent<Pipe>().FadeOut(fadeTime, 0f, fadeMaterial); // Fade out the oldest finished pipe
            finishedPipes.RemoveAt(0); // Remove the faded pipe from the list
            totalPipeCount--; // Decrement the total pipe count
        }
    }

    public void ResetPipes()
    {
        activePipes = 0; // Reset the active pipes count
        finishedPipes.Clear(); // Clear the list of finished pipes

        foreach (Pipe pipe in FindObjectsOfType<Pipe>())
        {
            Destroy(pipe.gameObject); // Destroy all active pipe GameObjects
        }

        StartCoroutine(SpawnPipes()); // Restart spawning pipes
    }

    Vector3 GetStartPosition()
    {
        return Vector3.zero; // Start at the origin
    }

    void OnDrawGizmos()
    {
        // Optionally, you can draw gizmos for visualization if needed.
    }
}
