using UnityEngine;
using System.Collections;

namespace Division.Enemy
{
    public class ZombieHealth : Core.BaseHealth
    {
        [Header("Zombie Settings")] public float destroyDelay = 2.0f; // 사망 애니메이션 시간 고려

        protected override void HandleDeath()
        {
            Debug.Log("Zombie Died.");

            // 2. 사망 애니메이션 실행
            // Animator anim = GetComponent<Animator>();
            // if(anim != null) anim.SetTrigger("Die");

            // 3. 오브젝트 제거 (일정 시간 후)
            StartCoroutine(DestroyRoutine());

            // 멀티플레이 시 서버에서 Destroy 패킷을 보내면 클라이언트는 그것을 수신해 처리
        }

        IEnumerator DestroyRoutine()
        {
            yield return new WaitForSeconds(destroyDelay);
            Destroy(gameObject);
        }
    }
}