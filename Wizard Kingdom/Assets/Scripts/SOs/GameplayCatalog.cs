using System.Collections.Generic;
using Balloons;
using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "Gameplay Catalog", menuName = "Gameplay/Gameplay Catalog")]
    public class GameplayCatalog : ScriptableObject
    {
        public List<WizardData> wizards = new();
        public List<BackgroundData> backgrounds = new();
        public List<BalloonData> balloons = new();

        public WizardData FindWizard(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var w in wizards)
            {
                if (w != null && w.id == id) return w;
            }
            return null;
        }

        public BackgroundData FindBackground(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var b in backgrounds)
            {
                if (b != null && b.id == id) return b;
            }
            return null;
        }

        public BalloonData FindBalloon(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var balloon in balloons)
            {
                if (balloon != null && balloon.id == id) return balloon;
            }
            return null;
        }

        public BalloonData FindBalloon(Symbol symbol)
        {
            foreach (var balloon in balloons)
            {
                if (balloon != null && balloon.symbol == symbol) return balloon;
            }
            return null;
        }
    }
}
