using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Shop;

[AutoBindPrefix("UI")]
public class ShopCategoryPanel : UIComponent
{
    [AutoBind] [SerializeField] private TMP_Text titleText;
    
    // 스크롤 뷰 위젯 (내부적으로 Content를 관리)
    [AutoBind] [SerializeField] private UIScrollViewWidget scrollWidget;
    
    [Header("Prefabs")]
    [SerializeField] private ShopItemCard itemCardPrefab;

    private List<ShopItemCard> spawnedCards = new List<ShopItemCard>();

    public void Bind(string categoryTitle, List<ShopRuntimeItem> items, Action<ShopItemCard> onClick, Action<ShopItemCard> onHoverEnter, Action<ShopItemCard> onHoverExit)
    {
        if (titleText != null)
        {
            titleText.text = categoryTitle;
        }

        ClearCards();

        if (itemCardPrefab == null || scrollWidget == null || items == null)
        {
            Debug.LogWarning($"[ShopCategoryPanel] {categoryTitle} 렌더링 실패: Prefab이나 ScrollWidget이 누락되었습니다.");
            return;
        }

        Transform contentRoot = scrollWidget.ContentRoot;
        if (contentRoot == null) return;

        foreach (var item in items)
        {
            if (item == null) continue;

            ShopItemCard card = Instantiate(itemCardPrefab, contentRoot);
            card.Bind(item);
            
            card.OnClicked += onClick;
            card.OnHoverEnter += onHoverEnter;
            card.OnHoverExit += onHoverExit;
            
            spawnedCards.Add(card);
        }
        
        // 아이템 생성 후 스크롤 위치를 맨 앞으로 초기화
        scrollWidget.ResetScrollPosition();
    }

    public void RefreshCards()
    {
        foreach (var card in spawnedCards)
        {
            if (card != null) card.Refresh();
        }
    }

    private void ClearCards()
    {
        foreach (var card in spawnedCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        spawnedCards.Clear();
    }
}
