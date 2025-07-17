using System;
using System.Collections;
using UnityEngine;

public class AutoClicker : MonoBehaviour
{
    [SerializeField] private GameEvent clickEvent;
    private Coroutine autoClickCoroutine;

    private bool isAutoClickActive = false;

    private int autoClickCounter;

    public void ToggleAutoClick()
    {
        if (isAutoClickActive)
        {
            StopCoroutine(autoClickCoroutine);
            isAutoClickActive = false;
            Debug.Log($"Auto clicking is stopped");
        }
        else
        {
            autoClickCoroutine = StartCoroutine(AutoClickLoop());
            isAutoClickActive = true;

            Debug.Log($"Auto clicking is started");
        }
    }

    private IEnumerator AutoClickLoop()
    {
        while (true)
        {
            if (UIController.IsClaimed)
            {
                clickEvent.Raise(NoDetails.Instance);
                autoClickCounter++;
                Debug.Log($"Total Autoclicks {autoClickCounter}");
            }
            yield return new WaitForSeconds(0.20f); //5 click/sec --> 1 click/0.2sec
        }
    }
}
