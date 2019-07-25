using TMPro;
using UnityEngine;
using Yle.Fi.EnhancedScroller;

namespace Yle.Fi
{
    public class ProgramView : EnhancedScrollerCellView
    {
        [SerializeField] private TextMeshProUGUI _nameLabel = default;

        public void Show(ContentData data)
        {
            _nameLabel.text = data.ItemTitle.Value;

            ShowGameObject();
        }
    }
}