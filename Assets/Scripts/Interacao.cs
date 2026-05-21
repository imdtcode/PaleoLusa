using UnityEngine;

public class DinoInfoKeyboardTest : MonoBehaviour
{
    public GameObject dinoInfo;

    void Start()
    {
        if (dinoInfo != null)
            dinoInfo.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Carregaste no T");

            if (dinoInfo != null)
                dinoInfo.SetActive(!dinoInfo.activeSelf);
        }
    }
}