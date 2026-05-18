using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // Drives the ult charge bar, the active duration drain, and the two ult
    // ability slots (X time stop, Z wave). Pulses the "READY!" label when full.
    public class UltBarUI : MonoBehaviour
    {
        [Header("Wiring")]
        public UltSystem ult;
        public UltAbilities abilities;
        public UltChargeAttack chargeAttack;
        public Image chargeFill;
        public Image ability1Fill;
        public Image ability2Fill;
        public Text readyLabel;

        [Header("Colors")]
        public Color chargingColor = new Color(0.6f, 0.3f, 1f, 0.9f);
        public Color readyColor = new Color(1f, 0.9f, 0.2f, 1f);
        public Color activeColor = new Color(1f, 0.3f, 0.8f, 1f);
        public Color lockedColor = new Color(0.25f, 0.25f, 0.3f, 0.7f);

        void Update()
        {
            if (ult == null) return;

            if (ult.IsActive) UpdateActiveBar();
            else UpdateChargeBar();

            UpdateTimeStopSlot();
            UpdateWaveSlot();
        }

        void UpdateActiveBar()
        {
            if (chargeFill != null)
            {
                chargeFill.fillAmount = ult.ActiveFraction;
                chargeFill.color = activeColor;
            }
            if (readyLabel != null) readyLabel.enabled = false;
        }

        void UpdateChargeBar()
        {
            if (chargeFill != null)
            {
                chargeFill.fillAmount = ult.Charge;
                chargeFill.color = ult.IsReady ? readyColor : chargingColor;
            }
            if (readyLabel == null) return;

            readyLabel.enabled = ult.IsReady;
            if (ult.IsReady)
            {
                float pulse = 0.65f + 0.35f * Mathf.Sin(Time.time * 8f);
                readyLabel.color = new Color(readyColor.r, readyColor.g, readyColor.b, pulse);
            }
        }

        void UpdateTimeStopSlot()
        {
            if (ability1Fill == null || abilities == null) return;

            if (!ult.IsActive)
            {
                ability1Fill.fillAmount = 1f;
                ability1Fill.color = lockedColor;
                return;
            }

            ability1Fill.fillAmount = 1f - abilities.TimeStopCooldownFraction;
            ability1Fill.color = abilities.TimeStopActive ? activeColor : chargingColor;
        }

        void UpdateWaveSlot()
        {
            if (ability2Fill == null || chargeAttack == null) return;

            if (!ult.IsActive)
            {
                ability2Fill.fillAmount = 1f;
                ability2Fill.color = lockedColor;
                return;
            }

            if (chargeAttack.IsCharging)
            {
                ability2Fill.fillAmount = chargeAttack.ChargeFraction;
                ability2Fill.color = activeColor;
                return;
            }

            ability2Fill.fillAmount = 1f - chargeAttack.CooldownFraction;
            ability2Fill.color = chargingColor;
        }
    }
}
