using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ListView : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {

    [SerializeField]
    private GameObject ItemPrefab;

    [SerializeField]
    private bool SnapToChildren;

    [SerializeField]
    private bool ClampToInitalPosition;

    [SerializeField]
    private float Deceleration = 1.5f;

    [SerializeField]
    private float Elasticity = 1f;

    [SerializeField]
    private float flingTimeThreshold = 0.35f;

    [SerializeField]
    private float contentCapacityCoefficient = 1.5f;

    [SerializeField]
    private float circularThresholdRatio = 4f;

    [SerializeField]
    private bool horizontal;

    private Vector2 AnchoredPosition {
        get {
            return contentRect.anchoredPosition;
        }
        set {
            if (horizontal) {
                value.y = 0f;
            }
            else {
                value.x = 0f;
            }
            contentRect.anchoredPosition = value;
        }
    }

    private bool routeToParent = false;

    private PoolManager poolManager;
    private RectTransform viewRect;

    protected List<ListItemInfo> itemsCollection;

    public List<ListItemInfo> ItemsCollection {
        get {
            if (itemsCollection == null) {
                itemsCollection = new List<ListItemInfo>();
            }
            return itemsCollection;
        }
        set {
            itemsCollection = value;
            InstantiateList();
        }
    }

    private LayoutGroup layoutGroup;

    private float itemSize;

    public float ItemSize {
        get {
            return itemSize + spacing;
        }
    }

    private bool easing, dragging;

    public int MaxItemsOnScreen {
        get {
            return Mathf.Min(ItemsCount, screenCapacity);
        }
    }

    private int screenCapacity;
    private int firstItemIndex;

    private float itemsOut;

    [SerializeField]
    private RectTransform contentRect;

    public int LastItemIndex { get { return firstItemIndex + maxListSize - 1; } }

    public int ListCount { get { return contentRect.childCount; } }

    public int ItemsCount { get { return ItemsCollection.Count; } }

    private int maxListSize;

    private float internalOffset;
    private Vector2 initialContentPosition;
    private float threshold;
    private float spacing;
    private Vector2 dragEndTarget;
    private float startDragTime;
    private Vector2 externalOffset;

    private Vector2 velocity;

    public Vector2 Velocity {
        get { return velocity; }
        private set {
            if (horizontal) {
                value.y = 0f;
            }
            else {
                value.x = 0f;
            }
            velocity = value;
        }
    }

    private Vector2 previousPosition;

    protected virtual void Awake() {
        poolManager = PoolManager.Instance;
        viewRect = GetComponent<RectTransform>();
        initialContentPosition = AnchoredPosition;
        Init();
        poolManager.InitPool(ItemPrefab, maxListSize, maxListSize);
    }

    protected virtual void OnEnable() {
        InstantiateList();
    }

    protected virtual void OnDisable() {
        ClearList();
    }

    public void ClearList() {
        itemsCollection.Clear();
        if (horizontal) layoutGroup.padding.right = 0;
        else layoutGroup.padding.top = 0;
        poolManager.DespawnChildren(ItemPrefab, contentRect);
        firstItemIndex = 0;
    }

    public void AddItem(ListItemInfo newItem) {
        ItemsCollection.Add(newItem);
        if (!gameObject.activeInHierarchy) {
            return;
        }
        if (ListCount < maxListSize) {
            SpawnItem().GetComponent<ListItem>().ItemInfo = newItem;
        }
    }

    public void AddItemAt(ListItemInfo newItem, int position) {
        if (position > -1 && position < firstItemIndex) {
            itemsCollection.Insert(position, newItem);
            firstItemIndex++;
        }
        else if (position < LastItemIndex) {
            itemsCollection.Insert(position, newItem);
        }
        else if (position <= ItemsCount) {
            itemsCollection.Insert(position, newItem);
        }
    }

    public void RemoveItemAt(int index) {
        if (index > ItemsCount - 1) {
            return;
        }
        itemsCollection.RemoveAt(index);
        if (index > firstItemIndex && index < LastItemIndex) { // remove item from screen
            for (int i = index; i < LastItemIndex; i++) {
                Transform itemTransform = contentRect.GetChild(i);
                ListItem item = itemTransform.GetComponent<ListItem>();
                item.ItemInfo = itemsCollection[i];
            }
        }
    }

    private GameObject SpawnItem() {
        return poolManager.SpawnObject(ItemPrefab, contentRect);
    }

    private void Init() {
        float visibleAreaSize;
        if (horizontal) {
            visibleAreaSize = viewRect.rect.width;
            itemSize = ItemPrefab.GetComponent<LayoutElement>().preferredWidth;
            HorizontalLayoutGroup layout = GetComponentInChildren<HorizontalLayoutGroup>();
            spacing = layout.spacing;
            layoutGroup = layout;
        }
        else {
            visibleAreaSize = viewRect.rect.height;
            itemSize = ItemPrefab.GetComponent<LayoutElement>().preferredHeight;
            VerticalLayoutGroup layout = GetComponentInChildren<VerticalLayoutGroup>();
            spacing = layout.spacing;
            layoutGroup = layout;
        }
        screenCapacity = Mathf.RoundToInt(visibleAreaSize / ItemSize);
        itemsOut = Mathf.RoundToInt(screenCapacity / circularThresholdRatio);
        internalOffset = Mathf.Abs(AnchoredPosition.y - viewRect.anchoredPosition.y);
        threshold = internalOffset;
        maxListSize = Mathf.RoundToInt(contentCapacityCoefficient * screenCapacity);
        //previousPosition = AnchoredPosition;
    }

    private void InstantiateList() {
        if (!gameObject.activeInHierarchy) {
            return;
        }
        //contentRect.DestroyChildren();
        //layoutGroup.padding.top = 0; // what if initial padding was not 0 ?
        int count = Mathf.Min(maxListSize, ItemsCount);
        for (int i = 0; i < count; i++) {
            SpawnItem().GetComponent<ListItem>().ItemInfo = ItemsCollection[i];
        }
    }

    public void OnItemOutUp() {
        if (LastItemIndex >= ItemsCount - 1) {
            //print("CLAMP UP ?");
            return;
        }
        else {
            firstItemIndex++;
            //Utils.Log("UP dragEndTarget={0} firstIndex={1}", dragEndTarget, firstItemIndex);
            Transform itemTransform = contentRect.GetChild(0);
            ListItem item = itemTransform.GetComponent<ListItem>();
            item.ItemInfo = itemsCollection[LastItemIndex];
            layoutGroup.padding.top += (int)ItemSize;
            itemTransform.SetAsLastSibling();
            threshold = (firstItemIndex + itemsOut) * ItemSize + internalOffset;
        }
    }

    public void OnItemOutDown() {
        if (firstItemIndex == 0) {
            //print("CLAMP DOWN ?");
            return;
        }
        firstItemIndex--;
        //Utils.Log("DOWN dragEndTarget={0} firstIndex={1}", dragEndTarget, firstItemIndex);
        Transform itemTransform = contentRect.GetChild(ListCount - 1);
        ListItem item = itemTransform.GetComponent<ListItem>();
        item.ItemInfo = itemsCollection[firstItemIndex];
        layoutGroup.padding.top -= (int)ItemSize;
        itemTransform.SetAsFirstSibling();
        threshold = (firstItemIndex + itemsOut) * ItemSize + internalOffset;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (!horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
            routeToParent = true;
        else if (horizontal && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
            routeToParent = true;
        else
            routeToParent = false;

        if (routeToParent) {
            DoForParents<IBeginDragHandler>((parent) => { parent.OnBeginDrag(eventData); });
            return;
        }
        easing = false;
        dragging = true;
        velocity = Vector2.zero;
        //startDragPosition = AnchoredPosition;
        startDragTime = Time.time;
        previousPosition = AnchoredPosition;
    }

    public void OnDrag(PointerEventData eventData) {
        if (routeToParent) {
            DoForParents<IDragHandler>((parent) => { parent.OnDrag(eventData); });
            return;
        }
        Vector3 vPosition = AnchoredPosition;
        vPosition.y += eventData.delta.y;
        AnchoredPosition = vPosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (routeToParent) {
            DoForParents<IEndDragHandler>((parent) => { parent.OnEndDrag(eventData); });
            return;
        }
        dragEndTarget.y = AnchoredPosition.y;
        if (Time.time - startDragTime < flingTimeThreshold) {
            dragEndTarget.y += velocity.y / Deceleration;
        }
        else {
            velocity = Vector2.zero;
        }

        //velocity = Vector2.zero;

        if (SnapToChildren) {
            Snap();
        }
        Clamp();
        //Utils.Log("velocity={0} dragTime={1}", velocity, dragTime);
        easing = true;
        //StartCoroutine("EasingCoroutine");
        dragging = false;
        //Utils.Log("target={0} velocity={1}", dragEndTarget.y, velocity.y);
    }

    private void LateUpdate() {
        if (!dragging && !easing) {
            return;
        }

        // 1. Check if we need to move item : drag threshold exceeded + not previous state
        // 2. Get scroll/drag direction : UP or DOWN
        externalOffset = AnchoredPosition - viewRect.anchoredPosition;
        //Utils.Log("distance={0} threshold={1}", externalOffset.y, threshold);
        if (externalOffset.y >
            threshold) {
            OnItemOutUp();
        }
        else if (externalOffset.y <
           threshold - ItemSize) {
            OnItemOutDown();
        }
        float deltaTime = Time.unscaledDeltaTime;

        if (dragging) {
            Vector3 newVelocity = (AnchoredPosition - previousPosition) / deltaTime;
            velocity = Vector3.Lerp(velocity, newVelocity, deltaTime * 10);
            previousPosition = AnchoredPosition;
        }
        else { // easing
            Vector2 position = AnchoredPosition;
            if (velocity != Vector2.zero) {
                float speed = velocity.y;
                position.y = Mathf.SmoothDamp(AnchoredPosition.y,
                    dragEndTarget.y, ref speed, Elasticity, Mathf.Infinity, deltaTime);
                velocity.y = speed;
            }
            else {
                deltaTime = Time.deltaTime;
                position.y = Utils.Spring(position.y, dragEndTarget.y, deltaTime);
            }
            AnchoredPosition = position;
        }
    }

    private void Snap() {
        if (!SnapToChildren) return;
        float totalDistance = dragEndTarget.y - initialContentPosition.y - internalOffset;
        dragEndTarget.y = ItemSize * Mathf.RoundToInt(totalDistance / ItemSize);
    }

    private void Clamp() {
        float clampedPosition;
        if (ClampToInitalPosition)
            clampedPosition = Mathf.Clamp(dragEndTarget.y, initialContentPosition.y,
                initialContentPosition.y + contentRect.sizeDelta.y - ItemSize);
        else
            clampedPosition = Mathf.Clamp(dragEndTarget.y, initialContentPosition.y + internalOffset,
                initialContentPosition.y + internalOffset + (ItemsCount - MaxItemsOnScreen) * ItemSize - spacing
                /*float.MaxValue*/);
        if (dragEndTarget.y != clampedPosition) {
            velocity.y = 0f;
            dragEndTarget.y = clampedPosition;
        }
    }

    /// <summary>
    /// Do action for all parents
    /// </summary>
    private void DoForParents<T>(Action<T> action) where T : IEventSystemHandler {
        Transform parent = transform.parent;
        while (parent != null) {
            foreach (var component in parent.GetComponents<Component>()) {
                if (component is T)
                    action((T)(IEventSystemHandler)component);
            }
            parent = parent.parent;
        }
    }

    /// <summary>
    /// Always route initialize potential drag event to parents
    /// </summary>
    public void OnInitializePotentialDrag(PointerEventData eventData) {
        DoForParents<IInitializePotentialDragHandler>((parent) => { parent.OnInitializePotentialDrag(eventData); });
    }
}