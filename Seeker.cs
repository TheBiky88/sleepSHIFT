using UnityEngine;
using Turrets.Projectiles.Interfaces;

namespace Turrets
{
    public class Seeker : BaseTurret
    {
        [SerializeField] private GameObject turretHead;       

       
        protected override void Update()
        {
            base.Update();
            if (enemiesInRange.Count > 0)
            {
                turretHead.transform.LookAt(enemiesInRange[0].transform.position);
            }
        }

        protected override void Fire(GameObject target)
        {
            GameObject projectile = GameManager.Instance.GetBullet(m_settings.m_towerType);
            IProjectilePoolable poolable = projectile.GetComponent<IProjectilePoolable>();
            poolable.SetTargetAndDamage(target, m_settings.m_baseDamage + bonusDamage);
            poolable.Initialize(transform.position + Vector3.up, Quaternion.identity);

            GameObject sound = SoundManager.Instance.turretSoundManager.seekingSoundPool.GetObject();
            if (sound != null)
            {
                sound.GetComponent<ObjectPooling.IPoolable>().Initialize(Vector3.zero, Quaternion.identity);
            }
        }
    }
}
