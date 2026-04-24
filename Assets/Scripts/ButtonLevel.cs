//using UnityEngine;
//using UnityEngine.UI;

//public class ButtonLevel : MonoBehaviour
//{
//    public int levelIndex;
//    private UIManager uiManager;

//    public void Setup(UIManager manager, int index)
//    {
//        uiManager = manager;
//        levelIndex = index;

//        GetComponent<Button>().onClick.AddListener(OnClick);
//    }

//    public void OnClick()
//    {
//        uiManager.OnButtonClicked(levelIndex);
//    }
//}
using UnityEngine;
using UnityEngine.UI;

public class ButtonLevel : MonoBehaviour
{
    public int levelIndex;
    private UIManager uiManager;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(UIManager manager, int index)
    {
        Debug.Log("Setup called for level " + index + " on " + gameObject.name);
        uiManager = manager;
        levelIndex = index;

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        Debug.Log("ButtonLevel clicked: " + levelIndex + " on " + gameObject.name );
        if (uiManager != null)
        {
            uiManager.OnButtonClicked(levelIndex);
        }
        else
        {
            Debug.LogError("uiManager is null on " + gameObject.name);
        }
    }
}