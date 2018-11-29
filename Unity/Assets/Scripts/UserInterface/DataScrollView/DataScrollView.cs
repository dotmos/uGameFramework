using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static UserInterface.GameDataScrollView;

namespace UserInterface {
    public class DataScrollView : GameComponent, IResizeListener, IScrollHandler {

        public GameObject cellTemplate;
        public RectTransform viewport;
        public TableLayoutGroup table;
        public float scrollSensitivity = 1f;
        public UserInterface.Scrollbar.GMScrollbar verticalScrollbar;
        public UserInterface.Scrollbar.GMScrollbar horizontalScollbar;

        private const int extraRows = 2;
        private const int extraColumns = 2;

        protected int dataCount = 0;
        //The amount of rows that are needed to fill the viewport
        protected int rowCount = 0;

        //The index of data that is on top of the table elements / in the first table row.
        protected int topDataIndex = 0;

        private Vector3[] fourTableCorners;
        //The sizes of the viewport. This is the visible area
        private float viewportHeight;
        private float viewportWidth;
        //The sizes of the content. This is the rect that is scrolled. It's overlapping the viewport slightly as it has some additional rows for recycling elements.
        private float contentHeight;
        private float contentWidth;
        private RectTransform tableRectTransform;

        protected List<List<GameObject>> rowObjects = new List<List<GameObject>>();

        /// <summary>
        /// This is the key to start the engine... vroooooom!!!
        /// </summary>
        protected bool isReady;

        // Use this for initialization
        protected override void AfterBind() {
            cellTemplate.SetActive(false);

            tableRectTransform = table.transform as RectTransform;
        }

        public void Setup(List<ColumnDefinition> columns, int dataCount) {
            this.dataCount = dataCount;
            table.ColumnWidths = new float[columns.Count];

            for (int c = 0; c < columns.Count; ++c) {
                table.ColumnWidths[c] = columns[c].width;
            }

            verticalScrollbar.value = 1;
            horizontalScollbar.value = 1;

            verticalScrollbar.onValueChanged.AddListener(OnScrollVertical);
            horizontalScollbar.onValueChanged.AddListener(OnScrollHorizontal);

            isReady = true;
        }

        protected virtual void OnEnable() {}

        protected virtual void OnDisable() {
            if (isUpdating) StopCoroutine("UpdateRows");
        }


        protected virtual void InitializeTable() {
            if (isUpdating) StopCoroutine("UpdateRows");

            if (!isReady || !gameObject.activeInHierarchy) return;

            GetViewportSize();

            rowHeight = table.MinimumRowHeight + table.RowSpacing;
            rowCount = Mathf.CeilToInt(viewportHeight / rowHeight);
            rowCount = Mathf.Clamp(rowCount, 0, dataCount) + extraRows;

            contentHeight = (table.padding.top + table.padding.bottom) + (rowCount * rowHeight) - table.RowSpacing;
            contentWidth = table.padding.left + table.padding.right;

            foreach (float width in table.ColumnWidths) {
                contentWidth += width + table.ColumnSpacing;
            }

            //Remove the last columnSpacing from contentWidth
            contentWidth -= table.ColumnSpacing;

            //Set scrollbar correctly
            UpdateVerticalScrollbar();
            UpdateHorizontalScrollbar();

            //Set scroll values
            //Vertical
            scrollHeight = (dataCount * rowHeight + (table.padding.top + table.padding.bottom - table.RowSpacing)) - contentHeight;
            viewportOffset = rowHeight - (viewportHeight % rowHeight);
            viewportOffset = viewportOffset == table.MinimumRowHeight ? 0 : viewportOffset;

            //Horizontal
            scrollWidth = contentWidth - viewportWidth;
            OnScrollHorizontal(horizontalScollbar.value);

            UpdateTopDataIndex(false);

            StartCoroutine("UpdateRows");
        }

        private bool isUpdating;

        IEnumerator UpdateRows() {
            isUpdating = true;

            //Add or remove rows to match row count
            while (rowObjects.Count != rowCount) {
                if (rowObjects.Count < rowCount) AddRow();
                else RemoveRow(rowObjects.Last());

                yield return null;
            }

            yield return new WaitForEndOfFrame();

            OnTopDataIndexChanged(topDataIndex);
            OnScrollVertical(verticalScrollbar.value);
            isUpdating = false;
        }

        void AddRow() {
            List<GameObject> _row = new List<GameObject>();

            //Spawn all cells for row
            for (int i = 0; i < table.ColumnWidths.Length; ++i) {
                GameObject cellGO = Instantiate(cellTemplate);
                cellGO.SetActive(true);
                cellGO.name = "row" + rowObjects.Count + "_column" + i;
                cellGO.transform.SetParent(table.transform, false);
                _row.Add(cellGO);
            }

            rowObjects.Add(_row);
            OnRowAdded(_row, rowObjects.Count - 1);
        }

