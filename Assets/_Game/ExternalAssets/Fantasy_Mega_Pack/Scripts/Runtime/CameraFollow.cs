// CameraFollow.cs - simple smooth top-down camera follow.
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Smooth LateUpdate camera follow for the player.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smooth = 8f;
        public Vector3 offset = new Vector3(0f, 0f, -10f);

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 goal = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        }
    }
}
