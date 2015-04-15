using System.Collections;
using System.ComponentModel;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.LayoutElement))]
public class ListItem : MonoBehaviour {
    private ListItemInfo itemInfo = null;

    public ListItemInfo ItemInfo {
        get { return itemInfo; }
        set {
            if (itemInfo != value) {
                itemInfo = value;
                SubscribeToChanges();
                SetDefaultUI();
            }
        }
    }

    protected virtual void OnEnable() {
        SubscribeToChanges();
    }

    protected virtual void OnDisable() {
        UnsubscribeToChanges();
    }

    protected void SubscribeToChanges() {
        if (ItemInfo != null) {
            ItemInfo.PropertyChanged += UpdateUI;
        }
    }

    protected void UnsubscribeToChanges() {
        if (ItemInfo != null)
            ItemInfo.PropertyChanged -= UpdateUI;
    }

    protected virtual void UpdateUI(object sender, PropertyChangedEventArgs args) {
    }

    protected virtual void SetDefaultUI() {
    }
}