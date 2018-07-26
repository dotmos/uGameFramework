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

        public void Initialize(ProposalElement proposal, AutoCompletionWindow autoCompleteWindow) {
            this.autoCompleteWindow = autoCompleteWindow;
            this.proposalElement = proposal;

            proposalOutput.text = proposal.simple;
        }

        protected override void OnClick() {
            base.OnClick();
            ApplyProposal();
        }

        public void SelectItem() {
            button.Select();
        }

        public void ApplyProposal() {
            if (autoCompleteWindow != null) autoCompleteWindow.ApplyProposal(proposalElement);
        }
    }
}
