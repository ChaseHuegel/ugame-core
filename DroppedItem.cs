using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

[RequireComponent(typeof(GentleBob))]
public class DroppedItem : MonoBehaviour
{
    [SerializeField] private float pickupDistance = 4.0f;
    [SerializeField] private float collisionHeight = 1.0f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float gain = 3.0f;

    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private ItemStack stack;

    private float velocity = 0.0f;
    private Timer pickupDelay;

    public ItemStack GetStack() { return stack; }
    public void SetStack(ItemStack stack) { this.stack = stack; UpdateViewModel(); }

    private void Start()
    {
        UpdateViewModel();
        pickupDelay = new Timer( GameMaster.GetSettings().ItemPickupDelay.RandomValue() );
    }

    private void Update()
    {
        //  Only attempt pickups every master tick and once the drop timer has passed
        if ( GameMaster.MasterTicked() && (pickupDelay == null || pickupDelay.Tick()) )
        {
            pickupDelay = null;

            if (Util.DistanceUnsquared(transform.position, GameMaster.GetPlayer().transform.position) <= pickupDistance)
            {
                stack = GameMaster.GetPlayer().GetInventory().Add( stack );

                if (stack.IsValid() == false)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

    private void LateUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, collisionHeight, collisionMask) == false)
        {
            velocity += gain * gravity * Time.deltaTime;   //  Velocity should grow to create a sense of weight
            transform.position = Vector3.MoveTowards(transform.position, transform.position + Vector3.down, velocity * Time.deltaTime);
        }
        else
        {
            velocity = 0.0f;
        }
    }

    private void UpdateViewModel()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        int viewCount = stack.GetAmount() / 8;
        if (viewCount < 1) viewCount = 1;
        Transform root = this.transform;

        for (int i = 0; i < viewCount; i++)
        {
            GameObject obj = Instantiate(stack.GetItem().GetWorldModel(), Vector3.zero, Quaternion.identity, root);
            if (i == 0) root = obj.transform;

            obj.transform.eulerAngles = new Vector3(30, 0, 0);

            if (viewCount > 1)
            {
                obj.transform.localPosition = new Vector3(
                    Random.value * 1f - 0.5f,
                    Random.value * 0.5f - 0.25f,
                    Random.value * 1f - 0.5f
                    );
            }
            else
            {
                obj.transform.localPosition = Vector3.zero;
            }
        }

        this.GetComponent<GentleBob>().SetTarget(root);
    }
}
