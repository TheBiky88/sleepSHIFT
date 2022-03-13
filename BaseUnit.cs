using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using ObjectPooling;
using Units.Interfaces;
using Units.Settings;

namespace Units
{
    public class BaseUnit : MonoBehaviour, IUnitPoolable, IDamagable
    {
        [SerializeField] protected UnitSettings settings;
        [SerializeField] protected int health = 10;
        [SerializeField] protected NodeManager[] path;
        [SerializeField] protected int targetIndex;
        [SerializeField] protected NodeManager targetNode;
        [SerializeField] protected bool isDead;
        [SerializeField] protected bool stunned = false;
        protected Coroutine followPath;
        public ReturnToPoolEvent onDeconstruct { get; } = new ReturnToPoolEvent() ;

        NodeManager currentWayPoint;

        public void SetTargetNode(NodeManager targetNode)
        {
            this.targetNode = targetNode;
        }

        public virtual void Initialize(Vector3 pos, Quaternion rotation)
        {
            transform.SetPositionAndRotation(pos, rotation);
            isDead = false;
            stunned = false;
            health = settings.m_baseHealth * WaveSpawner.Instance.m_Wave;
            gameObject.SetActive(true);            
            RequestPath();
        }

        public virtual void Deconstruct()
        {
            isDead = true;
            targetNode = null;
            SoundManager.Instance.playSound(SoundType.death);
            WaveSpawner.Instance.RemoveSpawnedUnit(gameObject);
            if (followPath != null)
            {
                StopCoroutine(followPath);
            }
            path = new NodeManager[0];
            onDeconstruct.Invoke(gameObject);
            gameObject.SetActive(false);
        }

        public virtual void TakeDamage(int amount)
        {
            health -= amount;
            if (health <= 0 && !isDead)
            {
                GameManager.Instance.ModifyPoints(settings.m_scoreOnDeath);
                Deconstruct();
            }
        }

        public void RequestPath()
        {
            if (followPath != null)
            {
                StopCoroutine(followPath);
            }
            NodeManager startNode = GetClosestNode();
            if (path.Length > 0)
            {
                startNode = GetStartNodeFromPath();
            }
            PathRequestManager.Instance.RequestPath(startNode, targetNode, OnPathFound);           
        }



        public virtual void OnPathFound(NodeManager[] newPath, bool pathSuccessful)
        {
            if (pathSuccessful)
            {
                path = newPath;                
                followPath = StartCoroutine("FollowPath");
            }
            else
            {
                RequestPath();
            }
        }

        protected virtual IEnumerator FollowPath()
        {
            targetIndex = 0;
            currentWayPoint = path[targetIndex];

            while (true)
            {
                Vector2 unitPosXZ = new Vector2(transform.position.x, transform.position.z);
                Vector2 wayPointPosXZ = new Vector2(currentWayPoint.transform.position.x, currentWayPoint.transform.position.z);

                if (Vector2.Distance(unitPosXZ, wayPointPosXZ) < 0.01f)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        break;
                    }

                    currentWayPoint = path[targetIndex];
                }
                Vector3 wayPointPos = currentWayPoint.transform.position;
                Vector3 targetPosNoY = new Vector3(wayPointPos.x, transform.position.y, wayPointPos.z);

                if (!stunned && !isDead && !GameManager.Instance.paused)
                {
                    // Rotate towards checkpoint
                    Vector3 targetDir = targetPosNoY - transform.position;
                    float step = settings.m_rotationSpeed * Time.deltaTime;
                    Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
                    transform.rotation = Quaternion.LookRotation(newDir);

                    // Move towards checkpoint
                    transform.position = Vector3.MoveTowards(transform.position, targetPosNoY, settings.m_movementSpeed * Time.deltaTime);
                }

                yield return null;
            }

            GameManager.Instance.ModifyHealth(-settings.m_damageToPlayer);
            Deconstruct();
        }

       public NodeManager GetStartNodeFromPath()
       {
            if (!currentWayPoint.isPassable)
            {
                if (targetIndex > 0)
                {
                    return path[targetIndex - 1];
                }
                else
                {
                    return GetClosestNode();
                }
            }
            else
            {
                return currentWayPoint;
            }
       }

        protected NodeManager GetClosestNode()
        {
            NodeManager[] nodes = FindObjectsOfType<NodeManager>();

            if (nodes.Length <= 0)
            {
                Debug.LogError("No nodes found!");
            }         
            
            NodeManager closestNode = null;
            float distanceToClosestNode = float.MaxValue;

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].isPassable)
                {
                    float distanceToNode = Vector3.Distance(transform.position, nodes[i].transform.position);
                    if (distanceToNode < distanceToClosestNode)
                    {
                        closestNode = nodes[i];
                        distanceToClosestNode = distanceToNode;
                    }
                }
            }
            return closestNode;
        }


    }
}