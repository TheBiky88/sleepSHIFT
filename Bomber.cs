using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ObjectPooling;
using Turrets.Projectiles.Interfaces;

namespace Turrets
{
    public class Bomber : BaseTurret
    {
        protected override void Fire(GameObject target)
        {
            GameObject projectile = GameManager.Instance.GetBullet(m_settings.m_towerType);
            IProjectilePoolable poolable = projectile.GetComponent<IProjectilePoolable>();
            poolable.SetTargetAndDamage(target, m_settings.m_baseDamage + bonusDamage);
            poolable.Initialize(transform.position + (Vector3.up * 2), Quaternion.identity);

            GameObject sound = SoundManager.Instance.turretSoundManager.bomberSoundPool.GetObject();
            if (sound != null)
            {
                sound.GetComponent<ObjectPooling.IPoolable>().Initialize(Vector3.zero, Quaternion.identity);
            }
        }
    }
}