using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AR.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace ARApp
{
    /// <summary>
    /// Plays one reaction for a tap selected by the template's screen-space interactor.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class CharacterTapReaction : MonoBehaviour
    {
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private string reactionTrigger = "React";
        [SerializeField] private string idleState = "Base Layer.idle";

        private XRSimpleInteractable interactable;
        private int reactionTriggerHash;
        private bool reactionInProgress;
        private bool hasLeftIdle;

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            reactionTriggerHash = Animator.StringToHash(reactionTrigger);

            if (characterAnimator == null)
            {
                Debug.LogError("Assign the character's Animator to Character Tap Reaction.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(OnSelectEntered);
        }

        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            if (characterAnimator != null)
                characterAnimator.ResetTrigger(reactionTriggerHash);
            reactionInProgress = false;
            hasLeftIdle = false;
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (reactionInProgress || !characterAnimator.isActiveAndEnabled || !IsIdle())
                return;

            // Selection also supports manipulation gestures. Only accept the
            // existing tap input; do not create a second input action or raycast.
            if (!(args.interactorObject is XRRayInteractor rayInteractor) ||
                !(rayInteractor.selectInput.GetObjectReference() is ScreenSpaceSelectInput input))
                return;

            if (!input.tapStartPositionInput.TryReadValue(out _) ||
                input.dragCurrentPositionInput.TryReadValue(out _) ||
                input.pinchGapDeltaInput.TryReadValue(out _) ||
                input.twistDeltaRotationInput.TryReadValue(out _))
                return;

            reactionInProgress = true;
            hasLeftIdle = false;
            characterAnimator.SetTrigger(reactionTriggerHash);
        }

        private void LateUpdate()
        {
            if (!reactionInProgress)
                return;

            // Wait for the Animator to consume the trigger and leave idle before
            // accepting its eventual return through jump -> fall -> idle.
            if (!IsIdle())
            {
                hasLeftIdle = true;
                return;
            }

            if (!hasLeftIdle)
                return;

            characterAnimator.ResetTrigger(reactionTriggerHash);
            reactionInProgress = false;
        }

        private bool IsIdle()
        {
            return !characterAnimator.IsInTransition(0) &&
                characterAnimator.GetCurrentAnimatorStateInfo(0).IsName(idleState);
        }
    }
}
