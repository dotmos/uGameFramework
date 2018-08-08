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

        [HideInInspector]
        public bool isInEditMode;

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
                editButton.onClick.AddListener(Edit);
                editButton.gameObject.SetActive(true);

                //Save button
                saveButton.onClick.AddListener(Save);
            } else {
                editButton.gameObject.SetActive(false);
                ActivateEditMode(false);
            }
        }

        public void Save() {
            ActivateEditMode(false);
            isInEditMode = false;
            OnSave();
        }

        public void Edit() {
            ActivateEditMode(true);
            isInEditMode = true;
            OnEdit();
        }
    }
}
