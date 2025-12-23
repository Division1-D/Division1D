using System.Collections.Generic;
using UnityEngine;

namespace Division.Dialogue
{
    [CreateAssetMenu(menuName = "Dialogue/Character Standing")]
    public class CharacterStanding : ScriptableObject
    {
        [System.Serializable]
        public struct Standing
        {
            public string name; //캐릭터 이름
            public List<Sprite> sprites; //각 인덱스별로 표정 변화
        }
    
        public List<Standing> standings;

        public Sprite GetSprite(string characterName, int index)
        {
            try
            {
                return standings.Find(x => x.name == characterName).sprites[index];
            }
            catch
            {
                return null;
            }
        
        }
    }
}
