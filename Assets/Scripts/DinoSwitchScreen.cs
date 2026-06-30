using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DinoPanelSwitcher : MonoBehaviour
{
    private static readonly List<DinoPanelSwitcher> Instances = new List<DinoPanelSwitcher>();
    private static DinoPanelSwitcher openInstance;

    [Header("Paineis")]
    public GameObject dinoInfoRoot;
    public GameObject infoPanel;
    public GameObject comparePanel;
    public GameObject xrayPanel;

    [Header("Dinossauro")]
    public Animator animator;
    public MonoBehaviour[] scriptsToDisable = new MonoBehaviour[0];

    [Header("Barreira de Proximidade")]
    [Tooltip("SphereCollider que bloqueia o jogador quando o painel está aberto.")]
    public SphereCollider proximityWall;

    [Header("Teste PC")]
    public bool enableKeyboardDebugInput = true;
    public Key toggleKey = Key.T;
    public Key nextPanelKey = Key.U;

    private bool[] cachedScriptStates = new bool[0];
    private float cachedAnimatorSpeed = 1f;
    private int currentPanel;
    private bool isOpen;
    private bool isPausedByPanel;

    public bool IsOpen => isOpen;

    void OnEnable()
    {
        EnsureScriptArray();

        if (!Instances.Contains(this))
            Instances.Add(this);
    }

    void OnDisable()
    {
        if (openInstance == this)
            openInstance = null;

        PauseDino(false);
        Instances.Remove(this);
    }

    void Start()
    {
        EnsureScriptArray();
        ForceClose();
    }

    void Update()
    {
        if (!enableKeyboardDebugInput || Keyboard.current == null)
            return;

        if (!ReferenceEquals(this, GetKeyboardTarget()))
            return;

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
            TogglePanel();

        if (Keyboard.current[nextPanelKey].wasPressedThisFrame)
            NextPanel();
    }

    public void TogglePanel()
    {
        if (isOpen)
            ForceClose();
        else
            ForceOpen();
    }

    public void NextPanel()
    {
        if (!isOpen)
            return;

        currentPanel = (currentPanel + 1) % 3;
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

    public static void CloseOpenPanel()
    {
        if (openInstance != null)
            openInstance.ForceClose();
    }

    public static void NextOpenPanel()
    {
        if (openInstance != null)
            openInstance.NextPanel();
    }

    private void ForceOpen()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsOpen)
            return;

        if (openInstance != null && openInstance != this)
            openInstance.ForceClose();

        isOpen = true;
        openInstance = this;

        if (dinoInfoRoot != null)
            dinoInfoRoot.SetActive(true);

        ShowPanel(currentPanel);
        PauseDino(true);
    }

    private void ForceClose()
    {
        isOpen = false;
        currentPanel = 0;

        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (comparePanel != null)
            comparePanel.SetActive(false);

        if (xrayPanel != null)
            xrayPanel.SetActive(false);

        if (dinoInfoRoot != null)
            dinoInfoRoot.SetActive(false);

        if (openInstance == this)
            openInstance = null;

        PauseDino(false);
    }

    private void ShowPanel(int index)
    {
        currentPanel = Mathf.Clamp(index, 0, 2);

        if (dinoInfoRoot != null && !dinoInfoRoot.activeSelf)
            dinoInfoRoot.SetActive(true);

        if (infoPanel != null)
            infoPanel.SetActive(currentPanel == 0);

        if (comparePanel != null)
            comparePanel.SetActive(currentPanel == 1);

        if (xrayPanel != null)
            xrayPanel.SetActive(currentPanel == 2);
    }

    private void PauseDino(bool pause)
    {
        EnsureScriptArray();

        if (pause)
        {
            if (!isPausedByPanel)
            {
                CacheScriptStates();

                if (animator != null)
                    cachedAnimatorSpeed = animator.speed;
            }

            if (animator != null)
                animator.speed = 0f;

            for (int i = 0; i < scriptsToDisable.Length; i++)
            {
                if (scriptsToDisable[i] != null)
                    scriptsToDisable[i].enabled = false;
            }

            if (proximityWall != null)
                proximityWall.enabled = true;

            isPausedByPanel = true;
            return;
        }

        if (!isPausedByPanel)
            return;

        if (animator != null)
            animator.speed = cachedAnimatorSpeed;

        if (proximityWall != null)
            proximityWall.enabled = false;

        for (int i = 0; i < scriptsToDisable.Length; i++)
        {
            MonoBehaviour script = scriptsToDisable[i];
            if (script != null && i < cachedScriptStates.Length)
                script.enabled = cachedScriptStates[i];
        }

        isPausedByPanel = false;
    }

    private void CacheScriptStates()
    {
        EnsureScriptArray();

        if (cachedScriptStates.Length != scriptsToDisable.Length)
            cachedScriptStates = new bool[scriptsToDisable.Length];

        for (int i = 0; i < scriptsToDisable.Length; i++)
        {
            MonoBehaviour script = scriptsToDisable[i];
            cachedScriptStates[i] = script != null && script.enabled;
        }
    }

    private static DinoPanelSwitcher GetKeyboardTarget()
    {
        if (openInstance != null && openInstance.isActiveAndEnabled)
            return openInstance;

        Camera mainCamera = Camera.main;
        DinoPanelSwitcher closest = null;
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < Instances.Count; i++)
        {
            DinoPanelSwitcher instance = Instances[i];
            if (instance == null || !instance.isActiveAndEnabled || !instance.enableKeyboardDebugInput)
                continue;

            float score = mainCamera == null
                ? i
                : (instance.transform.position - mainCamera.transform.position).sqrMagnitude;

            if (score < closestDistance)
            {
                closestDistance = score;
                closest = instance;
            }
        }

        return closest;
    }

    private void EnsureScriptArray()
    {
        if (scriptsToDisable == null)
            scriptsToDisable = new MonoBehaviour[0];
    }
}
