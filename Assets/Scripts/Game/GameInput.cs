using GauntletRun.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GauntletRun.Game
{
    /// <summary>
    /// Controls from the Input System: keyboard (A/D or arrows, W/Space/Up, S/Down/Ctrl), gamepad
    /// (stick or D-pad, A to jump, B to slide) and swipes on a touch screen or with the mouse.
    /// </summary>
    public sealed class GameInput
    {
        const float StickThreshold = 0.6f;
        /// <summary>A swipe must travel this share of the screen height.</summary>
        const float SwipeShare = 0.06f;

        Vector2 stickLast;
        bool swiping;
        bool swiped;
        Vector2 swipeStart;

        static Keyboard Keys => Keyboard.current;
        static Gamepad Pad => Gamepad.current;

        /// <summary>The run input for this frame (one-shot presses).</summary>
        public RunInput Read()
        {
            var input = new RunInput();
            if (Keys != null)
            {
                if (Keys.aKey.wasPressedThisFrame || Keys.leftArrowKey.wasPressedThisFrame) input.Steer = -1;
                if (Keys.dKey.wasPressedThisFrame || Keys.rightArrowKey.wasPressedThisFrame) input.Steer = 1;
                input.Jump |= Keys.wKey.wasPressedThisFrame || Keys.upArrowKey.wasPressedThisFrame || Keys.spaceKey.wasPressedThisFrame;
                input.Slide |= Keys.sKey.wasPressedThisFrame || Keys.downArrowKey.wasPressedThisFrame || Keys.leftCtrlKey.wasPressedThisFrame;
            }
            if (Pad != null)
            {
                if (Pad.dpad.left.wasPressedThisFrame) input.Steer = -1;
                if (Pad.dpad.right.wasPressedThisFrame) input.Steer = 1;
                input.Jump |= Pad.buttonSouth.wasPressedThisFrame || Pad.dpad.up.wasPressedThisFrame;
                input.Slide |= Pad.buttonEast.wasPressedThisFrame || Pad.dpad.down.wasPressedThisFrame;
                Vector2 stick = Pad.leftStick.ReadValue();
                if (stick.x < -StickThreshold && stickLast.x >= -StickThreshold) input.Steer = -1;
                if (stick.x > StickThreshold && stickLast.x <= StickThreshold) input.Steer = 1;
                if (stick.y > StickThreshold && stickLast.y <= StickThreshold) input.Jump = true;
                if (stick.y < -StickThreshold && stickLast.y >= -StickThreshold) input.Slide = true;
                stickLast = stick;
            }
            ReadSwipe(ref input);
            return input;
        }

        void ReadSwipe(ref RunInput input)
        {
            bool pressed;
            Vector2 position;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                pressed = true;
                position = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null)
            {
                pressed = Mouse.current.leftButton.isPressed;
                position = Mouse.current.position.ReadValue();
            }
            else
            {
                return;
            }
            if (!pressed)
            {
                swiping = false;
                return;
            }
            if (!swiping)
            {
                swiping = true;
                swiped = false;
                swipeStart = position;
                return;
            }
            if (swiped)
            {
                return;
            }
            Vector2 delta = position - swipeStart;
            float needed = Mathf.Max(30f, UnityEngine.Screen.height * SwipeShare);
            if (delta.magnitude < needed)
            {
                return;
            }
            swiped = true;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                input.Steer = delta.x > 0f ? 1 : -1;
            }
            else if (delta.y > 0f)
            {
                input.Jump = true;
            }
            else
            {
                input.Slide = true;
            }
        }

        public bool Pause =>
            (Keys != null && (Keys.escapeKey.wasPressedThisFrame || Keys.pKey.wasPressedThisFrame)) ||
            (Pad != null && Pad.startButton.wasPressedThisFrame);

        /// <summary>The main choice of a menu: Enter or Space, or the gamepad's A.</summary>
        public bool Confirm =>
            (Keys != null && (Keys.enterKey.wasPressedThisFrame || Keys.numpadEnterKey.wasPressedThisFrame || Keys.spaceKey.wasPressedThisFrame)) ||
            (Pad != null && (Pad.buttonSouth.wasPressedThisFrame || Pad.startButton.wasPressedThisFrame));

        /// <summary>Going back from a menu: Escape or Backspace, or the gamepad's B.</summary>
        public bool Back =>
            (Keys != null && (Keys.escapeKey.wasPressedThisFrame || Keys.backspaceKey.wasPressedThisFrame)) ||
            (Pad != null && Pad.buttonEast.wasPressedThisFrame);

        public bool Restart => (Keys != null && Keys.rKey.wasPressedThisFrame) || (Pad != null && Pad.buttonNorth.wasPressedThisFrame);

        public bool Shop => (Keys != null && Keys.tabKey.wasPressedThisFrame) || (Pad != null && Pad.buttonWest.wasPressedThisFrame);

        /// <summary>-1 or +1 when a left or right step through a menu was pressed this frame.</summary>
        public int Navigate()
        {
            int step = 0;
            if (Keys != null)
            {
                if (Keys.aKey.wasPressedThisFrame || Keys.leftArrowKey.wasPressedThisFrame) step = -1;
                if (Keys.dKey.wasPressedThisFrame || Keys.rightArrowKey.wasPressedThisFrame) step = 1;
            }
            if (Pad != null)
            {
                if (Pad.dpad.left.wasPressedThisFrame) step = -1;
                if (Pad.dpad.right.wasPressedThisFrame) step = 1;
            }
            return step;
        }
    }
}
