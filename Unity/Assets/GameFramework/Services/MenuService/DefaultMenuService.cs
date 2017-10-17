using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Service.Menu{
    public class DefaultMenuService : IMenuService {

        Dictionary<Type, IMenuScreen> menus = new Dictionary<Type, IMenuScreen>();

        List<IMenuScreen> activeMenus = new List<IMenuScreen>();

        List<Type> doNotCloseCache = new List<Type>();

        List<Type> globalDoNotClose = new List<Type>();

        /// <summary>
        /// Show menu of Type menuType
        /// </summary>
        /// <param name="menuType">Menu type.</param>
        /// <param name="closeOtherMenus">If set to <c>true</c> close other menus.</param>
        /// <param name="doNotClose">Do not close.</param>
        public void ShowMenu(Type menuType, bool closeOtherMenus = true, List<Type> doNotClose = null)
        {
            //if menu is already open, do nothing
            if(menus.ContainsKey(menuType) && activeMenus.Contains(menus[menuType])) return;

            List<IMenuScreen> activeMenusCache = new List<IMenuScreen>();
            activeMenusCache.AddRange(activeMenus);
            //Close all other menus
            if(closeOtherMenus)
            {
                doNotCloseCache.Clear();
                if(doNotClose != null) doNotCloseCache.AddRange(doNotClose);
                doNotCloseCache.AddRange(globalDoNotClose);

                for(int i=0; i<activeMenusCache.Count; ++i)
                {
                    //If there is no doNotClose/globalDoNotClose list, simply close all menus
                    if(doNotCloseCache.Count == 0)
                    {
                        CloseMenu(activeMenusCache[i].GetType());
                    }
                    //Otherwise check if active menu is not on doNotClose and not globalDoNotClose list
                    else if( !doNotCloseCache.Contains( activeMenusCache[i].GetType()) )
                    {
                        CloseMenu(activeMenusCache[i].GetType());
                    }

                }
            }

            //Open menu if it is not already open
            if(menus.ContainsKey(menuType) && !activeMenusCache.Contains(menus[menuType]))
            {
                menus[menuType].Show();
                activeMenus.Add(menus[menuType]);
            }
        }

        /// <summary>
        /// Closes the menu of Type menuType
        /// </summary>
        /// <param name="menuType">Menu type.</param>
        public void CloseMenu(Type menuType)
        {
            List<IMenuScreen> activeMenusCache = new List<IMenuScreen>();
            activeMenusCache.AddRange(activeMenus);
            for(int i=0; i<activeMenusCache.Count; ++i)
            {
                if(activeMenusCache[i].GetType() == menuType)
                {
                    activeMenusCache[i].Hide();
                    activeMenus.Remove(activeMenus[i]);
                    return;
                }
            }
        }

        /// <summary>
        /// Close all menus
        /// </summary>
        public void CloseMenu()
        {
            //Close all active menus
            for(int i=0; i<activeMenus.Count; ++i) activeMenus[i].Hide();

            activeMenus.Clear();
        }

        /// <summary>
        /// Adds a new menu which can then be shown/closed
        /// </summary>
        /// <param name="menuScreen">Menu screen.</param>
        public void AddMenu(IMenuScreen menuScreen)
        {
            Type menuType = menuScreen.GetType();
            if(!menus.ContainsKey(menuType))
            {
                menus.Add(menuType, menuScreen);
            }

        }

        /// <summary>
        /// Adds a menu that should never automatically be closed
        /// </summary>
        /// <param name="menuType">Menu type.</param>
        public void AddDontCloseMenu(Type menuType){
            if(globalDoNotClose.Contains(menuType)) return;

            globalDoNotClose.Add(menuType);
        }
        /// <summary>
        /// Removes a menu that should never automatically be closed
        /// </summary>
        /// <param name="menuType">Menu type.</param>
        public void RemoveDontCloseMenu(Type menuType){
            if(globalDoNotClose.Contains(menuType)) globalDoNotClose.Remove(menuType);
        }
    }
}