using System;
using System.Collections;
using Division.Player;
using UnityEngine;
using Division.Enemy;

namespace Division.Core
{
    public class Hitbox : MonoBehaviour
    {
        public bool isPlayer;
        
        private string myType;
        private BaseHealth health;
        
        public Canvas canvasExclamationMark;

        
        // 쿨타임 계산을 위한 변수 추가
        private float nextDamageTime = 0f;
        private float damageCooldown = 1.0f; // 1초 쿨타임

        private void Start()
        {
            myType = transform.tag;
            health = transform.parent.GetComponent<BaseHealth>();
        }

        // Enter 대신 Stay를 사용합니다.
        // Stay는 충돌 범위 안에 있는 동안 계속 호출됩니다.
        void OnTriggerStay2D(Collider2D other)
        {
            if (isPlayer)
            {
                if (other.CompareTag("Enemy"))
                {
                    // 현재 시간이 '다음 데미지를 입을 수 있는 시간'보다 큰지 확인
                    if (!canvasExclamationMark.enabled) //if (Time.time >= nextDamageTime)
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
                else
                {
                    Debug.Log("player에게 뭐가 닿았는데 별거아닌거같다");
                    return;
                }
            }
            else
            {
                Debug.Log("좀비에게 뭐가 닿앆다");
                if (other.CompareTag("Skill"))
                {
                    Skill skill = other.transform.GetComponent<Skill>();
                    int damage = skill.GetDamage();
                    Debug.Log("zombie damaged!"+damage);
                    skill.DestroyThis();
                    StartCoroutine(DamageCooltime());

                    health.transform.GetComponent<ZombieAI>().hasDetectedPlayer = true;
                    
                    if (health != null)
                    {
                        health.TakeDamage(damage);

                        // 데미지를 입었으므로, 다음 데미지 가능 시간을 현재시간 + 1초로 설정
                        nextDamageTime = Time.time + damageCooldown;
                    }
                }
            }
            
        }

        IEnumerator DamageCooltime()
        {
            if(canvasExclamationMark!=null) canvasExclamationMark.enabled = true;
            yield return new WaitForSeconds(damageCooldown);
            if(canvasExclamationMark!=null) canvasExclamationMark.enabled = false;
        }
    }
      
}