using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace ARApp
{
    /// Allows placement only when the previously spawned object has been destroyed.
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ObjectSpawner))]
    public sealed class SingleInstancePlacement : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Spawner responsible for creating and positioning the object.")]
        private ObjectSpawner objectSpawner;

        private GameObject currentInstance;

        private void Reset()
        {
            objectSpawner = GetComponent<ObjectSpawner>();
        }

        private void Awake()
        {
            if (objectSpawner == null)
                objectSpawner = GetComponent<ObjectSpawner>();
        }

        private void OnEnable()
        {
            objectSpawner.objectSpawned += OnObjectSpawned;
        }

        private void OnDisable()
        {
            if (objectSpawner != null)
                objectSpawner.objectSpawned -= OnObjectSpawned;
        }

        /// Receives the surface position and normal from ARInteractorSpawnTrigger.
        public void RequestSpawn(Vector3 position, Vector3 normal)
        {
            if (!isActiveAndEnabled)
                return;

            // Unity treats a destroyed object as null. Hiding or disabling it
            // does not free the slot, but the existing Clear All Objects action does.
            if (currentInstance != null)
                return;

            // The success event records the instance synchronously. A failed
            // attempt leaves the slot available for the next placement request.
            objectSpawner.TrySpawnObject(position, normal);
        }

        private void OnObjectSpawned(GameObject spawnedObject)
        {
            currentInstance = spawnedObject;
        }
    }
}
