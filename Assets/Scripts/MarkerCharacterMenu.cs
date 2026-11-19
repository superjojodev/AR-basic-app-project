using UnityEngine;
using UnityEngine.UI;

namespace ARApp
{
    /// <summary>
    /// Reuses the template object menu for choosing a character at a tracked marker.
    /// </summary>
    public sealed class MarkerCharacterMenu : MonoBehaviour
    {
        [SerializeField] private TrackedImagePlacement placement;
        [SerializeField] private Button openButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private Animator menuAnimator;

        private static readonly int ShowParameter = Animator.StringToHash("Show");
        private bool menuOpen;

        private void Awake()
        {
            if (placement != null && openButton != null && cancelButton != null &&
                menuRoot != null && menuAnimator != null)
                return;

            Debug.LogError("Assign the placement component and character menu references.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            openButton.onClick.AddListener(ShowMenu);
            cancelButton.onClick.AddListener(HideMenu);
            placement.CharacterSelected += HideMenu;
            HideMenu();
        }

        private void LateUpdate()
        {
            // Close stale choices immediately after tracking loss.
            if (!placement.CanSpawn && menuOpen)
                HideMenu();

            openButton.gameObject.SetActive(placement.CanSpawn && !menuOpen);
        }

        private void ShowMenu()
        {
            if (!placement.CanSpawn)
                return;

            menuOpen = true;
            menuRoot.SetActive(true);
            menuAnimator.SetBool(ShowParameter, true);
            openButton.gameObject.SetActive(false);
        }

        private void HideMenu()
        {
            menuOpen = false;
            if (menuRoot != null)
            {
                if (menuAnimator != null && menuAnimator.isActiveAndEnabled)
                    menuAnimator.SetBool(ShowParameter, false);
                menuRoot.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (placement != null)
                placement.CharacterSelected -= HideMenu;
            if (openButton != null)
            {
                openButton.onClick.RemoveListener(ShowMenu);
                openButton.gameObject.SetActive(false);
            }
            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(HideMenu);
            HideMenu();
        }
    }
}
