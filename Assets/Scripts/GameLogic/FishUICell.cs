using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public class FishUICell : MonoBehaviour
    {
        [SerializeField] Image _image;
        [SerializeField] TextMeshProUGUI _valueText;
        [SerializeField] TextMeshProUGUI _fishCountText;
        
        
        public void SetData(Sprite sprite, int value, int count)
        {
            if (_image != null)
                _image.sprite = sprite;
            if (_valueText != null)
                _valueText.text = $"价值: {value}";
            if (_fishCountText != null)
                _fishCountText.text = $"x{count}";
        }
    }
}