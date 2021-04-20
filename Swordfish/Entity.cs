using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

[RequireComponent(typeof(CharacterController))]
public class Entity : Damageable
{
    #region Events

    public event EventHandler<EntityAttackEvent> OnEntityAttackEvent;
    public class EntityAttackEvent : Event
    {
        public Entity entity;
    }

    public event EventHandler<EntityAttackHitEvent> OnEntityAttackHitEvent;
    public class EntityAttackHitEvent : Event
    {
        public Entity entity;
        public Damageable target;
    }

    public event EventHandler<EntityAttackMissEvent> OnEntityAttackMissEvent;
    public class EntityAttackMissEvent : Event
    {
        public Entity entity;
    }

    public event EventHandler<EntityUpdateEvent> OnEntityUpdateEvent;
    public class EntityUpdateEvent : Event
    {
        public Entity entity;
    }

	public event EventHandler<AttributeChangeEvent> OnAttributeChangeEvent;
    public class AttributeChangeEvent : Event
    {
        public AttributeChangeCause cause;
		public Attributes attribute;
        public Entity entity;
        public float amount;
    }

	public event EventHandler<EnduranceChangeEvent> OnEnduranceChangeEvent;
    public class EnduranceChangeEvent : Event
    {
        public AttributeChangeCause cause;
        public Entity entity;
        public float amount;
    }
    #endregion

    #region Variables

	[Header("References")]
	[SerializeField] protected Animator[] animators;
	[SerializeField] protected Inventory inventory;

    [Header("Combat")]
    [SerializeField] protected LayerMask attackableLayers;
	[SerializeField] protected Transform attackRoot = null;

    [Space(10)]
	[SerializeField] protected Vector3 attackOffset     = Vector3.zero;
	[SerializeField] protected Vector3 attackExtents    = Vector3.one;
	[SerializeField] protected float attackDuration     = 0.4f;
	[SerializeField] protected float attackRate		    = 1.0f;
	[SerializeField] protected float attackDamage		= 10.0f;
	[SerializeField] protected DamageType attackType	= DamageType.BLUDGEONING;
    [SerializeField] protected float invulnTime 	    = 0.2f;

    [Header("Movement")]
	[SerializeField, Tooltip("What layers are used for raycasts & manual physics operations.")]
	protected LayerMask collisionMask;

	[Space(10)]
	[Tooltip("Whether this character can move while midair.")]
	public bool midairControl = true;

	[Tooltip("How fast this character moves in units/second.")]
    public float moveSpeed = 5.0f;
	[Tooltip("How fast this character walks in units/second.")]
    public float walkSpeed = 3.0f;
	[Tooltip("How fast this character sprints in units/second.")]
    public float sprintSpeed = 10.0f;
	[Tooltip("How high this character jumps in units/second.")]
    public float jumpStrength = 12.0f;

	[Space(10)]
	[Tooltip("Whether this character is affected by gravity.")]
    public bool gravityEnabled = true;
	[Tooltip("Gravity strength in units/second.")]
    public float gravity = 50.0f;
	[Tooltip("Terminal velocity as a factor of gravity.")]
    public float terminalGravityFactor = 40.0f;
	[Tooltip("Multiplier for exponential gravity. Leave 1.0 for no exponential growth.")]
    public float gravityGain = 1.0f;

	[Space(10)]
	[Tooltip("The maximum distance to check for a slope to slide down.")]
	public float slideDistance = 0.5f;

    [Header("Debug")]
	[SerializeField] protected bool isAttacking = false;
	[SerializeField] protected bool attacked = false;
    [SerializeField] protected bool isGrounded = false;
	[SerializeField] protected bool isSliding = false;
	[SerializeField] protected bool isJumping = false;
	[SerializeField] protected bool isFalling = false;
	[SerializeField] protected bool isMoving = false;
	[SerializeField] protected bool isWalking = false;
	[SerializeField] protected bool isSprinting = false;

