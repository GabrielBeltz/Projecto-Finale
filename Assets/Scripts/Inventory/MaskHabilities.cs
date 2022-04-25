using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MaskHabilities : MonoBehaviour
{
    public AbilityPassiveSlots Passive;
    public AbilityActiveSlots[] Actives;
    private PlayerController playerController;
    public Image[] images;
    // nesse valor todos os upgrades começam com 100% de chance, e essa chance diminui em 34% a cada rank pego.
    [Range(0, 0.45f)]public float DeramdomizeFactor = 0.34f; 
    float chanceDash = 1, chanceMobility = 1, chanceAttack = 1, chanceHealth  = 1, chanceHook = 1, chanceTantrum = 1, chanceKnives = 1, chanceBoomerang = 1, chanceShield = 1;
    int dashIndex = -1, jumpIndex = -1, attackIndex = -1, hookIndex = -1, healthIndex = -1, tantrumIndex = -1, knivesIndex = -1, rangedIndex = -1, shieldIndex = -1, lastIndex = -1;

    void Awake() 
    { 
        playerController = GetComponent<PlayerController>();
        Actives = new AbilityActiveSlots[2];
    } 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item;
        if(!collision.gameObject.TryGetComponent<Item>(out item)) return;
        if(item.item is MaskObject) 
        {
            var a = item.item as MaskObject;
            switch(a.habilities)
            {
                case Habilities.Random:
                    PickRandomly();
                    break;
                case Habilities.Upgrade:
                    UpgradeAbility();
                    break;
                case Habilities.Dash:
                    ActivateDash();
                    break;
                case Habilities.Jump:
                    ActivateJump();
                    break;
                case Habilities.Attack:
                    ActivateAttack();
                    break;
                case Habilities.Health:
                    ActivateHealth();
                    break;
                case Habilities.Hook:
                    ActivateHook();
                    break;
                case Habilities.Tantrum:
                    ActivateTantrum();
                    break;
                case Habilities.Knives:
                    ActivateKnives();
                    break;
                case Habilities.Boomerang:
                    ActivateRanged();
                    break;
                case Habilities.Shield:
                    ActivateShield();
                    break;
            }
        }

        item.Deactivate();
    }

    // Partes comentadas não estão implementadas ainda
    void PickRandomly()
    {
        float totalChance = chanceDash + chanceMobility + chanceAttack; // + chanceHook + chanceKnives + chanceTantrum + chanceBoomerang + chanceShield;
        float roll = Random.Range(0, totalChance);
        if(roll < chanceDash) ActivateDash();
        else if(roll < chanceDash + chanceMobility) ActivateJump();
        else ActivateAttack();
        //else if(roll < chanceDash + chanceDoubleJump + chanceAttack) ActivateAttack();
        //else if(roll < chanceDash + chanceDoubleJump + chanceAttack + chanceHook) ActivateHook();
        //else if(roll < chanceDash + chanceDoubleJump + chanceAttack + chanceHook + chanceKnives) ActivateKnives();
        //else if(roll < chanceDash + chanceDoubleJump + chanceAttack + chanceHook + chanceKnives + chanceTantrum) ActivateTantrum();
        //else if(roll < chanceDash + chanceDoubleJump + chanceAttack + chanceHook + chanceKnives + chanceTantrum + chanceBoomerang) ActivateTantrum();
        //else ActivateShield();
    }

    void UpgradeAbility()
    {

    }

    void ShowItemInfo(string ID, int index) => StartCoroutine(ItemInfo(ID, index));

    IEnumerator ItemInfo(string ID, int index)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0;
        if(index == -1) 
        {
            index = lastIndex + 1; 
        }
        if(lastIndex < index) lastIndex = index;
        //images[index].sprite = text;

        // TODO: Criar animação do sprite aparecer no centro da tela, escurecer a tela no fundo do sprite e transicionar pra posição original 
        //for(int i = 0; i < 300; i++)
        //{
            
            yield return new WaitForSecondsRealtime(0.01f);
        //}

        Time.timeScale = originalTimeScale;
    }

    void ActivateDash() 
    { 
        if(playerController.DashRank < 1)
        {
            playerController.DashRank = 1;
            ShowItemInfo("Dash", dashIndex);
            if(dashIndex == -1) dashIndex = lastIndex;
        }
        else
        {
            //upgrade;
        }
    }

    void ActivateAttack() 
    { 
        if(playerController.AttackRank < 1)
        {
            playerController.AttackRank = 1;
            ShowItemInfo("Attack", attackIndex);
            if(attackIndex == -1) attackIndex = lastIndex;
        }
        else
        {
            //upgrade;
        }
        StatsManager.Instance.AddDamageMultiplier(0.5f / playerController.AttackRank, $"Attack Rank {playerController.AttackRank}");
    }

    void ActivateJump() 
    {
        if(playerController.AttackRank < 1)
        {
            playerController.JumpRank = 1;
            ShowItemInfo("Mobility", jumpIndex);
            if(jumpIndex == -1) jumpIndex = lastIndex;
        }
        else
        {
            //upgrade;
        }
    }

    void ActivateHook() 
    {
        if(playerController.AttackRank < 1)
        {
            playerController.HookRank = 1;
            ShowItemInfo("Hook", hookIndex);
            if(hookIndex == -1) hookIndex = lastIndex;
        }
        else
        {
            //upgrade;
        }
    }

    void ActivateHealth() 
    { 
        if(playerController.AttackRank < 1)
        {
            playerController.HookRank = 1;
            ShowItemInfo("Health", healthIndex);
            if(healthIndex == -1) healthIndex = lastIndex;
        }
        else
        {
            //upgrade;
        }
    }

    void ActivateTantrum() 
    { 
        if(playerController.AttackRank < 1)
        {
            playerController.TantrumRank = 1;
            ShowItemInfo("Tantrum", tantrumIndex);
            if(tantrumIndex == -1) tantrumIndex = lastIndex;
        }
        else
        {
            //upgrade;
        }
    }

    void ActivateKnives() 
    { 
        if(playerController.AttackRank < 1)
        {
            playerController.KnivesRank = 1;
            ShowItemInfo("Knives", knivesIndex);
            if(knivesIndex == -1) knivesIndex = lastIndex;
        }
        else
        {
            //upgrade;
        }
    }

    void ActivateRanged() 
    { 
        if(playerController.AttackRank < 1)
        {
            playerController.BoomerangRank = 1;
            ShowItemInfo("Ranged", rangedIndex);
            if(rangedIndex == -1) rangedIndex = lastIndex;
        }
        else
        {
            //upgrade;
        }
    }

    void ActivateShield() 
    { 
        if(playerController.AttackRank < 1)
        {
            playerController.ShieldRank = 1;
            ShowItemInfo("Shield", shieldIndex);
            if(shieldIndex == -1) shieldIndex = lastIndex;
        }
        else
        {
            //upgrade;
        }
    }

    public enum AbilityPassiveSlots { None, Mobility, Attack, Health }
    public enum AbilityActiveSlots { None, Dash, Hook, Tantrum, Knives, Shield, Ranged }
}
