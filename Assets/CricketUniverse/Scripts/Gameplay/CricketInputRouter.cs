using UnityEngine.InputSystem;

namespace CricketUniverse.Gameplay
{
    /// <summary>Small device adapter: the match controller consumes actions, never device details.</summary>
    internal static class CricketInputRouter
    {
        public static bool BowlPressed => Keyboard.current?.enterKey.wasPressedThisFrame == true
            || Gamepad.current?.startButton.wasPressedThisFrame == true;

        public static bool ShotPressed => Keyboard.current?.spaceKey.wasPressedThisFrame == true
            || Gamepad.current?.buttonSouth.wasPressedThisFrame == true;

        public static bool LoftPressed => Keyboard.current?.leftShiftKey.wasPressedThisFrame == true
            || Gamepad.current?.rightShoulder.wasPressedThisFrame == true;

        public static float MoveAxis
        {
            get
            {
                float keyboard = (Keyboard.current?.dKey.isPressed == true ? 1f : 0f)
                    - (Keyboard.current?.aKey.isPressed == true ? 1f : 0f);
                float stick = Gamepad.current?.leftStick.x.ReadValue() ?? 0f;
                return UnityEngine.Mathf.Abs(keyboard) > 0f ? keyboard : stick;
            }
        }

        public static float AimAxis
        {
            get
            {
                float keyboard = (Keyboard.current?.rightArrowKey.wasPressedThisFrame == true ? 1f : 0f)
                    - (Keyboard.current?.leftArrowKey.wasPressedThisFrame == true ? 1f : 0f);
                float pad = Gamepad.current?.dpad.x.ReadValue() ?? 0f;
                if (UnityEngine.Mathf.Abs(keyboard) > 0f) return keyboard;
                if (UnityEngine.Mathf.Abs(pad) > 0.6f) return UnityEngine.Mathf.Sign(pad);
                return 0f;
            }
        }
    }
}