        void RemoveRow(List<GameObject> row) {
            foreach (GameObject cell in row) {
                Destroy(cell.gameObject);
            }

            OnRowRemoved(rowObjects.IndexOf(row));
            rowObjects.Remove(row);
        }

        protected virtual void OnRowAdded(List<GameObject> row, int rowIndex) { }
        protected virtual void OnRowRemoved(int rowIndex) { }


#region SCROLLING

        /// <summary>
        /// Everything concerning vertical scrolling
        /// </summary>
        float scrollHeight;
        float rowHeight;
        float viewportOffset;
        float topOffset;

        void OnScrollVertical(float normalizedScrollValue) {
            UpdateTopDataIndex();
            UpdateVerticalScrollPosition(normalizedScrollValue);
        }

        protected void UpdateVerticalScrollPosition(float normalizedScrollValue) {
            if (tableRectTransform == null) return;

            if (scrollHeight > 0) {
                //We have to invert the scroll value as GMScrollbar only supports bottom to top scrolling
                float scrollValue = (1f - normalizedScrollValue);
                topOffset = topDataIndex * rowHeight - viewportOffset * scrollValue;
                float yValue = scrollValue * (scrollHeight + extraRows * rowHeight) - topOffset;
                tableRectTransform.anchoredPosition = new Vector2(tableRectTransform.anchoredPosition.x, yValue);
            } else {
                tableRectTransform.anchoredPosition = new Vector2(tableRectTransform.anchoredPosition.x, 0);
            }
        }

        /// <summary>
        /// Everything concerning horizontal scrolling
        /// </summary>
        float scrollWidth;

        void OnScrollHorizontal(float normalizedScrollValue) {

            if (scrollWidth > 0) {
                if (tableRectTransform == null) return;
                //We have to invert the scroll value as GMScrollbar only supports right to left scrolling
                float scrollValue = (1f - normalizedScrollValue);

                float xValue = scrollValue * scrollWidth;
                tableRectTransform.anchoredPosition = new Vector2(-xValue, tableRectTransform.anchoredPosition.y);
            } else {
                tableRectTransform.anchoredPosition = new Vector2(0, tableRectTransform.anchoredPosition.y);
            }
        }

        void UpdateVerticalScrollbar() {
            verticalScrollbar.size = Mathf.Clamp((float)rowCount / (float)dataCount, 0.1f, 1f);
            if (verticalScrollbar.size == 1) verticalScrollbar.value = 1;
        }

        void UpdateHorizontalScrollbar() {
            horizontalScollbar.size = Mathf.Clamp(viewportWidth / contentWidth, 0.1f, 1f);
            if (horizontalScollbar.size == 1) horizontalScollbar.value = 1;
        }

        public void OnScroll(PointerEventData eventData) {
            if (verticalScrollbar.size == 1) verticalScrollbar.value = 1;
            else verticalScrollbar.value += eventData.scrollDelta.y * (scrollSensitivity * (1f - verticalScrollbar.size));
        }

        #endregion

        /// <summary>
        /// Updates the index of the data that is filled into the top row based on the position of the vertical scrollbar
        /// </summary>
        int lastTopDataIndex = 0;

        void UpdateTopDataIndex(bool fireCallback = true) {
            //We have to invert the scroll value as GMScrollbar only supports bottom to top scrolling
            float scrollValue = (1f - verticalScrollbar.value);
            topDataIndex = Mathf.Clamp(Mathf.FloorToInt((dataCount - rowCount + extraRows) * scrollValue), 0, dataCount - rowCount + extraRows);

            //Fire callback if top data index changed
            if (topDataIndex != lastTopDataIndex) {
                if (fireCallback) OnTopDataIndexChanged(topDataIndex);
                lastTopDataIndex = topDataIndex;
            }
        }

        protected virtual void OnTopDataIndexChanged(int newIndex) {}

        /// <summary>
        /// Sets the correct size of the viewport
        /// </summary>
        void GetViewportSize() {
            fourTableCorners = new Vector3[4];
            viewport.GetWorldCorners(fourTableCorners);
            viewportHeight = (fourTableCorners[1].y - fourTableCorners[0].y) / table.transform.lossyScale.y;
            viewportWidth = (fourTableCorners[2].x - fourTableCorners[1].x) / table.transform.lossyScale.x;
        }

        /// <summary>
        /// This is called when a draghandler is used to resize the window (via interface)
        /// </summary>
        public void OnResize() {
            InitializeTable();
        }
    }
}
