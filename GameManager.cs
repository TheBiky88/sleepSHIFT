using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using ObjectPooling;
using Turrets.Settings;
using UI;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }
    
    public int points { private set; get; } = 0;

    private float waveTimer = 10;
    private int spawnedUnits = 5;

    public UnityEvent OnDeath = new UnityEvent();

    [Header("Starting Values")]
    [SerializeField] private int life = 25;
    [SerializeField] private int startingPoints;
    
    [Header("Tower Builder")]
    public GameObject SelectedTower;
    public List<GameObject> Towers;   

    [Header("Tower Object Pool")]
    [SerializeField] private ObjectPoolSettings seekingTurretPoolSettings;
    public ObjectPool seekingTurretPool;
    [SerializeField] private ObjectPoolSettings bombTurretPoolSettings;
    public ObjectPool bombTurretPool;
    [SerializeField] private ObjectPoolSettings stunTurretPoolSettings;
    public ObjectPool stunTurretPool;

    [Header("Projectiles Object Pool")]
    [SerializeField] private ObjectPoolSettings seekingBulletPoolSettings;
    private ObjectPool seekingBulletPool;
    [SerializeField] private ObjectPoolSettings bombPoolSettings;
    private ObjectPool bombPool;

    [Header("Unit Object Pool")]
    [SerializeField] private ObjectPoolSettings spiderPoolSettings;
    private ObjectPool spiderPool;
    [SerializeField] private ObjectPoolSettings slimeMonsterPoolSettings;
    private ObjectPool slimeMonsterPool;
    [SerializeField] private ObjectPoolSettings flyingMonkeyPoolSettings;
    private ObjectPool flyingMonkeyPool;
    [SerializeField] private ObjectPoolSettings dragonPoolSettings;
    private ObjectPool dragonPool;

    [Header("Start & end tile")]
    public Transform endTile;
    public Transform startTile;
    [Header("Paused runtime variable")]
    public bool paused;

    public bool gameOver;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
        points = startingPoints;
    }

    private void Start()
    {
        UIManager.Instance.UpdateLives(life);
        UIManager.Instance.UpdatePoints(points);

        bombPool = new ObjectPool(bombPoolSettings);
        seekingBulletPool = new ObjectPool(seekingBulletPoolSettings);

        spiderPool = new ObjectPool(spiderPoolSettings);
        slimeMonsterPool = new ObjectPool(slimeMonsterPoolSettings);
        flyingMonkeyPool = new ObjectPool(flyingMonkeyPoolSettings);
        dragonPool = new ObjectPool(dragonPoolSettings);

        seekingTurretPool = new ObjectPool(seekingTurretPoolSettings);
        bombTurretPool = new ObjectPool(bombTurretPoolSettings);
        stunTurretPool = new ObjectPool(stunTurretPoolSettings);

    }

    public GameObject GetBullet(TowerTypeEnum towerType)
    {
        switch (towerType)
        {
            case TowerTypeEnum.BOMBER:
                return bombPool.GetObject();
            case TowerTypeEnum.SEEKING:
                return seekingBulletPool.GetObject();
            default:
                Debug.LogError("No projectile found for " + towerType.ToString() + " tower type!");
                return null;
        }
    }

    public GameObject GetEnemy(Units.Settings.UnitTypeEnum enemyType)
    {
        switch (enemyType)
        {
            case Units.Settings.UnitTypeEnum.SPIDER:
                return spiderPool.GetObject();
            case Units.Settings.UnitTypeEnum.SLIME_MONSTER:
                return slimeMonsterPool.GetObject();
            case Units.Settings.UnitTypeEnum.DRAGON:
                return dragonPool.GetObject();
            case Units.Settings.UnitTypeEnum.FLYING_MONKEY:
                return flyingMonkeyPool.GetObject();
            default:
                Debug.LogError("Enemy type not implemented type of: " + enemyType.ToString());
                return null;
        }
    }
        
    public void SelectTower(ObjectPool pool)
    {
        SelectedTower = pool.GetObject();
        UIManager.Instance.OnSelectedTowerChange(SelectedTower.GetComponent<Turrets.BaseTurret>().m_settings);
    }

    public void BasicTower(InputAction.CallbackContext context)
    {
        if (context.performed && !gameOver && !paused)
        {
            if (SelectedTower != null)
            {
                SelectedTower.GetComponent<IPoolable>().onDeconstruct.Invoke(SelectedTower);
            }

            SelectTower(seekingTurretPool);
        }
    }

    public void BombTower(InputAction.CallbackContext context)
    {
        if (context.performed && !gameOver && !paused)
        {
            if (SelectedTower != null)
            {
                SelectedTower.GetComponent<IPoolable>().onDeconstruct.Invoke(SelectedTower);
            }

            SelectTower(bombTurretPool);
        }
    }
    public void StunTower(InputAction.CallbackContext context)
    {
        if (context.performed && !gameOver && !paused)
        {
            if (SelectedTower != null)
            {
                SelectedTower.GetComponent<IPoolable>().onDeconstruct.Invoke(SelectedTower);
            }

            SelectTower(stunTurretPool);
        }
    }

    public void Escape(InputAction.CallbackContext context)
    {
        if (context.started && !gameOver)
        {
            paused = !paused;
            UIManager.Instance.Pause(paused);
        }
    }

    public void Pause()
    {
        if (!gameOver)
        {
            paused = !paused;
            UIManager.Instance.Pause(paused);
        }
    }

    //manage enemies, waves, enemy count, health
   

    public void ModifyPoints(int amount)
    {
        if (!gameOver && !paused)
        {
            points += amount;
            UIManager.Instance.UpdatePoints(points);
        }
    }

    public void ModifyHealth(int amount)
    {
        if (!gameOver && !paused)
        {
            life += amount;
            UIManager.Instance.UpdateLives(life);
            if (life <= 0)
            {
                SetGameOver();  
                StopAllCoroutines();
                OnDeath.Invoke();
            }
        }
    }

    public void SetGameOver()
    {
        gameOver = true;
    }
}