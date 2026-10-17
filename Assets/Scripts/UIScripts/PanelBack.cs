using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PanelBack : MonoBehaviour, ICancelHandler
{
    [SerializeField] GameObject previousPanel;
    [SerializeField] GameObject firstSelected;
    [SerializeField] GameObject buttonToReselect;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public void OnCancel(BaseEventData eventData) => GoBack();

    public void GoBack()
    {
        previousPanel.SetActive(true);
        gameObject.SetActive(false);
        EventSystem.current.StartCoroutine(Reselect());
    }

    IEnumerator Reselect()
    {
        yield return null;
        Debug.Log("Reselecting: " + (buttonToReselect != null ? buttonToReselect.name : "NULL")
                  + " | active: " + (buttonToReselect != null && buttonToReselect.activeInHierarchy));
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonToReselect);
        Debug.Log("Now selected: " + EventSystem.current.currentSelectedGameObject);
    }
}