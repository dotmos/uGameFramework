﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace UserInterface.Scrollbar {
    /// <summary>
    /// Class that manages the different directional strategies.
    /// Helps to remove switch/case statements in favor of a dictionaty lookup and a corrsponding behavior or strategy.
    /// Implements the StrategyPattern (https://www.dofactory.com/net/strategy-design-pattern).
    /// </summary>
    internal class DirectionalStrategy {
        /// <summary>
        /// Lookup table with predefined strategies.
        /// </summary>
        public static readonly Dictionary<Direction, DirectionalStrategy> lookup = new Dictionary<Direction, DirectionalStrategy>() {
            { Direction.RightToLeft, new DirectionalStrategy((handleCorner, remainingSize) => 1f - (handleCorner.x / remainingSize), Axis.Horizontal, true) },
            { Direction.LeftToRight, new DirectionalStrategy((handleCorner, remainingSize) => handleCorner.x / remainingSize, Axis.Horizontal, false) },
            { Direction.BottomToTop, new DirectionalStrategy((handleCorner, remainingSize) => handleCorner.y / remainingSize, Axis.Vertical, false) },
            { Direction.TopToBottom, new DirectionalStrategy((handleCorner, remainingSize) => 1f - handleCorner.y / remainingSize, Axis.Vertical, true) },
        };

        public Func<Vector2, float, float> updateDragBehavior = null;
        public Axis axis;
        public bool reverse;
        public int axisValue;

        private Func<float, float, float> chooseValueByAxis = null;
        private Func<float, float, float> chooseValueByReverse = null;

        public DirectionalStrategy(Func<Vector2, float, float> updateDragBehavior, Axis axis, bool reverse) {
            this.updateDragBehavior = updateDragBehavior;
            this.axis = axis;
            this.reverse = reverse;
            this.axisValue = (int)axis;

            chooseValueByAxis = axisValue == 0 ? chooseValueByAxis = (valueWhenH, valueWhenV) => valueWhenH : chooseValueByAxis = (valueWhenH, valueWhenV) => valueWhenV;
            chooseValueByReverse = reverse ? (valueWhenR, valueWhenNotR) => valueWhenR : chooseValueByReverse = (valueWhenR, valueWhenNotR) => valueWhenNotR;
        }

        public float ChooseValueByAxis(float valueWhenHorizontal, float valueWhenVertical) {
            return chooseValueByAxis(valueWhenHorizontal, valueWhenVertical);
        }

        public float ChooseValueByReverse(float valueWhenReversed, float valueWhenNotReversed) {
            return chooseValueByReverse(valueWhenReversed, valueWhenNotReversed);
        }
    }
}

