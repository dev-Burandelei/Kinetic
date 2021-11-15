using UnityEngine;
using UnityEngine.UI;

public class BigLoadoutOption : MonoBehaviour
{
    LoadoutSlot primarySlot;
    LoadoutSlot secondarySlot;

    // Start is called before the first frame update
    void Awake()
    {
        secondarySlot = GetComponentInChildren<LoadoutSlot>();
        secondarySlot.GetComponent<DropSlot>().OnInserted += OnSecondaryInsert;
        DragDrop dragDrop = GetComponent<DragDrop>();
        dragDrop.OnInsert += OnPrimaryInsert;
        dragDrop.OnRemove += OnPrimaryRemove;
    }

    private void Start()
    {
        secondarySlot.GetComponent<DropSlot>().Offset = GetComponent<RectTransform>().anchoredPosition;
    }


    public void SetSecondaryAbility(string secondaryAbility)
    {
        secondarySlot.GetComponent<DropSlot>().Type = secondaryAbility;
        secondarySlot.GetComponentInChildren<Text>().text = secondaryAbility;
    }

    void OnSecondaryInsert(DragDrop option)
    {
        option.GetComponent<LoadoutOption>().Ability.GetComponent<SecondaryAbility>().ParentAbility = 
            GetComponent<LoadoutOption>().Ability.GetComponent<Ability>();
    }

    public void OnPrimaryInsert(DropSlot slot)
    {
        primarySlot = slot.GetComponent<LoadoutSlot>();
        if (primarySlot.NextSlot == null || primarySlot.NextSlot.GetComponent<DropSlot>().InsertedDragDrop != null)
            secondarySlot.gameObject.SetActive(false);
        else
        {
            secondarySlot.gameObject.SetActive(true);
            secondarySlot.LoadoutNumber = primarySlot.LoadoutNumber;
            secondarySlot.AbilityNumber = primarySlot.AbilityNumber + 1;
            primarySlot.NextSlot.GetComponent<DropSlot>().Label.SetActive(false);
        }

        secondarySlot.GetComponent<DropSlot>().Offset = GetComponent<RectTransform>().anchoredPosition;
    }

    void OnPrimaryRemove(DropSlot slot)
    {
        DropSlot secondaryDropSlot = secondarySlot.GetComponent<DropSlot>();

        if (secondaryDropSlot.InsertedDragDrop && secondaryDropSlot.InsertedDragDrop.OnRemove != null)
            secondaryDropSlot.InsertedDragDrop.OnRemove.Invoke(secondaryDropSlot);

        if (secondaryDropSlot.gameObject.activeInHierarchy)
        {
            primarySlot.NextSlot.GetComponent<DropSlot>().Label.SetActive(true);
            secondarySlot.gameObject.SetActive(false);
        }
    }
}