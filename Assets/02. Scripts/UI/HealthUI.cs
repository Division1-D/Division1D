using UnityEngine;
using UnityEngine.UI;
using Core;

namespace UI
{
    public class HealthUI : MonoBehaviour
    {
        [Header("Target")]
        public BaseHealth targetHealth; // HP를 추적할 대상 (플레이어 또는 좀비)
        
        [Header("UI Component")]
        public Image healthBarFill; // Fill Type이 Horizontal/Vertical인 이미지

        void OnEnable()
        {
            if (targetHealth != null)
            {
                // 이벤트 구독 (HP가 변할 때만 UI 갱신 로직 실행 -> 성능 최적화)
                targetHealth.OnHealthChanged += UpdateHealthBar;
            }
        }

        void OnDisable()
        {
            // 이벤트 구독 해제 (메모리 누수 방지)
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= UpdateHealthBar;
            }
        }

        // 이벤트에 의해 호출되는 함수
        void UpdateHealthBar(float current, float max)
        {
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = current / max;
            }
        }
        
        // 런타임에 동적으로 타겟을 바꿀 경우 (예: 클릭한 적의 체력바 표시)
        public void SetTarget(BaseHealth newTarget)
        {
            // 기존 구독 해제
            if (targetHealth != null) targetHealth.OnHealthChanged -= UpdateHealthBar;
            
            targetHealth = newTarget;
            
            // 새 구독 및 즉시 갱신
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged += UpdateHealthBar;
                // 강제 갱신을 위해 현재 체력값으로 한 번 호출해줄 수도 있음
            }
        }
    }    
}