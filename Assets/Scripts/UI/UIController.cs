using System;
using UnityEngine;

namespace Yle.Fi
{
    public sealed class UIController : UIElement
    {
        [SerializeField] private ScrollerPanel _scrollerPanel = default;

        public event Action RequestNewData;

        public void Show(BindableList<ContentData> tvProgramDatas)
        {
            _scrollerPanel.Show(tvProgramDatas);
            _scrollerPanel.RequestNewData += RequestNewDataHandler;

            UI.AddDisposable(() => _scrollerPanel.RequestNewData -= RequestNewDataHandler);
        }

        private void RequestNewDataHandler()
        {
            RequestNewData?.Invoke();
        }
    }
}