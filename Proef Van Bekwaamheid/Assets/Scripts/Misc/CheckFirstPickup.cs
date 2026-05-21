using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CheckFirstPickup : MonoBehaviour
{
    [SerializeField] private UnityEvent _onFirstPickUp;

    private Collider[] _itemCollider;

    void Start()
    {
        Initialise();
    }

    public void Initialise()
    {
        _itemCollider = null;
        Item[] items = FindObjectsByType<Item>(FindObjectsSortMode.None);
        _itemCollider = new Collider[items.Length];

        for (int i = 0; i < _itemCollider.Length; i++)
            _itemCollider[i] = items[i].gameObject.GetComponent<Collider>();
    }

    void Update()
    {
        for (int i = 0; i < _itemCollider.Length; i++)
        {
            if (_itemCollider[i].enabled)
                continue;
            
            _onFirstPickUp.Invoke();
            enabled = false;
        }
    }
}
