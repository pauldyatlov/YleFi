using System;
using UnityEngine;

namespace Yle.Fi
{
    public class UIElement : MonoBehaviour, IDisposable
    {
        // ReSharper disable once InconsistentNaming
        protected readonly UIParent UI = new UIParent();

        public void Display()
        {
            ShowGameObject();
        }

        public void Dispose()
        {
            UI.Dispose();

            HideGameObject();
        }

        protected void ShowGameObject()
        {
            gameObject.SetActive(true);
        }

        protected void HideGameObject()
        {
            gameObject.SetActive(false);
        }
    }
}