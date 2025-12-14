using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Watchers")]
        public bool canControlPlayer = true;

		[Space(10)]
        [Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public float bubbleTimeScale;
        public float pushPull;
        public bool jump;
		public bool sprint;
		public bool shoot;
        public bool skill;
        public bool grab;
        public bool aim;
		public bool slowmo;
		public bool dodge;
        public bool switchWeapon;
        public bool switchSkill;
        public bool pause;
        public bool menu;
        public bool freeCamera;

        [Header("Movement Settings")]
		public bool analogMovement;

        [Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }
        public void OnBubbleTimeScale(InputValue value)
        {
            BubbleTimeScale(value.Get<float>());
        }
        public void OnPushPull(InputValue value)
        {
            PushPull(value.Get<float>());
        }

        public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
		
		public void OnShoot(InputValue value)
		{
			ShootInput(shoot = value.isPressed);
        }

        public void OnSkill(InputValue value)
        {
            SkillInput(value.isPressed);
        }

        public void OnAim(InputValue value)
		{
			AimInput(value.isPressed);
		}

		public void OnSlowMo(InputValue value)
		{
			SlowMoInput(value.isPressed);
		}

		public void OnDodge(InputValue value)
		{
			DodgeInput(value.isPressed);
		}

		public void OnPause(InputValue value)
		{
			PauseInput(value.isPressed);
        }

        public void OnSwitchWeapon(InputValue value)
        {
            SwitchWeaponInput(value.isPressed);
        }

        public void OnSwitchSkill(InputValue value)
        {
            SwitchSkillInput(value.isPressed);
        }

        public void OnMenu(InputValue value)
        {
            MenuInput(value.isPressed);
        }

        public void OnFreeCamera(InputValue value)
        {
            FreeCameraInput(value.isPressed);
        }

        public void OnGrab(InputValue value)
        {
            GrabInput(value.isPressed);
        }
#endif


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
        }
        public void BubbleTimeScale(float direction)
		{
			bubbleTimeScale = direction == 0f ? 0f : Mathf.Sign(direction);
        }

        private void PushPull(float direction)
        {
            pushPull = direction;
        }

        public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		
		private void ShootInput(bool newShootState)
		{
			shoot = newShootState;
		}
		
		private void AimInput(bool newAimState)
		{
			aim = newAimState;
		}

		private void SlowMoInput(bool valueIsPressed)
        {
			slowmo = valueIsPressed;
        }
		
		private void DodgeInput(bool valueIsPressed)
		{
			dodge = valueIsPressed;
		}
		
		private void PauseInput(bool valueIsPressed)
		{
			pause = valueIsPressed;
		}
		
		private void SwitchWeaponInput(bool valueIsPressed)
		{
			switchWeapon = valueIsPressed;
        }

        private void SwitchSkillInput(bool isPressed)
        {
			switchSkill = isPressed;
        }

        private void SkillInput(bool valueIsPressed)
		{
			skill = valueIsPressed;
        }

        private void MenuInput(bool valueIsPressed)
        {
            menu = valueIsPressed;
        }

        private void FreeCameraInput(bool valueIsPressed)
        {
            freeCamera = valueIsPressed;
        }

        private void GrabInput(bool valueIsPressed)
        {
            grab = valueIsPressed;
        }

        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorLockedState(cursorLocked);
		}

		public void SetCursorLockedState(bool newState)
		{
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
            cursorLocked = newState;
		}

        internal void Reset()
        {
            throw new NotImplementedException();
        }
    }
	
}
