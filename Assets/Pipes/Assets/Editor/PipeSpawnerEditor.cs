using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PipeSpawner))]
public class PipeSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PipeSpawner spawner = (PipeSpawner)target; // Get the target object

        // Create a slider for desired active pipe count with a range from 1 to 8
        spawner.desiredActivePipeCount = EditorGUILayout.IntSlider("Desired Active Pipe Count", spawner.desiredActivePipeCount, 1, 8);

        // Create a slider for pipe speed with a range from 1 to 40
        spawner.pipeSpeed = EditorGUILayout.Slider("Pipe Speed", spawner.pipeSpeed, 1f, 40f);

        // Create a slider for max pipes on screen with a range from desiredActivePipeCount to 12
        spawner.maxPipesOnScreen = EditorGUILayout.IntSlider("Max Pipes On Screen", spawner.maxPipesOnScreen, spawner.desiredActivePipeCount, 12);

        // Create a slider for min pipe turns with a range from 1 to 200
        spawner.minPipeTurns = EditorGUILayout.IntSlider("Min Pipe Turns", spawner.minPipeTurns, 1, 200);

        // Create a slider for max pipe turns with a range from minPipeTurns to 400
        spawner.maxPipeTurns = EditorGUILayout.IntSlider("Max Pipe Turns", spawner.maxPipeTurns, spawner.minPipeTurns, 400);

        // Create an object field for the material used for the pipes
        spawner.material = (Material)EditorGUILayout.ObjectField("Material", spawner.material, typeof(Material), false);

        // Create an object field for the material used for fading the pipes
        spawner.fadeMaterial = (Material)EditorGUILayout.ObjectField("Fade Material", spawner.fadeMaterial, typeof(Material), false);

        // Create a float field for fade time
        spawner.fadeTime = EditorGUILayout.FloatField("Fade Time", spawner.fadeTime);

        // Create an object field for the paused image
        spawner.pausedImage = (Texture2D)EditorGUILayout.ObjectField("Paused Image", spawner.pausedImage, typeof(Texture2D), false);

        // Create a button to reset the pipes
        if (GUILayout.Button("Reset Pipes"))
        {
            spawner.ResetPipes(); // Ensure this method exists in PipeSpawner
        }

        // If any GUI element has changed, mark the target object as dirty to save changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
