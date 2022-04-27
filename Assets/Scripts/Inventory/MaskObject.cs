public class MaskObject : ItemObject
{
    public AbilitiesEnum abilities;
    public Ability assignedAbility;

    public void Awake() => type = ItemType.AbilityItem;
}

public enum AbilitiesEnum { None, Dash, Mobility, Attack, Health, Hook, Tantrum, Knives, Ranged, Shield }
public enum AbilityPassiveSlots { None, Mobility, Attack, Health }
public enum AbilityActiveSlots { None, Dash, Hook, Tantrum, Knives, Shield, Ranged }
public struct Ability
{
    public AbilitiesEnum Type;
    public int Rank;
}
