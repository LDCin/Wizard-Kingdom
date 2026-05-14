using System.Collections;
using ObjectPool;
using SOs;

namespace Balloons
{
    public class BalloonPool : ObjectPool<Balloon, BalloonData, Symbol>
    {
        private IEnumerator Start()
        {
            yield return InitializeAsync();
        }

        protected override Symbol GetKeyFromData(BalloonData data)
        {
            return data.symbol;
        }

        protected override Symbol GetKeyFromItem(Balloon balloon)
        {
            return balloon.Symbol;
        }

        protected override void ApplyDataToItem(Balloon balloon, BalloonData data)
        {
            balloon.InitBalloonData(
                data.symbol,
                data.sprite,
                data.runtimeAnimatorController
            );
        }

        public Balloon GetBalloon(Symbol symbol)
        {
            return Get(symbol);
        }

        public void ReturnBalloon(Balloon balloon)
        {
            Return(balloon);
        }
    }
}