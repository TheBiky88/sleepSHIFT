using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Turrets.Settings;
using Turrets;
using Turrets.Interfaces;
using ObjectPooling;

public class Builder : MonoBehaviour
{
    public GameObject tower = null;
    Vector3 pos = Vector3.zero;
    [SerializeField] private bool busyBuilding = false;    
    [SerializeField] private int enemiesToCheck;
    [SerializeField] private bool enemiesCanReachEnd;

    private GameTile workingTile;
    public void BuildOrUpgradeTower(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CameraManager.Instance.gt != null && !GameManager.Instance.gameOver && !GameManager.Instance.paused)
            {

                // Building
                if (!CameraManager.Instance.gt.occupied && !busyBuilding && enemiesToCheck <= 0)
                {
                    BuildTower();
                }


                // Upgrading
                else
                {
                    UpgradeTower(CameraManager.Instance.gt);
                }
            }
        }
    }

    public void DestroyTower(InputAction.CallbackContext context)
    {
        if (CameraManager.Instance.gt != null && CameraManager.Instance.gt.occupied && !GameManager.Instance.paused && !GameManager.Instance.gameOver)
        {
            SoundManager.Instance.playSound(SoundType.delete);
            // put it back in its pool
            GameTile gt = CameraManager.Instance.gt;
            BaseTurret turret = gt.GetComponent<GameTile>().Tower.GetComponent<BaseTurret>();

            int sellPrice = turret.m_settings.m_turretCost;

            if (!turret.upgraded)
            {
                sellPrice /= 2;
            }

            GameManager.Instance.ModifyPoints(sellPrice);
            turret.Deconstruct();
            gt.occupied = false;
            gt.buildable = true;
            gt.upgradeable = true;
            gt.Tower = null;
            gt.GetComponentInChildren<NodeManager>().isPassable = true;

            foreach (GameObject unit in WaveSpawner.Instance.spawnedUnits)
            {
                unit.GetComponent<Units.BaseUnit>().RequestPath();
            }
        }
    }

    public void CheckTile(InputAction.CallbackContext context)
    {
        Debug.Log(CameraManager.Instance.gt);
    }

    private void UpgradeTower(GameTile gt)
    {
        // check which tower is there
        if (gt.occupied)
        {
            BaseTurret turret = gt.Tower.GetComponent<BaseTurret>();
            TowerSettings settings = turret.m_settings;
            IUpgradeable upgrade = turret.GetComponent<IUpgradeable>();

            if (!turret.upgraded)
            {
                if (GameManager.Instance.points >= settings.m_turretCost * 2)
                {
                    GameManager.Instance.ModifyPoints(-settings.m_turretCost * 2);
                    SoundManager.Instance.playSound(SoundType.place);

                    gt.upgradeable = false;

                    upgrade.IncreaseDamage(settings.m_baseDamage);
                    upgrade.IncreaseRange(settings.m_baseRange);
                    upgrade.ReduceShotcooldown(settings.m_baseShotCooldown / 3);

                    turret.upgraded = true;
                }
            }
        }
    }
    
    private void BuildTower()
    {
        if (GameManager.Instance.SelectedTower != null && CameraManager.Instance.gt != null)
        {
            TowerSettings towerSettings = GameManager.Instance.SelectedTower.GetComponent<BaseTurret>().m_settings;
            if (CheckCost(towerSettings.m_turretCost))
            {
                // check if tower can be built, if so, it builds it
                busyBuilding = true;
                workingTile = CameraManager.Instance.gt;
                enemiesCanReachEnd = true;
                enemiesToCheck = 0;
                BuildChecker();
            }
        }
    }

    private void PathPossible(NodeManager[] path, bool sucess)
    {        
        NodeManager nm = workingTile.GetComponentInChildren<NodeManager>();

        if (sucess)
        {
            // build tower
            SoundManager.Instance.playSound(SoundType.place);

            pos = workingTile.GetComponentInChildren<NodeManager>().transform.position;

            tower = GameManager.Instance.SelectedTower;
            tower.GetComponent<IPoolable>().Initialize(pos, Quaternion.identity);

            workingTile.Tower = tower;

            TowerSettings towerSettings = GameManager.Instance.SelectedTower.GetComponent<BaseTurret>().m_settings;

            GameManager.Instance.ModifyPoints(-towerSettings.m_turretCost);

            foreach (GameObject unit in WaveSpawner.Instance.spawnedUnits)
            {
                unit.GetComponent<Units.BaseUnit>().RequestPath();
            }

            //grab new tower with towertype
            TowerTypeEnum towertype = towerSettings.m_towerType;

            switch (towertype)
            {
                case TowerTypeEnum.SEEKING:
                    GameManager.Instance.SelectTower(GameManager.Instance.seekingTurretPool);
                    break;
                case TowerTypeEnum.BOMBER:
                    GameManager.Instance.SelectTower(GameManager.Instance.bombTurretPool);
                    break;
                case TowerTypeEnum.STUNNER:
                    GameManager.Instance.SelectTower(GameManager.Instance.stunTurretPool);
                    break;
            }


            workingTile.occupied = true;
            workingTile.buildable = false;
            nm.isPassable = false;
        }

        else
        {
            workingTile.occupied = false;
            workingTile.buildable = true;
            workingTile.Tower = null;
            nm.isPassable = true;           
        }

        busyBuilding = false;
        workingTile = null;
    }

    private void BuildChecker()
    {
        NodeManager nm = workingTile.GetComponentInChildren<NodeManager>();

        enemiesToCheck = WaveSpawner.Instance.spawnedUnits.Count;
        if (enemiesToCheck > 0)
        {
            foreach (GameObject unit in WaveSpawner.Instance.spawnedUnits)
            {
                PathRequestManager.Instance.RequestPath(unit.GetComponent<Units.BaseUnit>().GetStartNodeFromPath(), GameManager.Instance.endTile.GetComponent<NodeManager>(), EnemyPathComplete);
            }
        }
        else
        {
            PathRequestManager.Instance.RequestPath(GameManager.Instance.startTile.GetComponent<NodeManager>(), GameManager.Instance.endTile.GetComponent<NodeManager>(), PathPossible);
        }
    }

    private void EnemyPathComplete(NodeManager[] path, bool sucess)
    {
        enemiesToCheck--;

        if (!sucess)
        {
            enemiesCanReachEnd = false;
        }


        if (enemiesToCheck <= 0 && !enemiesCanReachEnd)
        {
            NodeManager nm = workingTile.GetComponentInChildren<NodeManager>();

            workingTile.occupied = false;
            workingTile.buildable = true;
            nm.isPassable = true;
            busyBuilding = false;
            workingTile = null;
        }
        else if (enemiesToCheck <= 0 && enemiesCanReachEnd)
        {
            PathRequestManager.Instance.RequestPath(GameManager.Instance.startTile.GetComponent<NodeManager>(), GameManager.Instance.endTile.GetComponent<NodeManager>(), PathPossible);
        }
    }

    

    private bool CheckCost(int cost)
    {
        if (GameManager.Instance.points >= cost)
        {
            return true;
        }
        else return false;
    }
}
