using System;
using CellClicker.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CellClicker.Hive
{
    /// <summary>
    /// The DNA Hive: tap it to earn DNA. It only handles the tap itself; visual and audio
    /// feedback live in <see cref="HiveTapFeedback"/> components that listen to <see cref="Tapped"/>.
    /// Uses UI pointer events, so every finger in a multi-touch counts as its own tap.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class DnaHive : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private DnaBank bank;
        [SerializeField] private double dnaPerTap = 1;

        public double DnaPerTap => dnaPerTap;

        /// <summary>Fired after DNA has been added for a tap.</summary>
        public event Action<HiveTap> Tapped;

        public void OnPointerDown(PointerEventData eventData)
        {
            bank.Add(dnaPerTap);
            Tapped?.Invoke(new HiveTap(eventData.position, eventData.pressEventCamera, dnaPerTap));
        }
    }
}
