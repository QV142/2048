using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileUI : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    public Image backgroundImage;

    public void SetValue(int value)
    {
        if (value > 0)
        {
            numberText.text = value.ToString();

            int length = numberText.text.Length;
            if (length <= 2) numberText.fontSize = 150;
            else if (length == 3) numberText.fontSize = 100;
            else if (length == 4) numberText.fontSize = 75;
            else numberText.fontSize = 60;

            numberText.color = (value == 2 || value == 4) ? Color.black : Color.white;
        }
        else
        {
            numberText.text = "";
        }

        backgroundImage.color = GetColorByValue(value);
    }

    Color GetColorByValue(int val)
    {
        switch (val)
        {
            case 2: return new Color32(238, 228, 218, 255);
            case 4: return new Color32(237, 224, 200, 255);
            case 8: return new Color32(242, 177, 121, 255);
            case 16: return new Color32(245, 149, 99, 255);
            case 32: return new Color32(246, 124, 95, 255);
            case 64: return new Color32(246, 94, 59, 255);
            case 128: return new Color32(237, 207, 114, 255);
            case 256: return new Color32(237, 204, 97, 255);
            case 512: return new Color32(237, 200, 80, 255);
            case 1024: return new Color32(237, 197, 63, 255);
            case 2048: return new Color32(237, 194, 46, 255);
            default: return new Color32(204, 192, 179, 255);
        }
    }
}
