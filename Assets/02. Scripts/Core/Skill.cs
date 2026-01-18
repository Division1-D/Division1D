using System;
using System.Collections;
using UnityEngine;

namespace Division.Core{
    public enum SkillType
    {   //투사체,  플레이어에게 붙어있음
        Projectile, AttachToPlayer
    }

    public enum SkillTarget
    {
        Multi, Single
    }
    
    public class Skill : MonoBehaviour
    {
        //스킬 종류 (투사체인지, 플레이어에게 붙어있는지)
        [SerializeField] private SkillType baseSkillType=SkillType.AttachToPlayer;
        public virtual SkillType skillType => baseSkillType;
        
        //쿨타임
        [SerializeField] private float baseCooltime = 1f;
        public virtual float cooltime => baseCooltime;
        
        //기본 공격력
        [SerializeField] private int baseDamage = 25;
        public virtual int damage => baseDamage;
        
        //공격 범위 (x, y)
        [SerializeField] private Vector2Int baseAttackRange = new Vector2Int(4,1);
        public virtual Vector2Int attackRange => baseAttackRange;
        
        //공격 범위에서 플레이어가 어디에 있는지 (-1:본인 비포함, 0:본인 포함, 그외는 자기 주변 등...)
        [SerializeField] private int basePlayerRangeX = -1;
        public virtual int skillPlayerRangeX => basePlayerRangeX;
        
        //지속시간 (투사체, 한번 공격 같은 경우에는 영향 없음)
        [SerializeField] private float baseDuration = 0.5f;
        public virtual float duration => baseDuration;
        
        //단일 타겟/멀티 타겟 여부 (관통 공격은 멀티)
        [SerializeField] private SkillTarget baseSkillTarget = SkillTarget.Multi;
        public virtual SkillTarget skillTarget => baseSkillTarget;
        
        //효과 여부 리스트 (스턴, 슬로우 등)
        [SerializeField] private Effect baseEffectType;
        public virtual Effect effectType => baseEffectType;
        
        /// <summary>
        /// 샤우터
        /// - 1스킬 : AttachToPlayer, cooltime:?, damage:40 또는 30%, range:(4, 1), playerX:-1, MULTI
        /// - 2스킬 : AttachToPlayer, cooltime:1.5f, damage:20, range:(4, 1), playerX:-1, MULTI
        /// 프리스비
        /// - 1스킬 : Projectile, cooltime:?, damage:?, range:(5, 1), playerX:-1, MULTI
        /// - 2스킬 : AttachToPlayer, cooltime:?, damage:?, range:(3, 3), playerX:1, MULTI
        /// 싱어송라이터
        /// - 1스킬 : AttachToPlayer, cooltime:?, damage:?, range:(6, 3), playerX:0, MULTI
        /// - 2스킬 : AttachToPlayer, cooltime:?, damage:?, range:(2, 1), playerX:0, MULTI
        /// 로맨틱매지션
        /// - 1스킬 : Projectile, cooltime:?, damage:?, range:(8, 1), playerX:-1, SINGLE
        /// - 2스킬 : AttachToPlayer, cooltime:?, damage:?, range:(3, 3), playerX:1, MULTI
        /// 치어리더
        /// - 1스킬 : AttachToPlayer, cooltime:1.2f, damage:25, range:(4, 3), playerX:-1, MULTI
        /// - 2스킬 : AttachToPlayer, cooltime:7f, damage:?, range:(7, 7), playerX:4, MULTI, duration:5.5f
        /// </summary>
        
        public virtual void OnSkillUse()
        {
            
        }

        protected void Start()
        {
            
        }

        protected void Update()
        {
            
        }

        public int GetDamage()
        {
            return damage;
        }

        public void CreateProjectile(Transform startPos, Vector2 direction)
        {
            StartCoroutine(ThrowProjectile(startPos, direction));
        }

        IEnumerator ThrowProjectile(Transform startPos, Vector2 direction)
        {
 //           Debug.Log("Throwing Projectile");
            GameObject obj = Instantiate(this.gameObject);
            obj.transform.position = startPos.position;
                //startPos에서 direction 방향으로 attackRange만큼 던진다.

            Vector2 addVec;
            addVec = new Vector2(attackRange.x*direction.x, attackRange.y*direction.y);
            addVec+=new Vector2(direction.x*basePlayerRangeX, direction.y*basePlayerRangeX);
            
            //방향은 (1,0)(-1,0) (0,-1)(0,1)중 하나.
            Vector3 endPos = new Vector3(startPos.position.x+addVec.x, startPos.position.y+addVec.y, 0);
            
  //          Debug.Log("direction"+direction+", attackRange:"+attackRange);
//            Debug.Log(startPos.position+" + "+addVec+" = "+endPos);

            //this.transform.position.x>=endPos.x&&this.transform.position.y>=endPos.y
            while (Vector3.Distance(startPos.position, endPos)>=0)
            {
                //Debug.Log("throwing Projectile"+obj.transform.position);
               if(obj!=null) obj.transform.position += new Vector3(direction.x*0.05f, direction.y*0.05f, 0f);
                yield return null;
            }
            
            
            
        }

        public void DestroyThis()
        {
            if(skillTarget==SkillTarget.Single)
                Destroy(this.gameObject);
        }
    }
}