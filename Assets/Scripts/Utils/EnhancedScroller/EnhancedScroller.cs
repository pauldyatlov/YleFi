using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using JetBrains.Annotations;

namespace Yle.Fi.EnhancedScroller
{
    public delegate void CellViewVisibilityChangedDelegate(EnhancedScrollerCellView cellView);

    public delegate void ScrollerScrolledDelegate(EnhancedScroller scroller, Vector2 val, float scrollPosition);

    public delegate void ScrollerSnappedDelegate(EnhancedScroller scroller, int cellIndex, int dataIndex);

    public delegate void ScrollerScrollingChangedDelegate(EnhancedScroller scroller, bool scrolling);

    public delegate void ScrollerTweeningChangedDelegate(EnhancedScroller scroller, bool tweening);

    public enum EScrollDirectionEnum
    {
        Vertical,
        Horizontal
    }

    public enum ECellViewPositionEnum
    {
        Before,
        After
    }

    public enum EScrollbarVisibilityEnum
    {
        Always,
        Never
    }

    public enum EContainerPlacement
    {
        Bottom,
        Top
    }

    public enum ETweenType
    {
        Immediate,
        Linear,
        Spring,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic
    }

    [RequireComponent(typeof(ScrollRect))]
    public class EnhancedScroller : MonoBehaviour
    {
        private enum EListPositionEnum
        {
            First,
            Last
        }

        [SerializeField] private ScrollRect _scrollRect = default;
        [SerializeField] private HorizontalOrVerticalLayoutGroup _layoutGroup = default;

        [SerializeField] private RectTransform _scrollRectTransform = default;
        [SerializeField] private RectTransform _container = default;

        [SerializeField] private bool _loop = false;

        [SerializeField] private EScrollDirectionEnum ScrollDirection = default;
        [SerializeField] private ETweenType SnapTweenType = default;
        [SerializeField] private EContainerPlacement ContainerPlacement = EContainerPlacement.Top;

        [SerializeField] private bool Snapping = default;
        [SerializeField] private float SnapVelocityThreshold = default;
        [SerializeField] private float SnapWatchOffset = default;
        [SerializeField] private float SnapJumpToOffset = default;
        [SerializeField] private float SnapCellCenterOffset = default;
        [SerializeField] private bool SnapUseCellSpacing = default;
        [SerializeField] private float SnapTweenTime = default;

        [SerializeField] private LayoutElement _firstPadder = default;
        [SerializeField] private LayoutElement _lastPadder = default;
        [SerializeField] private RectTransform _recycledCellViewContainer = default;

        public float Spacing;
        public RectOffset Padding;

        private const int OffsetValue = 50;

        private float ScrollSize
        {
            get
            {
                var rect = _container.rect;
                var scrollRect = _scrollRectTransform.rect;
                var size = ScrollDirection == EScrollDirectionEnum.Vertical
                    ? rect.height - scrollRect.height
                    : rect.width - scrollRect.width;

                return size - 70;
            }
        }

        public IEnhancedScrollerDelegate Delegate
        {
            set
            {
                _delegate = value;
                _reloadData = true;
            }
        }

        public float ScrollPosition
        {
            private get { return _scrollPosition; }
            set
            {
                value = Mathf.Clamp(value, 0,
                    GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, ECellViewPositionEnum.Before));

                if (!(Math.Abs(_scrollPosition - value) > Mathf.Epsilon))
                    return;

                _scrollPosition = value;

                if (ScrollDirection == EScrollDirectionEnum.Vertical)
                    _scrollRect.verticalNormalizedPosition = 1f - _scrollPosition / ScrollSize;
                else
                    _scrollRect.horizontalNormalizedPosition = _scrollPosition / ScrollSize;

                _refreshActive = true;
            }
        }

        public EScrollbarVisibilityEnum ScrollbarVisibility
        {
            set
            {
                _scrollbarVisibility = value;

                if (_scrollbar == null || _cellViewOffsetArray.Count <= 0)
                    return;

                if (_cellViewOffsetArray.Last() < ScrollRectSize || _loop)
                    _scrollbar.gameObject.SetActive(_scrollbarVisibility == EScrollbarVisibilityEnum.Always);
                else
                    _scrollbar.gameObject.SetActive(_scrollbarVisibility != EScrollbarVisibilityEnum.Never);
            }
        }

        public Vector2 Velocity
        {
            set { _scrollRect.velocity = value; }
        }

