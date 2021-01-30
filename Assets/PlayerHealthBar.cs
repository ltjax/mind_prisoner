using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public Sprite HeartSprite;
    private float HeartSize = 50f;

    public void SetHealth(int health)
    {
        RemoveHearts();

        for (int i = 0; i < health; i++)
        {
            GameObject heartContainer = new GameObject("healthIndicator");
            var rectTransform = heartContainer.AddComponent<RectTransform>();
            rectTransform.transform.SetParent(transform);
            rectTransform.sizeDelta = new Vector2(HeartSize, HeartSize);
            rectTransform.localPosition = new Vector3(i * HeartSize, 0, 0);

            Image image = heartContainer.AddComponent<Image>();
            image.sprite = HeartSprite;
            image.preserveAspect = true;
            heartContainer.transform.SetParent(transform);
        }
    }

    private void RemoveHearts()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
