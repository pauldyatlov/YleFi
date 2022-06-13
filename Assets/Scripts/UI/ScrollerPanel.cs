using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Yle.Fi
{
    internal sealed class ScrollerPanel : UIElement
    {
        [SerializeField] private ProgramView _programViewTemplate;
        [SerializeField] private RectTransform _programsContainer;

        [SerializeField] private GameObject _loader;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _button;

        [SerializeField] private TextMeshProUGUI _errorLabel;
        [SerializeField] private ScrollRect _scrollRect;

        public event Action<string> RequestNewData;
        public event Action RequestNextData;

        private BindableList<ContentData> _contentDatas;
        private bool _isRequesting;

        internal void Show(BindableList<ContentData> contentDatas)
        {
            _contentDatas = contentDatas;

            _inputField.onEndEdit.AddListener(arg => RequestNewData?.Invoke(arg));
            AddDisposable(() => _inputField.onEndEdit.RemoveAllListeners());

            _button.onClick.AddListener(() => RequestNewData?.Invoke(_inputField.text));
            AddDisposable(() => _button.onClick.RemoveAllListeners());

            AddDisposable(new BindableViewList<ContentData, ProgramView>(contentDatas,
                _programViewTemplate,
                _programsContainer,
                (item, view) =>
                {
                    view.Show(item);
                }));
        }

        internal void SetRequestingStatus(bool isRequesting)
        {
            _isRequesting = isRequesting;

            _loader.gameObject.SetActive(isRequesting);
            _button.gameObject.SetActive(!isRequesting);
        }

        internal void DisplayError(string requestError)
        {
            _errorLabel.text = requestError;
        }

        private void Update()
        {
            if (_isRequesting)
                return;

            if (_contentDatas.Count() < Engine.REQUEST_LIMIT)
                return;

            if (_scrollRect.verticalNormalizedPosition <= 0)
                RequestNextData?.Invoke();
        }
    }
}