        public float LinearVelocity
        {
            private get
            {
                var velocity = _scrollRect.velocity;
                return ScrollDirection == EScrollDirectionEnum.Vertical
                    ? velocity.y
                    : velocity.x;
            }
            set
            {
                _scrollRect.velocity = ScrollDirection == EScrollDirectionEnum.Vertical
                    ? new Vector2(0, value)
                    : new Vector2(value, 0);
            }
        }

        private bool IsScrolling { get; set; }

        private int StartCellViewIndex { get; set; }

        private int EndCellViewIndex { get; set; }

        private int NumberOfCells
        {
            get { return _delegate != null ? _delegate.GetNumberOfCells(this) : 0; }
        }

        private float ScrollRectSize
        {
            get
            {
                var rect = _scrollRectTransform.rect;
                return ScrollDirection == EScrollDirectionEnum.Vertical
                    ? rect.height
                    : rect.width;
            }
        }

        private EScrollbarVisibilityEnum _scrollbarVisibility;

        public CellViewVisibilityChangedDelegate CellViewVisibilityChanged;
        public ScrollerScrolledDelegate ScrollerScrolled;
        public ScrollerSnappedDelegate ScrollerSnapped;
        public ScrollerScrollingChangedDelegate ScrollerScrollingChanged;
        public ScrollerTweeningChangedDelegate ScrollerTweeningChanged;

        private Scrollbar _scrollbar;
        private IEnhancedScrollerDelegate _delegate;

        private bool _reloadData;
        private bool _refreshActive;

        private readonly SmallList<EnhancedScrollerCellView> _recycledCellViews =
            new SmallList<EnhancedScrollerCellView>();

        private readonly SmallList<float> _cellViewSizeArray = new SmallList<float>();
        private readonly SmallList<float> _cellViewOffsetArray = new SmallList<float>();

        private readonly SmallList<EnhancedScrollerCellView> _activeCellViews =
            new SmallList<EnhancedScrollerCellView>();

        private float _scrollPosition;
        private int _loopFirstCellIndex;
        private int _loopLastCellIndex;
        private float _loopFirstScrollPosition;
        private float _loopLastScrollPosition;
        private float _loopFirstJumpTrigger;
        private float _loopLastJumpTrigger;
        private float _lastScrollRectSize;
        private bool _lastLoop;

        private int _snapCellViewIndex;
        private int _snapDataIndex;
        private bool _snapJumping;
        private bool _snapInertia;
        private EScrollbarVisibilityEnum _lastScrollbarVisibility;
        private float _tweenTimeLeft;

        public EnhancedScrollerCellView GetCellView(EnhancedScrollerCellView cellPrefab)
        {
            var cellView = GetRecycledCellView(cellPrefab);
            if (cellView != null)
                return cellView;

            cellView = Instantiate(cellPrefab, _container, true);
            return cellView;
        }

        public void ReloadData()
        {
            _reloadData = false;

            _scrollPosition = 0;

            RecycleAllCells();

            if (_delegate != null)
                Resize(true);
        }

        public void JumpToDataIndex(int dataIndex,
            float scrollerOffset = 0,
            float cellOffset = 0,
            bool useSpacing = true,
            ETweenType tweenType = ETweenType.Immediate,
            float tweenTime = 0f,
            Action jumpComplete = null
        )
        {
            var cellOffsetPosition = 0f;

            if (Math.Abs(cellOffset) > Mathf.Epsilon)
            {
                var cellSize = _delegate != null ? _delegate.GetCellViewSize(this, dataIndex) : 0;

                if (useSpacing)
                {
                    cellSize += Spacing;

                    if (dataIndex > 0 && dataIndex < NumberOfCells - 1)
                        cellSize += Spacing;
                }

                cellOffsetPosition = cellSize * cellOffset;
            }

            float newScrollPosition;

            var offset = -(scrollerOffset * ScrollRectSize) + cellOffsetPosition;

            if (_loop)
            {
                var set1Position = GetScrollPositionForCellViewIndex(dataIndex, ECellViewPositionEnum.Before) + offset;
                var set2Position =
                    GetScrollPositionForCellViewIndex(dataIndex + NumberOfCells, ECellViewPositionEnum.Before) + offset;
                var set3Position =
                    GetScrollPositionForCellViewIndex(dataIndex + NumberOfCells * 2, ECellViewPositionEnum.Before) +
                    offset;

                var set1Diff = Mathf.Abs(_scrollPosition - set1Position);
                var set2Diff = Mathf.Abs(_scrollPosition - set2Position);
                var set3Diff = Mathf.Abs(_scrollPosition - set3Position);

                newScrollPosition = set1Diff < set2Diff
                    ? set1Diff < set3Diff ? set1Position : set3Position
                    : set2Diff < set3Diff
                        ? set2Position
                        : set3Position;
            }
            else
            {
                newScrollPosition = GetScrollPositionForDataIndex(dataIndex, ECellViewPositionEnum.Before) + offset;
            }

            newScrollPosition = Mathf.Clamp(newScrollPosition, 0,
                GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, ECellViewPositionEnum.Before));

