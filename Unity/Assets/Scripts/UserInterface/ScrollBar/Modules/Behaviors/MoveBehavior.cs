using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserInterface.Scrollbar {
    /// <summary>
    /// Class that manages the different movement strategies.
    /// Helps to remove switch/case statements in favor of a dictionaty lookup and a corrsponding behavior or strategy.
    /// Implements the StrategyPattern (https://www.dofactory.com/net/strategy-design-pattern).
    /// </summary>
    internal class MoveStrategy {
        /// <summary>
        /// Lookup table with predefined strategies.
        /// </summary>
        public static readonly Dictionary<MoveDirection, MoveStrategy> lookup = new Dictionary<MoveDirection, MoveStrategy>() {
            { MoveDirection.Left, new MoveStrategy(MoveDirection.Left) },
            { MoveDirection.Right, new MoveStrategy(MoveDirection.Right) },
            { MoveDirection.Up, new MoveStrategy(MoveDirection.Up) },
            { MoveDirection.Down, new MoveStrategy(MoveDirection.Down) },
        };

        public Func<Axis, bool> axisValid = null;
        public Func<GMScrollbar, Selectable> findSelectable = null;
        public Func<Navigation.Mode, Axis, bool> selectableShouldBeNull = null;
        public Func<DirectionalStrategy, float, float, float> movement = null;

        /// <summary>
        /// Constructor. Creates fitting strategies based on the given MoveDireciton.
        /// </summary>
        /// <param name="moveDirection">The move direction to derive strategies from.</param>
        public MoveStrategy(MoveDirection moveDirection) {
            switch (moveDirection) {
                case MoveDirection.Left:
                    findSelectable = (sbar) => sbar.FindSelectableOnLeft();
                    break;
                case MoveDirection.Right:
                    findSelectable = (sbar) => sbar.FindSelectableOnRight();
                    break;
                case MoveDirection.Up:
                    findSelectable = (sbar) => sbar.FindSelectableOnUp();
                    break;
                case MoveDirection.Down:
                    findSelectable = (sbar) => sbar.FindSelectableOnDown();
                    break;
            }

            if (moveDirection == MoveDirection.Up || moveDirection == MoveDirection.Down) {
                selectableShouldBeNull = (mode, axis) => mode == Navigation.Mode.Automatic && axis == Axis.Vertical;
                axisValid = (axis) => axis == Axis.Vertical;
            }
            else {
                selectableShouldBeNull = (mode, axis) => mode == Navigation.Mode.Automatic && axis == Axis.Horizontal;
                axisValid = (axis) => axis == Axis.Horizontal;
            }

            if (moveDirection == MoveDirection.Left || moveDirection == MoveDirection.Down) {
                movement = (behavior, value, stepSize) => behavior.ChooseValueByReverse(value + stepSize, value - stepSize);
            }
            else {
                movement = (behavior, value, stepSize) => behavior.ChooseValueByReverse(value - stepSize, value + stepSize);
            }
        }
    }
}
