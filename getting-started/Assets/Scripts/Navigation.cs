using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Exit the application
            Application.Quit();

            // If running in the Unity editor, stop playing the scene
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        int sceneCount = SceneManager.sceneCountInBuildSettings;

        // Loop through keys 1 to number of scenes
        for (int i = 0; i < sceneCount; i++)
        {
            // Convert the loop index to the corresponding KeyCode (Alpha1, Alpha2, etc.)
            KeyCode key = KeyCode.Alpha1 + i;
            if (Input.GetKeyDown(key))
            {
                LoadSceneByIndex(i);
            }
        }
    }

    private float cooldownTime = 0.5f; // Cooldown duration in seconds
    private float lastSceneSwitchTime = 1f;

    void LoadSceneByIndex(int sceneIndex)
    {
        // Switching scenes too quickly can cause a Unity crash
        // due to RiveScreen.OnGUI() rendering.
        if (Time.time - lastSceneSwitchTime >= cooldownTime &&
            sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
            lastSceneSwitchTime = Time.time;
        }
    }

}
