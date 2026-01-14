using UnityEngine;
using System;

namespace Core
{
// 서버 연동을 고려해 로직과 데이터 처리에 집중하는 베이스 클래스
    public abstract class BaseHealth : MonoBehaviour
    {
        [Header("Base Settings")] [SerializeField]
        protected float maxHealth = 50f;

        protected float currentHealth;

        [Header("Regeneration")] [SerializeField]
        protected float regenPerSecond = 0f; // 0이면 재생 안 함

        public bool IsDead { get; protected set; } = false;

        // UI나 네트워크 매니저가 구독할 이벤트
        // <현재 체력, 최대 체력>
        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath; // 사망/부상 시 발생

        protected virtual void Start()
        {
            currentHealth = maxHealth;
            // 초기화 시 UI 갱신을 위해 이벤트 호출
            NotifyHealthChanged();
        }

        protected virtual void Update()
        {
            // 서버가 아니라 클라이언트에서 단순 재생을 처리할 경우 사용
            // (만약 완전한 서버 권한 게임이면 서버에서 처리하고 결과만 받아야 함)
            if (!IsDead && regenPerSecond > 0 && currentHealth < maxHealth)
            {
                Heal(regenPerSecond * Time.deltaTime, false);
            }
        }

        // 데미지 처리
        public virtual void TakeDamage(float amount)
        {
            if (IsDead) return;

            currentHealth -= amount;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                IsDead = true;
                HandleDeath(); // 자식 클래스에서 정의할 구체적인 행동 호출
                OnDeath?.Invoke(); // 외부(게임매니저 등)에 알림
            }

            NotifyHealthChanged();
        }

        // 회복 처리
        // isRegen: 재생 효과인지 즉시 회복인지 구분 (이펙트 처리 등을 위해)
        public virtual void Heal(float amount, bool isInstantHeal = true)
        {
            if (IsDead) return; // 죽은 상태에선 회복 불가 (게임 기획에 따라 변경 가능)

            currentHealth += amount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            NotifyHealthChanged();

            if (isInstantHeal)
            {
                // 여기에 힐 이펙트나 사운드 재생 로직 추가 가능
                // Debug.Log("Immediate Heal!");
            }
        }

        // 초당 재생량 설정 (아이템 획득 등)
        public void SetRegenRate(float rate)
        {
            regenPerSecond = rate;
        }

        // 체력 변경 알림
        protected void NotifyHealthChanged()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        // 자식 클래스에서 반드시 구현해야 하는 사망/부상 처리 함수
        protected abstract void HandleDeath();
    }
}