using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MaskHabilities : MonoBehaviour
{
    public AbilityPassiveSlots Passive;
    public AbilityActiveSlots ActiveA, ActiveB;

    public Chances SpawnChances;

    [Header("References")]
    public AbilitiesInfos AbilitiesInfos;
    private PlayerController playerController;
    public SwitchActivesMenu SwitchActivesMenu;
    public SwitchPassiveMenu SwitchPassiveMenu;
    public UpgradeAbilitiesMenu UpgradeAbilitiesMenu;
    Ability tempAbilityRef;

    Item tempObjRef;

    int dashIndex = -1, mobilityIndex = -1, attackIndex = -1, hookIndex = -1, healthIndex = -1, tantrumIndex = -1, knivesIndex = -1, rangedIndex = -1, shieldIndex = -1, lastIndex = -1;

    void Awake() 
    { 
        playerController = GetComponent<PlayerController>();
        playerController.AbilitiesController = this;
    } 

    public Ability GetRandomAbility()
    {
        Ability temp = new Ability();
        int activeChance = SpawnChances.Dash + SpawnChances.Hook + SpawnChances.Knives + SpawnChances.Tantrum + SpawnChances.Ranged + SpawnChances.Shield;
        int passiveChance = SpawnChances.Mobility + SpawnChances.Attack + SpawnChances.Health;

        bool passive = Random.Range(0, activeChance + passiveChance) < passiveChance;

        #region Garantia de passiva se já tiver duas ativas e nenhuma passiva, garantia de ativa se já tiver passiva e slot ativo disponível
        if(Passive != AbilityPassiveSlots.None)
        {
            if(ActiveA == AbilityActiveSlots.None || ActiveB == AbilityActiveSlots.None) 
                passive = false;
        }
        else if(ActiveA != AbilityActiveSlots.None && ActiveB != AbilityActiveSlots.None && Passive == AbilityPassiveSlots.None)
                passive = true;
        #endregion

        if(activeChance + passiveChance == 0)
        {
            return temp;
        }
        else if(passiveChance == 0) passive = false;
        else if(activeChance == 0) passive = true;

        temp.Type = passive ? PickSemiRandomPassive(passiveChance) : PickSemiRandomActive(activeChance);
        temp.Rank = 1;
        return temp;
    }

    public void NewAbilityInteraction(Item gObj)
    {
        tempAbilityRef = new Ability();
        tempObjRef = gObj;

        bool isPassive = gObj.assignedAbility.Type == AbilitiesEnum.Attack;
        isPassive |= gObj.assignedAbility.Type ==  AbilitiesEnum.Health;
        isPassive |= gObj.assignedAbility.Type ==  AbilitiesEnum.Mobility;

        if(isPassive)
        {
            if(Passive == AbilityPassiveSlots.None) ActivateAbility(gObj.assignedAbility.Type, -1);
            else
            {
                SwitchPassiveMenu.Activate(
                    AbilitiesInfos.GetFullInfo(Passive.ToString()),
                    GetAbilityRank((AbilitiesEnum)System.Enum.Parse(typeof(AbilitiesEnum), Passive.ToString())),
                    AbilitiesInfos.GetFullInfo(gObj.assignedAbility.Type.ToString()),
                    gObj.assignedAbility);
                SwitchPassiveMenu.gameObject.SetActive(true);
                tempAbilityRef = gObj.assignedAbility;
            }
        }
        else
        {
            if(ActiveA == AbilityActiveSlots.None) ActivateAbility(gObj.assignedAbility.Type, 0);
            else if(ActiveB == AbilityActiveSlots.None) ActivateAbility(gObj.assignedAbility.Type, 1);
            else
            {
                SwitchActivesMenu.Activate(
                    AbilitiesInfos.GetFullInfo(ActiveA.ToString()),
                    GetAbilityRank((AbilitiesEnum)System.Enum.Parse(typeof(AbilitiesEnum), ActiveA.ToString())),
                    AbilitiesInfos.GetFullInfo(ActiveB.ToString()),
                    GetAbilityRank((AbilitiesEnum)System.Enum.Parse(typeof(AbilitiesEnum), ActiveB.ToString())),
                    AbilitiesInfos.GetFullInfo(gObj.assignedAbility.Type.ToString()),
                    gObj.assignedAbility);
                SwitchActivesMenu.gameObject.SetActive(true);
                tempAbilityRef = gObj.assignedAbility;
            }
        }
        
        tempObjRef.EndInteraction(tempAbilityRef);
    }

    public void DeactivateAbility(int slot)
    {
        AbilitiesEnum tempAbility;
        Ability output = new Ability();

        if(slot == 0)
        {
            System.Enum.TryParse(ActiveA.ToString(), out tempAbility);
            ActiveA = AbilityActiveSlots.None;
        }
        else if(slot == 1)
        {
            System.Enum.TryParse(ActiveB.ToString(), out tempAbility);
            ActiveB = AbilityActiveSlots.None;
        }
        else
        {
            System.Enum.TryParse(Passive.ToString(), out tempAbility);
            Passive = AbilityPassiveSlots.None;
        }
        
        output.Type = tempAbility;
        output.Rank = GetAbilityRank(tempAbility);

        switch(tempAbility)
        {
            case AbilitiesEnum.Dash:
                playerController.AbilityRanks.DashRank = 0;
                break;
            case AbilitiesEnum.Mobility:
                playerController.AbilityRanks.MobilityRank = 0;
                StatsManager.Instance.MoveSpeed.Reset();
                break;
            case AbilitiesEnum.Attack:
                playerController.AbilityRanks.AttackRank = 0;
                StatsManager.Instance.Damage.Reset();
                break;
            case AbilitiesEnum.Health:
                playerController.AbilityRanks.HealthRank = 0;
                int oldHealth = playerController.ModdedTotalHealth;
                StatsManager.Instance.Health.Reset();
                StatsManager.Instance.KnockbackResistance.Reset();
                RecalculateHealth(oldHealth);
                break;
            case AbilitiesEnum.Hook:
                playerController.AbilityRanks.HookRank = 0;
                break;
            case AbilitiesEnum.Tantrum:
                playerController.AbilityRanks.TantrumRank = 0;
                break;
            case AbilitiesEnum.Knives:
                playerController.AbilityRanks.KnivesRank = 0;
                break;
            case AbilitiesEnum.Ranged:
                playerController.AbilityRanks.RangedRank = 0;
                break;
            case AbilitiesEnum.Shield:
                playerController.AbilityRanks.ShieldRank = 0;
                break;
        }

        tempObjRef.EndInteraction(output);
    }

    AbilitiesEnum PickSemiRandomActive(int totalChance)
    {
        int roll = Random.Range(0, totalChance);
        if(roll < SpawnChances.Dash)
        {
            SpawnChances.Dash = 0;
            return AbilitiesEnum.Dash;
        }
        else if(roll < SpawnChances.Dash + SpawnChances.Hook)
        {
            SpawnChances.Hook = 0;
            return AbilitiesEnum.Hook;
        }
        else if(roll < SpawnChances.Dash + SpawnChances.Hook + SpawnChances.Knives)
        {
            SpawnChances.Knives = 0;
            return AbilitiesEnum.Knives;
        }
        else if(roll < SpawnChances.Dash + SpawnChances.Hook + SpawnChances.Knives + SpawnChances.Tantrum) 
        {
            SpawnChances.Tantrum = 0;
            return AbilitiesEnum.Tantrum;
        }
        else if(roll < SpawnChances.Dash + SpawnChances.Hook + SpawnChances.Knives + SpawnChances.Tantrum + SpawnChances.Ranged) 
        {
            SpawnChances.Ranged = 0;
            return AbilitiesEnum.Ranged;
        }

        SpawnChances.Shield = 0;
        return AbilitiesEnum.Shield;
    }

    AbilitiesEnum PickSemiRandomPassive(int totalChance)
    {
        int roll = Random.Range(0, totalChance);
        if(roll <  SpawnChances.Mobility) 
        { 
            SpawnChances.Mobility = 0;
            return AbilitiesEnum.Mobility;
        }
        else if(roll < SpawnChances.Mobility + SpawnChances.Attack) 
        { 
            SpawnChances.Attack = 0;
            return AbilitiesEnum.Attack;
        }

        SpawnChances.Health = 0;
        return AbilitiesEnum.Health;
    }

    public void ActivateAbility(Ability ability, int slot)
    {
        for(int i = 0; i < ability.Rank; i++)
        {
            ActivateAbility(ability.Type, slot);
        }
    }

    public void UpgradeInteraction()
    {
        UpgradeAbilitiesMenu.Activate(
                    AbilitiesInfos.GetFullInfo(ActiveA.ToString()),
                    GetAbilityRank((AbilitiesEnum)System.Enum.Parse(typeof(AbilitiesEnum), ActiveA.ToString())),
                    AbilitiesInfos.GetFullInfo(ActiveB.ToString()),
                    GetAbilityRank((AbilitiesEnum)System.Enum.Parse(typeof(AbilitiesEnum), ActiveB.ToString())),
                    AbilitiesInfos.GetFullInfo(Passive.ToString()),
                    GetAbilityRank((AbilitiesEnum)System.Enum.Parse(typeof(AbilitiesEnum), Passive.ToString()))
            );

        UpgradeAbilitiesMenu.gameObject.SetActive(true);
    }

    public void UpgradeAbility(int slot)
    {
        AbilitiesEnum tempAbility;
        switch(slot)
        {
            case 0:
                System.Enum.TryParse(ActiveA.ToString(), out tempAbility);
                break;
            case 1:
                System.Enum.TryParse(ActiveB.ToString(), out tempAbility);
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
        if(playerController.AbilityRanks.DashRank < 1)
        {
            playerController.PlInputs.SetInput("Dash", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Dash;
            else ActiveB = AbilityActiveSlots.Dash;
            playerController.AbilityRanks.DashRank = 1;
            if(dashIndex == -1) dashIndex = lastIndex;
        }
        else playerController.AbilityRanks.DashRank++;
        ShowItemInfo(dashIndex, playerController.AbilityRanks.DashRank);
    }

    void ActivateAttack(int slot) 
    { 
        if(playerController.AbilityRanks.AttackRank < 1)
        {
            Passive = AbilityPassiveSlots.Attack;
            playerController.AbilityRanks.AttackRank = 1;
            if(attackIndex == -1) attackIndex = lastIndex;
        }
        else playerController.AbilityRanks.AttackRank++;
        StatsManager.Instance.Damage.AddMultiplier($"Attack Rank {playerController.AbilityRanks.AttackRank}", 0.5f / playerController.AbilityRanks.AttackRank);
        ShowItemInfo(attackIndex, playerController.AbilityRanks.AttackRank);
    }

    void ActivateMobility(int slot) 
    {
        if(playerController.AbilityRanks.MobilityRank < 1)
        {
            Passive = AbilityPassiveSlots.Mobility;
            playerController.AbilityRanks.MobilityRank = 1;
            if(mobilityIndex == -1) mobilityIndex = lastIndex;
        }
        else playerController.AbilityRanks.MobilityRank++;

        StatsManager.Instance.MoveSpeed.AddMultiplier($"Mobility Rank {playerController.AbilityRanks.MobilityRank}", 0.05f);
        ShowItemInfo(mobilityIndex, playerController.AbilityRanks.MobilityRank);
    }

    void ActivateHook(int slot) 
    {
        if(playerController.AbilityRanks.HookRank < 1)
        {
            playerController.PlInputs.SetInput("Hook", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Hook;
            else ActiveB = AbilityActiveSlots.Hook;
            playerController.AbilityRanks.HookRank = 1;
            if(hookIndex == -1) hookIndex = lastIndex;
        }
        else playerController.AbilityRanks.HookRank++;
            
        ShowItemInfo(hookIndex, playerController.AbilityRanks.HookRank);
    }

    void ActivateHealth(int slot) 
    { 
        if(playerController.AbilityRanks.HealthRank < 1)
        {
            Passive = AbilityPassiveSlots.Health;
            playerController.AbilityRanks.HealthRank = 1;
            if(healthIndex == -1) healthIndex = lastIndex;
        }
        else playerController.AbilityRanks.HealthRank++;

        if(playerController.AbilityRanks.HealthRank < 3)
        {
            int oldHealth = playerController.ModdedTotalHealth;
            StatsManager.Instance.Health.AddMultiplier($"Health Rank {playerController.AbilityRanks.HealthRank}", 0.3334f);
            RecalculateHealth(oldHealth);
        }
        StatsManager.Instance.KnockbackResistance.AddMultiplier($"Health Rank {playerController.AbilityRanks.HealthRank}", -0.1f * playerController.AbilityRanks.HealthRank);
        ShowItemInfo(healthIndex, playerController.AbilityRanks.HealthRank);
    }

    void RecalculateHealth(int oldHealth)
    {
        int currentDamage = oldHealth - playerController.CurrentHealth;
        if(currentDamage > playerController.CurrentHealth) playerController.ReceiveDamage(currentDamage, Vector3.zero);
        else playerController.CurrentHealth = playerController.ModdedTotalHealth - currentDamage;
    } 

    void ActivateTantrum(int slot) 
    { 
        if(playerController.AbilityRanks.TantrumRank < 1)
        {
            playerController.PlInputs.SetInput("Tantrum", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Tantrum;
            else ActiveB = AbilityActiveSlots.Tantrum;
            playerController.AbilityRanks.TantrumRank = 1;
            if(tantrumIndex == -1) tantrumIndex = lastIndex;
        }
        else playerController.AbilityRanks.TantrumRank++;

        ShowItemInfo(tantrumIndex, playerController.AbilityRanks.TantrumRank);
    }

    void ActivateKnives(int slot) 
    { 
        if(playerController.AbilityRanks.KnivesRank < 1)
        {
            playerController.PlInputs.SetInput("Knives", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Knives;
            else ActiveB = AbilityActiveSlots.Knives;
            playerController.AbilityRanks.KnivesRank = 1;
            if(knivesIndex == -1) knivesIndex = lastIndex;
        }
        else playerController.AbilityRanks.KnivesRank++;

        ShowItemInfo(knivesIndex, playerController.AbilityRanks.KnivesRank);
    }

    void ActivateRanged(int slot) 
    {
        if(playerController.AbilityRanks.RangedRank < 1)
        {
            playerController.PlInputs.SetInput("Ranged", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Ranged;
            else ActiveB = AbilityActiveSlots.Ranged;
            playerController.AbilityRanks.RangedRank = 1;
            if(rangedIndex == -1) rangedIndex = lastIndex;
        }
        else playerController.AbilityRanks.RangedRank++;
        
        ShowItemInfo(rangedIndex, playerController.AbilityRanks.RangedRank);
    }

    void ActivateShield(int slot) 
    {
        if(playerController.AbilityRanks.ShieldRank < 1)
        {
            playerController.PlInputs.SetInput("Shield", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Shield;
            else ActiveB = AbilityActiveSlots.Shield;
            playerController.AbilityRanks.ShieldRank = 1;
            if(shieldIndex == -1) shieldIndex = lastIndex;
        }
        else playerController.AbilityRanks.ShieldRank++;

        ShowItemInfo(shieldIndex, playerController.AbilityRanks.ShieldRank);
    }

    #endregion

    #region Utils
    int GetAbilityRank(AbilitiesEnum ability)
    {
        switch(ability)
        {
            case AbilitiesEnum.Dash: return playerController.AbilityRanks.DashRank;
            case AbilitiesEnum.Mobility: return playerController.AbilityRanks.MobilityRank;
            case AbilitiesEnum.Attack: return playerController.AbilityRanks.AttackRank;
            case AbilitiesEnum.Health: return playerController.AbilityRanks.HealthRank;
            case AbilitiesEnum.Hook: return playerController.AbilityRanks.HookRank;
            case AbilitiesEnum.Tantrum: return playerController.AbilityRanks.TantrumRank;
            case AbilitiesEnum.Knives: return playerController.AbilityRanks.KnivesRank;
            case AbilitiesEnum.Ranged: return playerController.AbilityRanks.RangedRank;
            default: return playerController.AbilityRanks.ShieldRank;
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

    [System.Serializable]
    public class Chances
    {
        public int Dash = 1, Mobility = 1, Attack = 1, Health = 1, Hook = 1, Tantrum = 1, Knives = 1, Ranged = 1, Shield = 1;
    }
}