    [Header("Data")]
	protected CharacterController controller;
	protected Vector3 center;
	protected Vector3 bottom;

    protected Timer enduTimer;
    protected Timer hitpointTimer;
	protected Timer updateTimer;
    protected Timer attackCooldown;
	protected Timer invulnTimer;

	protected float currentSpeed;
    protected Vector3 movementDirection = Vector3.zero;
	protected Vector3 facingDirection = Vector3.forward;
	protected Vector3 velocity = Vector3.zero;

	public float GetCurrentSpeed() { return currentSpeed; }

	public void SetInventory(Inventory inventory) { this.inventory = inventory; }
	public Inventory GetInventory() { return inventory; }
    #endregion

    #region Functions

    public override void Awake()
    {
        base.Awake();

        if (HasAttribute(Attributes.ENDURANCE) == false) AddAttribute(Attributes.ENDURANCE);

		Initialize();
    }

	private void Start()
	{
		// Initialize();
	}

	public virtual void Initialize()
	{
		enduTimer 		= new Timer(Constants.END_REGEN_TIME, true);
        hitpointTimer 	= new Timer(Constants.HP_REGEN_TIME, true);
		updateTimer 	= new Timer(Constants.ENTITY_UPDATE_TIME, true);
		attackCooldown 	= new Timer(attackRate);
		invulnTimer 	= new Timer(invulnTime);

		controller 	= this.GetComponent<CharacterController>();

		if (animators[0] == null) animators[0] = this.GetComponent<Animator>();
		if (attackRoot == null) attackRoot = this.transform;
		if (inventory == null) inventory = new Inventory(36);

        Spawn();
	}

	private void Update()
	{
		UpdateOrigins();

		TickBegin();
		Tick();
		TickEnd();

		UpdateAnimator();
	}

	private void FixedUpdate()
	{
		FixedTickBegin();
		FixedTick();
		FixedTickEnd();
	}

	public virtual void UpdateAnimator()
	{
		foreach (Animator animator in animators)
		{
			animator.SetBool("Attacking", isAttacking);
			animator.SetBool("Moving", isMoving);
			animator.SetBool("Walking", isWalking);
			animator.SetBool("Sprinting", isSprinting);
			animator.SetBool("Jumping", isJumping);
			animator.SetBool("Falling", isFalling);
		}
	}

    public virtual void UpdateEntity()
    {
    }

	public virtual void TickBegin()
	{
	}

	public virtual void Tick()
	{
        if (enduTimer.Tick() == true)
        {
            ModifyEndurance(Constants.END_REGEN_AMOUNT);
        }

        if (hitpointTimer.Tick() == true)
        {
            Heal(Constants.HP_REGEN_AMOUNT);
        }

		if (updateTimer.Tick() == true)
        {
			//  Invoke an update event
            EntityUpdateEvent e = new EntityUpdateEvent{ entity = this };
            OnEntityUpdateEvent?.Invoke(this, e);
            if (e.cancel == false) { UpdateEntity(); }   //  call entity update if this hasn't been cancelled by any subscriber
        }

		attackCooldown.Tick();
		invulnTimer.Tick();

        HandleGrounding();
        HandleVelocity();
	}

	public virtual void TickEnd()
	{
        HandleMovement();
	}

	public virtual void FixedTickBegin()
	{
	}

	public virtual void FixedTick()
	{

	}

	public virtual void FixedTickEnd()
	{

	}

	public void Spawn()
    {
        GetAttribute(Attributes.HEALTH).Max();
        GetAttribute(Attributes.ENDURANCE).Max();
    }

	public void Kill()
	{
		Destroy(gameObject);
	}

    public bool HasEndurance(float cost)
    {
		cost = Math.Abs(cost);	//	Make sure cost isn't negative!
        return (GetAttributeValue(Attributes.ENDURANCE) >= GetAttributeValue(Attributes.ENDURANCE) - cost) && (GetAttributeValue(Attributes.ENDURANCE) - cost >= 0);
    }

