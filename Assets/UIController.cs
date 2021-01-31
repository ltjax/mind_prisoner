using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private const int MINIMAP_SIZE = 5;
    Text hint;
    List<Image> minimapPanel = new List<Image>();
    RoomManager roomManager;

    // Start is called before the first frame update
    void Start()
    {
        roomManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<RoomManager>();
        hint = GetComponentInChildren<Text>();
        for (int y = 0; y < MINIMAP_SIZE; ++y)
        {
            for (int x = 0; x < MINIMAP_SIZE; ++x)
            {
                var name = string.Format("UI/Minimap/Room{0}{1}", x, y);
                var image = GameObject.Find(name).GetComponent<Image>();
                Debug.Assert(image != null);
                image.enabled = false;
                minimapPanel.Add(image);
            }
        }
        Debug.Assert(hint != null);
        UpdateMinimap(Vector2Int.zero);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ShowText(string text)
    {
        StartCoroutine(TextAnimation(text));

    }

    IEnumerator TextAnimation(string text)
    {
        hint.color = Color.white;
        hint.text = "";
        for (int i = 0; i < text.Length; ++i)
        {
            hint.text += text[i];
            yield return new WaitForSecondsRealtime(0.01f);
        }
        yield return new WaitForSecondsRealtime(4.0f);
        for (float alpha = 1.0f; alpha > 0.0f; alpha -= 0.03f)
        {
            var color = hint.color;
            color.a = alpha;
            hint.color = color;
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }

    public void LetGo()
    {
        ShowText("As I let go of my past, I feel like new paths are opening up...");
    }

    public void UpdateMinimap(Vector2Int center)
    {
        var offset = MINIMAP_SIZE / 2;
        for (int y = 0; y < MINIMAP_SIZE; ++y)
        {
            for (int x = 0; x < MINIMAP_SIZE; ++x)
            {
                var index = y * MINIMAP_SIZE + x;
                var cell = center + new Vector2Int(x - offset, y - offset);
                var state = roomManager.CheckRoom(cell);
                var panel = minimapPanel[index];
                switch (state)
                {
                    case RoomManager.RoomState.NonExistant:
                        panel.enabled = false;
                        break;
                    case RoomManager.RoomState.Visited:
                        panel.enabled = true;
                        panel.color = new Color(1.0f, 1.0f, 1.0f, 0.75f);
                        break;
                    case RoomManager.RoomState.Unvisited:
                        panel.enabled = true;
                        panel.color = new Color(1.0f, 1.0f, 0.8f, 0.2f);
                        break;
                }
            }
        }
    }
}
