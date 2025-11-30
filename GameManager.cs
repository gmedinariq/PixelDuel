using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Config Duelos")]
    public int maxRounds = 10;
    public int maxFails = 3;
    public float startEnemyReactionTime = 1f;

    [Header("Estado")]
    public int score = 0;
    private int currentRound = 0;
    private int fails = 0;
    private float enemyReactionTime;
    private bool waitingForShoot = false;
    private bool inputLocked = false;
    private float shootSignalTime;
    private float gameTimer = 0f;
    public float timeLimit = 120f; // 2 min

    [Header("Personajes")]
    public GameObject player;
    public GameObject enemy;

    [Header("PowerUps")]
    public GameObject powerUpPrefab;
    private GameObject currentPowerUp;
    public bool shieldActive = false;

    private UIManager ui;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        ui = FindFirstObjectByType<UIManager>();
        if (ui == null) Debug.LogError("❗No hay UIManager en la escena.");
    }

    void Start()
    {
        enemyReactionTime = startEnemyReactionTime;
        score = 0;
        currentRound = 0;
        fails = 0;
        waitingForShoot = false;
        inputLocked = false;
        shieldActive = false;
        gameTimer = 0f;

        if (ui != null && ui.panelEnd != null)
            ui.panelEnd.SetActive(false);

        StartNextRound();
    }

    void Update()
    {
        if (ui == null) return;
        if (inputLocked) return;

        // Timer global
        gameTimer += Time.deltaTime;
        ui.UpdateTimer(timeLimit - gameTimer);
        if (gameTimer >= timeLimit)
        {
            EndGame(true);
            return;
        }

        bool press =
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);

        if (!press) return;

        // Si el jugador dispara ANTES de la señal
        if (!waitingForShoot)
        {
            inputLocked = true;
            fails++;

            ui.ShowMessage("¡DISPARASTE ANTES!");
            HideBoth();

            if (fails >= maxFails)
            {
                EndGame(false);
                return;
            }

            enemyReactionTime = Mathf.Max(0.2f, enemyReactionTime - 0.1f);
            Invoke(nameof(StartNextRound), 1f);
            return;
        }

        // Si ya salió "DISPARA" → resolver duelo
        ResolveDuel();
    }

    // ================= RONDAS =================

    public void StartNextRound()
    {
        inputLocked = false;
        waitingForShoot = false;
        shieldActive = false;

        currentRound++;
        if (currentRound > maxRounds)
        {
            EndGame(true);
            return;
        }

        if (player) player.SetActive(true);
        if (enemy) enemy.SetActive(true);

        ui.UpdateRounds(currentRound, maxRounds);
        ui.UpdateScore(score);
        ui.ShowMessage("PREPÁRATE...");

        SpawnPowerUp();

        float delay = Random.Range(1f, 3f);
        Invoke(nameof(ShowShootSignal), delay);
    }

    void ShowShootSignal()
    {
        waitingForShoot = true;
        shootSignalTime = Time.time;
        ui.ShowMessage("DISPARA");

        // Si el jugador no dispara a tiempo, enemigo gana ronda
        Invoke(nameof(EnemyWinsRound), enemyReactionTime);
    }

    // ================= DUELO =================

    void ResolveDuel()
    {
        waitingForShoot = false;
        inputLocked = true;
        CancelInvoke(nameof(EnemyWinsRound));

        float reaction = Time.time - shootSignalTime;
        bool playerWins = reaction < enemyReactionTime;

        if (currentPowerUp) Destroy(currentPowerUp);

        if (playerWins)
        {
            score += 10;
            ui.ShowMessage("¡GANASTE LA RONDA!");
            if (enemy) enemy.SetActive(false);
        }
        else
        {
            if (shieldActive)
            {
                shieldActive = false;
                ui.ShowMessage("🛡️ ESCUDO TE SALVÓ");
            }
            else
            {
                fails++;
                ui.ShowMessage("PERDISTE LA RONDA");
                if (player) player.SetActive(false);
            }
        }

        ui.UpdateScore(score);
        ui.UpdateRounds(currentRound, maxRounds);

        if (fails >= maxFails)
        {
            EndGame(false);
            return;
        }

        if (currentRound >= maxRounds)
        {
            EndGame(true);
            return;
        }

        enemyReactionTime = Mathf.Max(0.2f, enemyReactionTime - 0.1f);
        Invoke(nameof(StartNextRound), 1f);
    }

    public void EnemyWinsRound()
    {
        if (!waitingForShoot || inputLocked) return;

        waitingForShoot = false;
        inputLocked = true;

        if (currentPowerUp) Destroy(currentPowerUp);

        if (shieldActive)
        {
            shieldActive = false;
            ui.ShowMessage("🛡️ ESCUDO TE SALVÓ");
            Invoke(nameof(StartNextRound), 1f);
            return;
        }

        fails++;
        ui.ShowMessage("¡ENEMIGO GANA!");
        if (player) player.SetActive(false);

        if (fails >= maxFails)
        {
            EndGame(false);
        }
        else
        {
            enemyReactionTime = Mathf.Max(0.2f, enemyReactionTime - 0.1f);
            Invoke(nameof(StartNextRound), 1f);
        }
    }

    void HideBoth()
    {
        if (player) player.SetActive(false);
        if (enemy) enemy.SetActive(false);
    }

    // ============== POWER-UPS ==============

    void SpawnPowerUp()
    {
        if (powerUpPrefab == null) return;

        if (currentPowerUp != null)
            Destroy(currentPowerUp);

        Vector3 pos = new Vector3(
            Random.Range(-6f, 6f),
            Random.Range(-2.5f, 2.5f),
            0f
        );

        currentPowerUp = Instantiate(powerUpPrefab, pos, Quaternion.identity);

        PowerUp pu = currentPowerUp.GetComponent<PowerUp>();
        if (pu != null)
        {
            int r = Random.Range(0, 3);
            pu.type = (PowerUp.Type)r;
        }
    }

    // llamados desde PowerUp

    public void ForceWinRound()
    {
        if (inputLocked) return;

        inputLocked = true;
        waitingForShoot = false;

        score += 10;
        ui.ShowMessage("⭐ POWER-UP: ¡GANASTE LA RONDA!");
        if (enemy) enemy.SetActive(false);
        if (currentPowerUp) Destroy(currentPowerUp);

        Invoke(nameof(StartNextRound), 1f);
    }

    public void ActivateShield()
    {
        shieldActive = true;
        ui.ShowMessage("🛡️ ESCUDO ACTIVADO");
    }

    public void ApplySlowToEnemy()
    {
        enemyReactionTime += 0.4f;
        ui.ShowMessage("⏳ ENEMIGO MÁS LENTO");
    }

    // ============== FIN DEL JUEGO ==============

    void EndGame(bool won)
    {
        waitingForShoot = false;
        inputLocked = true;
        CancelInvoke();

        string msg = won
            ? "¡HAS GANADO EL DUELO!\nPuntuación: " + score
            : "HAS PERDIDO EL DUELO\nPuntuación: " + score;

        ui.ShowEnd(msg);
    }

    public void Restart()
    {
        SceneManager.LoadScene("PixelDuel_Scene");
    }
}
