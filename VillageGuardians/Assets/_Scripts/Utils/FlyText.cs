using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlyText : MonoBehaviour
{

    public Color color;
    public string content;
    Text t;

    void Start()
    {
        t = transform.GetComponent<Text>();
        t.text = content;
        t.color = color;
        t.rectTransform.anchoredPosition = new Vector2(t.rectTransform.anchoredPosition.x, t.rectTransform.anchoredPosition.y + 30);
    }

    float fadeTimer = 1;
    void Update()
    {
        fadeTimer -= Time.deltaTime;
        t.color = new Color(color.r, color.g, color.b, fadeTimer);
        t.rectTransform.anchoredPosition = new Vector2(t.rectTransform.anchoredPosition.x, t.rectTransform.anchoredPosition.y + (1 - fadeTimer));
        if (fadeTimer < 0)
        {
            GameObject.Destroy(this.gameObject);
        }
    }

}
