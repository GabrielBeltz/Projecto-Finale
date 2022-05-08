using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MaskHabilities : MonoBehaviour
{
    public AbilityPassiveSlots Passive;
    public AbilityActiveSlots ActiveA, ActiveB;

    public Chances SpawnChances;
    Chances startingChances;

    [Header("References")]
    public AbilitiesInfos AbilitiesInfos;
    public SwitchActivesMenu SwitchActivesMenu;
    public SwitchPassiveMenu SwitchPassiveMenu;
    public UpgradeAbilitiesMenu UpgradeAbilitiesMenu;
    Ability tempAbilityRef;

    Item tempObjRef;

    void Start()
    {
        startingChances = SpawnChances;
        PlayerController.Instance.OnPlayerDeath += DeactivateAllAbilities;
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
            if(Passive == AbilityPassiveSlots.None) ActivateAbility(gObj.assignedAbility.Type, 2);
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

    void DeactivateAllAbilities()
    {
        DeactivateAbility(0);
        DeactivateAbility(1);
        DeactivateAbility(2);
        SpawnChances = startingChances;
        InteractionManager.Instance.SetKey("Abilities", 0);
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
                PlayerController.Instance.AbilityRanks.DashRank = 0;
                break;
            case AbilitiesEnum.Mobility:
                PlayerController.Instance.AbilityRanks.MobilityRank = 0;
                break;
            case AbilitiesEnum.Attack:
                PlayerController.Instance.AbilityRanks.AttackRank = 0;
                StatsManager.Instance.Damage.Reset();
                break;
            case AbilitiesEnum.Health:
                PlayerController.Instance.AbilityRanks.HealthRank = 0;
                int oldHealth = PlayerController.Instance.ModdedTotalHealth;
                StatsManager.Instance.Health.Reset();
                StatsManager.Instance.KnockbackResistance.Reset();
                RecalculateHealth(oldHealth);
                break;
            case AbilitiesEnum.Hook:
                PlayerController.Instance.AbilityRanks.HookRank = 0;
                break;
            case AbilitiesEnum.Tantrum:
                PlayerController.Instance.AbilityRanks.TantrumRank = 0;
                break;
            case AbilitiesEnum.Knives:
                PlayerController.Instance.AbilityRanks.KnivesRank = 0;
                break;
            case AbilitiesEnum.Ranged:
                PlayerController.Instance.AbilityRanks.RangedRank = 0;
                break;
            case AbilitiesEnum.Shield:
                PlayerController.Instance.AbilityRanks.ShieldRank = 0;
                break;
        }

        if(tempObjRef != null) tempObjRef.EndInteraction(output);
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

    #region Abilities Activation

    void ActivateDash(int slot) 
    { 
        if(PlayerController.Instance.AbilityRanks.DashRank < 1)
        {
            PlayerController.Instance.PlInputs.SetInput("Dash", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Dash;
            else ActiveB = AbilityActiveSlots.Dash;
            PlayerController.Instance.AbilityRanks.DashRank = 1;
        }
        else PlayerController.Instance.AbilityRanks.DashRank++;
        ShowItemInfo(slot, PlayerController.Instance.AbilityRanks.DashRank, AbilitiesEnum.Dash);
    }

    void ActivateAttack(int slot) 
    { 
        if(PlayerController.Instance.AbilityRanks.AttackRank < 1)
        {
            Passive = AbilityPassiveSlots.Attack;
            PlayerController.Instance.AbilityRanks.AttackRank = 1;
        }
        else PlayerController.Instance.AbilityRanks.AttackRank++;
        StatsManager.Instance.Damage.AddMultiplier($"Attack Rank {PlayerController.Instance.AbilityRanks.AttackRank}", 0.5f / PlayerController.Instance.AbilityRanks.AttackRank);
        ShowItemInfo(slot, PlayerController.Instance.AbilityRanks.AttackRank, AbilitiesEnum.Attack);
    }

    void ActivateMobility(int slot) 
    {
        if(PlayerController.Instance.AbilityRanks.MobilityRank < 1)
        {
            Passive = AbilityPassiveSlots.Mobility;
            PlayerController.Instance.AbilityRanks.MobilityRank = 1;
        }
        else PlayerController.Instance.AbilityRanks.MobilityRank++;

        ShowItemInfo(slot, PlayerController.Instance.AbilityRanks.MobilityRank, AbilitiesEnum.Mobility);
    }

    void ActivateHook(int slot) 
    {
        if(PlayerController.Instance.AbilityRanks.HookRank < 1)
        {
            PlayerController.Instance.PlInputs.SetInput("Hook", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Hook;
            else ActiveB = AbilityActiveSlots.Hook;
            PlayerController.Instance.AbilityRanks.HookRank = 1;
        }
        else PlayerController.Instance.AbilityRanks.HookRank++;
            
        ShowItemInfo(slot, PlayerController.Instance.AbilityRanks.HookRank, AbilitiesEnum.Hook);
    }

    void ActivateHealth(int slot) 
    { 
        if(PlayerController.Instance.AbilityRanks.HealthRank < 1)
        {
            Passive = AbilityPassiveSlots.Health;
            PlayerController.Instance.AbilityRanks.HealthRank = 1;
        }
        else PlayerController.Instance.AbilityRanks.HealthRank++;

        if(PlayerController.Instance.AbilityRanks.HealthRank < 3)
        {
            int oldHealth = PlayerController.Instance.ModdedTotalHealth;
            StatsManager.Instance.Health.AddMultiplier($"Health Rank {PlayerController.Instance.AbilityRanks.HealthRank}", 0.3334f);
            RecalculateHealth(oldHealth);
        }
        StatsManager.Instance.KnockbackResistance.AddMultiplier($"Health Rank {PlayerController.Instance.AbilityRanks.HealthRank}", -0.1f * PlayerController.Instance.AbilityRanks.HealthRank);
        ShowItemInfo(slot, PlayerController.Instance.AbilityRanks.HealthRank, AbilitiesEnum.Health);
    }

    void RecalculateHealth(int oldHealth)
    {
        int currentDamage = oldHealth - PlayerController.Instance.CurrentHealth;
        if(currentDamage > PlayerController.Instance.CurrentHealth) PlayerController.Instance.ReceiveDamage(currentDamage, Vector3.zero);
        else PlayerController.Instance.CurrentHealth = PlayerController.Instance.ModdedTotalHealth - currentDamage;
    } 

    void ActivateTantrum(int slot) 
    { 
        if(PlayerController.Instance.AbilityRanks.TantrumRank < 1)
        {
            PlayerController.Instance.PlInputs.SetInput("Tantrum", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Tantrum;
            else ActiveB = AbilityActiveSlots.Tantrum;
            PlayerController.Instance.AbilityRanks.TantrumRank = 1;
        }
        else PlayerController.Instance.AbilityRanks.TantrumRank++;

        ShowItemInfo(slot, PlayerController.Instance.AbilityRanks.TantrumRank, AbilitiesEnum.Tantrum);
    }

    void ActivateKnives(int slot) 
    { 
        if(PlayerController.Instance.AbilityRanks.KnivesRank < 1)
        {
            PlayerController.Instance.PlInputs.SetInput("Knives", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Knives;
            else ActiveB = AbilityActiveSlots.Knives;
            PlayerController.Instance.AbilityRanks.KnivesRank = 1;
        }
        else PlayerController.Instance.AbilityRanks.KnivesRank++;

        ShowItemInfo(slot, PlayerController.Instance.AbilityRanks.KnivesRank, AbilitiesEnum.Knives);
    }

    void ActivateRanged(int slot) 
    {
        if(PlayerController.Instance.AbilityRanks.RangedRank < 1)
        {
            PlayerController.Instance.PlInputs.SetInput("Ranged", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Ranged;
            else ActiveB = AbilityActiveSlots.Ranged;
            PlayerController.Instance.AbilityRanks.RangedRank = 1;
        }
        else PlayerController.Instance.AbilityRanks.RangedRank++;
        
        ShowItemInfo(slot, PlayerController.Instance.AbilityRanks.RangedRank, AbilitiesEnum.Ranged);
    }

    void ActivateShield(int slot) 
    {
        if(PlayerController.Instance.AbilityRanks.ShieldRank < 1)
        {
            PlayerController.Instance.PlInputs.SetInput("Shield", slot == 0);
            if(slot == 0) ActiveA = AbilityActiveSlots.Shield;
            else ActiveB = AbilityActiveSlots.Shield;
            PlayerController.Instance.AbilityRanks.ShieldRank = 1;
        }
        else PlayerController.Instance.AbilityRanks.ShieldRank++;

        ShowItemInfo(slot, PlayerController.Instance.AbilityRanks.ShieldRank, AbilitiesEnum.Shield);
    }

    #endregion

    #region Utils
    public int GetAbilityRank(AbilitiesEnum ability)
    {
        switch(ability)
        {
            case AbilitiesEnum.Dash: return PlayerController.Instance.AbilityRanks.DashRank;
            case AbilitiesEnum.Mobility: return PlayerController.Instance.AbilityRanks.MobilityRank;
            case AbilitiesEnum.Attack: return PlayerController.Instance.AbilityRanks.AttackRank;
            case AbilitiesEnum.Health: return PlayerController.Instance.AbilityRanks.HealthRank;
            case AbilitiesEnum.Hook: return PlayerController.Instance.AbilityRanks.HookRank;
            case AbilitiesEnum.Tantrum: return PlayerController.Instance.AbilityRanks.TantrumRank;
            case AbilitiesEnum.Knives: return PlayerController.Instance.AbilityRanks.KnivesRank;
            case AbilitiesEnum.Ranged: return PlayerController.Instance.AbilityRanks.RangedRank;
            default: return PlayerController.Instance.AbilityRanks.ShieldRank;
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
    
    void ShowItemInfo(int slot, int rank, AbilitiesEnum type) => PlayerController.Instance.HUDController.ShowAbility(slot, rank, AbilitiesInfos.GetFullInfo(type.ToString()));
    
    #endregion

    [System.Serializable]
    public class Chances
    {
        public int Dash = 1, Mobility = 1, Attack = 1, Health = 1, Hook = 1, Tantrum = 1, Knives = 1, Ranged = 1, Shield = 1;
    }
}
