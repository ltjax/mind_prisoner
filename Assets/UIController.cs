using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    Text hint;

    // Start is called before the first frame update
    void Start()
    {
        hint = GetComponentInChildren<Text>();
        Debug.Assert(hint != null);
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
}
