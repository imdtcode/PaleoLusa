using UnityEngine;

public class DinoInfoUI : MonoBehaviour
{
    public GameObject infoCanvas;
    public DinoPanelSwitcher panelSwitcher;

    void Awake()
    {
        if (panelSwitcher == null)
            panelSwitcher = GetComponentInParent<DinoPanelSwitcher>();
    }

    void Start()
    {
        if (panelSwitcher != null)
        {
            panelSwitcher.ClosePanel();
            return;
        }

        if (infoCanvas != null)
            infoCanvas.SetActive(false);
    }

    public void ToggleInfo()
    {
        if (panelSwitcher != null)
        {
            panelSwitcher.TogglePanel();
            return;
        }

        if (infoCanvas != null)
            infoCanvas.SetActive(!infoCanvas.activeSelf);
    }
}
