using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.XR.CoreUtils;

// Attach to any persistent GameObject (e.g. XR Origin).
// Requires a World Space Canvas assigned to menuCanvas.
// Wire OnClick of each button in the Inspector to the public methods below.
public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    [Header("Canvas")]
    public Canvas menuCanvas;
    public float distanceFromCamera = 1.8f;
    [Tooltip("Vertical offset above camera position. Raise if menu appears too low.")]
    public float heightOffset = 0.0f;

    [Header("Panels (children of the canvas)")]
    public GameObject mainPanel;
    public GameObject instructionsPanel;

    [Header("Volume UI")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeLabel;

    [Header("VR Camera (optional override)")]
    [Tooltip("Leave empty — auto-resolved from XROrigin. Fill only if camera lookup fails.")]
    public Camera vrCamera;

    private XROrigin _xrOrigin;
    private bool _open;
    private readonly List<(GameObject obj, int layer)> _dinoLayers = new();

    public bool IsOpen => _open;

    void Awake()
    {
        Instance = this;
        _xrOrigin = GetComponent<XROrigin>();
        if (_xrOrigin == null) _xrOrigin = FindFirstObjectByType<XROrigin>();
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);
    }

    void Start()
    {
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
        if (!_open || menuCanvas == null) return;

        Camera cam = GetVRCamera();
        if (cam == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.01f) forward = cam.transform.forward;
        forward.Normalize();

        Vector3 targetPos = cam.transform.position + forward * distanceFromCamera;
        targetPos.y = cam.transform.position.y + heightOffset;
        menuCanvas.transform.SetPositionAndRotation(targetPos, Quaternion.LookRotation(forward));
    }

    // ── Called by Y / M key via VRInputHandler ───────────────────────────────
    public void Toggle()
    {
        if (_open) Close();
        else       Open();
    }

    public void Open()
    {
        _open = true;
        menuCanvas.gameObject.SetActive(true);
        ShowMain();
        PlaceInFrontOfCamera();

        DinoPanelSwitcher.CloseOpenPanel();

        // Move ONLY dino CCs to layer 2 (Ignore Raycast) so XR rays reach menu buttons.
        // Filtering by "Dino" layer ensures the player's own CC is never touched.
        int dinoLayer = LayerMask.NameToLayer("Dino");
        _dinoLayers.Clear();
        foreach (var cc in FindObjectsByType<CharacterController>(FindObjectsSortMode.None))
        {
            if (cc.gameObject.layer != dinoLayer) continue;
            _dinoLayers.Add((cc.gameObject, cc.gameObject.layer));
            cc.gameObject.layer = 2;
        }
    }

    public void Close()
    {
        _open = false;
        menuCanvas.gameObject.SetActive(false);

        foreach (var (obj, layer) in _dinoLayers)
            if (obj != null) obj.layer = layer;
        _dinoLayers.Clear();
    }

    // ── Button callbacks (wire in Inspector) ─────────────────────────────────

    public void ShowInstructions()
    {
        if (mainPanel != null)        mainPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
    }

    public void ShowMain()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (mainPanel != null)         mainPanel.SetActive(true);
    }

    public void ExitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private Camera GetVRCamera()
    {
        if (_xrOrigin != null && _xrOrigin.Camera != null && _xrOrigin.Camera.isActiveAndEnabled)
            return _xrOrigin.Camera;
        if (vrCamera != null) return vrCamera;
        return Camera.main;
    }

    private void PlaceInFrontOfCamera()
    {
        Camera cam = GetVRCamera();
        if (cam == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
        forward.Normalize();

        Vector3 pos = cam.transform.position + forward * distanceFromCamera;
        pos.y = cam.transform.position.y + heightOffset;
        menuCanvas.transform.position = pos;
        menuCanvas.transform.rotation = Quaternion.LookRotation(forward);
    }

    public void SyncVolumeUI(float value)
    {
        if (volumeSlider != null) volumeSlider.value = value;
        UpdateVolumeLabel(value);
    }

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
}
