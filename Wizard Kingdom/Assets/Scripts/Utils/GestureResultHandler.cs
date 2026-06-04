using System;
using GestureRecognizer;
using UnityEngine;

namespace Managers
{
    public class GestureResultHandler : MonoBehaviour
    {
        public static event Action<string> OnDrawSymbol;
        public static event Action OnRecognitionFinished;

        #region Analysis And Design Properties

        public string RecognizedSymbol { get; private set; }

        #endregion

        #region Analysis And Design Methods

        public string RecognizeSymbol()
        {
            return RecognizedSymbol;
        }
        public string recognizeSymbol() => RecognizeSymbol();

        #endregion

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
            RecognizedSymbol = shapeName;

            Debug.Log($"Shape: {shapeName}, Score: {score}");

            OnDrawSymbol?.Invoke(shapeName);

            OnRecognitionFinished?.Invoke();
        }
    }

    public class DrawAreaView : DrawDetector
    {
        [SerializeField] private GestureResultHandler _gestureResultHandler;

        #region Analysis And Design Properties

        public GameObject InDrawSymbol => gameObject;

        #endregion

        #region Analysis And Design Methods

        public string SubmitDrawing()
        {
            return _gestureResultHandler != null
                ? _gestureResultHandler.RecognizeSymbol()
                : null;
        }
        public string submitDrawing() => SubmitDrawing();

        #endregion
    }
}
