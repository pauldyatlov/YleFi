using System;
using UnityEngine.UI;

namespace Yle.Fi.EnhancedScroller
{
    public class EnhancedScrollerCellView : UIElement
    {
        public string CellIdentifier;
        public LayoutElement LayoutElement;

        [NonSerialized] public int CellIndex;
        [NonSerialized] public int DataIndex;
        [NonSerialized] public bool IsActive;
    }
}