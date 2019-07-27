using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Yle.Fi
{
    public class ScrollerPanel : UIElement
    {
        [SerializeField] private ProgramView _programViewTemplate = default;
        [SerializeField] private RectTransform _programsContainer = default;

        [SerializeField] private GameObject _loader = default;
        [SerializeField] private TMP_InputField _inputField = default;
        [SerializeField] private Button _button = default;

        [SerializeField] private TextMeshProUGUI _errorLabel = default;
        [SerializeField] private ScrollRect _scrollRect = default;

        public event Action<string> RequestNewData;
        public event Action RequestNextData;

        private BindableList<ContentData> _contentDatas;
        private bool _isRequesting;

        public void Show(BindableList<ContentData> contentDatas)
        {
            _contentDatas = contentDatas;

            _inputField.onEndEdit.AddListener(arg => RequestNewData?.Invoke(arg));
            UI.AddDisposable(() => _inputField.onEndEdit.RemoveAllListeners());

            _button.onClick.AddListener(() => RequestNewData?.Invoke(_inputField.text));
            UI.AddDisposable(() => _button.onClick.RemoveAllListeners());

            UI.AddDisposable(new BindableViewList<ContentData, ProgramView>(contentDatas,
                _programViewTemplate,
                _programsContainer,
                (item, view) => { view.Show(item); }));
        }

        public void SetRequestingStatus(bool isRequesting)
        {
            _isRequesting = isRequesting;

            _loader.gameObject.SetActive(isRequesting);
            _button.gameObject.SetActive(!isRequesting);
        }

        public void DisplayError(string requestError)
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