using UnityEngine;
using System.Collections;

public class DinoControllerSwitcher : MonoBehaviour
{
    [Header("Animator do dinossauro")]
    public Animator animator;

    [Header("Controllers")]
    public RuntimeAnimatorController idleController;
    public RuntimeAnimatorController walkController;
    public RuntimeAnimatorController runController;
    public RuntimeAnimatorController sleepController;

    [Header("Tempo entre mudanças")]
    public float minDelay = 2f;
    public float maxDelay = 5f;

    private RuntimeAnimatorController currentController;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        SetController(idleController);
        StartCoroutine(RandomLoop());
    }

    IEnumerator RandomLoop()
    {
        RuntimeAnimatorController[] controllers =
        {
            idleController,
            walkController,
            runController,
            sleepController
        };

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            RuntimeAnimatorController nextController =
                controllers[Random.Range(0, controllers.Length)];

            if (nextController == null)
                continue;

            if (nextController == currentController)
                continue;

            SetController(nextController);
        }
    }

    void SetController(RuntimeAnimatorController controller)
    {
        if (controller == null || animator == null)
            return;

        animator.runtimeAnimatorController = controller;

        // força o Animator a reiniciar corretamente com o novo controller
        animator.Rebind();
        animator.Update(0f);

        currentController = controller;

        Debug.Log("Dino mudou para: " + controller.name);
    }
}