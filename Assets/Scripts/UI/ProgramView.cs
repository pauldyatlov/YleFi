using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Yle.Fi
{
    public class ProgramView : UIElement, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private GameObject _expandedPanel = default;

        [SerializeField] private TextMeshProUGUI _nameLabel = default;
        [SerializeField] private TextMeshProUGUI _descriptionLabel = default;
        [SerializeField] private TextMeshProUGUI _programTypeLabel = default;
        [SerializeField] private TextMeshProUGUI _episodeNumberLabel = default;
        [SerializeField] private TextMeshProUGUI _creatorLabel = default;

        [SerializeField] private Image _background = default;
        [SerializeField] private Color _defaultColor = default;
        [SerializeField] private Color _hoverColor = default;

        private bool _selected;

        public void Show(ContentData data)
        {
            _nameLabel.text = data.Title.Value.SubstringIfNecessary(20);

            _descriptionLabel.text = data.Description.Value;
            _programTypeLabel.text = data.Type;
            _episodeNumberLabel.text = $"Episode {data.EpisodeNumber}";

            var creatorsCount = data.Creator.Length;
            if (creatorsCount > 0)
            {
                if (creatorsCount == 1)
                {
                    _creatorLabel.text = $"{data.Creator[0].Type} {data.Creator[0].Name}";
                }
                else
                {
                    var creators = string.Join(", ", data.Creator.Select(x => x.Name));
                    _creatorLabel.text = creators;
                }
            }
            else
            {
                _creatorLabel.text = string.Empty;
            }

            ShowGameObject();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_selected)
                return;

            _background.color = _hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_selected)
                return;

            _background.color = _defaultColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _selected = !_selected;

            SetExpandedStatus(_selected);
        }

        private void SetExpandedStatus(bool expanded)
        {
            _expandedPanel.SetActive(expanded);
        }
    }
}