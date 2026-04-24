using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Text infoText;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GridManager gridManager;

    private List<GameObject> buttons = new List<GameObject>();
    Color doneColor = Color.green;
    Color playingColor = Color.red;
    Color lockedColor = Color.gray;


    private void Start()
    {
        CreateButtons();
    }

    // This method creates buttons for each game level, sets their initial state, and assigns click listeners.
    private void CreateButtons()
    {
        try
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
                Image img = btnObj.GetComponent<Image>();
                Button btn = btnObj.GetComponent<Button>();
                img.color = lockedColor;
                TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                    txt.text = "Game Level " + (i + 1);

                if (btn == null)
                {
                    Debug.LogError("Button component missing on prefab: " + btnObj.name);
                    continue;
                }

                int index = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnButtonClicked(index)); //??

                buttons.Add(btnObj);
            }
            SetPlayLevel(0); // indicate first level of the game
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error creating buttons: " + ex);

        }
    }

    // This method is called when a button is clicked. It updates the play level and informs the GridManager.
    public void OnButtonClicked(int levelIndex)
    {

        SetPlayLevel(levelIndex);

        if (gridManager != null)
        {
            gridManager.gameLevel = levelIndex + 1;
            gridManager.NextGameLevel(levelIndex + 1);
        }
    }

    // This method updates the button states based on the current level index.
    public void SetPlayLevel(int levelIndex)
    {
        for (int i = 0 ; i < 10; i++)
        {
            Button btn = buttons[i].GetComponent<Button>();
            Image img = buttons[i].GetComponent<Image>();
            TextMeshProUGUI txtButton = buttons[i].GetComponentInChildren<TextMeshProUGUI>();

            btn.interactable = true; // all buttons clickable

            if (levelIndex == i) {
                img.color = playingColor;
                txtButton.text = " Play!! ";
            }
            if (levelIndex > i) {
                img.color = doneColor;
                txtButton.text = "Done Level " + (i + 1) + " ";
            }
            if (levelIndex < i){
                img.color = lockedColor;
                txtButton.text = "Game Level " + (i + 1) + " ";
            }
        }
    }


}

