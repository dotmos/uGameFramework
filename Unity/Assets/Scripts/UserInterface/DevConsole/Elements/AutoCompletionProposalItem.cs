using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Service.Scripting;

namespace UserInterface {
    public class AutoCompletionProposalItem : ButtonBase {
        public Text proposalOutput;

        private AutoCompletionWindow autoCompleteWindow;
        private ProposalElement proposalElement;
        private RectTransform rectTransform;

        public RectTransform RectTransform {
            get {
                if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            } private set {
                rectTransform = value;
            }
        }

        public void Initialize(ProposalElement proposal, AutoCompletionWindow autoCompleteWindow) {
            this.autoCompleteWindow = autoCompleteWindow;
            this.proposalElement = proposal;

            proposalOutput.text = proposal.simple;
        }

        public void SelectItem() {
            button.Highlight(true);
        }

        public void DeselectItem() {
            button.Highlight(false);
        }

        /// <summary>
        /// This is triggered by the OnClick function as well as from the Submit eventTrigger of the button.
        /// </summary>
        public void ApplyProposal() {
            if (autoCompleteWindow != null) autoCompleteWindow.ApplyProposal(proposalElement);
        }
    }
}
