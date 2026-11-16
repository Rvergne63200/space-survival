using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Main menu (scene 0_MainMenu): new game, load a game, quit.
// The layout is in Assets/UI/Menus (MainMenu.uxml, SaveSlot.uxml, Menus.uss), editable in the UI Builder:
// the elements are found by their name, a missing element only disables its feature.
[RequireComponent(typeof(UIDocument))]
public class MainMenuUI : MonoBehaviour
{
    // One line of the save list (SaveSlot.uxml)
    [SerializeField] private VisualTreeAsset saveSlotTemplate;
    [SerializeField] private string worldScene = "0_World";

    private VisualElement mainPanel;
    private VisualElement newGamePanel;
    private VisualElement loadPanel;
    private VisualElement confirmPanel;
    private readonly List<VisualElement> panels = new List<VisualElement>();

    private TextField saveNameField;
    private TextField seedField;
    private Label newGameError;
    private Label noSaveLabel;
    private Label confirmLabel;
    private ScrollView saveList;

    private SaveSlotInfo saveToDelete;

    private void OnEnable()
    {
        // Free cursor, no player control
        ModeManager.Instance.EnableMode(PlayingMode.Menu);
    }

    private void OnDisable()
    {
        ModeManager.Instance.DisableMode(PlayingMode.Menu);
    }

    // In Start: the UIDocument builds its elements in its OnEnable
    private void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        mainPanel = AddPanel(root, "main-panel");
        newGamePanel = AddPanel(root, "new-game-panel");
        loadPanel = AddPanel(root, "load-panel");
        confirmPanel = AddPanel(root, "confirm-panel");

        saveNameField = root.Q<TextField>("save-name-field");
        seedField = root.Q<TextField>("seed-field");
        newGameError = root.Q<Label>("new-game-error");
        noSaveLabel = root.Q<Label>("no-save-label");
        confirmLabel = root.Q<Label>("confirm-label");
        saveList = root.Q<ScrollView>("save-list");

        BindButton(root, "new-game-button", ShowNewGame);
        BindButton(root, "load-game-button", ShowLoad);
        BindButton(root, "quit-button", Quit);

        BindButton(root, "random-seed-button", RandomizeSeed);
        BindButton(root, "create-game-button", CreateGame);
        BindButton(root, "new-game-back-button", () => ShowPanel(mainPanel));

        BindButton(root, "load-back-button", () => ShowPanel(mainPanel));

        BindButton(root, "confirm-yes-button", ConfirmDelete);
        BindButton(root, "confirm-no-button", () => ShowPanel(loadPanel));

        ShowPanel(mainPanel);
    }

    private VisualElement AddPanel(VisualElement root, string name)
    {
        VisualElement panel = root.Q(name);

        if (panel == null)
        {
            Debug.LogWarning("Main menu: no element named \"" + name + "\" in the UXML.");
            return null;
        }

        panels.Add(panel);
        return panel;
    }

    private void BindButton(VisualElement root, string name, Action action)
    {
        Button button = root.Q<Button>(name);

        if (button == null)
        {
            Debug.LogWarning("Main menu: no button named \"" + name + "\" in the UXML.");
            return;
        }

        button.clicked += action;
    }

    // Panels are hidden with the "hidden" class (Menus.uss)
    private void ShowPanel(VisualElement shown)
    {
        foreach (VisualElement panel in panels)
        {
            panel.EnableInClassList("hidden", panel != shown);
        }
    }

    private void SetText(Label label, string text)
    {
        if (label != null)
        {
            label.text = text;
        }
    }

    // ---------- New game ----------

    private void ShowNewGame()
    {
        if (saveNameField != null)
        {
            saveNameField.value = "Partie " + (SaveManager.Instance.ListSaves().Count + 1);
        }

        RandomizeSeed();
        SetText(newGameError, "");
        ShowPanel(newGamePanel);
    }

    private void RandomizeSeed()
    {
        if (seedField != null)
        {
            seedField.value = SaveManager.RandomSeed().ToString();
        }
    }

    private void CreateGame()
    {
        string saveName = saveNameField != null ? saveNameField.value.Trim() : "";

        if (saveName.Length == 0)
        {
            SetText(newGameError, "Saisis un nom de partie.");
            return;
        }

        if (SaveManager.Instance.SaveNameExists(saveName))
        {
            SetText(newGameError, "Une partie porte déjà ce nom.");
            return;
        }

        int seed = seedField != null && seedField.value.Trim().Length > 0 ? SaveManager.ParseSeed(seedField.value) : SaveManager.RandomSeed();

        SaveManager.Instance.NewGame(saveName, seed);
        LoadWorld();
    }

    // ---------- Load a game ----------

    private void ShowLoad()
    {
        RefreshSaveList();
        ShowPanel(loadPanel);
    }

    private void RefreshSaveList()
    {
        List<SaveSlotInfo> saves = SaveManager.Instance.ListSaves();

        noSaveLabel?.EnableInClassList("hidden", saves.Count > 0);

        if (saveList == null || saveSlotTemplate == null) return;

        saveList.Clear();

        foreach (SaveSlotInfo info in saves)
        {
            VisualElement slot = saveSlotTemplate.Instantiate();

            SetText(slot.Q<Label>("save-name"), info.name);
            SetText(slot.Q<Label>("save-info"), "Seed " + info.seed + "  -  " + info.lastSaveTime.ToLocalTime().ToString("g"));

            Button loadButton = slot.Q<Button>("save-load-button");
            if (loadButton != null)
            {
                loadButton.clicked += () => LoadGame(info);
            }

            Button deleteButton = slot.Q<Button>("save-delete-button");
            if (deleteButton != null)
            {
                deleteButton.clicked += () => AskDelete(info);
            }

            saveList.Add(slot);
        }
    }

    private void LoadGame(SaveSlotInfo info)
    {
        if (SaveManager.Instance.LoadGame(info.id))
        {
            LoadWorld();
        }
        else
        {
            // Unreadable file: refresh the list
            RefreshSaveList();
        }
    }

    private void AskDelete(SaveSlotInfo info)
    {
        saveToDelete = info;
        SetText(confirmLabel, "Supprimer la partie « " + info.name + " » ?");
        ShowPanel(confirmPanel);
    }

    private void ConfirmDelete()
    {
        if (saveToDelete != null)
        {
            SaveManager.Instance.DeleteSave(saveToDelete.id);
            saveToDelete = null;
        }

        ShowLoad();
    }

    // ---------- Scene ----------

    private void LoadWorld()
    {
        SaveManager.Instance.BeforeSceneChange();
        SceneManager.LoadScene(worldScene, LoadSceneMode.Single);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
