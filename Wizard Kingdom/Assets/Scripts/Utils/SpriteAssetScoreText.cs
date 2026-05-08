using System.Text;
using TMPro;
using UnityEngine;

public class SpriteAssetScoreText : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    [Header("Tên TMP Sprite Asset tương ứng 0 -> 9")]
    [SerializeField] private string[] digitAssetNames =
    {
        "big_0",
        "big_1",
        "big_2",
        "big_3",
        "big_4",
        "big_5",
        "big_6",
        "big_7",
        "big_8",
        "big_9"
    };

    [Header("Ví dụ minDigits = 5 thì 45 sẽ hiện 00045")]
    [SerializeField] private int minDigits = 1;

    private readonly StringBuilder builder = new StringBuilder(128);
    private int currentScore = -1;

    public void SetScore(int score)
    {
        if (score == currentScore)
            return;

        currentScore = score;

        string value = Mathf.Max(0, score).ToString();

        if (value.Length < minDigits)
            value = value.PadLeft(minDigits, '0');

        builder.Clear();

        foreach (char c in value)
        {
            int digit = c - '0';

            builder.Append("<sprite=\"");
            builder.Append(digitAssetNames[digit]);
            builder.Append("\" index=0>");
        }

        scoreText.text = builder.ToString();
    }
}