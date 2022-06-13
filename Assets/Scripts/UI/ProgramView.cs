using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Yle.Fi
{
    internal sealed class ProgramView : UIElement, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private GameObject _expandedPanel;

        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;
        [SerializeField] private TextMeshProUGUI _programTypeLabel;
        [SerializeField] private TextMeshProUGUI _episodeNumberLabel;
        [SerializeField] private TextMeshProUGUI _creatorLabel;

        [SerializeField] private Image _background;
        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _hoverColor;

        private bool _selected;

        internal void Show(ContentData data)
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