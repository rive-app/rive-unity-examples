using UnityEngine;
using UnityEngine.SceneManagement;

public class BackMenuController : MonoBehaviour
{
    [SerializeField] private int menuIndex = -1;
    [SerializeField] private string menuName = "";
    public static BackMenuController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist this object across scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Destroy any duplicates
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuName != "")
            {
                SceneManager.LoadScene(menuName, LoadSceneMode.Single);

            }
            if (menuIndex >= 0)
            {
                SceneManager.LoadScene(0, LoadSceneMode.Single);
            }
        }
    }
}
