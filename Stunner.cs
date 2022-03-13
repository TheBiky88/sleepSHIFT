using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units.Interfaces;

namespace Turrets
{
    public class Stunner : BaseTurret
    {
        protected override void Fire(GameObject target)
        {
            foreach (GameObject Enemy in enemiesInRange)
            {
                Enemy.GetComponent<IStunnable>().GetStunned(m_settings.m_stunDuration);
            }

            GameObject sound = SoundManager.Instance.turretSoundManager.stunnerSoundPool.GetObject();
            if (sound != null)
            {
                sound.GetComponent<ObjectPooling.IPoolable>().Initialize(Vector3.zero, Quaternion.identity);
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<IStunnable>() != null)
            {
                enemiesInRange.Add(other.gameObject);
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<IStunnable>() != null)
            {
                enemiesInRange.Remove(other.gameObject);
            }
        }
    }
}