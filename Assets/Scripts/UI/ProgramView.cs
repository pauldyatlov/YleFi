using TMPro;
using UnityEngine;

namespace Yle.Fi
{
    public class ProgramView : UIElement
    {
        [SerializeField] private TextMeshProUGUI _nameLabel = default;

        public void Show(ContentData data)
        {
            _nameLabel.text = data.ItemTitle.Value;

            ShowGameObject();
        }
    }
}