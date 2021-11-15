using System.Collections.Generic;
using UnityEngine;

public class LevelUpSystem : MonoBehaviour
{
    public static LevelUpSystem LUS;

    [SerializeField]
    List<RectTransform> initialSlots;
    [SerializeField]
    RectTransform initialPassiveSlot;
    [SerializeField]
    GameObject NewAbilityText;
    [SerializeField]
    GameObject NewAbilitySquare;

    [Header("Prefab References")]
    public GameObject optionPrefab;
    public GameObject heavyOptionPrefab;

    List<LoadoutOption> optionsBank;
    List<LoadoutOption> optionsShown;

    bool loweredMenu = false;
    int siblingBaseCount = 0;

    // Start is called before the first frame update
    void Awake()
    {
        if (LUS)
            Destroy(LUS);

        LUS = this;
    }

    void Start() {

        LoadoutManager loadout = ActorsManager.Player.GetComponent<LoadoutManager>();
        optionsBank = new List<LoadoutOption>();
        optionsShown = new List<LoadoutOption>();
        siblingBaseCount = transform.GetChildCount();

        int i = 0;
        foreach (LoadoutManager.Option option in loadout.InitialOptions)
        {
            LoadoutOption loadoutOption = GenerateOptionInstance(option).GetComponent<LoadoutOption>();
            initialSlots[i].GetComponent<DropSlot>().OnDrop(loadoutOption.gameObject);
            if (i != 0 && option.secondaryAbility.Length > 0)
                i++;
            i++;
        }

        foreach (LoadoutManager.Option option in loadout.Options)
        {
            GameObject GO = GenerateOptionInstance(option);
            if (option.ability.name == "Killheal")
                initialPassiveSlot.GetComponent<DropSlot>().OnDrop(GO.GetComponent<LoadoutOption>().gameObject);
            else
            {
                GO.SetActive(false);
                LoadoutOption loadoutOption = GO.GetComponent<LoadoutOption>();
                optionsBank.Add(loadoutOption);
            }
        }
    }

    GameObject GenerateOptionInstance(LoadoutManager.Option option)
    {
        GameObject newInstance = null;
        if (option.secondaryAbility.Length <= 0)
            newInstance = Instantiate(optionPrefab);
        else
        {
            newInstance = Instantiate(heavyOptionPrefab);
            newInstance.GetComponent<BigLoadoutOption>().SetSecondaryAbility(option.secondaryAbility);
        }

        LoadoutOption loadoutOption = newInstance.GetComponent<LoadoutOption>();
        newInstance.transform.SetParent(transform);
        if (option.secondaryAbility.Length > 0)
            newInstance.transform.SetSiblingIndex(siblingBaseCount);

        loadoutOption.Ability = option.ability;
        loadoutOption.isPassive = option.isPassive;
        loadoutOption.OnInsert += ChooseOption;

        return newInstance;
    }


    public void LevelUp()
    {
        NewAbilityText.SetActive(true);
        NewAbilitySquare.SetActive(true);
        SetLoweredMenu(false);

        for (int i = 0; i < 3; i++) {
            if (optionsBank.Count <= 0)
                return;

            int rnd = Random.Range(0, optionsBank.Count);
            optionsBank[rnd].gameObject.SetActive(true);
            optionsShown.Add(optionsBank[rnd]);
            optionsBank[rnd].GetComponent<RectTransform>().anchoredPosition = new Vector2(-40 + i*258, 166);
            if (optionsBank[rnd].GetComponent<BigLoadoutOption>())
                SetLoweredMenu(true);

            optionsBank.RemoveAt(rnd);
        }
    }

    void SetLoweredMenu(bool loweredMenu)
    {
        if (this.loweredMenu == loweredMenu)
            return;

        this.loweredMenu = loweredMenu;
        foreach (RectTransform rectTransform in GetComponentsInChildren<RectTransform>())
        {
            if (rectTransform.gameObject.name == "Background")
                rectTransform.anchoredPosition += loweredMenu ? -new Vector2(0f, 60f) : new Vector2(0f, 60f);
        }
    }

    void ChooseOption(LoadoutOption option)
    {
        if (!optionsShown.Contains(option))
            return;

        optionsShown.Remove(option);
        foreach (LoadoutOption shownOption in optionsShown)
        {
            shownOption.GetComponent<RectTransform>().anchoredPosition = new Vector3(-1000, 1000);
            shownOption.gameObject.SetActive(false);
            optionsBank.Add(shownOption);
        }
        optionsShown.Clear();
        NewAbilityText.SetActive(false);
        NewAbilitySquare.SetActive(false);
    }
}