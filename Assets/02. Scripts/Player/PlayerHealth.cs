using UnityEngine;

namespace Division.Player
{
    public class PlayerHealth : Core.BaseHealth
    {
        public Canvas mobileControlCanvas;
        
        [Header("Player Specific")]
        public float moveSpeedPenaltyRatio = 0.5f; // 부상 시 속도 감소 비율 있었나?
        
        public bool isDead = false;

        public bool GetIsDead()
        {
            return this.isDead;
        }

        public void SetIsDead(bool isDead)
        {
            this.isDead = isDead;
            mobileControlCanvas.enabled = !isDead;
        }
        
        protected override void HandleDeath()
        {
            // 플레이어는 '사망'이 아니라 '부상' 처리
            Debug.Log("Player is Injured! (HP <= 0)");
            
            // 예: 플레이어 움직임 스크립트를 가져와서 속도를 줄이거나 조작 불능 처리
            // var movement = GetComponent<PlayerMovement>();
            // if (movement != null) movement.SetInjuryState(true);
            WhenDied();
            // 멀티플레이 시 여기서 서버에 "나 쓰러졌어" 패킷 전송
        }
        
        // 부활 기능 (멀티플레이에서 아군이 살려줄 때 등)
        public void Revive(float restoreAmount)
        {
            IsDead = false;
            currentHealth = restoreAmount;
            NotifyHealthChanged();
            Debug.Log("Player Revived!");
            
            // 부상 상태 해제 로직 추가
            SetIsDead(false);
        }

        void WhenDied()
        {
            SetIsDead(true);
        }
        
        
        
    }
}