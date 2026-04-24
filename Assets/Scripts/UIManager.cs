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


    private void CreateButtons()
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
            
            if (btn == null) {
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

    // for leaning porposes for colors
    public void OnButtonClicked(int levelIndex)
    {

        SetPlayLevel(levelIndex);

        if (gridManager != null)
        {
            gridManager.gameLevel = levelIndex + 1;
            gridManager.NextGameLevel(levelIndex + 1);
        }
        //for (int i = 0; i < buttons.Count; i++) {
        //    Button btn = buttons[i].GetComponent<Button>();
        //    Image img = buttons[i].GetComponent<Image>();
        //    TextMeshProUGUI txtButton = buttons[i].GetComponentInChildren<TextMeshProUGUI>();

        //    btn.interactable = true; // all buttons clickable

        //    if (i < levelIndex)
        //    {
        //        img.color = doneColor;
        //        txtButton.text = "Done Level " + (i + 1);
        //    }
        //    else if (i == levelIndex)
        //    {
        //        img.color = playingColor;
        //        txtButton.text = "Playing!!";
        //        gridManager.NextGameLevel(levelIndex + 1);
        //    }
        //    else
        //    {
        //        img.color = lockedColor;
        //        txtButton.text = "Game Level " + (i + 1);
        //    }
        //}
    }
   
    public void SetPlayLevel(int levelIndex)
    {
        //Image imgPlaying = buttons[levelIndex].GetComponent<Image>();
        //TextMeshProUGUI txtButtonPlaying = buttons[levelIndex].GetComponentInChildren<TextMeshProUGUI>();
        //imgPlaying.color = playingColor;
        //txtButtonPlaying.text = "Playing!!";

        //if (levelIndex >= 1)
        //{
        //    Image imgDone = buttons[levelIndex - 1].GetComponent<Image>();
        //    TextMeshProUGUI txtButton = buttons[levelIndex - 1].GetComponentInChildren<TextMeshProUGUI>();
        //    imgDone.color = doneColor;
        //    txtButton.text = "Done Level " + (levelIndex);
        //}
            //levelIndex--;

            for (int i = 0 ; i < 10; i++)
            {
                Button btn = buttons[i].GetComponent<Button>();
                Image img = buttons[i].GetComponent<Image>();
                TextMeshProUGUI txtButton = buttons[i].GetComponentInChildren<TextMeshProUGUI>();

                btn.interactable = true; // all buttons clickable
                if (levelIndex == i)
                    {
                        img.color = playingColor;
                        txtButton.text = "Playing!!";
                    }
                    if (levelIndex > i)
                    {
                        img.color = doneColor;
                        txtButton.text = "Done Level " + (i + 1);
                    }
                    if (levelIndex < i)
                    {
                        img.color = lockedColor;
                        txtButton.text = "Game Level " + (i + 1);
                    }
            }
        

    }


}

