using Division.Player;
using UnityEngine;
using UI;

namespace Player
{
    public class SkillController : MonoBehaviour
    {
        public PlayerHealth playerHealth;
        
        [Header("UI References")] public SkillButton skillBtn1;
        public SkillButton skillBtn2;

        [Header("Player State")] public Animator playerAnimator; // 애니메이션 연동 예시

        public SkillGaugeUI gaugeUI;
        RoleManager roleManager;
        private void Start()
        {
            roleManager = GetComponent<RoleManager>();
            
            InitializeSkills();
        }

        private void InitializeSkills()
        {
            if (playerHealth.GetIsDead()) return;
         
            // --- 1번 스킬 설정: 게이지 (GAUGE) ---
            // 예: 소화기 (누르고 있는 동안 계속 나감)
            skillBtn1.skillButtonNumber = 2;
            skillBtn1.type = SkillButtonType.NORMAL;
            
            /*
            // --- 1번 스킬 설정: 게이지 (GAUGE) ---
            // 예: 소화기 (누르고 있는 동안 계속 나감)
            skillBtn1.skillButtonNumber = 2;
            skillBtn1.type = SkillButtonType.GAUGE;
            skillBtn1.maxGauge = 180f;
            skillBtn1.gaugeInitialUse = 25f;
            skillBtn1.gaugeCapacity = 10f; // 초당 소모
            skillBtn2.gaugeRecovery = 5f;
            // 실제 동작 연결 (bool 값으로 켜짐/꺼짐 전달됨)
            skillBtn1.onGaugeSkill = (isActive) =>
            {
                if (isActive)
                {
                    Debug.Log("플레이어: 소화기 ON");
                    // fireSprayEffect.SetActive(true);
                    //gaugeUI.SetTargetSkill(skillBtn1);
                    
                }
                else
                {
                    Debug.Log("플레이어: 소화기 OFF");
                    // fireSprayEffect.SetActive(false);
                }
            };
            */
            
            if (skillBtn1.IsPressed)
            {
              //  roleManager.UseSkill(0);
                //gaugeUI.SetTargetSkill(skillBtn1);
            }
            

            // --- (참고) 2번 스킬을 CHARGE
            skillBtn2.type = SkillButtonType.CHARGE;
            skillBtn2.coolTime = 3.0f;
            skillBtn2.chargeTime = 1.0f;

            skillBtn2.onNormalSkill = () => Debug.Log("플레이어: 풀 스윙 (일반 공격)");
            skillBtn2.onChargeSkill = () => { Debug.Log("플레이어: 차지한 풀 스윙 (차지 공격)!"); };
            
            if (skillBtn1.IsPressed)
            {
                gaugeUI.SetTargetSkill(skillBtn1);
            }
            else if (skillBtn2.IsPressed)
            {
                gaugeUI.SetTargetSkill(skillBtn2);
            }
            else
            {
                gaugeUI.SetTargetSkill(null);
            }
            
        }

        // 여기에 실제 스킬 구현 함수들을 작성하고 연결해도 됩니다.
        // 근데 여기에 할지 새로 팔지는 흠...
        // private void CastFireball() { ... }

        void Update()
        {
            if (playerHealth.GetIsDead()) return;
            
            if (skillBtn1.IsPressed)
            {
                
                //gaugeUI.SetTargetSkill(skillBtn1);
            }
            else if (skillBtn2.IsPressed)
            {
                gaugeUI.SetTargetSkill(skillBtn2);
            }
        }
    }
}