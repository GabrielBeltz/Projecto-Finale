using System;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    public FrameInputs Inputs;
    public PlayerController player;
    public MaskHabilities habilities;
    public PauseController PauseController;
    public bool CanMove = true;
    PlayerHook hook;
    bool HeldA, HeldB;

    private void Start()
    {
        if(Inputs.A == null) Inputs.A = new AbilityButtonInput();
        if(Inputs.B == null) Inputs.B = new AbilityButtonInput();
        hook = GetComponent<PlayerHook>();
        PlayerController.Instance.OnPlayerDeath += ResetInputs;
    }

    private void Update() => GatherInputs();

    public void GatherInputs()
    {
        if(Input.GetButtonDown("Cancel")) PauseController.PlayerPause();

        if(Time.timeScale == 0)
        {
            HeldA = HeldA || Input.GetButtonUp("AbilityA") && !CanMove;
            HeldB = HeldB || Input.GetButtonUp("AbilityB") && !CanMove;
        }
        else // Se o jogo tiver pausado não coleta mais inputs.
        {
            Inputs.RawX = CanMove? (int)Input.GetAxisRaw("Horizontal") : 0;
            Inputs.RawY = CanMove? (int)Input.GetAxisRaw("Vertical") : 0;
            Inputs.X = CanMove? Input.GetAxis("Horizontal") : 0;
            Inputs.Y = CanMove? Input.GetAxis("Vertical") : 0;
            Inputs.A.down = CanMove? Input.GetButtonDown("AbilityA") : false;
            Inputs.A.up = CanMove? Input.GetButtonUp("AbilityA") : false;
            Inputs.B.down = CanMove? Input.GetButtonDown("AbilityB") : false;
            Inputs.B.up = CanMove? Input.GetButtonUp("AbilityB") : false;

            if(HeldA) Inputs.A.up = true;
            if(HeldB) Inputs.B.up = true;
            HeldA = false;
            HeldB = false;

            player.WallOnRight &= Inputs.RawX > 0;
            player.WallOnLeft &= Inputs.RawX < 0;
            player.OnWall = player.WallOnLeft || player.WallOnRight;
            player.OnWall &= player.GripTimer > 0.5f;

            if(player.CurrentHealth > 0 && Inputs.RawX != 0 && !player.IsKnockbacked) player.MyAnimator.SetBool("FellDown", false);

            if(player.IsKnockbacked || player.MyAnimator.GetBool("FellDown")) return;

            if(Input.GetButtonDown("Fire1") && CanMove ) player.ExecuteAttack();
            if(Input.GetButtonDown("Submit") ) player.ExecuteInteraction();

            if(Input.GetButtonDown("Jump") && CanMove)
            {
                if(hook.Traveling) hook.UnnatachHook(true);
                else if(player.OnWall)
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
    }
    
    public void SetInput(string name, bool AbilityA)
    {     
        if(AbilityA)Inputs.A.name = name;
        else Inputs.B.name = name;
    }

    void ResetInputs()
    {
        SetInput("", true);
        SetInput("", false);
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