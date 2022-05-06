using System;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    public FrameInputs Inputs;
    public PlayerController player;
    public MaskHabilities habilities;
    public PauseController PauseController;
    PlayerHook hook;

    private void Start()
    {
        if(Inputs.A == null) Inputs.A = new AbilityButtonInput();
        if(Inputs.B == null) Inputs.B = new AbilityButtonInput();
        hook = GetComponent<PlayerHook>();
    }

    private void Update() => GatherInputs();

    public void GatherInputs()
    {
        if(Input.GetButtonDown("Cancel")) PauseController.PlayerPause();

        if(!(Time.timeScale > 0)) return; // Se o jogo tiver pausado n�o coleta mais inputs.
        
        Inputs.RawX = (int)Input.GetAxisRaw("Horizontal");
        Inputs.RawY = (int)Input.GetAxisRaw("Vertical");
        Inputs.X = Input.GetAxis("Horizontal");
        Inputs.Y = Input.GetAxis("Vertical");
        Inputs.A.down = Input.GetButtonDown("AbilityA");
        Inputs.A.up = Input.GetButtonUp("AbilityA");
        Inputs.B.down = Input.GetButtonDown("AbilityB");
        Inputs.B.up = Input.GetButtonUp("AbilityB");

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
            if(hook.Traveling)
            {
                hook.Traveling = false;
                PlayerController.Instance.TimeLeftGrounded = Time.time;
                PlayerController.Instance._rb.velocity = Vector3.zero;
                PlayerController.Instance.FallImpact = false;
            }

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

    public bool GetInputDown(string name) => Inputs.A.name == name ? Inputs.A.down : Inputs.B.name == name? Inputs.B.down : false;
    public bool GetInputUp(string name) => Inputs.A.name == name ? Inputs.A.up : Inputs.B.name == name? Inputs.B.up : false;

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
        public bool down, up;
    }
}