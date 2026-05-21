using UnityEngine;

public class DinoInfoToggle : MonoBehaviour
{
    public GameObject dinoInfo;

    private bool isOpen = false;

    void Start()
    {
        if (dinoInfo != null)
            dinoInfo.SetActive(false);
    }

    public void ToggleInfo()
    {
        isOpen = !isOpen;

        if (dinoInfo != null)
            dinoInfo.SetActive(isOpen);
    }
}