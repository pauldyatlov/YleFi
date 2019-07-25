using System;
using UnityEngine;
using UnityEngine.UI;

namespace Yle.Fi
{
    public sealed class UIController : UIElement
    {
        [SerializeField] private Button _button = default;

        public void Show(Action onButtonClick)
        {
            _button.onClick.AddListener(() => onButtonClick());

            UI.AddDisposable(() => _button.onClick.RemoveAllListeners());
        }
    }
}