	public void ModifyEndurance(float amount, AttributeChangeCause cause = AttributeChangeCause.FORCED)
	{
		//  Invoke an endurance change event
        EnduranceChangeEvent e = new EnduranceChangeEvent{ cause = cause, entity = this, amount = amount };
        OnEnduranceChangeEvent?.Invoke(this, e);
        if (e.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber

		GetAttribute(Attributes.ENDURANCE).Modify(e.amount);
	}

	public bool CostModifyEndurance(float amount, AttributeChangeCause cause = AttributeChangeCause.FORCED)
	{
		if (HasEndurance(amount)) { ModifyEndurance(amount, cause); return true; }

		return false;
	}

    public void TriggerHitInvulnerable()
    {
        if (isInvulnerable() == false) invulnTimer.Reset();
    }

	public bool isInvulnerable()
	{
		return (invulnerable == false ? !invulnTimer.IsDone() : true);	//	if invuln = false, check timer. otherwise, return true
	}

	public bool isAttackReady()
	{
		return attackCooldown.IsDone();
	}

	public void Attack()
	{
		if (isAttackReady())
		{
			//  Invoke an attack event
            EntityAttackEvent e = new EntityAttackEvent{ entity = this };
            OnEntityAttackEvent?.Invoke(this, e);
            if (e.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber

			attackCooldown.Reset(attackRate);
			StartCoroutine("AttackRoutine");
		}
	}

	private IEnumerator AttackRoutine()
	{
		Timer thisTimer = new Timer(attackDuration);
		bool didHit = false;
		isAttacking = true;

		while (thisTimer.Tick() == false) { yield return null; }

		attacked = true;
		Collider[] hits = Physics.OverlapBox(attackRoot.rotation * new Vector3(attackOffset.x, attackOffset.y, attackOffset.z) + attackRoot.position, attackExtents, attackRoot.rotation, attackableLayers);

		Damageable victim;
		for (int i = 0; i < hits.Length; i++)
		{
			didHit = true;
			victim = hits[i].gameObject.GetComponent<Damageable>();

            bool canDamage = ( (victim is Entity) && ((Entity)victim).isInvulnerable() ) ? false : true;    //  False if victim is an entity and invulnerable

			if (victim != null && canDamage)
            {
                //  Invoke a hit event
                EntityAttackHitEvent e = new EntityAttackHitEvent{ entity = this, target = victim };
                OnEntityAttackHitEvent?.Invoke(this, e);
                if (e.cancel == true) { yield break; }   //  return if the event has been cancelled by any subscriber

                //  Damage the victim and trigger entity hit invulnerability
                victim.Damage(attackDamage, attackRoot.rotation * new Vector3(attackOffset.x, attackOffset.y, attackOffset.z + attackExtents.z) + attackRoot.position, AttributeChangeCause.ATTACKED, this, attackType);
                if (victim is Entity) ((Entity)victim).TriggerHitInvulnerable();
            }
		}

		if (didHit == false)
        {
            //  Invoke a miss event
            EntityAttackMissEvent e = new EntityAttackMissEvent{ entity = this };
            OnEntityAttackMissEvent?.Invoke(this, e);
            if (e.cancel == true) { yield break; }   //  return if the event has been cancelled by any subscriber
        }

		isAttacking = false;
		while (isAttackReady() == false) yield return null;
		attacked = false;
	}

	private void UpdateOrigins()
	{
		center = transform.position + controller.center;
		bottom = transform.position - new Vector3(0, controller.height / 2, 0) - controller.center;
	}

    private void HandleGrounding()
	{
		isSliding = false;
		isGrounded = ( (controller.collisionFlags & CollisionFlags.Below) != 0 );	//	Use builtin flags (bit mask) to determine whether grounded

		if (isGrounded == false && isJumping == false)	//	If not grounded or jumping perform a second check to handle sliding
		{
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, slideDistance, collisionMask) == true)
			{
				isGrounded = isSliding = true; //	We are grounded and sliding
				controller.Move(new Vector3(0, -hit.distance, 0));
			}
		}
	}

