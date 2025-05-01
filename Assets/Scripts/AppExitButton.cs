using UnityEngine;

public class AppExitButton : MonoBehaviour
{
    public void ExitApp()
    {
#if UNITY_EDITOR
        // Im Editor: Playmode stoppen
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Auf Gerät: App beenden
        Application.Quit();
#endif
    }
}
