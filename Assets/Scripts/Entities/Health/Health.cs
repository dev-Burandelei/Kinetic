using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [HideInInspector]
    public int CurrentHealth = 5;
    public int CriticalHealth = 1;
    public int MaxHealth = 5;
    bool critical = false;
    bool died = false;

    [HideInInspector]
    public UnityAction<int> OnDamage;
    [HideInInspector]
    public UnityAction<int, Attack> OnDamageAttack;
    [HideInInspector]
    public UnityAction OnCriticalLevel;
    [HideInInspector]
    public UnityAction<int> OnHeal;
    [HideInInspector]
    public UnityAction OnDie;

    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = MaxHealth;
    }


    public void InflictDamage(int damage)
    {
        InflictDamage(damage, null);
    }


    public void InflictDamage(int damage, Attack source)
    {
        if (died)
            return;

        CurrentHealth -= damage;
        OnDamage?.Invoke(damage);
        OnDamageAttack?.Invoke(damage, source);

        if (CurrentHealth <= CriticalHealth && !critical)
        {
            critical = true;
            if (OnCriticalLevel != null)
                OnCriticalLevel.Invoke();
        }

        if (CurrentHealth <= 0)
            Kill();
    }


    public void Kill()
    {
        if (died)
            return;

        died = true;
        if (OnDie != null)
            OnDie.Invoke();
    }

    public void Heal(int heal)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + heal);
        OnHeal?.Invoke(heal);

        if (CurrentHealth > CriticalHealth && critical)    
            critical = false;
    }
}