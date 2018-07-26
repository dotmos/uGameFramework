using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Scrollbar;
using Service.Scripting;

namespace UserInterface {
    public class AutoCompletionWindow : MonoBehaviour {
        public GameObject proposalItemPrefab;
        public Transform proposalItemContainer;
        [Space]
        public GMScrollbar scrollbar;
        public InputField consoleInput;

        private List<AutoCompletionProposalItem> proposalItems = new List<AutoCompletionProposalItem>();
        private Proposal currentProposal;
        private int currentSelectedElement;

        public bool IsActive {
            get {
                return gameObject.activeSelf;
            } private set { }
        }

        private void Awake() {
            Activate(false);
        }

        public void UpdateList(Proposal proposal) {
            ClearItems();

            currentProposal = proposal;

            foreach (ProposalElement proposalElement in proposal.proposalElements) {
                SpawnItem(proposalElement);
            }

            proposalItems[0].SelectItem();
            currentSelectedElement = 0;
        }

        public void ApplyProposal(ProposalElement proposalElement) {
            string beginning = consoleInput.text.Substring(0, currentProposal.replaceStringStart);
            string end = consoleInput.text.Substring(currentProposal.replaceStringEnd, consoleInput.text.Length - currentProposal.replaceStringEnd);
            consoleInput.text = beginning + proposalElement.full + end;
            consoleInput.caretPosition = currentProposal.replaceStringStart + proposalElement.full.Length;

            ClearItems();
            Activate(false);
        }

        public void SelectNextElement() {
            currentSelectedElement = (currentSelectedElement + 1) % proposalItems.Count;
            proposalItems[currentSelectedElement].SelectItem();
        }

        public void SelectPreviousElement() {
            currentSelectedElement = (currentSelectedElement - 1) % proposalItems.Count;
            proposalItems[currentSelectedElement].SelectItem();
        }

        void SpawnItem(ProposalElement proposal) {
            GameObject proposalItemGO = Instantiate(proposalItemPrefab) as GameObject;
            proposalItemGO.transform.SetParent(proposalItemContainer, false);
            AutoCompletionProposalItem proposalItem = proposalItemGO.GetComponent<AutoCompletionProposalItem>();
            proposalItem.Initialize(proposal, this);
            proposalItems.Add(proposalItem);
        }

        void ClearItems() {
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
