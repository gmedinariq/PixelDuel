using UnityEngine;
using UnityEngine.InputSystem;

public class PowerUp : MonoBehaviour
{
    public enum Type { InstantWin, Shield, SlowEnemy }
    public Type type;

    [Header("Sprites")]
    public Sprite spriteInstantWin;
    public Sprite spriteShield;
    public Sprite spriteSlowEnemy;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (sr == null) return;

        switch (type)
        {
            case Type.InstantWin:
                sr.sprite = spriteInstantWin;
                break;
            case Type.Shield:
                sr.sprite = spriteShield;
                break;
            case Type.SlowEnemy:
                sr.sprite = spriteSlowEnemy;
                break;
        }
    }

    void Update()
    {
        bool press =
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);

        if (!press) return;

        Vector2 inputPos = Vector2.zero;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            inputPos = Mouse.current.position.ReadValue();
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            inputPos = Touchscreen.current.primaryTouch.position.ReadValue();

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(inputPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            Activate();
        }
    }

    void Activate()
    {
        var gm = GameManager.Instance;
        var ui = UIManager.Instance;

        if (gm == null || ui == null) return;

        switch (type)
        {
            case Type.InstantWin:
                gm.ForceWinRound();
                break;
            case Type.Shield:
                gm.ActivateShield();
                break;
            case Type.SlowEnemy:
                gm.ApplySlowToEnemy();
                break;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.player == null) return;
        if (other.gameObject == gm.player)
        {
            Activate();
        }
    }
}
