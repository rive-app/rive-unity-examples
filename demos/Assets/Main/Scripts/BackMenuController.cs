using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class BackMenuController : MonoBehaviour
{
    [SerializeField] private int menuIndex = 0;
    [SerializeField] private string menuName = "";
    [SerializeField] private InputActionReference backAction;



    private void OnEnable()
    {
        if (backAction != null && backAction.action != null)
        {
            backAction.action.Enable();
            backAction.action.performed += OnBackPerformed;
        }
    }

    private void OnDisable()
    {
        if (backAction != null && backAction.action != null)
        {
            backAction.action.performed -= OnBackPerformed;
            backAction.action.Disable();
        }
    }

    private void OnBackPerformed(InputAction.CallbackContext ctx)
    {

        if (!string.IsNullOrEmpty(menuName))
        {
            SceneManager.LoadScene(menuName);
        }
        else if (menuIndex >= 0)
        {
            SceneManager.LoadScene(menuIndex);
        }

    }

    public static void NavigateToMainMenu()
    {
        SceneManager.LoadScene(0);
    }


}
