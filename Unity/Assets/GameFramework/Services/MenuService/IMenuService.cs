using System;
using System.Collections.Generic;

namespace Service.Menu{
    public interface IMenuService{
        /// <summary>
        /// Show menu of Type menuType
        /// </summary>
        /// <param name="menuType">Menu type.</param>
        /// <param name="closeOtherMenus">If set to <c>true</c> close other menus.</param>
        void ShowMenu(Type menuType, bool closeOtherMenus = true, List<Type> doNotClose = null);
        /// <summary>
        /// Close all menus
        /// </summary>
        void CloseMenu();
        /// <summary>
        /// Closes the menu of Type menuType
        /// </summary>
        /// <param name="menuType">Menu type.</param>
        void CloseMenu(Type menuType);
        /// <summary>
        /// Adds a new menu which can then be shown/closed
        /// </summary>
        /// <param name="menuScreen">Menu screen.</param>
        void AddMenu(IMenuScreen menuScreen);

        /// <summary>
        /// Adds a menu that should never automatically be closed
        /// </summary>
        /// <param name="menuType">Menu type.</param>
        void AddDontCloseMenu(Type menuType);
        /// <summary>
        /// Removes a menu that should never automatically be closed
        /// </summary>
        /// <param name="menuType">Menu type.</param>
        void RemoveDontCloseMenu(Type menuType);
    }
}