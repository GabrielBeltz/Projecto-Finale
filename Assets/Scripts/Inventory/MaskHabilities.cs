using UnityEngine;
using System.Collections;
using TMPro;

public class MaskHabilities : MonoBehaviour
{
    private PlayerController playerController;
    public TextMeshProUGUI[] texts;
    // nesse valor todos os upgrades começam com 100% de chance, e essa chance diminui em 34% a cada rank pego.
    [Range(0, 0.45f)]public float DeramdomizeFactor = 0.34f; 
    float likelyPickDash = 1f, likelyPickDoubleJump = 1f;
    int dashTextIndex = -1, doubleJumpTextIndex = -1, lastIndex = -1; 

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
                case Habilities.DoubleJump:
                    ActivateDoubleJump();
                    break;
            }
        }

        item.Deactivate();
    }

    void DetermineChance()
    {
        likelyPickDash = 1 - Mathf.Clamp01(playerController.DashRank * DeramdomizeFactor);
        likelyPickDoubleJump = 1 - Mathf.Clamp01(playerController.JumpRank * DeramdomizeFactor);
    }

    void PickRandomly()
    {
        DetermineChance();
        float chance = likelyPickDash + likelyPickDoubleJump;
        if(Random.Range(0, chance) < likelyPickDash) ActivateDash();
        else ActivateDoubleJump();
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
        
        ShowItemInfo(playerController.DashRank > 2 ? "Dash ++" : playerController.DashRank > 1 ? "Dash +" : "Dash", dashTextIndex);
        if(dashTextIndex == -1) dashTextIndex = lastIndex;
    }

    void ActivateDoubleJump() 
    {
        playerController.JumpRank++; 

        ShowItemInfo(playerController.JumpRank > 2 ? "Double & Wall Jump +" : playerController.DashRank > 1 ? "Wall Jump +" : "Wall Jump", doubleJumpTextIndex);
        if(doubleJumpTextIndex == -1) doubleJumpTextIndex = lastIndex;
    }
}
