using UnityEngine;

namespace GameLogic.FishingComponent
{
    public class FishHook : MonoBehaviour
    {
        public void HorizontalMovement(Vector2 delta)
        {
            var pos = transform.localPosition;
            pos.x += delta.x;
            pos.x = Mathf.Clamp(pos.x, -2.5f, 2.5f);
            transform.localPosition = pos;
        }
    }
}