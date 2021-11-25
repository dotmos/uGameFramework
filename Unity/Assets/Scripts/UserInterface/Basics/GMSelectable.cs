using UnityEngine.UI;

namespace UserInterface {
    public class GMSelectable : Selectable
    {
        public override bool IsInteractable() {
            //Is Only interactable if it has a parent (thus is part of a canvas).
            //This is used to avoid that it is considered for navigation creation
            //event though it has been returned (to root) by an object pool
            return base.IsInteractable() && transform.parent != null;
        }
    }
}
