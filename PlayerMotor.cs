using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

public class PlayerMotor : Player
{
    [Header("Interaction")]
    public float interactRange = 3.0f;
    public LayerMask interactableMask;
    public CameraMotor cameraMotor;

	public override void Initialize()
    {
		base.Initialize();

        OnEntityUpdateEvent += OnUpdate;
        OnEntityAttackHitEvent += OnHit;
        OnEntityAttackMissEvent += OnMiss;
        GetInventory().OnInventoryChangeEvent += OnPickup;
	}

    public override void Tick()
    {
		base.Tick();

        movementDirection = Vector3.zero;
        isSprinting = false;
        isWalking = false;

        if (UIMaster.GetState() == UIState.FOCUSED) return; //  Don't allow interaction while the UI is focused
        if (GameMaster.GetState() != GameState.PLAYING) return; //  Don't allow interaction while the gamestate isn't in play mode

        if (Input.GetButton("Forward")) movementDirection += Vector3.forward;
        if (Input.GetButton("Backward")) movementDirection += Vector3.back;
        if (Input.GetButton("Left")) movementDirection += Vector3.left;
        if (Input.GetButton("Right")) movementDirection += Vector3.right;

        if (movementDirection != Vector3.zero && Input.GetButton("Sprint") && HasEndurance(Constants.END_COST_SPRINT))
        {
            isSprinting = true;
        }

        if (movementDirection != Vector3.zero && Input.GetButton("Walk"))
        {
            isWalking = true;
        }

		if (isGrounded == true)
        {
            if (Input.GetButton("Jump") && CostModifyEndurance(Constants.END_COST_JUMP))
            {
                velocity.y = jumpStrength;

                isJumping = true;
            }
        }

		if (Input.GetButton("Attack"))
		{
			Attack();
		}

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Offset interact range based on camera distance. Offset is relative to character facing, so invert it
        float range = interactRange;
        if (cameraMotor.view == CameraMotor.CameraView.THIRD_PERSON) range += -cameraMotor.thirdPersonOffset.z;
        else if (cameraMotor.view == CameraMotor.CameraView.FIRST_PERSON) range += -cameraMotor.fpOffset.z;

        if (Physics.Raycast(ray, out hit, range, interactableMask) == true)
        {
            Interactable component = hit.collider.gameObject.GetComponent<Interactable>();

            if (component != null)
            {
                UIMaster.GetHook("look_display").Enable();
                UIMaster.SendText(UITextType.SUBTEXT, component.GetDisplayText());

                if (Input.GetButton("Interact"))
                {
                    component.Interact(this);
                }
            }
            else
            {
                UIMaster.GetHook("look_display").Disable();
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Swordfish.Terrain terrain = GameObject.Find("Terrain").GetComponent<Swordfish.Terrain>();

            terrain.ModifyTerrain((int)transform.position.x, (int)transform.position.z, 1);
            terrain.ModifyTerrain((int)transform.position.x, (int)transform.position.z + 1, 1);
            terrain.ModifyTerrain((int)transform.position.x + 1, (int)transform.position.z + 1, 1);
            terrain.ModifyTerrain((int)transform.position.x + 1, (int)transform.position.z, 1);
            terrain.ModifyTerrain((int)transform.position.x - 1, (int)transform.position.z + 1, 1);
            terrain.ModifyTerrain((int)transform.position.x - 1, (int)transform.position.z, 1);
            terrain.ModifyTerrain((int)transform.position.x, (int)transform.position.z - 1, 1);
            terrain.ModifyTerrain((int)transform.position.x + 1, (int)transform.position.z - 1, 1);

            transform.position += new Vector3(0, 2, 0);
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            Swordfish.Terrain terrain = GameObject.Find("Terrain").GetComponent<Swordfish.Terrain>();

            terrain.ModifyTerrain((int)transform.position.x, (int)transform.position.z, -1);
            terrain.ModifyTerrain((int)transform.position.x, (int)transform.position.z + 1, -1);
            terrain.ModifyTerrain((int)transform.position.x + 1, (int)transform.position.z + 1, -1);
            terrain.ModifyTerrain((int)transform.position.x + 1, (int)transform.position.z, -1);
        }
    }

    private void OnUpdate(object sender, EntityUpdateEvent e)
    {
        if (isSprinting) CostModifyEndurance(Constants.END_COST_SPRINT);
    }

    private void OnHit(object sender, EntityAttackHitEvent e)
    {
        //  Play a hit sound
        AudioSource.PlayClipAtPoint(GameMaster.GetAudioDatabase()?.Get("meleeHit")?.GetClip(), e.entity.transform.position);
    }

    private void OnMiss(object sender, EntityAttackMissEvent e)
    {
        //  Play a miss sound
        AudioSource.PlayClipAtPoint(GameMaster.GetAudioDatabase()?.Get("meleeMiss")?.GetClip(), transform.position);
    }

    private void OnPickup(object sender, Inventory.InventoryChangeEvent e)
    {
        if (e.reason != Inventory.InventoryChangeReason.ADD) return;

        //  Play a pickup sound
        AudioSource.PlayClipAtPoint(GameMaster.GetAudioDatabase()?.Get("itemPickup")?.GetClip(), transform.position);
    }
}
