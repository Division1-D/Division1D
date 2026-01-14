using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SkillGaugeUI : MonoBehaviour
    {
        [Header("References")]
        SkillButton targetSkill; // 관찰할 스킬 버튼 (에디터에서 연결)
        public Image gaugeFillImage;    // 게이지/차지량을 보여줄 이미지 (Image Type: Filled)
        public GameObject uiContainer;  // (선택) NORMAL 타입일 때 아예 숨기려면 부모 객체 연결


        private void Start()
        {
            gaugeFillImage.fillAmount = 0f;
        }

        private void Update()
        {
            if (targetSkill == null || gaugeFillImage == null) return;

            UpdateGaugeDisplay();
        }

        public void SetTargetSkill(SkillButton skill)
        {
            targetSkill = skill;
        }
        
        private void UpdateGaugeDisplay()
        {
            float targetFillAmount = 0f;
            bool shouldShow = true; // UI를 보여줄지 여부

            switch (targetSkill.type)
            {
                case SkillButtonType.GAUGE:
                    // 게이지 타입: (현재 게이지 / 최대 게이지) 비율로 표시
                    // 항상 보여지거나, 필요에 따라 숨길 수 있음
                    if (targetSkill.MaxGauge > 0)
                    {
                        targetFillAmount = targetSkill.CurrentGauge / targetSkill.MaxGauge;
                    }
                    break;

                case SkillButtonType.CHARGE:
                    // 차지 타입: (현재 차지 시간 / 목표 차지 시간) 비율로 표시
                    // 누르고 있을 때만 보여주고, 평소엔 0(안보임)으로 처리
                    if (targetSkill.IsPressed && targetSkill.ChargeTime > 0)
                    {
                        targetFillAmount = targetSkill.CurrentChargeTimer / targetSkill.ChargeTime;
                        // 100% 넘어가도 1로 고정
                        targetFillAmount = Mathf.Clamp01(targetFillAmount); 
                    }
                    else
                    {
                        targetFillAmount = 0f; // 안 누르면 게이지 비움
                        shouldShow = false; // 아예 숨기고 싶다면 이 주석 해제
                    }
                    break;

                case SkillButtonType.NORMAL:
                    // 일반 타입: 게이지 바가 필요 없음
                    targetFillAmount = 0f;
                    shouldShow = false;
                    break;
            }

            // fillAmount 갱신
            gaugeFillImage.fillAmount = targetFillAmount;

            // NORMAL 타입에서 아예 안 보이게
            if (uiContainer != null)
            {
                uiContainer.SetActive(shouldShow);
            }
        }
    }
}