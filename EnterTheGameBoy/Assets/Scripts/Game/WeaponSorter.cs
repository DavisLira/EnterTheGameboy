using UnityEngine;

public class WeaponSorter : MonoBehaviour
{
    private SpriteRenderer weaponRenderer;
    private SpriteRenderer playerRenderer;

    void Start()
    {
        weaponRenderer = GetComponent<SpriteRenderer>();

        // 1. Sobe até achar a raiz do Player (onde está o NetworkIdentity ou Controller)
        // Usamos o PlayerWeaponController como referência para achar a raiz
        var playerRoot = GetComponentInParent<PlayerWeaponController>();

        if (playerRoot != null)
        {
            // 2. A partir da raiz, procura o YSort em QUALQUER filho (incluindo dentro do ModelHolder)
            YSort ysortScript = playerRoot.GetComponentInChildren<YSort>();
            
            if (ysortScript != null)
            {
                playerRenderer = ysortScript.GetComponent<SpriteRenderer>();
            }
            else
            {
                // Fallback: Se não achar YSort, tenta achar qualquer SpriteRenderer no ModelHolder
                // Isso ajuda se o YSort estiver em um objeto e o Sprite em outro
                var anim = playerRoot.GetComponentInChildren<Animator>();
                if(anim) playerRenderer = anim.GetComponent<SpriteRenderer>();
            }
        }
        
        if (playerRenderer == null)
        {
            Debug.LogWarning("WeaponSorter: Não consegui achar o Sprite do Player para seguir a ordem!");
        }
    }

    void LateUpdate()
    {
        if (weaponRenderer != null && playerRenderer != null)
        {
            // Copia a Layer de Ordenação (ex: Default, Entities)
            weaponRenderer.sortingLayerID = playerRenderer.sortingLayerID;
            
            // Define a ordem como Player + 1 (Sempre na frente)
            weaponRenderer.sortingOrder = playerRenderer.sortingOrder + 1;
        }
    }
}