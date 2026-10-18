using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    // Main menu references
    GameObject MainMenu, OptionsMenu;
    Button Play, LevelT, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Options, Exit, OptionsBack;

    // Pause menu references
    GameObject PauseMenu, PauseOptionsMenu;
    Button Resume, MainMenuButton, OptionsButton, PauseOptionsBack;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    //look for references
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearReferences();

        // Search only inside the scene's canvas (name must match yours)
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogWarning("Canvas not found in " + scene.name); return; }
        Transform root = canvas.transform;

        if (scene.name == "Menu")
        {
            MainMenu = GetObj(root, "MainMenu");
            OptionsMenu = GetObj(root, "OptionsMenu");
            Play = Get<Button>(root, "PlayButton");
            LevelT = Get<Button>(root, "LevelT");
            Level1 = Get<Button>(root, "Level1");

            Level2 = Get<Button>(root, "Level2");

            Level3 = Get<Button>(root, "Level3");

            Level4 = Get<Button>(root, "Level4");

            Level5 = Get<Button>(root, "Level5");

            Level6 = Get<Button>(root, "Level6");

            Level7 = Get<Button>(root, "Level7");
            var levels = new Button[]{ LevelT, Level1, Level2, Level3, Level4, Level5, Level6, Level7 };
            foreach(var b in levels) {
                b.gameObject.SetActive(false);
            }
            Options = Get<Button>(root, "OptionsButton");
            Exit = Get<Button>(root, "ExitButton");
            OptionsBack = Get<Button>(root, "BackButton");

            Bind(Play, OnPlayButtonClicked);
            Bind(LevelT, () => SceneManager.LoadScene("LevelT"));
            Bind(Level1, () => SceneManager.LoadScene("Level1"));
            Bind(Level2, () => SceneManager.LoadScene("Level2"));
            Bind(Level3, () => SceneManager.LoadScene("Level3"));
            Bind(Level4, () => SceneManager.LoadScene("Level4"));
            Bind(Level5, () => SceneManager.LoadScene("Level5"));
            Bind(Level6, () => SceneManager.LoadScene("Level6"));
            Bind(Level7, () => SceneManager.LoadScene("Level7"));

            Bind(Options, OnOptionsButtonClicked);
            Bind(Exit, OnExitButtonClicked);
            Bind(OptionsBack, OnOptionBackButtonClicked);

            SelectFirstButton(transform);
        }
        else if (scene.name == "LevelT")
        {
            PauseMenu = GetObj(root, "PauseMenu");
            PauseOptionsMenu = GetObj(root, "OptionsMenu");
            Resume = Get<Button>(root, "ResumeButton");
            MainMenuButton = Get<Button>(root, "MainMenu");
            OptionsButton = Get<Button>(root, "OptionsButton");
            PauseOptionsBack = Get<Button>(root, "BackButton");

            Bind(Resume, OnResumeButtonClicked);
            Bind(MainMenuButton, OnMainMenuButtonClicked);
            Bind(OptionsButton, OnOptionButtonClicked);
            Bind(PauseOptionsBack, OnPauseOptionsBackClick);
        }
    }
    //clear all old references
    void ClearReferences()
    {
        MainMenu = OptionsMenu = PauseMenu = PauseOptionsMenu = null;
        Play = Options = Exit = OptionsBack = null;
        Resume = MainMenuButton = OptionsButton = PauseOptionsBack = null;
    }

    // Finds a child by name anywhere under root, INCLUDING inactive objects
    GameObject GetObj(Transform root, string objName)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == objName) return t.gameObject;

        Debug.LogWarning(objName + " not found");
        return null;
    }
    void SelectFirstButton(Transform root)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button.gameObject.activeInHierarchy && button.interactable)
            {
                UnityEngine.EventSystems.EventSystem.current
                    ?.SetSelectedGameObject(button.gameObject);
                return;
            }
        }
    }
    T Get<T>(Transform root, string objName) where T : Component
    {
        var go = GetObj(root, objName);
        return go != null ? go.GetComponent<T>() : null;
    }

    void Bind(Button btn, UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    // MainMenu
    public void OnPlayButtonClicked() {
        Play.gameObject.SetActive(false);
        Options.gameObject.SetActive(false);
        Exit.gameObject.SetActive(false);

        LevelT.gameObject.SetActive(true);
        Level1.gameObject.SetActive(true);
        Level2.gameObject.SetActive(true);
        Level3.gameObject.SetActive(true);
        Level4.gameObject.SetActive(true);
        Level5.gameObject.SetActive(true);
        Level6.gameObject.SetActive(true);
        Level7.gameObject.SetActive(true);

        UnityEngine.EventSystems.EventSystem.current
                    ?.SetSelectedGameObject(LevelT.gameObject);
        //SceneManager.LoadScene("LevelT");
    }

    public void OnExitButtonClicked() => Application.Quit();

    public void OnOptionsButtonClicked()
    {
        MainMenu.SetActive(false);
        OptionsMenu.SetActive(true);
    }

    public void OnOptionBackButtonClicked()
    {
        MainMenu.SetActive(true);
        OptionsMenu.SetActive(false);
    }

    // PauseMenu
    public void OnResumeButtonClicked()
    {
        PauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
    }
    public void OnOptionButtonClicked()
    {
        PauseOptionsMenu.SetActive(true);
        PauseMenu.SetActive(false);
    }
    public void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1.0f;
    }

    public void OnPauseOptionsBackClick()
    {
        PauseOptionsMenu.SetActive(false);
        PauseMenu.SetActive(true);
    }
}