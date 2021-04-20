using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public class Damageable : Attributable
{
    #region Events

    public event EventHandler<DamageEvent> OnDamageEvent;
    public class DamageEvent : Event
    {
        public AttributeChangeCause cause;
        public Damageable victim;
        public Damageable attacker;
        public DamageType type;
        public float damage;
    }

    public event EventHandler<HealthRegainEvent> OnHealthRegainEvent;
    public class HealthRegainEvent : Event
    {
        public AttributeChangeCause cause;
        public Damageable entity;
        public Damageable healer;
        public float amount;
    }

    public event EventHandler<DeathEvent> OnDeathEvent;
    public class DeathEvent : Event
    {
        public AttributeChangeCause cause;
        public Damageable victim;
        public Damageable attacker;
    }
    #endregion

    #region Variables

    [Header("Damageable")]
    [SerializeField] protected bool invulnerable = false;
    [SerializeField] protected DamageType[] weaknesses = new DamageType[0];
    [SerializeField] protected DamageType[] resistances = new DamageType[0];
    [SerializeField] protected DamageType[] immunities = new DamageType[0];
    [SerializeField] protected Vector3 center = Vector3.zero;
    #endregion

    #region Methods

    public virtual void Awake()
    {
        if (HasAttribute(Attributes.HEALTH) == false) AddAttribute(Attributes.HEALTH);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 2)
        {
            float damage = collision.relativeVelocity.magnitude * 3;
            Damage( damage, collision.contacts[0].point, AttributeChangeCause.FORCED, null, DamageType.BLUDGEONING );
        }
    }
    #endregion

    #region Functions

    public bool isDead() { return GetAttributeValue(Attributes.HEALTH) == 0; }
    public bool isAlive() { return GetAttributeValue(Attributes.HEALTH) > 0; }

    public void Damage(float damage, Vector3 hitPoint, AttributeChangeCause cause = AttributeChangeCause.FORCED, Damageable attacker = null, DamageType type = DamageType.NONE)
    {
        //  Invoke a damage event
        DamageEvent e = new DamageEvent{ cause = cause, victim = this, attacker = attacker, type = type, damage = damage };
        OnDamageEvent?.Invoke(this, e);
        if (e.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber

        bool hadImmunity = false;
        bool hadWeakness = false;
        bool hadResistance = false;

        //  Check for immunity
        for (int i = 0; i < immunities.Length; i++)
        {
            if (e.type == immunities[i])
            {
                e.damage = 0;
                hadImmunity = true;
                break;
            }
        }

        //  Modify any damage by any weaknesses or resistances
        if (e.damage > 0 && e.type != DamageType.NONE)
        {
            for (int i = 0; i < weaknesses.Length; i++)
            {
                if (e.type == weaknesses[i])
                {
                    e.damage *= 2;
                    hadWeakness = true;
                    break;
                }
            }

            for (int i = 0; i < resistances.Length; i++)
            {
                if (e.type == resistances[i])
                {
                    e.damage /= 2;
                    hadResistance = true;
                    break;
                }
            }
        }

        //  If the damage is enough to kill, invoke a death event
        if (GetAttributeValue(Attributes.HEALTH) - e.damage <= 0)
        {
            DeathEvent e2 = new DeathEvent{ cause = cause, victim = this, attacker = attacker };
            OnDeathEvent?.Invoke(this, e2);
            if (e2.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber
        }

        //  Update health
        GetAttribute(Attributes.HEALTH).Modify(-e.damage);

        //  Send indicator
        Color indicatorColor = Color.gray * Color.gray;
        if (hadImmunity) indicatorColor = Color.red;
        if (hadWeakness) indicatorColor = Color.white;
        if (hadResistance) indicatorColor = Color.yellow;

        if (hitPoint == null) UIMaster.SendFloatingIndicator(this.transform.position + this.transform.rotation * center, e.damage.ToString("#.0"), indicatorColor);
        else UIMaster.SendFloatingIndicator(hitPoint, e.damage.ToString("0.0"), indicatorColor);
    }

    public void Heal(float amount, AttributeChangeCause cause = AttributeChangeCause.FORCED, Damageable healer = null)
    {
        if (GetAttribute(Attributes.HEALTH).GetPercent() == 1.0f) return;

        //  Invoke a heal event
        HealthRegainEvent e = new HealthRegainEvent{ cause = cause, entity = this, healer = healer, amount = amount };
        OnHealthRegainEvent?.Invoke(this, e);
        if (e.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber

        //  Update health
        GetAttribute(Attributes.HEALTH).Modify(e.amount);

        //  Send indicator
        UIMaster.SendFloatingIndicator(this.transform.position + this.transform.rotation * center, e.amount.ToString("#.0"), Color.green);
    }
    #endregion

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(this.transform.position + this.transform.rotation * center, this.transform.forward * 2);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position + this.transform.rotation * center, this.transform.right * 2);
    }
}

}