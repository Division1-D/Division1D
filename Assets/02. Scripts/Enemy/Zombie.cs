using UnityEngine;

namespace Division.Enemy{
    public class Zombie : MonoBehaviour
    {
        public int health = 100;
        public int damage = 20;

        public int GetDamage()
        {
            return damage;
        }
    }
}