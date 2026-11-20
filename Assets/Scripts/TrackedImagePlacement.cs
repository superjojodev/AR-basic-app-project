using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARApp
{
    /// <summary>
    /// Keeps one character aligned with a registered image while it is tracked.
    /// Image recognition is independent of QR payload validation (not implemented yet).
    /// </summary>
    public sealed class TrackedImagePlacement : MonoBehaviour
    {
        [SerializeField] private ARTrackedImageManager imageManager;
        [SerializeField] private GameObject[] characterPrefabs;
        [SerializeField] private Transform placementIndicator;
        [SerializeField] private string referenceImageName = "marker-001";

        [Tooltip("Position within the complete image, measured from its top-left corner (0 to 1).")]
        [SerializeField] private Vector2 placementPoint = new Vector2(0.75927734f, 0.75927734f);
        [SerializeField] private float surfaceOffset;
        [SerializeField] private Vector3 rotationOffset;
        [Tooltip("Multiplier applied to the prefab's existing scale.")]
        [SerializeField, Min(0.001f)] private float characterScale = 0.25f;

        private ARTrackedImage currentImage;
        private GameObject characterInstance;
        private int currentCharacterIndex = -1;
        private Quaternion spawnYaw = Quaternion.identity;

        public bool CanSpawn => isActiveAndEnabled && IsTracking(currentImage);
        public event System.Action CharacterSelected;

        private void Awake()
        {
            if (imageManager != null && characterPrefabs != null && characterPrefabs.Length > 0 && placementIndicator != null)
                return;

            Debug.LogError("Assign the image manager, character prefabs and placement indicator.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            SetPreviewVisible(false);
        }

        private void Update()
        {
            // Limited image tracking can still provide the last known marker pose.
            // Keep that target instead of discarding it when the user steps back.
            if (currentImage == null || currentImage.trackingState == TrackingState.None)
            {
                var trackedMarker = FindTrackedMarker();
                if (trackedMarker != null)
                    currentImage = trackedMarker;
            }

            if (!IsTracking(currentImage))
            {
                SetPreviewVisible(false);
                bool canKeepCharacter = characterInstance != null &&
                    imageManager.isActiveAndEnabled && currentImage != null &&
                    currentImage.trackingState == TrackingState.Limited &&
                    ARSession.state == ARSessionState.SessionTracking;

                if (canKeepCharacter)
                {
                    // ARCore assumes an unseen image stays stationary. Use its
                    // retained pose so session-coordinate corrections still apply.
                    var retainedPose = GetPlacementPose();
                    characterInstance.transform.SetPositionAndRotation(
                        retainedPose.position, retainedPose.rotation * spawnYaw);
                }
                SetCharacterVisible(canKeepCharacter);
                return;
            }

            var pose = GetPlacementPose();
            if (characterInstance == null)
            {
                // Lift the indicator slightly to avoid overlapping the marker surface.
                placementIndicator.SetPositionAndRotation(
                    pose.position + currentImage.transform.up * 0.001f, pose.rotation);
                SetPreviewVisible(true);
                return;
            }

            SetPreviewVisible(false);
            characterInstance.transform.SetPositionAndRotation(pose.position, pose.rotation * spawnYaw);
            SetCharacterVisible(true);
        }

        public void SpawnCharacter(int characterIndex)
        {
            // Recheck on click: tracking may have changed since the last preview.
            if (!CanSpawn)
                return;

            if (characterIndex < 0 || characterIndex >= characterPrefabs.Length || characterPrefabs[characterIndex] == null)
            {
                Debug.LogWarning("Choose a valid character prefab index.", this);
                return;
            }

            var characterPrefab = characterPrefabs[characterIndex];
            if (characterInstance != null && currentCharacterIndex == characterIndex)
            {
                CharacterSelected?.Invoke();
                return;
            }

            var pose = GetPlacementPose();
            if (characterInstance != null)
            {
                // Destroy is deferred; deactivate now to stop rendering, input and audio immediately.
                characterInstance.SetActive(false);
                Destroy(characterInstance);
            }

            spawnYaw = GetCameraFacingYaw(pose);
            characterInstance = Instantiate(characterPrefab, pose.position, pose.rotation * spawnYaw, transform);
            characterInstance.transform.localScale = characterPrefab.transform.localScale * characterScale;
            currentCharacterIndex = characterIndex;
            SetPreviewVisible(false);
            CharacterSelected?.Invoke();
        }

        private Quaternion GetCameraFacingYaw(Pose pose)
        {
            var camera = Camera.main;
            if (camera == null)
                return Quaternion.identity;

            // Measure heading in marker space: only rotate around the character's Y axis.
            // Ignore camera elevation so the character never tilts toward the device.
            var direction = Quaternion.Inverse(pose.rotation) * (camera.transform.position - pose.position);
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.000001f)
                return Quaternion.identity;

            float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, yaw, 0f);
        }

        private Pose GetPlacementPose()
        {
            // AR images lie in local XZ, with +Y normal to the surface and +Z
            // toward the top of the image. The point defaults to the QR centre.
            var localPosition = new Vector3(
                (placementPoint.x - 0.5f) * currentImage.size.x,
                surfaceOffset,
                (0.5f - placementPoint.y) * currentImage.size.y);

            return new Pose(
                currentImage.transform.TransformPoint(localPosition),
                currentImage.transform.rotation * Quaternion.Euler(rotationOffset));
        }

        private void SetPreviewVisible(bool visible)
        {
            if (placementIndicator != null)
                placementIndicator.gameObject.SetActive(visible);
        }

        private ARTrackedImage FindTrackedMarker()
        {
            if (!imageManager.isActiveAndEnabled)
                return null;

            foreach (var image in imageManager.trackables)
            {
                if (image.referenceImage.name == referenceImageName && IsTracking(image))
                    return image;
            }

            return null;
        }

        private bool IsTracking(ARTrackedImage image)
        {
            return imageManager != null && imageManager.isActiveAndEnabled &&
                image != null && image.trackingState == TrackingState.Tracking;
        }

        private void SetCharacterVisible(bool visible)
        {
            if (characterInstance != null && characterInstance.activeSelf != visible)
                characterInstance.SetActive(visible);
        }

        private void OnDisable()
        {
            SetPreviewVisible(false);
            SetCharacterVisible(false);
        }
    }
}
