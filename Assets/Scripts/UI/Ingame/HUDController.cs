using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Scene References")]
    public Transform UIScaler;

    [Header("Player Health")]
    public Transform HealthSpawn;
    public GameObject HealthIconPrefab;
    public Sprite Heart, HeartBroken;
    public Vector2 FullHDHealthIconsPivot = new Vector2(75, 1005);
    List<InstantiatedUIHP> Hearts;

    [Header("Abilities Ingame")]
    public AbilityView Passive;
    public AbilityView ActiveA, ActiveB;
    public TextMeshProUGUI PassiveRank, ActiveARank, ActiveBRank;

    void Start()
    {
        Hearts = new List<InstantiatedUIHP>();
        PlayerController.Instance.OnPlayerHealthChanged += InterfacePlayerHP;
        PlayerController.Instance.HUDController = this;
    }

    public void InterfacePlayerHP(int currentHealth, int maxHealth)
    {
        for(int i = 0; i < maxHealth; i++)
        {
            if(Hearts.Count <= i) Hearts.Add(new InstantiatedUIHP(Instantiate(HealthIconPrefab, HealthSpawn)));
            else if(!Hearts[i].Prefab.activeSelf) Hearts[i].Prefab.SetActive(true);
            Hearts[i].rect.anchoredPosition = new Vector2(FullHDHealthIconsPivot.x * (i + 1), (FullHDHealthIconsPivot.y - 1080f));
            Hearts[i].image.sprite = i < currentHealth ? Heart : HeartBroken;
        }

        if(maxHealth < Hearts.Count) for(int i = 0; i < Hearts.Count - maxHealth; i++) Hearts[Hearts.Count - i - 1].Prefab.SetActive(false);
    }

    public void ShowAbility(int slot, int rank, AbilityInfo ability)
    {
        switch(slot)
        {
            case 0:
                ActiveA.Activate(ability, 0);
                ActiveARank.text = rank > 2 ? "++" : rank > 1 ? "+" : "";
                break;
            case 1:
                ActiveB.Activate(ability, 0);
                ActiveBRank.text = rank > 2 ? "++" : rank > 1 ? "+" : "";
                break;
            case 2:
                Passive.Activate(ability, 0);
                PassiveRank.text = rank > 2 ? "++" : rank > 1 ? "+" : ""; 
                break;
        }
    }

    public class InstantiatedUIHP
    {
        public GameObject Prefab;
        public Image image;
        public RectTransform rect;

        public InstantiatedUIHP(GameObject instantiatedPrefab)
        {
            Prefab = instantiatedPrefab;
            image = Prefab.GetComponent<Image>();
            rect = Prefab.GetComponent<RectTransform>();
        }
    }
}