            if (useSpacing)
                newScrollPosition = Mathf.Clamp(newScrollPosition - Spacing, 0,
                    GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, ECellViewPositionEnum.Before));

            StartCoroutine(TweenPosition(tweenType, tweenTime, ScrollPosition, newScrollPosition, jumpComplete));
        }

        public void Snap()
        {
            if (NumberOfCells == 0)
                return;

            _snapJumping = true;

            LinearVelocity = 0;

            _snapInertia = _scrollRect.inertia;
            _scrollRect.inertia = false;

            var snapPosition = ScrollPosition + (ScrollRectSize * Mathf.Clamp01(SnapWatchOffset));

            _snapCellViewIndex = GetCellViewIndexAtPosition(snapPosition);
            _snapDataIndex = _snapCellViewIndex % NumberOfCells;

            JumpToDataIndex(_snapDataIndex, SnapJumpToOffset, SnapCellCenterOffset, SnapUseCellSpacing, SnapTweenType,
                SnapTweenTime, SnapJumpComplete);
        }

        private float GetScrollPositionForCellViewIndex(int cellViewIndex, ECellViewPositionEnum insertPosition)
        {
            if (NumberOfCells == 0)
                return 0;

            if (cellViewIndex == 0 && insertPosition == ECellViewPositionEnum.Before)
                return 0;

            if (cellViewIndex < _cellViewOffsetArray.Count)
            {
                return insertPosition == ECellViewPositionEnum.Before
                    ? _cellViewOffsetArray[cellViewIndex - 1] + Spacing +
                      (ScrollDirection == EScrollDirectionEnum.Vertical ? Padding.top : Padding.left)
                    : _cellViewOffsetArray[cellViewIndex] +
                      (ScrollDirection == EScrollDirectionEnum.Vertical ? Padding.top : Padding.left);
            }

            return _cellViewOffsetArray[_cellViewOffsetArray.Count - 2];
        }

        private float GetScrollPositionForDataIndex(int dataIndex, ECellViewPositionEnum insertPosition)
        {
            return GetScrollPositionForCellViewIndex(_loop ? _delegate.GetNumberOfCells(this) + dataIndex : dataIndex,
                insertPosition);
        }

        private int GetCellViewIndexAtPosition(float position)
        {
            return GetCellIndexAtPosition(position, 0, _cellViewOffsetArray.Count - 1);
        }

        private void Resize(bool keepPosition)
        {
            var originalScrollPosition = _scrollPosition;
            _cellViewSizeArray.Clear();

            var offset = AddCellViewSizes();

            if (_loop)
            {
                if (offset < ScrollRectSize)
                {
                    var additionalRounds = Mathf.CeilToInt(ScrollRectSize / offset);
                    DuplicateCellViewSizes(additionalRounds, _cellViewSizeArray.Count);
                }

                _loopFirstCellIndex = _cellViewSizeArray.Count;
                _loopLastCellIndex = _loopFirstCellIndex + _cellViewSizeArray.Count - 1;

                DuplicateCellViewSizes(2, _cellViewSizeArray.Count);
            }

            CalculateCellViewOffsets();

            _container.sizeDelta = ScrollDirection == EScrollDirectionEnum.Vertical
                ? new Vector2(_container.sizeDelta.x, _cellViewOffsetArray.Last() + Padding.top + Padding.bottom)
                : new Vector2(_cellViewOffsetArray.Last() + Padding.left + Padding.right, _container.sizeDelta.y);

            if (_loop)
            {
                _loopFirstScrollPosition =
                    GetScrollPositionForCellViewIndex(_loopFirstCellIndex, ECellViewPositionEnum.Before) +
                    Spacing * 0.5f;
                _loopLastScrollPosition =
                    GetScrollPositionForCellViewIndex(_loopLastCellIndex, ECellViewPositionEnum.After) -
                    ScrollRectSize +
                    Spacing * 0.5f;

                _loopFirstJumpTrigger = _loopFirstScrollPosition - ScrollRectSize;
                _loopLastJumpTrigger = _loopLastScrollPosition + ScrollRectSize;
            }

            ResetVisibleCellViews();

            if (keepPosition)
                ScrollPosition = originalScrollPosition;
            else
                ScrollPosition = _loop ? _loopFirstScrollPosition : 0;

            ScrollbarVisibility = _scrollbarVisibility;
        }

        private float AddCellViewSizes()
        {
            var offset = 0f;

            for (var i = 0; i < NumberOfCells; i++)
            {
                _cellViewSizeArray.Add(_delegate.GetCellViewSize(this, i) + (i == 0 ? 0 : _layoutGroup.spacing));
                offset += _cellViewSizeArray[_cellViewSizeArray.Count - 1];
            }

            return offset;
        }

        private void DuplicateCellViewSizes(int numberOfTimes, int cellCount)
        {
            for (var i = 0; i < numberOfTimes; i++)
                for (var j = 0; j < cellCount; j++)
                    _cellViewSizeArray.Add(_cellViewSizeArray[j] + (j == 0 ? _layoutGroup.spacing : 0));
        }

        private void CalculateCellViewOffsets()
        {
            _cellViewOffsetArray.Clear();

            var offset = 0f;

            for (var i = 0; i < _cellViewSizeArray.Count; i++)
            {
                offset += _cellViewSizeArray[i];
                _cellViewOffsetArray.Add(offset);
            }
        }

        [CanBeNull] private EnhancedScrollerCellView GetRecycledCellView(EnhancedScrollerCellView cellPrefab)
        {
            for (var i = 0; i < _recycledCellViews.Count; i++)
            {
                if (_recycledCellViews[i].CellIdentifier != cellPrefab.CellIdentifier)
                    continue;

                var cellView = _recycledCellViews.RemoveAt(i);
                return cellView;
            }

            return null;
        }

        private void ResetVisibleCellViews()
        {
            int startIndex;
            int endIndex;

            CalculateCurrentActiveCellRange(out startIndex, out endIndex);

            var i = 0;
            var remainingCellIndexes = new SmallList<int>();

            while (i < _activeCellViews.Count)
            {
                if (_activeCellViews[i].CellIndex < startIndex || _activeCellViews[i].CellIndex > endIndex)
                {
                    RecycleCell(_activeCellViews[i]);
                }
                else
                {
                    remainingCellIndexes.Add(_activeCellViews[i].CellIndex);
                    i++;
                }
            }

            if (remainingCellIndexes.Count == 0)
            {
                for (i = startIndex; i <= endIndex; i++)
                    AddCellView(i, EListPositionEnum.Last);
            }
            else
            {
                for (i = endIndex; i >= startIndex; i--)
                    if (i < remainingCellIndexes.First())
                        AddCellView(i, EListPositionEnum.First);

                for (i = startIndex; i <= endIndex; i++)
                    if (i > remainingCellIndexes.Last())
                        AddCellView(i, EListPositionEnum.Last);
            }

            StartCellViewIndex = startIndex;
            EndCellViewIndex = endIndex;

            SetPadders();
        }

        private void RecycleAllCells()
        {
            while (_activeCellViews.Count > 0)
                RecycleCell(_activeCellViews[0]);

            StartCellViewIndex = 0;
            EndCellViewIndex = 0;
        }

        private void RecycleCell(EnhancedScrollerCellView cellView)
        {
            _activeCellViews.Remove(cellView);
            _recycledCellViews.Add(cellView);

            cellView.transform.SetParent(_recycledCellViewContainer);

            cellView.DataIndex = 0;
            cellView.CellIndex = 0;
            cellView.IsActive = false;

            if (CellViewVisibilityChanged != null)
                CellViewVisibilityChanged(cellView);
        }

        private void AddCellView(int cellIndex, EListPositionEnum listPosition)
        {
            if (NumberOfCells == 0)
                return;

            var dataIndex = cellIndex % NumberOfCells;
            var cellView = _delegate.GetCellView(this, dataIndex, cellIndex);

            cellView.CellIndex = cellIndex;
            cellView.DataIndex = dataIndex;
            cellView.IsActive = true;

            var cellTransform = cellView.transform;

            cellTransform.SetParent(_container, false);
            cellTransform.localScale = Vector3.one;

            if (ScrollDirection == EScrollDirectionEnum.Vertical)
                cellView.LayoutElement.minHeight =
                    _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? _layoutGroup.spacing : 0);
            else
                cellView.LayoutElement.minWidth =
                    _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? _layoutGroup.spacing : 0);

            if (listPosition == EListPositionEnum.First)
                _activeCellViews.AddStart(cellView);
            else
                _activeCellViews.Add(cellView);

            switch (listPosition)
            {
                case EListPositionEnum.Last:
                    cellView.transform.SetSiblingIndex(_container.childCount - 2);
                    break;
                case EListPositionEnum.First:
                    cellView.transform.SetSiblingIndex(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("listPosition", listPosition, null);
            }

            if (CellViewVisibilityChanged != null)
                CellViewVisibilityChanged(cellView);
        }

        private void SetPadders()
        {
            if (NumberOfCells == 0)
                return;

            var firstSize = _cellViewOffsetArray[StartCellViewIndex] - _cellViewSizeArray[StartCellViewIndex];
            var lastSize = _cellViewOffsetArray.Last() - _cellViewOffsetArray[EndCellViewIndex];

            if (ScrollDirection == EScrollDirectionEnum.Vertical)
            {
                _firstPadder.minHeight = firstSize;
                _firstPadder.gameObject.SetActive(firstSize > 0);

                _lastPadder.minHeight = lastSize;
                _lastPadder.gameObject.SetActive(lastSize > 0);
            }
            else
            {
                _firstPadder.minWidth = firstSize;
                _firstPadder.gameObject.SetActive(firstSize > 0);

                _lastPadder.minWidth = lastSize;
                _lastPadder.gameObject.SetActive(lastSize > 0);
            }
        }

        private void RefreshActive()
        {
            _refreshActive = false;

            int startIndex;
            int endIndex;

            if (_loop)
            {
                Vector2 velocity;

                if (_scrollPosition < _loopFirstJumpTrigger)
                {
                    velocity = _scrollRect.velocity;
                    ScrollPosition = _loopLastScrollPosition - (_loopFirstJumpTrigger - _scrollPosition);
                    _scrollRect.velocity = velocity;
                }
                else if (_scrollPosition > _loopLastJumpTrigger)
                {
                    velocity = _scrollRect.velocity;
                    ScrollPosition = _loopFirstScrollPosition + (_scrollPosition - _loopLastJumpTrigger);
                    _scrollRect.velocity = velocity;
                }
            }

            CalculateCurrentActiveCellRange(out startIndex, out endIndex);

            if (startIndex == StartCellViewIndex && endIndex == EndCellViewIndex)
                return;

            ResetVisibleCellViews();
        }

        private void CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
        {
            startIndex = 0;
            endIndex = 0;

            var startPosition = _scrollPosition;
            var rect = _scrollRectTransform.rect;
            var endPosition = _scrollPosition + (ScrollDirection == EScrollDirectionEnum.Vertical
                                  ? rect.height
                                  : rect.width);

            endPosition += OffsetValue;

            startIndex = GetCellViewIndexAtPosition(startPosition);
            endIndex = GetCellViewIndexAtPosition(endPosition);
        }

        private int GetCellIndexAtPosition(float position, int startIndex, int endIndex)
        {
            while (true)
            {
                if (startIndex >= endIndex)
                    return startIndex;

                var middleIndex = (startIndex + endIndex) / 2;

                if (_cellViewOffsetArray[middleIndex] + (ScrollDirection == EScrollDirectionEnum.Vertical
                        ? Padding.top
                        : Padding.left) >= position)
                {
                    endIndex = middleIndex;
                    continue;
                }

                startIndex = middleIndex + 1;
            }
        }

        private void Awake()
        {
            if (_layoutGroup == null)
            {
                if (ScrollDirection == EScrollDirectionEnum.Vertical)
                    _layoutGroup = _container.gameObject.GetOrAddComponent<VerticalLayoutGroup>();
                else
                    _layoutGroup = _container.gameObject.GetOrAddComponent<HorizontalLayoutGroup>();
            }

            if (ScrollDirection == EScrollDirectionEnum.Vertical)
            {
                if (ContainerPlacement == EContainerPlacement.Top)
                {
                    _container.anchorMin = new Vector2(0, 1);
                    _container.anchorMax = Vector2.one;
                    _container.pivot = new Vector2(0.5f, 1f);
                }
                else
                {
                }
            }
            else
            {
                if (ContainerPlacement == EContainerPlacement.Top)
                {
                    _container.anchorMin = Vector2.zero;
                    _container.anchorMax = new Vector2(0, 1f);
                    _container.pivot = new Vector2(0, 0.5f);
                }
                else
                {
                }
            }

            if (ContainerPlacement == EContainerPlacement.Top)
            {
                _container.offsetMax = Vector2.zero;
                _container.offsetMin = Vector2.zero;
                _container.localScale = Vector3.one;
            }
            else
            {
            }

            _scrollRect.content = _container;

            _scrollbar = ScrollDirection == EScrollDirectionEnum.Vertical
                ? _scrollRect.verticalScrollbar
                : _scrollRect.horizontalScrollbar;

            _layoutGroup.spacing = Spacing;
            _layoutGroup.padding = Padding;
            _layoutGroup.childAlignment = ContainerPlacement == EContainerPlacement.Top
                ? TextAnchor.UpperLeft
                : TextAnchor.LowerLeft;

            _layoutGroup.childForceExpandHeight = true;
            _layoutGroup.childForceExpandWidth = true;

            _scrollRect.horizontal = ScrollDirection == EScrollDirectionEnum.Horizontal;
            _scrollRect.vertical = ScrollDirection == EScrollDirectionEnum.Vertical;

            if (_firstPadder == null)
            {
                var go = new GameObject("First Padder", typeof(RectTransform));
                go.transform.SetParent(_container, false);
                _firstPadder = go.AddComponent<LayoutElement>();
            }

            if (_lastPadder == null)
            {
                var go = new GameObject("Last Padder", typeof(RectTransform));
                go.transform.SetParent(_container, false);
                _lastPadder = go.AddComponent<LayoutElement>();
            }

            if (_recycledCellViewContainer == null)
            {
                var go = new GameObject("Recycled Cells");
                go.transform.SetParent(_scrollRect.transform, false);
                _recycledCellViewContainer = go.AddComponent<RectTransform>();
            }

            _recycledCellViewContainer.gameObject.SetActive(false);

            _lastScrollRectSize = ScrollRectSize;
            _lastLoop = _loop;
            _lastScrollbarVisibility = _scrollbarVisibility;
        }

        private void Update()
        {
            if (_reloadData)
                ReloadData();

            if (_loop && Math.Abs(_lastScrollRectSize - ScrollRectSize) > Mathf.Epsilon || _loop != _lastLoop)
            {
                Resize(true);

                _lastScrollRectSize = ScrollRectSize;
                _lastLoop = _loop;
            }

            if (_lastScrollbarVisibility != _scrollbarVisibility)
            {
                ScrollbarVisibility = _scrollbarVisibility;
                _lastScrollbarVisibility = _scrollbarVisibility;
            }

            if (Math.Abs(LinearVelocity) > Mathf.Epsilon && !IsScrolling)
            {
                IsScrolling = true;
                if (ScrollerScrollingChanged != null) ScrollerScrollingChanged(this, true);
            }
            else if (Math.Abs(LinearVelocity) < Mathf.Epsilon && IsScrolling)
            {
                IsScrolling = false;
                if (ScrollerScrollingChanged != null) ScrollerScrollingChanged(this, false);
            }
        }

        private void LateUpdate()
        {
            if (_refreshActive)
                RefreshActive();
        }

        private void OnEnable()
        {
            _scrollRect.onValueChanged.AddListener(OnValueChangedHandler);
        }

        private void OnDisable()
        {
            _scrollRect.onValueChanged.RemoveListener(OnValueChangedHandler);
        }

        private void OnValueChangedHandler(Vector2 value)
        {
            _scrollPosition = ScrollDirection == EScrollDirectionEnum.Vertical
                ? (1f - value.y) * ScrollSize
                : value.x * ScrollSize;

            _refreshActive = true;

            if (ScrollerScrolled != null)
                ScrollerScrolled(this, value, _scrollPosition);

            if (Snapping && !_snapJumping && Mathf.Abs(LinearVelocity) <= SnapVelocityThreshold)
                Snap();

            RefreshActive();
        }

        private void SnapJumpComplete()
        {
            _snapJumping = false;
            _scrollRect.inertia = _snapInertia;

            if (ScrollerSnapped != null)
                ScrollerSnapped(this, _snapCellViewIndex, _snapDataIndex);
        }


        private IEnumerator TweenPosition(ETweenType tweenType, float time, float start, float end,
            Action onTweenCompleted)
        {
            if (tweenType == ETweenType.Immediate || Math.Abs(time) < Mathf.Epsilon)
            {
                ScrollPosition = end;
            }
            else
            {
                _scrollRect.velocity = Vector2.zero;

                if (ScrollerTweeningChanged != null)
                    ScrollerTweeningChanged(this, true);

                _tweenTimeLeft = 0;
                var newPosition = 0f;

                while (_tweenTimeLeft < time)
                {
                    switch (tweenType)
                    {
                        case ETweenType.Linear:
                            newPosition = LinearTween(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.Spring:
                            newPosition = SpringTween(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInQuad:
                            newPosition = EaseInQuad(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseOutQuad:
                            newPosition = EaseOutQuad(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInOutQuad:
                            newPosition = EaseInOutQuad(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInCubic:
                            newPosition = EaseInCubic(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseOutCubic:
                            newPosition = EaseOutCubic(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInOutCubic:
                            newPosition = EaseInOutCubic(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInQuart:
                            newPosition = EaseInQuart(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseOutQuart:
                            newPosition = EaseOutQuart(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInOutQuart:
                            newPosition = EaseInOutQuart(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInQuint:
                            newPosition = EaseInQuint(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseOutQuint:
                            newPosition = EaseOutQuint(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInOutQuint:
                            newPosition = EaseInOutQuint(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInSine:
                            newPosition = EaseInSine(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseOutSine:
                            newPosition = EaseOutSine(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInOutSine:
                            newPosition = EaseInOutSine(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInExpo:
                            newPosition = EaseInExpo(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseOutExpo:
                            newPosition = EaseOutExpo(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInOutExpo:
                            newPosition = EaseInOutExpo(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInCirc:
                            newPosition = EaseInCirc(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseOutCirc:
                            newPosition = EaseOutCirc(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInOutCirc:
                            newPosition = EaseInOutCirc(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInBounce:
                            newPosition = EaseInBounce(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseOutBounce:
                            newPosition = EaseOutBounce(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInOutBounce:
                            newPosition = EaseInOutBounce(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInBack:
                            newPosition = EaseInBack(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseOutBack:
                            newPosition = EaseOutBack(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInOutBack:
                            newPosition = EaseInOutBack(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInElastic:
                            newPosition = EaseInElastic(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseOutElastic:
                            newPosition = EaseOutElastic(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.EaseInOutElastic:
                            newPosition = EaseInOutElastic(start, end, _tweenTimeLeft / time);
                            break;
                        case ETweenType.Immediate:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("tweenType", tweenType, null);
                    }

                    if (_loop)
                    {
                        if (end > start && newPosition > _loopLastJumpTrigger)
                            newPosition = _loopFirstScrollPosition + (newPosition - _loopLastJumpTrigger);
                        else if (start > end && newPosition < _loopFirstJumpTrigger)
                            newPosition = _loopLastScrollPosition - (_loopFirstJumpTrigger - newPosition);
                    }

                    ScrollPosition = newPosition;

                    _tweenTimeLeft += Time.unscaledDeltaTime;

                    yield return null;
                }

                ScrollPosition = end;
            }

            onTweenCompleted();

            if (ScrollerTweeningChanged != null)
                ScrollerTweeningChanged(this, false);
        }

        private float LinearTween(float start, float end, float val)
        {
            return Mathf.Lerp(start, end, val);
        }

        private static float SpringTween(float start, float end, float val)
        {
            val = Mathf.Clamp01(val);
            val = (Mathf.Sin(val * Mathf.PI * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) *
                  (1f + 1.2f * (1f - val));
            return start + (end - start) * val;
        }

        private static float EaseInQuad(float start, float end, float val)
        {
            end -= start;
            return end * val * val + start;
        }

        private static float EaseOutQuad(float start, float end, float val)
        {
            end -= start;
            return -end * val * (val - 2) + start;
        }

        private static float EaseInOutQuad(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val + start;
            val--;
            return -end / 2 * (val * (val - 2) - 1) + start;
        }

        private static float EaseInCubic(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val + start;
        }

        private static float EaseOutCubic(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * (val * val * val + 1) + start;
        }

        private static float EaseInOutCubic(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val + start;
            val -= 2;
            return end / 2 * (val * val * val + 2) + start;
        }

        private static float EaseInQuart(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val * val + start;
        }

        private static float EaseOutQuart(float start, float end, float val)
        {
            val--;
            end -= start;
            return -end * (val * val * val * val - 1) + start;
        }

        private static float EaseInOutQuart(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val * val + start;
            val -= 2;
            return -end / 2 * (val * val * val * val - 2) + start;
        }

        private static float EaseInQuint(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val * val * val + start;
        }

        private static float EaseOutQuint(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * (val * val * val * val * val + 1) + start;
        }

        private static float EaseInOutQuint(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val * val * val + start;
            val -= 2;
            return end / 2 * (val * val * val * val * val + 2) + start;
        }

        private static float EaseInSine(float start, float end, float val)
        {
            end -= start;
            return -end * Mathf.Cos(val / 1 * (Mathf.PI / 2)) + end + start;
        }

        private static float EaseOutSine(float start, float end, float val)
        {
            end -= start;
            return end * Mathf.Sin(val / 1 * (Mathf.PI / 2)) + start;
        }

        private static float EaseInOutSine(float start, float end, float val)
        {
            end -= start;
            return -end / 2 * (Mathf.Cos(Mathf.PI * val / 1) - 1) + start;
        }

        private static float EaseInExpo(float start, float end, float val)
        {
            end -= start;
            return end * Mathf.Pow(2, 10 * (val / 1 - 1)) + start;
        }

        private static float EaseOutExpo(float start, float end, float val)
        {
            end -= start;
            return end * (-Mathf.Pow(2, -10 * val / 1) + 1) + start;
        }

        private static float EaseInOutExpo(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * Mathf.Pow(2, 10 * (val - 1)) + start;
            val--;
            return end / 2 * (-Mathf.Pow(2, -10 * val) + 2) + start;
        }

        private static float EaseInCirc(float start, float end, float val)
        {
            end -= start;
            return -end * (Mathf.Sqrt(1 - val * val) - 1) + start;
        }

        private static float EaseOutCirc(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * Mathf.Sqrt(1 - val * val) + start;
        }

        private static float EaseInOutCirc(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return -end / 2 * (Mathf.Sqrt(1 - val * val) - 1) + start;
            val -= 2;
            return end / 2 * (Mathf.Sqrt(1 - val * val) + 1) + start;
        }

        private static float EaseInBounce(float start, float end, float val)
        {
            end -= start;
            return end - EaseOutBounce(0, end, 1 - val) + start;
        }

        private static float EaseOutBounce(float start, float end, float val)
        {
            val /= 1f;
            end -= start;

            if (val < 1 / 2.75f)
                return end * (7.5625f * val * val) + start;

            if (val < 2 / 2.75f)
            {
                val -= 1.5f / 2.75f;
                return end * (7.5625f * val * val + .75f) + start;
            }

            if (val < 2.5 / 2.75)
            {
                val -= 2.25f / 2.75f;
                return end * (7.5625f * val * val + .9375f) + start;
            }

            val -= 2.625f / 2.75f;
            return end * (7.5625f * val * val + .984375f) + start;
        }

        private static float EaseInOutBounce(float start, float end, float val)
        {
            end -= start;
            var d = 1f;
            return val < d / 2
                ? EaseInBounce(0, end, val * 2) * 0.5f + start
                : EaseOutBounce(0, end, val * 2 - d) * 0.5f + end * 0.5f + start;
        }

        private static float EaseInBack(float start, float end, float val)
        {
            end -= start;
            val /= 1;
            var s = 1.70158f;
            return end * val * val * ((s + 1) * val - s) + start;
        }

        private static float EaseOutBack(float start, float end, float val)
        {
            var s = 1.70158f;
            end -= start;
            val = val / 1 - 1;
            return end * (val * val * ((s + 1) * val + s) + 1) + start;
        }

        private static float EaseInOutBack(float start, float end, float val)
        {
            var s = 1.70158f;
            end -= start;
            val /= .5f;

            if (val < 1)
            {
                s *= 1.525f;
                return end / 2 * (val * val * ((s + 1) * val - s)) + start;
            }

            val -= 2;
            s *= 1.525f;
            return end / 2 * (val * val * ((s + 1) * val + s) + 2) + start;
        }

        private static float EaseInElastic(float start, float end, float val)
        {
            end -= start;

            var d = 1f;
            var p = d * .3f;
            float s;
            float a = 0;

            if (Math.Abs(val) < Mathf.Epsilon)
                return start;

            val = val / d;

            if (Math.Abs(val - 1) < Mathf.Epsilon)
                return start + end;

            if (Math.Abs(a) < Mathf.Epsilon || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            val = val - 1;
            return -(a * Mathf.Pow(2, 10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p)) + start;
        }

        private static float EaseOutElastic(float start, float end, float val)
        {
            end -= start;

            var d = 1f;
            var p = d * .3f;
            float s;
            float a = 0;

            if (Math.Abs(val) < Mathf.Epsilon)
                return start;

            val = val / d;
            if (Math.Abs(val - 1) < Mathf.Epsilon)
                return start + end;

            if (Math.Abs(a) < Mathf.Epsilon || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return a * Mathf.Pow(2, -10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p) + end + start;
        }

        private static float EaseInOutElastic(float start, float end, float val)
        {
            end -= start;

            var d = 1f;
            var p = d * .3f;
            float s;
            float a = 0;

            if (Math.Abs(val) < Mathf.Epsilon)
                return start;

            val = val / (d / 2);

            if (Math.Abs(val - 2) < Mathf.Epsilon)
                return start + end;

            if (Math.Abs(a) < Mathf.Epsilon || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (val < 1)
            {
                val = val - 1;
                return -0.5f * (a * Mathf.Pow(2, 10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p)) + start;
            }

            val = val - 1;
            return a * Mathf.Pow(2, -10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
        }
    }
}