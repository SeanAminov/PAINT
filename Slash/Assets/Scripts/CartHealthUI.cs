using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // Top-left HUD bar showing cart HP, mirrors CarriageHealthBar in world.
    public class CartHealthUI : MonoBehaviour
    {
        [Header("Wiring")]
        public CarriageHealth health;
        public Image fill;
        public Text label;

        [Header("Format")]
        public string format = "Cart {0}/{1}";

        void Update()
        {
            if (health == null) return;

            if (fill != null) fill.fillAmount = health.Fraction;
            if (label != null) label.text = string.Format(format, health.CurrentHP, health.maxHP);
        }
    }
}
