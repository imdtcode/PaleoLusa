using UnityEngine;
using UnityEngine.InputSystem;

public class DinoPanelSwitcher : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject dinoInfoRoot;
    public GameObject infoPanel;
    public GameObject comparePanel;
    public GameObject xrayPanel;

    [Header("Dinossauro")]
    public Animator animator;
    public MonoBehaviour[] scriptsToDisable;

    private int currentPanel = 0;
    private bool isOpen = false;

    void Start()
    {
        ForceClose();
    }

    void Update()
    {
        // Apenas para testar no PC
        if (Keyboard.current == null) return;

        if (Keyboard.current.tKey.wasPressedThisFrame)
            TogglePanel();

        if (Keyboard.current.uKey.wasPressedThisFrame)
            NextPanel();
    }

    // Esta função será chamada pelo VR
    public void TogglePanel()
    {
        if (isOpen)
            ForceClose();
        else
            ForceOpen();
    }

    // Esta função também pode ser chamada por botão VR
    public void NextPanel()
    {
        if (!isOpen) return;

        currentPanel++;
        if (currentPanel > 2)
            currentPanel = 0;

        ShowPanel(currentPanel);
    }

    public void ShowInfo()
    {
        ForceOpen();
        ShowPanel(0);
    }

    public void ShowCompare()
    {
        ForceOpen();
        ShowPanel(1);
    }

    public void ShowXray()
    {
        ForceOpen();
        ShowPanel(2);
    }

    public void ClosePanel()
    {
        ForceClose();
    }

    void ForceOpen()
    {
        isOpen = true;

        if (dinoInfoRoot != null)
            dinoInfoRoot.SetActive(true);

        ShowPanel(currentPanel);
        PauseDino(true);
    }

    void ForceClose()
    {
        isOpen = false;
        currentPanel = 0;

        if (infoPanel != null) infoPanel.SetActive(false);
        if (comparePanel != null) comparePanel.SetActive(false);
        if (xrayPanel != null) xrayPanel.SetActive(false);

        if (dinoInfoRoot != null)
            dinoInfoRoot.SetActive(false);

        PauseDino(false);
    }

    void ShowPanel(int index)
    {
        currentPanel = index;

        if (infoPanel != null) infoPanel.SetActive(index == 0);
        if (comparePanel != null) comparePanel.SetActive(index == 1);
        if (xrayPanel != null) xrayPanel.SetActive(index == 2);
    }

    void PauseDino(bool pause)
    {
        if (animator != null)
            animator.speed = pause ? 0f : 1f;

        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = !pause;
        }
    }
}