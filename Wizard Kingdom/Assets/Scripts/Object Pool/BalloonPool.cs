using System.Collections;
using SOs;
using Balloons;

namespace ObjectPool
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
            data.Init(balloon);
        }

        public Balloon GetBalloon(Symbol symbol)
        {
            return Get(symbol);
        }

        #region Analysis And Design Methods

        public Balloon InitBalloon(BalloonData data)
        {
            if (data == null) return null;
            Balloon balloon = GetBalloon(data.symbol);
            return data.Init(balloon);
        }

        #endregion

        public void ReturnBalloon(Balloon balloon)
        {
            Return(balloon);
        }
    }
}
