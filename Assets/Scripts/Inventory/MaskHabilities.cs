using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MaskHabilities : MonoBehaviour
{
    public AbilitiesInfos AbilitiesInfos;
    public AbilityPassiveSlots Passive;
    public AbilityActiveSlots[] Actives;
    private PlayerController playerController;
    Ability tempAbilityRef;

    Item tempObjRef;

    public int chanceDash = 1, chanceMobility = 1, chanceAttack = 1, chanceHealth  = 1, chanceHook = 1, chanceTantrum = 1, chanceKnives = 1, chanceRanged = 1, chanceShield = 1;
    int dashIndex = -1, mobilityIndex = -1, attackIndex = -1, hookIndex = -1, healthIndex = -1, tantrumIndex = -1, knivesIndex = -1, rangedIndex = -1, shieldIndex = -1, lastIndex = -1;

    void Awake() 
    { 
        playerController = GetComponent<PlayerController>();
        playerController.AbilitiesController = this;
        Actives = new AbilityActiveSlots[2];
    } 

    public void NewAbilityInteraction(MaskObject obj, Item gObj)
    {
        tempAbilityRef = new Ability();
        tempObjRef = gObj;
        int activeChance = chanceDash + chanceHook + chanceKnives + chanceTantrum + chanceRanged + chanceShield;
        int passiveChance = chanceMobility + chanceAttack + chanceHealth;

        bool passive = Random.Range(0, activeChance + passiveChance) < passiveChance;

        #region Garantia de passiva se já tiver duas ativas e nenhuma passiva, garantia de ativa se já tiver passiva e slot ativo disponível
        if(Passive != AbilityPassiveSlots.None)
        {
            if(Actives[0] == AbilityActiveSlots.None || Actives[0] == AbilityActiveSlots.None) 
                passive = false;
        }
        else if(Actives[0] != AbilityActiveSlots.None && Actives[0] != AbilityActiveSlots.None && Passive == AbilityPassiveSlots.None)
                passive = true;
        #endregion

        if(activeChance + passiveChance == 0)
        {
            Debug.LogWarning("Chances únicas de habilidades zeradas.");
            return;
        }
        else if(passiveChance == 0) passive = false;
        else if(activeChance == 0) passive = true;

        if(passive)
        {
            if(Passive == AbilityPassiveSlots.None) ActivateAbility(PickSemiRandomPassive(passiveChance), -1);
            else
            {
                // Tela de confirmação
            }
        }
        else
        {
            if(Actives[0] == AbilityActiveSlots.None) ActivateAbility(PickSemiRandomActive(activeChance), 0);
            else if(Actives[1] == AbilityActiveSlots.None) ActivateAbility(PickSemiRandomActive(activeChance), 1);
            else
            {
                // Tela de Substituição
            }
        }
        
        // TODO: só chamar isso caso não precise chamar nenhuma tela de confirmação, fora isso chamar
        tempObjRef.EndInteraction(tempAbilityRef);
    }

    public void DeactivateAbility(int slot)
    {
        AbilitiesEnum tempAbility;
        Ability output = new Ability();

        if(slot == 0)
        {
            Actives[0] = AbilityActiveSlots.None;
            System.Enum.TryParse(Actives[1].ToString(), out tempAbility);
        }
        else if(slot == 1)
        {
            Actives[1] = AbilityActiveSlots.None;
            System.Enum.TryParse(Actives[0].ToString(), out tempAbility);
        }
        else
        {
            Passive = AbilityPassiveSlots.None;
            System.Enum.TryParse(Passive.ToString(), out tempAbility);
        }
        
        output.Type = tempAbility;
        output.Rank = GetAbilityRank(tempAbility);

        switch(tempAbility)
        {
            case AbilitiesEnum.Dash:
                playerController.DashRank = 0;
                break;
            case AbilitiesEnum.Mobility:
                playerController.MobilityRank = 0;
                StatsManager.Instance.MoveSpeed.Reset();
                break;
            case AbilitiesEnum.Attack:
                playerController.AttackRank = 0;
                StatsManager.Instance.Damage.Reset();
                break;
            case AbilitiesEnum.Health:
                playerController.HealthRank = 0;
                StatsManager.Instance.Health.Reset();
                RecalculateHealth();;
                break;
            case AbilitiesEnum.Hook:
                playerController.HookRank = 0;
                break;
            case AbilitiesEnum.Tantrum:
                playerController.TantrumRank = 0;
                break;
            case AbilitiesEnum.Knives:
                playerController.KnivesRank = 0;
                break;
            case AbilitiesEnum.Ranged:
                playerController.RangedRank = 0;
                break;
            case AbilitiesEnum.Shield:
                playerController.ShieldRank = 0;
                break;
        }

        tempObjRef.EndInteraction(output);
    }

    AbilitiesEnum PickSemiRandomActive(int totalChance)
    {
        int roll = Random.Range(0, totalChance);
        if(roll < chanceDash) return AbilitiesEnum.Dash;
        else if(roll < chanceDash + chanceHook) return AbilitiesEnum.Hook;
        else if(roll < chanceDash + chanceHook + chanceKnives) return AbilitiesEnum.Knives;
        else if(roll < chanceDash + chanceHook + chanceKnives + chanceTantrum) return AbilitiesEnum.Tantrum;
        else if(roll < chanceDash + chanceHook + chanceKnives + chanceTantrum + chanceRanged) return AbilitiesEnum.Ranged;
        else return AbilitiesEnum.Shield;
    }

    AbilitiesEnum PickSemiRandomPassive(int totalChance)
    {
        int roll = Random.Range(0, totalChance);
        if(roll <  chanceMobility) return AbilitiesEnum.Mobility;
        else if(roll < chanceMobility + chanceAttack) return AbilitiesEnum.Attack;
        else return AbilitiesEnum.Health;
    }

    public void UpgradeAbility(int slot)
    {
        AbilitiesEnum tempAbility;
        switch(slot)
        {
            case 0:
                System.Enum.TryParse(Actives[0].ToString(), out tempAbility);
                break;
            case 1:
                System.Enum.TryParse(Actives[1].ToString(), out tempAbility);
                break;
            default:
                System.Enum.TryParse(Passive.ToString(), out tempAbility);
                break;
        }

        ActivateAbility(tempAbility, slot);
    }

    void ShowItemInfo(int index, int rank) => StartCoroutine(ItemInfo(index, rank));

    IEnumerator ItemInfo(int index, int rank)
    {
        yield return null;
        // ????
    }

    #region Abilities Activation

    void ActivateDash(int slot) 
    { 
        if(playerController.DashRank < 1)
        {
            playerController.PlInputs.SetInput("Dash", slot == 0);
            if(slot == 0) Actives[0] = AbilityActiveSlots.Dash;
            else Actives[1] = AbilityActiveSlots.Dash;
            chanceDash = 0;
            playerController.DashRank = 1;
            if(dashIndex == -1) dashIndex = lastIndex;
        }
        else playerController.DashRank++;
        ShowItemInfo(dashIndex, playerController.DashRank);
    }

    void ActivateAttack(int slot) 
    { 
        if(playerController.AttackRank < 1)
        {
            Passive = AbilityPassiveSlots.Attack;
            chanceAttack = 0;
            playerController.AttackRank = 1;
            if(attackIndex == -1) attackIndex = lastIndex;
        }
        else playerController.AttackRank++;
        StatsManager.Instance.Damage.AddMultiplier($"Attack Rank {playerController.AttackRank}", 0.5f / playerController.AttackRank);
        ShowItemInfo(attackIndex, playerController.AttackRank);
    }

    void ActivateMobility(int slot) 
    {
        if(playerController.MobilityRank < 1)
        {
            Passive = AbilityPassiveSlots.Mobility;
            chanceMobility = 0;
            playerController.MobilityRank = 1;
            if(mobilityIndex == -1) mobilityIndex = lastIndex;
        }
        else playerController.MobilityRank++;

        StatsManager.Instance.MoveSpeed.AddMultiplier($"Mobility Rank {playerController.MobilityRank}", 0.05f);
        ShowItemInfo(mobilityIndex, playerController.MobilityRank);
    }

    void ActivateHook(int slot) 
    {
        if(playerController.HookRank < 1)
        {
            playerController.PlInputs.SetInput("Hook", slot == 0);
            if(slot == 0) Actives[0] = AbilityActiveSlots.Hook;
            else Actives[1] = AbilityActiveSlots.Hook;
            chanceHook = 0;
            playerController.HookRank = 1;
            if(hookIndex == -1) hookIndex = lastIndex;
        }
        else playerController.HookRank++;
            
        ShowItemInfo(hookIndex, playerController.HookRank);
    }

    void ActivateHealth(int slot) 
    { 
        if(playerController.HealthRank < 1)
        {
            Passive = AbilityPassiveSlots.Health;
            chanceHealth = 0;
            playerController.HealthRank = 1;
            if(healthIndex == -1) healthIndex = lastIndex;
        }
        else playerController.HealthRank++;

        RecalculateHealth();

        if(playerController.HealthRank < 3) StatsManager.Instance.Health.AddMultiplier($"Health Rank {playerController.HealthRank}", 0.3334f);
        StatsManager.Instance.KnockbackResistance.AddMultiplier($"Health Rank {playerController.HealthRank}", 0.1f * playerController.HealthRank);
        ShowItemInfo(healthIndex, playerController.HealthRank);
    }

    void RecalculateHealth()
    {
        int totalHP = playerController.TotalHealth;
        int newTotalHP = Mathf.FloorToInt(totalHP + playerController.HealthRank * 0.334f);
        if(newTotalHP > 0) playerController.ReceiveHealing(newTotalHP - totalHP);
        else playerController.ReceiveDamage(newTotalHP - totalHP, Vector3.zero);
    } 

    void ActivateTantrum(int slot) 
    { 
        if(playerController.TantrumRank < 1)
        {
            playerController.PlInputs.SetInput("Tantrum", slot == 0);
            if(slot == 0) Actives[0] = AbilityActiveSlots.Tantrum;
            else Actives[1] = AbilityActiveSlots.Tantrum;
            chanceTantrum = 0;
            if(tantrumIndex == -1) tantrumIndex = lastIndex;
        }
        else playerController.TantrumRank++;

        ShowItemInfo(tantrumIndex, playerController.TantrumRank);
    }

    void ActivateKnives(int slot) 
    { 
        if(playerController.KnivesRank < 1)
        {
            playerController.PlInputs.SetInput("Knives", slot == 0);
            if(slot == 0) Actives[0] = AbilityActiveSlots.Knives;
            else Actives[1] = AbilityActiveSlots.Knives;
            chanceKnives = 0;
            playerController.KnivesRank = 1;
            if(knivesIndex == -1) knivesIndex = lastIndex;
        }
        else playerController.KnivesRank++;

        ShowItemInfo(knivesIndex, playerController.KnivesRank);
    }

    void ActivateRanged(int slot) 
    {
        if(playerController.RangedRank < 1)
        {
            playerController.PlInputs.SetInput("Ranged", slot == 0);
            if(slot == 0) Actives[0] = AbilityActiveSlots.Ranged;
            else Actives[1] = AbilityActiveSlots.Ranged;
            chanceRanged = 0;
            playerController.RangedRank = 1;
            if(rangedIndex == -1) rangedIndex = lastIndex;
        }
        else playerController.RangedRank++;
        
        ShowItemInfo(rangedIndex, playerController.RangedRank);
    }

    void ActivateShield(int slot) 
    {
        if(playerController.ShieldRank < 1)
        {
            playerController.PlInputs.SetInput("Shield", slot == 0);
            if(slot == 0) Actives[0] = AbilityActiveSlots.Shield;
            else Actives[1] = AbilityActiveSlots.Shield;
            chanceShield = 0;
            playerController.ShieldRank = 1;
            if(shieldIndex == -1) shieldIndex = lastIndex;
        }
        else playerController.ShieldRank++;

        ShowItemInfo(shieldIndex, playerController.ShieldRank);
    }

    #endregion

    #region Utils
    int GetAbilityRank(AbilitiesEnum ability)
    {
        switch(ability)
        {
            case AbilitiesEnum.Dash: return playerController.DashRank;
            case AbilitiesEnum.Mobility: return playerController.MobilityRank;
            case AbilitiesEnum.Attack: return playerController.AttackRank;
            case AbilitiesEnum.Health: return playerController.HealthRank;
            case AbilitiesEnum.Hook: return playerController.HookRank;
            case AbilitiesEnum.Tantrum: return playerController.TantrumRank;
            case AbilitiesEnum.Knives: return playerController.KnivesRank;
            case AbilitiesEnum.Ranged: return playerController.RangedRank;
            default: return playerController.ShieldRank;
        }
    }

    void ActivateAbility(AbilitiesEnum habilities, int slot)
    {
        switch(habilities)
        {
            case AbilitiesEnum.Dash:
                ActivateDash(slot);
                break;
            case AbilitiesEnum.Mobility:
                ActivateMobility(slot);
                break;
            case AbilitiesEnum.Attack:
                ActivateAttack(slot);
                break;
            case AbilitiesEnum.Health:
                ActivateHealth(slot);
                break;
            case AbilitiesEnum.Hook:
                ActivateHook(slot);
                break;
            case AbilitiesEnum.Tantrum:
                ActivateTantrum(slot);
                break;
            case AbilitiesEnum.Knives:
                ActivateKnives(slot);
                break;
            case AbilitiesEnum.Ranged:
                ActivateRanged(slot);
                break;
            case AbilitiesEnum.Shield:
                ActivateShield(slot);
                break;
        }
    }

    #endregion
}
