using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Scrollbar;
using Service.Scripting;
using UniRx;

namespace UserInterface {
    public class AutoCompletionWindow : GameComponent {
        public GameObject proposalItemPrefab;
        public Transform proposalItemContainer;
        [Space]
        public GMScrollRect scrollRect;
        public GMInputField consoleInput;

        private List<AutoCompletionProposalItem> proposalItems = new List<AutoCompletionProposalItem>();
        private Proposal currentProposal;

        [HideInInspector]
        public int currentSelectedElementID = -1;

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

            currentSelectedElementID = -1;
        }

        /// <summary>
        /// Adds the selected proposal to the input field.
        /// </summary>
        /// <param name="proposalElement"></param>
        public void ApplyProposal(ProposalElement proposalElement) {
            ClearItems();
            currentSelectedElementID = -1;

            consoleInput.Select();

            string beginning = consoleInput.text.Substring(0, currentProposal.replaceStringStart);
            string end = consoleInput.text.Substring(currentProposal.replaceStringEnd, consoleInput.text.Length - currentProposal.replaceStringEnd);
            consoleInput.text = beginning + proposalElement.full + end;
            consoleInput.caretPosition = currentProposal.replaceStringStart + proposalElement.full.Length;
        }

        /// <summary>
        /// Selects another entry in the auto completion window.
        /// </summary>
        /// <param name="direction">1 selects next element downwards. -1 selects next element upwards.</param>
        public void SwitchElement(int direction) {
            if (proposalItems.Count > 0) {
                currentSelectedElementID = Mathf.Clamp(currentSelectedElementID + direction, -1, proposalItems.Count - 1);

                if (currentSelectedElementID < 0) {
                    consoleInput.Select();
                } else {
                    proposalItems[currentSelectedElementID].SelectItem();

                    //scroll to element
                    //float scrollValue = 1 + proposalItems[currentSelectedElementID].RectTransform.anchoredPosition.y / scrollRect.content.sizeDelta.y;
                    float max = proposalItems.Count - 1;
                    float scrollValue = (max - currentSelectedElementID) / max;
                    scrollRect.verticalScrollbar.value = scrollValue;
                }
            } else {
                currentSelectedElementID = -1;
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
