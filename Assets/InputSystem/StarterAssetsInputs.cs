using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool shoot;
		public bool aim;
		public bool reload;
		public bool slowmo;
		public bool dodge;
		public bool pause;
		public bool switchWeapon;
		public bool aimGrenade;
		public bool restart;

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

		public void OnAim(InputValue value)
		{
			AimInput(value.isPressed);
		}

		public void OnReload(InputValue value)
		{
			ReloadInput(value.isPressed);
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

		public void OnAimGrenade(InputValue value)
		{
			AimGrenadeInput(value.isPressed);
		}
		
		public void OnRestart(InputValue value)
		{
			RestartInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
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
		
		private void ReloadInput(bool valueIsPressed)
		{
			reload = valueIsPressed;
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

		private void AimGrenadeInput(bool valueIsPressed)
		{
			aimGrenade = valueIsPressed;
		}
		
		private void RestartInput(bool valueIsPressed)
		{
			restart = valueIsPressed;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}