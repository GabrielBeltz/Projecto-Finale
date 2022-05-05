using System;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    public FrameInputs Inputs;
    public PlayerController player;
    public MaskHabilities habilities;
    public PauseController PauseController;
    public Action CallAbilityA, CallAbilityB;

    private void Start()
    {
        Inputs.A = new AbilityButtonInput();
        Inputs.B = new AbilityButtonInput();
    }

    public void GatherUnpausedInputs()
    {
        if(Input.GetButtonDown("Cancel")) PauseController.PlayerPause();
    }

    public void GatherInputs()
    {
        Inputs.RawX = (int)Input.GetAxisRaw("Horizontal");
        Inputs.RawY = (int)Input.GetAxisRaw("Vertical");
        Inputs.X = Input.GetAxis("Horizontal");
        Inputs.Y = Input.GetAxis("Vertical");
        if(Input.GetButtonDown("AbilityA")) 
        {
            CallAbilityA?.Invoke();
            Inputs.A.active = true;
        }
        else Inputs.A.active = false;
        if(Input.GetButtonDown("AbilityB")) 
        {
            CallAbilityB?.Invoke();
            Inputs.B.active = true;
        }
        else Inputs.B.active = false;

        player.WallOnRight &= Inputs.RawX > 0;
        player.WallOnLeft &= Inputs.RawX < 0;
        player.OnWall = player.WallOnLeft || player.WallOnRight;
        player.OnWall &= player.GripTimer > 0.5f;

        if(player.CurrentHealth > 0 && Inputs.RawX != 0 && !player.IsKnockbacked) player.MyAnimator.SetBool("FellDown", false);
        if(player.IsKnockbacked || player.MyAnimator.GetBool("FellDown")) return;
        if(Inputs.X != 0) player.SetFacingDirection(Inputs.X < 0);
        if(Input.GetButtonDown("Fire1")) player.ExecuteAttack();
        if(Input.GetButtonDown("Submit")) player.ExecuteInteraction();

        if(Input.GetButtonDown("Jump"))
        {
            if(player.OnWall)
            {
                player.ExecuteJump(true);
                return;
            }
            else if(!player.HasJumped)
            {

                if(player.IsGrounded) player.ExecuteJump(false);
                else if(Time.time < player.TimeLeftGrounded + player._coyoteTime) player.ExecuteJump(true);
                else if(player.DoubleJumpCharged)
                {
                    player.DoubleJumpCharged = false;
                    player.ExecuteJump(true);
                }
            }
        }
    }
    
    public void SetInput(string name, bool AbilityA)
    {     
        if(AbilityA)Inputs.A.name = name;
        else Inputs.B.name = name;
    }

    public bool GetInput(string name) => Inputs.A.name == name ? Inputs.A.active : Inputs.B.name == name? Inputs.B.active : false;

    [System.Serializable]
    public struct FrameInputs
    {
        public float X, Y;
        public int RawX, RawY;
        public AbilityButtonInput A, B;
    }

    [System.Serializable]
    public class AbilityButtonInput
    {
        public string name;
        public bool active;
    }
}