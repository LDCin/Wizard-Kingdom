using System;
using GestureRecognizer;
using UnityEngine;

namespace Managers
{
    public class GestureResultHandler : MonoBehaviour
    {
        public static event Action<string> OnDrawSymbol;
        public static event Action OnRecognitionFinished;

        public void OnRecognize(RecognitionResult result)
        {
            if (result == null || result.gesture == null)
            {
                Debug.Log("Không nhận diện được shape");

                OnRecognitionFinished?.Invoke();
                return;
            }

            string shapeName = result.gesture.id;
            float score = result.score.score;

            Debug.Log($"Shape: {shapeName}, Score: {score}");

            OnDrawSymbol?.Invoke(shapeName);

            OnRecognitionFinished?.Invoke();
        }
    }
}