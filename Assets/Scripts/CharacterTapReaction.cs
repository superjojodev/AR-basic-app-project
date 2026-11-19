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
        [SerializeField, Min(0f)] private float maximumTapMovement = 20f;
        [Tooltip("Optional sound played once on spawn and when a tap starts a reaction.")]
        [SerializeField] private AudioSource characterAudio;

        private XRSimpleInteractable interactable;
        private int reactionTriggerHash;
        private bool reactionInProgress;
        private bool hasLeftIdle;
        private ScreenSpaceSelectInput pendingTapInput;
        private int selectionEndedFrame = -1;
        private Vector2 pressPosition;
        private bool hasPressPosition;
        private bool pointerMovedTooFar;

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

        private void Start()
        {
            // Start runs once per instance, so tracking reacquisition won't replay the spawn sound.
            PlayCharacterSound();
        }

        private void PlayCharacterSound()
        {
            if (characterAudio != null && characterAudio.isActiveAndEnabled && characterAudio.clip != null)
                characterAudio.Play();
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
            Debug.Log("enabled tap reaction");
        }

        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
            pendingTapInput = null;
            if (characterAnimator != null)
                characterAnimator.ResetTrigger(reactionTriggerHash);
            reactionInProgress = false;
            hasLeftIdle = false;
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            Debug.Log("tap control entered");
            if (reactionInProgress || !characterAnimator.isActiveAndEnabled || !IsIdle())
                return;
            Debug.Log("tap control advanced");
            // Selection also supports manipulation gestures. Only accept the
            // existing tap input; do not create a second input action or raycast.
            if (!(args.interactorObject is XRRayInteractor rayInteractor) ||
                !(rayInteractor.selectInput.GetObjectReference() is ScreenSpaceSelectInput input))
                return;

            // Mouse selection starts on press, but the Tap interaction performs
            // on release. Keep this target until that input update is processed.
            pendingTapInput = input;
            selectionEndedFrame = -1;
            hasPressPosition = input.dragCurrentPositionInput.TryReadValue(out pressPosition);
            pointerMovedTooFar = false;
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (args.isCanceled)
                pendingTapInput = null;
            else
                selectionEndedFrame = Time.frameCount;
        }

        private void ProcessPendingTap()
        {
            if (pendingTapInput == null)
                return;

            var input = pendingTapInput;
            if (hasPressPosition && input.dragCurrentPositionInput.TryReadValue(out var position))
            {
                // The Editor mouse drag binding is active even for a stationary
                // press. Reject actual movement rather than the binding itself.
                pointerMovedTooFar |= Vector2.Distance(pressPosition, position) > maximumTapMovement;
            }

            var tapAction = input.tapStartPositionInput.inputActionReference?.action;
            bool tapCompleted = tapAction != null && tapAction.WasPerformedThisFrame();

            // Check after XRI selection processing, including the release frame.
            // A performed action may already have returned to Waiting by now.
            if (!tapCompleted)
            {
                if (selectionEndedFrame >= 0)
                    pendingTapInput = null;
                return;
            }

            pendingTapInput = null;
            if (pointerMovedTooFar || reactionInProgress ||
                !characterAnimator.isActiveAndEnabled || !IsIdle() ||
                input.pinchGapDeltaInput.TryReadValue(out _) ||
                input.twistDeltaRotationInput.TryReadValue(out _))
                return;

            reactionInProgress = true;
            hasLeftIdle = false;
            characterAnimator.SetTrigger(reactionTriggerHash);
            PlayCharacterSound();
        }

        private void LateUpdate()
        {
            ProcessPendingTap();

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
