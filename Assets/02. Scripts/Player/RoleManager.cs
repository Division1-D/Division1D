using System.Collections;
using UnityEngine;
using Division.Core;
using Player;

namespace Division.Player
{
    public enum Role
    {
        NO_ROLE, SHOUTER, FRISBEE, SINGERSONG, MAGICIAN, CHEERLEADER
    }
    public class RoleManager : MonoBehaviour
    {
        public Role role;
        public Skill[] skills=new Skill[2];
        
        PlayerMovement playerMovement;

        void Start()
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        
        public void UseSkill(int skillIndex)
        {
            switch (role)
            {
                case Role.SHOUTER:
                    ShouterSkills(skillIndex);
                    break;
                case Role.FRISBEE:
                    FrisbeeSkills(skillIndex);
                    break;
                case Role.SINGERSONG:
                    SingersongSkills(skillIndex);
                    break;
                case Role.MAGICIAN:
                    Debug.Log("magician");
                    MagicianSkills(skillIndex);
                    break;
                case Role.CHEERLEADER:
                    CheerleaderSkills(skillIndex);
                    break;
                case Role.NO_ROLE:
                    break;
            }
        }
        
        #region Shouter

        void ShouterSkills(int skillIndex)
        {
            if (skillIndex.Equals(0)) ShouterSkill1();
            else ShouterSkill2();
        }

        void ShouterSkill1()
        {
            
        }
        void ShouterSkill2()
        {
            
        }
        
        #endregion
        
        #region Frisbee

        void FrisbeeSkills(int skillIndex)
        {
            
        }
        
        
        
        #endregion
        
        #region Singersong

        void SingersongSkills(int skillIndex)
        {
            
        }
            
        
        
        #endregion
        
        #region Magician

        void MagicianSkills(int skillIndex)
        {
            Debug.Log("skillindex:"+skillIndex);
            if(skillIndex.Equals(2)) MagicianSkill1();
            else MagicianSkill2();
        }

        void MagicianSkill1()
        {
            Debug.Log("Magician Skill 1");
            StartCoroutine(IEMagicianSkill1());
        }

        IEnumerator IEMagicianSkill1()
        {
            Transform startPos = playerMovement.transform;
            Vector2 direction = playerMovement.currentDirection;
            for (int i = 0; i < 3; i++)
            {
                skills[0].CreateProjectile(startPos, direction);
                yield return new WaitForSeconds(0.1f);
            }
        }

        void MagicianSkill2()
        {
            
        }
        
        
        #endregion
        
        #region Cheerleader

        void CheerleaderSkills(int skillIndex)
        {
            
        }
            
        
        
        #endregion
    }
   
}