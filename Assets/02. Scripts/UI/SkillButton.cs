using System;
using Division.Player;
using UnityEngine;
using UnityEngine.UI; // 이미지(쿨타임 표시 등) 제어를 위해 필요하다면 추가
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// SkillButtonType
    /// NORMAL: 일반 버튼. 클릭 시 coolTime동안 Interactable=false가 된다. 쿨타임이 끝나면 다시 클릭할 수 있다.
    /// GAUGE: 게이지 방식 버튼. 클릭 시  gaugeInitialUse만큼 바로 깎인 다음에
    /// 프레임당 또는 일정 시간간격마다 gaugeCapacity만큼 깎임.
    /// 게이지가 gaugeInitialUse 미만일 때도 누를 수는 있지만 스킬 효과가 안나감
    /// CHARGE: 일반 버튼과 같은 동작을 하지만, 거기에 추가로 꾹 누르고 있을 시 차지가 된다.
    /// (차지 UI는 나중에 다른 스크립트에서 처리함.)
    /// chargeTime 이상동안 누르고 있으면 차지 상태며, 이때 버튼을 떼면 차지 공격이 나간다.
    /// chargeTime 미만일 때 버튼을 떼면 일반 공격이 나간다.
    /// (좀비고의 동작 방식) 포인터가 버튼을 누르고 있는 채 버튼의 범위를 벗어나도 스킬 사용or차지가 캔슬되지 않으며,
    /// 버튼을 떼었을 때 스킬이 사용되도록 함 (따라서 IPointerClick 대신 IPointerUp 씀)
    /// </summary>
    public enum SkillButtonType
    {
        NORMAL, GAUGE, CHARGE
    }

    public class SkillButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public PlayerHealth playerHealth;
        [Header("Settings")]
        public int skillButtonNumber = 0;
        public SkillButtonType type = SkillButtonType.NORMAL;
        
        [Header("Parameters")]
        public float coolTime = 2.0f; // 스킬 쿨타임
        public float gaugeCapacity = 20f; // 초당 게이지 소모량
        public float gaugeRecovery = 5f;  // 초당 게이지 회복량 (추가)
        public float maxGauge = 100f;     // 최대 게이지 양 (추가)
        public float gaugeInitialUse = 20f; // 게이지 스킬 발동 시 즉시 소모량
        public float chargeTime = 1.0f;     // 차징 완료까지 걸리는 시간

        [Header("State (Read Only)")]
        [SerializeField] private float currentCoolTimer = 0f; // 현재 쿨타임 (0이면 사용 가능)
        [SerializeField] private float currentGauge = 0f;     // 현재 보유 게이지
        [SerializeField] private float currentChargeTimer = 0f; // 현재 차징 진행 시간
        [SerializeField] private bool isPressed = false;      // 버튼 눌림 상태
        [SerializeField] private bool isGaugeActive = false;  // 게이지 스킬 활성화 여부

        // 외부(SkillController)에서 연결할 스킬 동작들
        public Action onNormalSkill;       // 일반 스킬 / 차지 미완성 시 나가는 스킬
        public Action onChargeSkill;       // 차지 완성 스킬
        public Action<bool> onGaugeSkill;  // 게이지 스킬 (bool: 활성화/비활성화 상태 전달)

        [Header("Properties")]
        // [중요] 외부 UI 스크립트에서 값을 읽어가기 위한 프로퍼티 추가
        public float CurrentGauge => currentGauge;
        public float MaxGauge => maxGauge;
        public float CurrentChargeTimer => currentChargeTimer;
        public float ChargeTime => chargeTime;
        public bool IsPressed => isPressed;
        
        private Image coolTimeFillImage;

        private void Start()
        {
            coolTimeFillImage=transform.parent.GetComponent<Image>(); // 부모 오브젝트, ex. Skill1
            // 초기화
            currentGauge = maxGauge;
            currentCoolTimer = 0f;

        }

        private void Update()
        {
            if (playerHealth.GetIsDead()) return;
            // 1. 쿨타임 회복 (공통)
            if (currentCoolTimer > 0)
            {
                currentCoolTimer -= Time.deltaTime;
                if (currentCoolTimer < 0) currentCoolTimer = 0;
                coolTimeFillImage.fillAmount = 1.0f - (currentCoolTimer / coolTime);
            }

            // 2. 타입별 동작 처리
            switch (type)
            {
                case SkillButtonType.NORMAL:
                    // 일반 버튼은 Update에서 특별한 처리가 필요 없음 (쿨타임만 돌면 됨)
                    break;

                case SkillButtonType.GAUGE:
                    HandleGaugeLogic();
                    break;

                case SkillButtonType.CHARGE:
                    HandleChargeLogic();
                    break;
            }
        }

        #region Logic By Type

        // --- GAUGE 타입 로직 ---
        private void HandleGaugeLogic()
        {
            // 눌려있고 & 게이지가 남아있다면 -> 소모
            if (isPressed && currentGauge > 0)
            {
                // 초기 소모 비용 체크는 OnPointerDown에서 했으므로 여기선 지속 소모
                if (isGaugeActive)
                {
                    currentGauge -= gaugeCapacity * Time.deltaTime;
                    coolTimeFillImage.fillAmount = currentGauge / maxGauge;
                    
                    // 스킬 효과 지속 실행 (필요하다면 여기서 매 프레임 호출 or 상태만 유지)
                    // 예: 레이저 발사 중

                    if (currentGauge <= 0)
                    {
                        currentGauge = 0;
                        StopGaugeSkill(); // 게이지 바닥나면 강제 종료
                    }
                }
            }
            // 안 눌려있으면 -> 회복
            else if (!isPressed && currentGauge < maxGauge)
            {
                currentGauge += gaugeRecovery * Time.deltaTime;
                if (currentGauge > maxGauge) currentGauge = maxGauge;
                coolTimeFillImage.fillAmount = currentGauge / maxGauge;
            }
        }

        private void StartGaugeSkill()
        {
            // 초기 비용 체크
            if (currentGauge >= gaugeInitialUse)
            {
                currentGauge -= gaugeInitialUse;
                isGaugeActive = true;
                onGaugeSkill?.Invoke(true); // 스킬 시작 알림
                Debug.Log($"[{skillButtonNumber}] 게이지 스킬 시작");
            }
            else
            {
                Debug.Log($"[{skillButtonNumber}] 게이지 부족!");
            }
        }

        private void StopGaugeSkill()
        {
            if (isGaugeActive)
            {
                isGaugeActive = false;
                onGaugeSkill?.Invoke(false); // 스킬 종료 알림
                Debug.Log($"[{skillButtonNumber}] 게이지 스킬 종료");
            }
        }

        // --- CHARGE 타입 로직 ---
        private void HandleChargeLogic()
        {
            if (isPressed && currentCoolTimer <= 0)
            {
                currentChargeTimer += Time.deltaTime;

                if (currentChargeTimer >= chargeTime)
                {
                    // 차징 완료 시각적 효과 등을 여기서 처리 가능
                    // Debug.Log("차지 완료! (대기 중)");
                }
            }
        }

        private void ExecuteChargeSkill()
        {
            // 쿨타임 중이면 실행 불가
            if (currentCoolTimer > 0) return;

            if (currentChargeTimer >= chargeTime)
            {
                Debug.Log($"[{skillButtonNumber}] 차지 스킬 발동!");
                onChargeSkill?.Invoke();
            }
            else
            {
                Debug.Log($"[{skillButtonNumber}] 일반(짧은) 스킬 발동!");
                onNormalSkill?.Invoke();
            }

            // 스킬 사용 후 쿨타임 적용 및 타이머 초기화
            currentCoolTimer = coolTime;
            currentChargeTimer = 0f;
        }

        #endregion

        #region Input Interfaces

        public void OnPointerDown(PointerEventData eventData)
        {
            if (playerHealth.GetIsDead()) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;

            isPressed = true;

            if (type == SkillButtonType.GAUGE)
            {
                StartGaugeSkill();
            }
            else if (type == SkillButtonType.CHARGE)
            {
                currentChargeTimer = 0f; // 차징 시작
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (playerHealth.GetIsDead()) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;

            isPressed = false;
            
            if (type == SkillButtonType.NORMAL)
            {
                if (currentCoolTimer <= 0)
                {
                    Debug.Log($"[{skillButtonNumber}] 일반 스킬 발동");
                    onNormalSkill?.Invoke();
                    currentCoolTimer = coolTime;
                }
                else
                {
                    Debug.Log($"[{skillButtonNumber}] 쿨타임 중입니다.");
                }
            }
            else if (type == SkillButtonType.GAUGE)
            {
                StopGaugeSkill();
            }
            else if (type == SkillButtonType.CHARGE)
            {
                ExecuteChargeSkill(); // 손을 뗄 때 차지 판정
            }
        }


        #endregion
    }
}