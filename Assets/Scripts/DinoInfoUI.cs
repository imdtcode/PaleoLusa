using UnityEngine;

public class DinoInfoUI : MonoBehaviour
{
    public GameObject infoCanvas;

    void Start()
    {
        infoCanvas.SetActive(false);
    }

    public void ToggleInfo()
    {
        infoCanvas.SetActive(!infoCanvas.activeSelf);
    }
}