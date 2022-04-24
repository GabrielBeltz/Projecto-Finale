using UnityEngine;
using System.Collections;
using TMPro;

public class MaskHabilities : MonoBehaviour
{
    private PlayerController playerController;
    public TextMeshProUGUI[] texts;
    // nesse valor todos os upgrades começam com 100% de chance, e essa chance diminui em 34% a cada rank pego.
    [Range(0, 0.45f)]public float DeramdomizeFactor = 0.34f; 
    float chanceDash, chanceDoubleJump, chanceAttack, chanceHook, chanceTantrum, chanceKnives, chanceBoomerang, chanceShield;
    int dashTextIndex = -1, jumpTextIndex = -1, attackTextIndex = -1, hookTextIndex = -1, 
    tantrumTextIndex = -1, knivesTextIndex = -1, boomerangTextIndex = -1, shieldTextIndex = -1, lastIndex = -1; 

    void Awake() => playerController = GetComponent<PlayerController>();

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
                case Habilities.Dash:
                    ActivateDash();
                    break;
                case Habilities.Jump:
                    ActivateJump();
                    break;
                case Habilities.Attack:
                    ActivateAttack();
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
                    ActivateBoomerang();
                    break;
                case Habilities.Shield:
                    ActivateShield();
                    break;
            }
        }

        item.Deactivate();
    }

    void DetermineChance()
    {
        chanceDash = 1 - Mathf.Clamp01(playerController.DashRank * DeramdomizeFactor);
        chanceDoubleJump = 1 - Mathf.Clamp01(playerController.JumpRank * DeramdomizeFactor);
        chanceAttack = 1 - Mathf.Clamp01(playerController.AttackRank * DeramdomizeFactor);
        chanceHook = 1 - Mathf.Clamp01(playerController.HookRank * DeramdomizeFactor);
        chanceTantrum = 1 - Mathf.Clamp01(playerController.TantrumRank * DeramdomizeFactor);
        chanceKnives = 1 - Mathf.Clamp01(playerController.KnivesRank * DeramdomizeFactor);
        chanceBoomerang = 1 - Mathf.Clamp01(playerController.BoomerangRank * DeramdomizeFactor);
        chanceShield = 1 - Mathf.Clamp01(playerController.ShieldRank * DeramdomizeFactor);
    }

    // Partes comentadas não estão implementadas ainda
    void PickRandomly()
    {
        DetermineChance();
        float totalChance = chanceDash + chanceDoubleJump + chanceAttack; // + chanceHook + chanceKnives + chanceTantrum + chanceBoomerang + chanceShield;
        float roll = Random.Range(0, totalChance);
        if(roll < chanceDash) ActivateDash();
        else if(roll < chanceDash + chanceDoubleJump) ActivateJump();
        else ActivateAttack();
        //else if(roll < chanceDash + chanceDoubleJump + chanceAttack) ActivateAttack();
        //else if(roll < chanceDash + chanceDoubleJump + chanceAttack + chanceHook) ActivateHook();
        //else if(roll < chanceDash + chanceDoubleJump + chanceAttack + chanceHook + chanceKnives) ActivateKnives();
        //else if(roll < chanceDash + chanceDoubleJump + chanceAttack + chanceHook + chanceKnives + chanceTantrum) ActivateTantrum();
        //else if(roll < chanceDash + chanceDoubleJump + chanceAttack + chanceHook + chanceKnives + chanceTantrum + chanceBoomerang) ActivateTantrum();
        //else ActivateShield();
        DetermineChance();
    }

    void ShowItemInfo(string text, int index) => StartCoroutine(ItemInfo(text, index));

    IEnumerator ItemInfo(string text, int index)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0;
        if(index == -1) 
        {
            index = lastIndex + 1; 
        }
        if(lastIndex < index) lastIndex = index;
        texts[index].text = text;

        // TODO: Criar animação do texto aparecer no centro da tela, escurecer a tela no fundo do texto e transicionar o texto pra posição original 
        //for(int i = 0; i < 300; i++)
        //{
            
            yield return new WaitForSecondsRealtime(0.01f);
        //}

        Time.timeScale = originalTimeScale;
    }

    void ActivateDash() 
    { 
        playerController.DashRank++;
        ShowItemInfo(playerController.DashRank > 2 ? "Damaging Dash" : playerController.DashRank > 1 ? "Invulnerable Dash" : "Dash", dashTextIndex);
        if(dashTextIndex == -1) dashTextIndex = lastIndex;
    }

    void ActivateAttack() 
    { 
        playerController.AttackRank++;
        StatsManager.Instance.AddDamageMultiplier(0.5f / playerController.AttackRank, $"Attack Rank {playerController.AttackRank}");
        ShowItemInfo(playerController.AttackRank > 2 ? "+ Damage +Range +Piercing" : playerController.AttackRank > 1 ? "+Damage +Range" : "+ Damage", attackTextIndex);
        if(attackTextIndex == -1) attackTextIndex = lastIndex;
    }

    void ActivateJump() 
    {
        playerController.JumpRank++;
        ShowItemInfo(playerController.JumpRank > 2 ? "Double & Wall Jump" : playerController.JumpRank > 1 ? "Wall Jump + Climb" : "Wall Jump", jumpTextIndex);
        if(jumpTextIndex == -1) jumpTextIndex = lastIndex;
    }

    void ActivateHook() 
    { 
        playerController.HookRank++;
        ShowItemInfo(playerController.HookRank > 2 ? "Hook ++" : playerController.HookRank > 1 ? "Hook +" : "Hook", hookTextIndex);
        if(hookTextIndex == -1) hookTextIndex = lastIndex;
    }

    void ActivateTantrum() 
    { 
        playerController.TantrumRank++;
        ShowItemInfo(playerController.TantrumRank > 2 ? "Hook ++" : playerController.TantrumRank > 1 ? "Hook +" : "Hook", tantrumTextIndex);
        if(tantrumTextIndex == -1) tantrumTextIndex = lastIndex;
    }

    void ActivateKnives() 
    { 
        playerController.KnivesRank++;
        ShowItemInfo(playerController.KnivesRank > 2 ? "Hook ++" : playerController.KnivesRank > 1 ? "Hook +" : "Hook", knivesTextIndex);
        if(knivesTextIndex == -1) knivesTextIndex = lastIndex;
    }

    void ActivateBoomerang() 
    { 
        playerController.BoomerangRank++;
        ShowItemInfo(playerController.BoomerangRank > 2 ? "Hook ++" : playerController.BoomerangRank > 1 ? "Hook +" : "Hook", boomerangTextIndex);
        if(boomerangTextIndex == -1) boomerangTextIndex = lastIndex;
    }

    void ActivateShield() 
    { 
        playerController.ShieldRank++;
        ShowItemInfo(playerController.ShieldRank > 2 ? "Hook ++" : playerController.ShieldRank > 1 ? "Hook +" : "Hook", shieldTextIndex);
        if(shieldTextIndex == -1) shieldTextIndex = lastIndex;
    }
}
