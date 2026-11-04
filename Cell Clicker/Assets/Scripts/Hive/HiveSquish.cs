using UnityEngine;

namespace CellClicker.Hive
{
    /// <summary>Makes the hive "breathe" while idle and squish on every tap.</summary>
    public class HiveSquish : HiveTapFeedback
    {
        [SerializeField] private float tapSquish = 0.07f;
        [SerializeField] private float maxSquish = 0.18f;
        [SerializeField] private float recoverSpeed = 14f;
        [SerializeField] private float breatheAmount = 0.02f;
        [SerializeField] private float breatheCyclesPerSecond = 0.5f;

        private float squish;

        protected override void OnHiveTapped(HiveTap tap)
        {
            squish = Mathf.Min(squish + tapSquish, maxSquish);
        }

        private void Update()
        {
            squish = Mathf.Lerp(squish, 0f, 1f - Mathf.Exp(-recoverSpeed * Time.deltaTime));

            float breathe = 1f + Mathf.Sin(Time.time * breatheCyclesPerSecond * Mathf.PI * 2f) * breatheAmount;
            transform.localScale = Vector3.one * (breathe - squish);
        }
    }
}
