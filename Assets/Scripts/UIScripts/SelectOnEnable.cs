using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectOnEnable : MonoBehaviour
{
    [SerializeField] GameObject buttonToSelect;

    void OnEnable()
    {
        StartCoroutine(SelectAfterDelay());
    }

    IEnumerator SelectAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        if (EventSystem.current == null) yield break;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonToSelect);
    }
}