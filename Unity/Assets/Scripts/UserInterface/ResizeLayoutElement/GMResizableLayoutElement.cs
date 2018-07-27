using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface {
    [AddComponentMenu(NamingHelper.ResizableLayout.Name, 34)]
    public class GMResizableLayoutElement : MonoBehaviour {
        public LayoutElement layoutElement;
        public Vector2 minimumDimensions = new Vector2(1280, 720);
        public Vector2 maximumDimensions = new Vector2(1920, 1080);

        public void ResizeLayoutElement(float xStep, float yStep) {
            layoutElement.minWidth = Mathf.Clamp(layoutElement.minWidth + xStep, minimumDimensions.x, maximumDimensions.x);
            layoutElement.minHeight = Mathf.Clamp(layoutElement.minHeight + yStep, minimumDimensions.y, maximumDimensions.y);
        }
	}
}
