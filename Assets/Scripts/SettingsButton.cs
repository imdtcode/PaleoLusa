using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    public enum ButtonType { Instructions, Exit, Close, Back }
    public ButtonType buttonType;

    public void Press()
    {
        if (PauseMenuController.Instance == null) return;

        switch (buttonType)
        {
            case ButtonType.Instructions: PauseMenuController.Instance.ShowInstructions(); break;
            case ButtonType.Exit:         PauseMenuController.Instance.ExitApp();          break;
            case ButtonType.Close:        PauseMenuController.Instance.Close();            break;
            case ButtonType.Back:         PauseMenuController.Instance.ShowMain();         break;
        }
    }
}
