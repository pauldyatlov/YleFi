using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yle.Fi
{
    public sealed class UIController : UIElement
    {
        [SerializeField] private ProgramView _programViewTemplate = default;
        [SerializeField] private RectTransform _programsContainer = default;

        [SerializeField] private Button _button = default;

        public event Action RequestNewData;
        
        public void Show(BindableList<ContentData> tvProgramDatas)
        {
            _button.onClick.AddListener(() => RequestNewData?.Invoke());

            UI.AddDisposable(() => _button.onClick.RemoveAllListeners());

            UI.AddDisposable(new BindableViewList<ContentData, ProgramView>(tvProgramDatas, _programViewTemplate,
                _programsContainer,
                (item, view) =>
                {
                    view.Show(item);
                }));
        }
    }
}