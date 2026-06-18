using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to any persistent GameObject (e.g. XR Origin).
// Requires a World Space Canvas assigned to menuCanvas.
// Wire OnClick of each button in the Inspector to the public methods below.
public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    [Header("Canvas")]
    public Canvas menuCanvas;
    public float distanceFromCamera = 1.8f;
    [Tooltip("How fast the menu follows your head. Lower = more lag, feels natural. 0 = doesn't follow.")]
    public float followSpeed = 4f;

    [Header("Panels (children of the canvas)")]
    public GameObject mainPanel;
    public GameObject instructionsPanel;

    [Header("Volume UI")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeLabel;

    private Camera _cam;
    private bool _open;

    public bool IsOpen => _open;

    void Awake()
    {
        Instance = this;
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);
    }

    void Start()
    {
        _cam = Camera.main;

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value    = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            UpdateVolumeLabel(AudioListener.volume);
        }
    }

    void LateUpdate()
    {
        if (!_open || menuCanvas == null || followSpeed <= 0f) return;
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.01f) forward = _cam.transform.forward;
        forward.Normalize();

        Vector3   targetPos = _cam.transform.position + forward * distanceFromCamera;
        Quaternion targetRot = Quaternion.LookRotation(forward);

        menuCanvas.transform.position = Vector3.Lerp(
            menuCanvas.transform.position, targetPos, Time.deltaTime * followSpeed);
        menuCanvas.transform.rotation = Quaternion.Slerp(
            menuCanvas.transform.rotation, targetRot, Time.deltaTime * followSpeed);
    }

    // ── Called by Y / M key via VRInputHandler ───────────────────────────────
    public void Toggle()
    {
        if (_open) Close();
        else       Open();
    }

    public void Open()
    {
        if (_cam == null) _cam = Camera.main;

        _open = true;
        menuCanvas.gameObject.SetActive(true);
        ShowMain();
        PlaceInFrontOfCamera();

        // Close any open dino panel so they don't overlap
        DinoPanelSwitcher.CloseOpenPanel();
    }

    public void Close()
    {
        _open = false;
        menuCanvas.gameObject.SetActive(false);
    }

    // ── Button callbacks (wire in Inspector) ─────────────────────────────────

    // "Instruções" button
    public void ShowInstructions()
    {
        if (mainPanel != null)        mainPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
    }

    // "Voltar" button inside instructionsPanel
    public void ShowMain()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (mainPanel != null)         mainPanel.SetActive(true);
    }

    // "Sair" button
    public void ExitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        UpdateVolumeLabel(value);
    }

    private void UpdateVolumeLabel(float value)
    {
        if (volumeLabel != null)
            volumeLabel.text = "Volume: " + Mathf.RoundToInt(value * 100) + "%";
    }

    private void PlaceInFrontOfCamera()
    {
        if (_cam == null) return;
        Transform cam = _cam.transform;

        Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
        forward.Normalize();

        menuCanvas.transform.position = cam.position + forward * distanceFromCamera;
        menuCanvas.transform.rotation = Quaternion.LookRotation(forward);
    }
}
