using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Division.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        Image healthBarFill;
        TMP_Text healthText;

        private void Start()
        {
            healthBarFill = GetComponent<Image>();
            Debug.Log(name+", "+healthBarFill);
            if(transform.childCount>0) healthText = transform.GetChild(0).GetComponent<TMP_Text>();
        }

        public void SetAmount(float current, float max)
        {
            if (healthBarFill == null)
                healthBarFill = GetComponent<Image>();
            healthBarFill.fillAmount = current / max;
            
//            Debug.Log(name+", hp="+current+"/"+max);
            
            if (healthText != null)
            {
                healthText.text = $"{current:f0}/{max:f0}";
            }
        }
    }
}