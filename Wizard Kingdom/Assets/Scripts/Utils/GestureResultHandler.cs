using GestureRecognizer;
using UnityEngine;
using Utils;

namespace Managers
{
    public class GestureResultHandler : MonoBehaviour
    {
        public void OnRecognize(RecognitionResult result)
        {
            if (result == null || result.gesture == null)
            {
                Debug.Log("Khong nhan dien duoc shape");
                Observer.Publish(ObserverEvent.RecognitionFinished);
                return;
            }

            string shapeName = result.gesture.id;
            float score = result.score.score;

            Debug.Log($"Shape: {shapeName}, Score: {score}");

            Observer.Publish(ObserverEvent.DrawSymbol, shapeName);
            Observer.Publish(ObserverEvent.RecognitionFinished);
        }
    }
}
