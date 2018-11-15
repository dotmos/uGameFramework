using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace UserInterface {
    public class DataScrollView : MonoBehaviour, IResizeListener, IScrollHandler {

        public GameObject cellTemplate;
        public RectTransform viewport;
        public TableLayoutGroup table;
        public float scrollSensitivity = 1f;
        public UserInterface.Scrollbar.GMScrollbar verticalScrollbar;
        public UserInterface.Scrollbar.GMScrollbar horizontalScollbar;

        private const int extraRows = 2;
        private const int extraColumns = 2;

        protected int dataCount = 10;

        //The index of data that is on top of the table elements
        protected int topDataIndex = 0;

        private Vector3[] fourTableCorners;
        private float viewportHeight;
        private float viewportWidth;
        private float contentHeight;
        private float contentWidth;
        private RectTransform tableRectTransform;

        protected List<List<GameObject>> dataRowObjects = new List<List<GameObject>>();

        private bool isReady;

        // Use this for initialization
        protected virtual void Awake() {
            cellTemplate.SetActive(false);

            tableRectTransform = table.transform as RectTransform;
        }

        public void Setup(List<float> columns, int dataCount) {
            this.dataCount = dataCount;
            table.ColumnWidths = new float[columns.Count];

            for (int c = 0; c < columns.Count; ++c) {
                table.ColumnWidths[c] = columns[c];
            }

            verticalScrollbar.value = 1;
            horizontalScollbar.value = 1;

            isReady = true;
        }


        protected virtual void InitializeTable() {
            if (!isReady || dataCount == 0) return;

            verticalScrollbar.onValueChanged.RemoveListener(OnScrollVertical);
            horizontalScollbar.onValueChanged.RemoveListener(OnScrollHorizontal);

            //Destroy old rows
            if (dataRowObjects.Count > 0) {
                foreach (List<GameObject> row in dataRowObjects) {
                    foreach (GameObject cell in row) {
                        Destroy(cell.gameObject);
                    }
                }

                dataRowObjects.Clear();
                dataRowObjects = new List<List<GameObject>>();
            }

            UpdateTableSizeData();

            contentHeight = table.padding.top + table.padding.bottom;
            contentWidth = 0f;

            foreach(float width in table.ColumnWidths) {
                contentWidth += width;
            }

            //Add rows
            for (int i = 0; i < dataCount; ++i) {
                AddRow();

                contentHeight += table.MinimumRowHeight;
                //Add extra rows
                if (contentHeight >= viewportHeight + (table.MinimumRowHeight * extraRows)) break;
                //Add spacing if we are not done yet
                contentHeight += table.RowSpacing;
            }

            //Set scrollbar correctly
            UpdateVerticalScrollbar();
            UpdateHorizontalScrollbar();

            //Set scroll values
            //Vertical
            rowHeight = table.MinimumRowHeight + table.RowSpacing;
            scrollHeight = (dataCount * rowHeight + (table.padding.top + table.padding.bottom - table.RowSpacing)) - contentHeight;
            viewportOffset = rowHeight - (viewportHeight % rowHeight);
            viewportOffset = viewportOffset == table.MinimumRowHeight ? 0 : viewportOffset;

            //Horizontal
            scrollWidth = contentWidth - viewportWidth;
  
            verticalScrollbar.onValueChanged.AddListener(OnScrollVertical);
            horizontalScollbar.onValueChanged.AddListener(OnScrollHorizontal);
        }

        void AddRow() {
            List<GameObject> _row = new List<GameObject>();

            //Spawn all cells for row
            for (int i = 0; i < table.ColumnWidths.Length; ++i) {
                GameObject cellGO = Instantiate(cellTemplate);
                cellGO.SetActive(true);
                cellGO.name = "row" + dataRowObjects.Count + "_column" + i;
                cellGO.transform.SetParent(table.transform, false);
                _row.Add(cellGO);
            }

            dataRowObjects.Add(_row);
        }

        float scrollHeight;
        float rowHeight;
        float viewportOffset;
        float topOffset;

        protected virtual void OnScrollVertical(float normalizedScrollValue) {
            if (tableRectTransform == null) return;
            //We have to invert the scroll value as GMScrollbar only supports bottom to top scrolling
            float scrollValue = (1f - normalizedScrollValue);

            topDataIndex = Mathf.FloorToInt(Mathf.Clamp((dataCount - dataRowObjects.Count + extraRows) * scrollValue, 0, dataCount - dataRowObjects.Count + extraRows));

            topOffset = topDataIndex * rowHeight - viewportOffset * scrollValue;
            float yValue = scrollValue * (scrollHeight + extraRows * rowHeight) - topOffset;
            tableRectTransform.anchoredPosition = new Vector2(tableRectTransform.anchoredPosition.x, yValue);

            UpdateRowOutput();
        }

        float scrollWidth;

        protected virtual void OnScrollHorizontal(float normalizedScrollValue) {
            if (tableRectTransform == null) return;
            //We have to invert the scroll value as GMScrollbar only supports right to left scrolling
            float scrollValue = (1f - normalizedScrollValue);

            float xValue = scrollValue * scrollWidth;
            tableRectTransform.anchoredPosition = new Vector2(-xValue ,tableRectTransform.anchoredPosition.y);
        }

        protected virtual void UpdateRowOutput() {
            int rowsToHide = (topDataIndex + extraRows) - (dataCount - dataRowObjects.Count + extraRows);

            if (rowsToHide >= 0) {
                for(int i = 0; i < extraRows; ++i) {
                    foreach(GameObject dataCell in dataRowObjects[dataRowObjects.Count - (i + 1)]) {
                        dataCell.SetActive(i < rowsToHide ? false : true);
                    }
                }
            }
        }

        void UpdateTableSizeData() {
            fourTableCorners = new Vector3[4];
            viewport.GetWorldCorners(fourTableCorners);
            viewportHeight = (fourTableCorners[1].y - fourTableCorners[0].y) / table.transform.lossyScale.y;
            viewportWidth = (fourTableCorners[2].x - fourTableCorners[1].x) / table.transform.lossyScale.x;
        }

        void UpdateVerticalScrollbar() {
            verticalScrollbar.size = Mathf.Clamp(dataRowObjects.Count / dataCount, 0.1f, 1f);
        }

        void UpdateHorizontalScrollbar() {
            horizontalScollbar.size = Mathf.Clamp(viewportWidth / contentWidth, 0.1f, 1f);
        }

        public void OnResize() {
            InitializeTable();
        }

        public void OnScroll(PointerEventData eventData) {
            verticalScrollbar.value += eventData.scrollDelta.y * scrollSensitivity;
        }
    }
}
