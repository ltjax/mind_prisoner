using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public RectTransform HeartSprite;
    private float HeartSize = 50f;

    public int CurrentHealth;

    public void SetHealth(int health)
    {
        CurrentHealth = health;
        RemoveHearts();
        for (int i = 0; i < health; i++) {
            var newHeart = Instantiate(HeartSprite, transform);
            newHeart.localPosition = new Vector3(newHeart.sizeDelta.x * i, 0, 0);
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
