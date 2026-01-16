using System;
using System.Collections;
using Division.Player;
using UnityEngine;
using Division.Enemy;

namespace Division.Core
{
    public class Hitbox : MonoBehaviour
    {
        private string myType;
        private PlayerHealth health;
        public Canvas canvasExclamationMark;

        
        // 쿨타임 계산을 위한 변수 추가
        private float nextDamageTime = 0f;
        private float damageCooldown = 1.0f; // 1초 쿨타임

        private void Start()
        {
            myType = transform.tag;
            health = transform.parent.GetComponent<PlayerHealth>();
        }

        // Enter 대신 Stay를 사용합니다.
        // Stay는 충돌 범위 안에 있는 동안 계속 호출됩니다.
        void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                // 현재 시간이 '다음 데미지를 입을 수 있는 시간'보다 큰지 확인
                if(!canvasExclamationMark.enabled)//if (Time.time >= nextDamageTime)
                {
                    int damage = other.transform.parent.GetComponent<ZombieAttack>().GetDamage();
                    StartCoroutine(DamageCooltime());
                    
                    if (health != null)
                    {
                        health.TakeDamage(damage);
                        
                        // 데미지를 입었으므로, 다음 데미지 가능 시간을 현재시간 + 1초로 설정
                        nextDamageTime = Time.time + damageCooldown;
                        
    //                    Debug.Log($"Damaged! Next damage available at: {nextDamageTime}");
                    }
                }
                
            }
        }

        IEnumerator DamageCooltime()
        {
            canvasExclamationMark.enabled = true;
            yield return new WaitForSeconds(damageCooldown);
            canvasExclamationMark.enabled = false;
        }
    }
      
}