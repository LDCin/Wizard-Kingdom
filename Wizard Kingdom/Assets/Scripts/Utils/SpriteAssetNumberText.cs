using System.Text;
using TMPro;
using UnityEngine;

public class SpriteAssetNumberText : MonoBehaviour
{
    private enum SpriteNumberMode
    {
        SingleSpriteAsset,   // 1 TMP Sprite Asset chứa đủ 0 -> 9
        SeparateSpriteAssets // 10 TMP Sprite Asset riêng cho 0 -> 9
    }

    [SerializeField] private TMP_Text numberText;

    [Header("Chọn kiểu dùng sprite")]
    [SerializeField] private SpriteNumberMode mode = SpriteNumberMode.SingleSpriteAsset;

    [Header("Dùng khi mode = SingleSpriteAsset")]
    [SerializeField] private TMP_SpriteAsset digitSpriteAsset;

    [SerializeField] private int[] digitIndexes =
    {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9
    };

    [Header("Dùng khi mode = SeparateSpriteAssets")]
    [SerializeField] private TMP_SpriteAsset[] digitSpriteAssets = new TMP_SpriteAsset[10];

    [Header("Ví dụ minDigits = 5 thì 45 sẽ hiện 00045")]
    [SerializeField] private int minDigits = 1;

    private readonly StringBuilder builder = new StringBuilder(128);
    private int currentValue = -1;

    private void Awake()
    {
        ApplySpriteAsset();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (numberText == null)
            numberText = GetComponent<TMP_Text>();

        if (digitIndexes == null || digitIndexes.Length != 10)
        {
            digitIndexes = new int[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9
            };
        }

        if (digitSpriteAssets == null || digitSpriteAssets.Length != 10)
        {
            digitSpriteAssets = new TMP_SpriteAsset[10];
        }

        ApplySpriteAsset();
    }
#endif

    private void ApplySpriteAsset()
    {
        if (numberText == null)
            return;

        if (mode == SpriteNumberMode.SingleSpriteAsset && digitSpriteAsset != null)
        {
            numberText.spriteAsset = digitSpriteAsset;
        }
    }

    public void SetValue(int value)
    {
        if (value == currentValue)
            return;

        currentValue = value;

        string textValue = Mathf.Max(0, value).ToString();

        if (textValue.Length < minDigits)
            textValue = textValue.PadLeft(minDigits, '0');

        builder.Clear();

        foreach (char c in textValue)
        {
            int digit = c - '0';

            if (mode == SpriteNumberMode.SingleSpriteAsset)
            {
                AppendDigitFromSingleAsset(digit);
            }
            else
            {
                AppendDigitFromSeparateAssets(digit);
            }
        }

        numberText.text = builder.ToString();
    }

    private void AppendDigitFromSingleAsset(int digit)
    {
        if (digitSpriteAsset == null)
        {
            Debug.LogWarning("Chưa gán digitSpriteAsset.", this);
            return;
        }

        int spriteIndex = digitIndexes[digit];

        builder.Append("<sprite index=");
        builder.Append(spriteIndex);
        builder.Append(">");
    }

    private void AppendDigitFromSeparateAssets(int digit)
    {
        TMP_SpriteAsset spriteAsset = digitSpriteAssets[digit];

        if (spriteAsset == null)
        {
            Debug.LogWarning($"Chưa gán TMP Sprite Asset cho số {digit}.", this);
            return;
        }

        builder.Append("<sprite=\"");
        builder.Append(spriteAsset.name);
        builder.Append("\" index=0>");
    }
}