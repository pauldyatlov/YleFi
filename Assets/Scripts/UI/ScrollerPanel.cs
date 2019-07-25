using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yle.Fi.EnhancedScroller;

namespace Yle.Fi
{
    public class ScrollerPanel : UIElement, IEnhancedScrollerDelegate
    {
        [SerializeField] private ProgramView _programViewTemplate = default;
        [SerializeField] private RectTransform _programsContainer = default;

        [SerializeField] private Button _button = default;

        public event Action RequestNewData;

        private BindableViewList<ContentData, ProgramView> _viewList;

        public void Show(BindableList<ContentData> tvProgramDatas)
        {
            _button.onClick.AddListener(() => RequestNewData?.Invoke());

            UI.AddDisposable(() => _button.onClick.RemoveAllListeners());

            _viewList = UI.AddDisposable(new BindableViewList<ContentData, ProgramView>(tvProgramDatas,
                _programViewTemplate,
                _programsContainer,
                (item, view) => { view.Show(item); }));
        }

        public int GetNumberOfCells(EnhancedScroller.EnhancedScroller scroller)
        {
            return _viewList.Count();
        }

        public float GetCellViewSize(EnhancedScroller.EnhancedScroller scroller, int dataIndex)
        {
            return 70;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller.EnhancedScroller scroller, int dataIndex,
            int cellIndex)
        {
            return scroller.GetCellView(_programViewTemplate);
        }
    }
}