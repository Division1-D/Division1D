using UnityEngine;
using UnityEngine.Rendering; // Volume 컴포넌트 사용을 위해 필요
using UnityEngine.Rendering.Universal; // URP 후처리 효과 사용을 위해 필요

namespace Division.Player
{
    public class DeathGrayScaleEffect : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("플레이어의 Health 스크립트를 연결하세요.")]
        public PlayerHealth playerHealth;
        
        public Volume globalVolume;

        [Header("Settings")]
        [Tooltip("흑백으로 변하는 속도입니다.")]
        public float transitionSpeed = 3f;

        // 우리가 제어할 실제 효과 인스턴스
        private ColorAdjustments colorAdjustments;
        
        // 목표 채도 값 (0: 정상, -100: 완전 흑백)
        private float targetSaturation = 0f;


        void Start()
        {
            // 1. 안전 장치: 연결이 안 되어 있으면 자동으로 찾아봅니다.
            if (playerHealth == null)
                playerHealth = FindObjectOfType<PlayerHealth>();
            if (globalVolume == null)
                globalVolume = FindObjectOfType<Volume>();

            // 2. Volume 프로필에서 'Color Adjustments' 효과를 가져옵니다.
            // 에디터 설정에서 이 효과를 꼭 추가해둬야 합니다.
            if (globalVolume.profile.TryGet(out colorAdjustments))
            {
                // 게임 시작 시에는 채도를 정상(0)으로 초기화합니다.
                colorAdjustments.saturation.value = 0f;
            }
            else
            {
                Debug.LogError("Global Volume 프로필에 'Color Adjustments' 효과가 없습니다! 에디터 설정을 확인하세요.");
            }
        }

        void Update()
        {
            // 필수 컴포넌트가 없으면 실행하지 않음
            if (playerHealth == null || colorAdjustments == null) return;

            // 사망 여부에 따라 목표 채도 값 설정
            if (playerHealth.GetIsDead())
            {
                targetSaturation = -100f;
            }
            else
            {
                // 다시 살아났을 때 색상을 복구하고 싶다면 0으로 설정
                // 죽은 상태로 끝이라면 이 else문은 필요 없음
                targetSaturation = 0f; 
            }

            // 현재 채도에서 목표 채도로 부드럽게 변경 (Lerp 사용)
            float currentSaturation = colorAdjustments.saturation.value;
            colorAdjustments.saturation.value = Mathf.Lerp(currentSaturation, targetSaturation, Time.deltaTime * transitionSpeed);
        }
    }
        
}