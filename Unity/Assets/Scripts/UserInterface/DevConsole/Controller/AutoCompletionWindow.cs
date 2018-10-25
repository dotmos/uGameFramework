using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Scrollbar;
using Service.Scripting;
using UniRx;
using System;

namespace UserInterface {
    public class AutoCompletionWindow : GameComponent {
        public GameObject proposalItemPrefab;
        public Transform proposalItemContainer;
        [Space]
        public GMScrollRect scrollRect;
        public GMInputField consoleInput;

        public bool HasContent {
            get {
                if (proposalItems.Count > 0) return true;
                else return false;
            }
        }

        private List<AutoCompletionProposalItem> proposalItems = new List<AutoCompletionProposalItem>();
        private Proposal currentProposal;

        [HideInInspector]
        public int currentSelectedElementID = 0;

        public bool IsActive {
            get {
                return gameObject.activeSelf;
            } private set { }
        }

        public void UpdateList(Proposal proposal) {
            ClearItems();

            currentProposal = proposal;

            foreach (ProposalElement proposalElement in proposal.proposalElements) {
                SpawnItem(proposalElement);
            }

            SelectElementByIndex(0);
        }

        /// <summary>
        /// Adds the selected proposal to the input field.
        /// </summary>
        /// <param name="proposalElement"></param>
        public void ApplyProposal(ProposalElement proposalElement) {
            if (currentProposal == null) return;

            ClearItems();

            currentSelectedElementID = 0;
            try {
                string beginning = consoleInput.text.Substring(0, currentProposal.replaceStringStart);
                string end = consoleInput.text.Substring(currentProposal.replaceStringEnd, consoleInput.text.Length - currentProposal.replaceStringEnd);
                consoleInput.text = beginning + proposalElement.full + end;
                consoleInput.caretPosition = currentProposal.replaceStringStart + proposalElement.full.Length;
            }
            catch (Exception e) {
                Debug.Log("Problem with autocompletion:");
                Debug.LogException(e);
                currentProposal = null;
            }
        }

        public void ApplyCurrentProposal() {
            if (currentProposal != null && currentSelectedElementID < currentProposal.proposalElements.Count) {
                ProposalElement currentProposalElement = currentProposal.proposalElements[currentSelectedElementID];
                ApplyProposal(currentProposalElement);
            }
        }

        /// <summary>
        /// Selects another entry in the auto completion window.
        /// </summary>
        /// <param name="direction">1 selects next element downwards. -1 selects next element upwards.</param>
        public void SwitchElement(int direction) {
            //Increase index
            int newIndex = Mathf.Clamp(currentSelectedElementID + direction, 0, proposalItems.Count - 1);
            SelectElementByIndex(newIndex);
        }

        void SelectElementByIndex(int index) {
            if (proposalItems.Count > 0) {
                try {
                    //Deselect previous item
                    if (currentSelectedElementID > -1) proposalItems[currentSelectedElementID].DeselectItem();
                }
                catch (Exception e) { }

                //Store current index
                currentSelectedElementID = index;
                //Select new item
                proposalItems[currentSelectedElementID].SelectItem();

                //scroll to element
                float max = proposalItems.Count - 1;
                float scrollValue = (max - currentSelectedElementID) / max;
                scrollRect.verticalScrollbar.value = scrollValue;
            } else {
                currentSelectedElementID = 0;
                scrollRect.verticalScrollbar.value = 0;
            }
        }

        void SpawnItem(ProposalElement proposal) {
            GameObject proposalItemGO = Instantiate(proposalItemPrefab) as GameObject;
            proposalItemGO.transform.SetParent(proposalItemContainer, false);
            AutoCompletionProposalItem proposalItem = proposalItemGO.GetComponent<AutoCompletionProposalItem>();
            proposalItem.Initialize(proposal, this);
            proposalItems.Add(proposalItem);
        }

        public void ClearItems() {
            foreach(AutoCompletionProposalItem go in proposalItems) {
                Destroy(go.gameObject);
            }

            proposalItems.Clear();
        }

        public void Activate(bool activate) {
            gameObject.SetActive(activate);
        }
    }
}
