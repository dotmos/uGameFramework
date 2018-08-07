using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserInterface {
    public class UIViewEditableElement : MonoBehaviour {
        public GameObject outputMode;
        public GameObject editMode;
        [Space]
        public GMButton editButton;
        public GMButton saveButton;

        private bool isEditable;

        protected bool IsEditable {
            get {
                return isEditable;
            } set {
                isEditable = value;
                MakeEditable(isEditable);
            } 
        }

        public virtual void Initialize() {
            MakeEditable(true);
            ActivateEditMode(false);
        }

        public void ActivateEditMode(bool activate) {
            editMode.SetActive(activate);
            outputMode.SetActive(!activate);
        }

        protected virtual void OnSave() {}
        protected virtual void OnEdit() {}

        void MakeEditable(bool isEditable) {
            if (isEditable) {
                //Edit button
                editButton.onClick.AddListener(
                    () => ActivateEditMode(true)
                );
                editButton.onClick.AddListener(OnEdit);
                editButton.gameObject.SetActive(true);

                //Save button
                saveButton.onClick.AddListener(
                    () => ActivateEditMode(false)
                );
                saveButton.onClick.AddListener(OnSave);
            } else {
                editButton.gameObject.SetActive(false);
                ActivateEditMode(false);
            }
        }
    }
}
