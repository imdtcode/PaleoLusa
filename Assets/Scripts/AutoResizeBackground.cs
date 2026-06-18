using UnityEngine;
using TMPro;

public class AutoResizeBackground : MonoBehaviour
{
    public TextMeshPro textMesh;
    public Transform background;

    public float paddingX = 0.5f;
    public float paddingY = 0.3f;

    private string lastText;

    void Update()
    {
        if (textMesh == null || background == null) return;
        if (textMesh.text == lastText) return;

        lastText = textMesh.text;
        textMesh.ForceMeshUpdate();

        Vector2 size = textMesh.GetRenderedValues(false);

        background.localScale = new Vector3(
            size.x + paddingX,
            size.y + paddingY,
            1f
        );
    }
}