    private void HandleVelocity()
    {
        velocity.x = velocity.z = 0.0f;	//	We don't want horizontal movement to carry over between updates. For non-input effects such as sliding on ice, charges, knockbacks, etc

		//	TODO: Add 'impulse' & friction for effects such as knockbacks, sliding on ice, charges, other forms of outside movement that decay over time

		if (gravityEnabled == true)
		{
			if (isGrounded == false) //  Handle gravity
			{
                if (velocity.y < 0) { velocity.y *= gravityGain; }	//	Perform gain before applying additional gravity
				velocity.y -= (gravity * Time.deltaTime);	//	Velocity should increase for a heavier feeling
			}
			else
			{
				velocity.y = -(gravity * Time.deltaTime);	//	Stay grounded using a constant velocity in case of character being bumped too far by collisions
			}

			if ((controller.collisionFlags & CollisionFlags.Above) != 0 && velocity.y >= 0.0f)	//	We hit our head during a jump. Stop moving up.
			{
				velocity.y = -(velocity.y * 0.25f);	//	Positive to hang, 0 to float, negative to bounce/snap
			}
		}
		else
		{
			velocity.y = 0.0f;	//	If gravity is disabled, don't carry over Y movement between updates
		}

		currentSpeed = 0.0f;
		isMoving = (movementDirection != Vector3.zero);
        if (isMoving)
		{
			//	Default to base move speed
			float speed = moveSpeed;

			//	Set speed based on state
			if 		(isSprinting) 	speed = sprintSpeed;
			else if (isWalking) 	speed = walkSpeed;

			velocity += speed * transform.TransformDirection(movementDirection);
			currentSpeed = speed;
		}

        if (velocity.y < -(gravity * terminalGravityFactor)) //  Clamp gravity to terminal velocity
        {
            velocity.y = -(gravity * terminalGravityFactor);
        }

		isFalling = velocity.y <= 0;
		if (isFalling && isGrounded) isFalling = false;
		if (isGrounded) isJumping = false;
    }

    private void HandleMovement()
    {
        if (midairControl == false && isGrounded == false)
		{
			velocity.x = velocity.z = 0.0f;	//	If midair control isn't allowed, then zero all horizontal movement
		}

		controller.Move(velocity * Time.deltaTime);	//	Perform movement at the end of a tick during fixed physics updates
    }

	//	DEBUG
	public virtual void OnDrawGizmos()
	{
		if (attackRoot == null) attackRoot = this.transform;

		if (Application.isPlaying == true)
		{
            if (isSliding) { Gizmos.color = Color.red; } else { Gizmos.color = Color.blue; }
			Gizmos.DrawRay(transform.position, Vector3.down * slideDistance);

			if (attacked == true)
			{
				Collider[] hits = Physics.OverlapBox(attackRoot.rotation * new Vector3(attackOffset.x, attackOffset.y, attackOffset.z) + attackRoot.position, attackExtents, attackRoot.rotation, attackableLayers);

				if (hits.Length > 0) Gizmos.color = Color.red; else Gizmos.color = Color.yellow;

				Gizmos.matrix = Matrix4x4.TRS(attackRoot.position, attackRoot.rotation, attackRoot.lossyScale);
				Gizmos.DrawWireCube(new Vector3(attackOffset.x, attackOffset.y, attackOffset.z), attackExtents);
			}
		}
		else if (Application.isPlaying == false)
		{
			Gizmos.color = Color.red;

			Gizmos.matrix = Matrix4x4.TRS(attackRoot.position, attackRoot.rotation, attackRoot.lossyScale);
			Gizmos.DrawWireCube(new Vector3(attackOffset.x, attackOffset.y, attackOffset.z), attackExtents);

            Gizmos.color = Color.blue;
			Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * slideDistance);
		}
	}
    #endregion
}

}