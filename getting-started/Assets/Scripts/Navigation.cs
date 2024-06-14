using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Used for legacy instruction text only.

public class Navigation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject inst = GameObject.Find("CanvasInstr");

        if (inst != null)
        {

            Text legacyText = GameObject.Find("Instructions").GetComponent<Text>();
            Text legacyTextShadow = GameObject.Find("InstructionsShadow").GetComponent<Text>();

            if (legacyText != null)
                legacyText.text = m_InstructionText;
            if (legacyTextShadow != null)
                legacyTextShadow.text = m_InstructionText;
        }
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

    private float cooldownTime = 1.00f; // Cooldown duration in seconds
    private float lastSceneSwitchTime = 0f;

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

    public static string m_InstructionText = "ESC to quit\nNumber keys to switch scenes";

    public static void DrawInstructions()
    {
        // Instructions
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = UnityEngine.Color.white;

        Vector2 shadowOffset = new Vector2(2, 2);
        GUIStyle shadowStyle = new GUIStyle(style);
        shadowStyle.normal.textColor = UnityEngine.Color.black;

        GUI.Label(new Rect(10 + shadowOffset.x, 10 + shadowOffset.y, 500, 30), m_InstructionText, shadowStyle);
        GUI.Label(new Rect(10, 10, 500, 30), m_InstructionText, style);
    }



}
