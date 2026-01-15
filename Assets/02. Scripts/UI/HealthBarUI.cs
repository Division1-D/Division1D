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
            healthText = transform.GetChild(0).GetComponent<TMP_Text>();
        }

        public void SetAmount(float current, float max)
        {
            healthBarFill.fillAmount = current / max;
            if (healthText != null)
            {
                healthText.text = $"{current:f0}/{max:f0}";
            }
        }
    }
}