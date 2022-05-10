public class MaskObject : ItemObject
{
    public AbilitiesEnum abilities;

    public void Awake() => type = ItemType.AbilityItem;
}

public enum AbilitiesEnum { None, Dash, Mobility, Attack, Health, Hook, Tantrum, Shield }
public enum AbilityPassiveSlots { None, Mobility, Attack, Health }
public enum AbilityActiveSlots { None, Dash, Hook, Tantrum, Shield }
[System.Serializable]
public struct Ability
{
    public AbilitiesEnum Type;
    public int Rank;
}
