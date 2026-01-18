using System.Collections.Generic;
using UnityEngine;

namespace Division.Core
{
    [System.Flags]
    public enum EffectType
    {
        //X,  스턴, 슬로우, 푸시,  힐,  부상회복,  추가데미지,  공격력 증가%,   크확 증가%
        None, Stun, Slow, Push, Heal, Recovery, AddDamage, damagePercent, CriticalPercent
    }

    [System.Serializable]
    public class SingleEffect
    {
        public EffectType effectType=EffectType.None;
        public float effectDuration=1f;

        public SingleEffect(EffectType effectType=EffectType.None, float effectDuration=1f)
        {
            this.effectType = effectType;
            this.effectDuration = effectDuration;
        }
    }
    
    public class Effect : MonoBehaviour
    {
        public List<SingleEffect> effects = new List<SingleEffect>();
        
    }
   
}