using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitCamera : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr == null) return;

        float spriteWidth = sr.bounds.size.x;
        float spriteHeight = sr.bounds.size.y;

        float worldHeight = Camera.main.orthographicSize * 2f;
        float worldWidth = worldHeight * Screen.width / Screen.height;

        Vector3 scale = transform.localScale;
        scale.x = worldWidth / spriteWidth;
        scale.y = worldHeight / spriteHeight;
        transform.localScale = scale;
    }
}
