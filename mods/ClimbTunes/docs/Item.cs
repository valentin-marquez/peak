// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Item
using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using pworld.Scripts.Extensions;
using Sirenix.Utilities;
using Unity.Collections;
using UnityEngine;
using Zorro.ControllerSupport;
using Zorro.Core;
using Zorro.Core.Serizalization;

public class Item : MonoBehaviourPunCallbacks, IInteractible
{
	[Flags]
	public enum ItemTags
	{
		None = 0,
		Mystical = 1,
		PackagedFood = 2,
		Berry = 4,
		Mushroom = 8,
		BingBong = 0x10,
		GourmandRequirement = 0x20
	}

	[Serializable]
	public class ItemUIData
	{
		public string itemName;

		public Texture2D icon;

		public bool hasMainInteract = true;

		public string mainInteractPrompt;

		public bool hasSecondInteract;

		public string secondaryInteractPrompt;

		public bool hasScrollingInteract;

		public string scrollInteractPrompt;

		public bool canDrop = true;

		public bool canPocket = true;

		public bool canThrow = true;

		public bool isShootable;

		public Vector3 iconPositionOffset;

		public Vector3 iconRotationOffset;

		public float iconScaleOffset = 1f;
	}

	public static readonly int PROPERTY_INTERACTABLE = Shader.PropertyToID("_Interactable");

	public static List<Item> ALL_ACTIVE_ITEMS = new List<Item>();

	public Vector3 defaultPos;

	public Vector3 defaultForward = new Vector3(0f, 0f, 1f);

	public float mass = 5f;

	public ItemState itemState;

	[SerializeField]
	private int carryWeight = 1;

	public float usingTimePrimary;

	public bool showUseProgress = true;

	public Action OnPrimaryStarted;

	public Action OnPrimaryHeld;

	public Action OnPrimaryFinishedCast;

	public Action OnPrimaryReleased;

	public Action OnPrimaryCancelled;

	public Action OnConsumed;

	public Action OnSecondaryStarted;

	public Action OnSecondaryHeld;

	public Action OnSecondaryFinishedCast;

	public Action OnSecondaryCancelled;

	public Action<ItemState> OnStateChange;

	public Action<float> OnScrolled;

	public Action<float> OnScrolledMouseOnly;

	public Action OnScrollButtonLeft;

	public Action OnScrollButtonRight;

	public ItemUIData UIData;

	[NonSerialized]
	public Transform backpackSlotTransform;

	private Optionable<(byte, BackpackReference)> backpackReference;

	private Optionable<RigidbodySyncData> m_lastState = Optionable<RigidbodySyncData>.None;

	protected PhotonView view;

	public int totalUses = -1;

	public ItemInstanceData data;

	public ItemTags itemTags;

	public Rigidbody rig;

	internal ItemActionBase[] itemActions;

	[HideInInspector]
	public Collider[] colliders;

	public ushort itemID;

	private MaterialPropertyBlock mpb;

	public Renderer mainRenderer;

	private double timeSinceTick;

	private ItemComponent[] itemComponents;

	protected Color originalTint;

	private ItemPhysicsSyncer physicsSyncer;

	[HideInInspector]
	public ItemParticles particles;

	private int packLayer;

	public Vector3 centerOfMass;

	private Character lastHolderCharacter;

	[ReadOnly]
	public Character wearerCharacter;

	[SerializeField]
	[ReadOnly]
	private Character _holderCharacter;

	[ReadOnly]
	public Character overrideHolderCharacter;

	public bool canUseOnFriend;

	[HideInInspector]
	public bool finishedCast;

	[HideInInspector]
	public float lastFinishedCast;

	internal float overrideProgress;

	internal Optionable<bool> overrideUsability;

	public Action onStashAction;

	internal bool overrideForceProgress;

	private float timeSinceWasActive;

	public int CarryWeight => carryWeight + Ascents.itemWeightModifier;

	public bool isUsingPrimary { get; private set; }

	public ItemCooking cooking { get; private set; }

	public Character holderCharacter
	{
		get
		{
			if ((bool)overrideHolderCharacter)
			{
				return overrideHolderCharacter;
			}
			return _holderCharacter;
		}
		set
		{
			if (value != null)
			{
				lastHolderCharacter = value;
			}
			_holderCharacter = value;
		}
	}

	public Character trueHolderCharacter => _holderCharacter;

	public bool isUsingSecondary { get; private set; }

	public float castProgress { get; private set; }

	public float progress => Mathf.Max(overrideProgress, castProgress);

	public bool shouldShowCastProgress
	{
		get
		{
			if (!showUseProgress || !(castProgress > 0f) || finishedCast)
			{
				return overrideForceProgress;
			}
			return true;
		}
	}

	public float totalSecondaryUsingTime
	{
		get
		{
			if (!canUseOnFriend)
			{
				return usingTimePrimary;
			}
			return usingTimePrimary * 0.7f;
		}
	}

	public bool inActiveList { get; private set; }

	private void Awake()
	{
		view = GetComponent<PhotonView>();
		cooking = base.gameObject.GetOrAddComponent<ItemCooking>();
		AddPhysics();
		GetItemActions();
		AddPropertyBlock();
		particles = GetComponent<ItemParticles>();
		if (!particles)
		{
			particles = base.gameObject.AddComponent<ItemParticles>();
		}
		itemComponents = GetComponents<ItemComponent>();
		physicsSyncer = GetComponent<ItemPhysicsSyncer>();
	}

	private void Start()
	{
		if (!HasData(DataEntryKey.ItemUses))
		{
			OptionableIntItemData optionableIntItemData = GetData<OptionableIntItemData>(DataEntryKey.ItemUses);
			optionableIntItemData.HasData = totalUses != -1;
			optionableIntItemData.Value = totalUses;
			if (totalUses > 0)
			{
				SetUseRemainingPercentage(1f);
			}
		}
		if (!rig.isKinematic)
		{
			WasActive();
		}
		packLayer = 1 << LayerMask.NameToLayer("Exclude Collisions");
	}

	public string GetItemName(ItemInstanceData data = null)
	{
		int num = 0;
		IntItemData value;
		if (data == null)
		{
			num = GetData<IntItemData>(DataEntryKey.CookedAmount).Value;
		}
		else if (data.TryGetDataEntry<IntItemData>(DataEntryKey.CookedAmount, out value))
		{
			num = value.Value;
		}
		if (num < 4)
		{
			return num switch
			{
				1 => "Cooked " + UIData.itemName, 
				2 => "Well-done " + UIData.itemName, 
				3 => "Burnt " + UIData.itemName, 
				_ => UIData.itemName, 
			};
		}
		return "Incinerated " + UIData.itemName;
	}

	private void AddPropertyBlock()
	{
		mpb = new MaterialPropertyBlock();
		mainRenderer = GetComponentInChildren<MeshRenderer>();
		mainRenderer.GetPropertyBlock(mpb);
	}

	private void GetItemActions()
	{
		itemActions = GetComponentsInChildren<ItemActionBase>();
	}

	private void AddPhysics()
	{
		rig = base.gameObject.GetOrAddComponent<Rigidbody>();
		rig.mass = mass;
		centerOfMass = rig.centerOfMass;
		rig.interpolation = RigidbodyInterpolation.Interpolate;
		rig.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		colliders = GetComponentsInChildren<Collider>();
	}

	protected virtual void Update()
	{
		if (itemState == ItemState.InBackpack)
		{
			if (backpackSlotTransform == null || !backpackSlotTransform.UnityObjectExists())
			{
				base.transform.position = new Vector3(0f, -500f, 0f);
			}
			else
			{
				base.transform.position = backpackSlotTransform.position - backpackSlotTransform.rotation * centerOfMass * 0.5f;
				base.transform.rotation = backpackSlotTransform.rotation;
			}
		}
		else if (itemState == ItemState.Ground && PhotonNetwork.IsMasterClient)
		{
			if (base.transform.position.y < -2000f)
			{
				PhotonNetwork.Destroy(base.gameObject);
			}
		}
		else if (itemState == ItemState.Held)
		{
			WasActive();
		}
		UpdateEntryInActiveList();
		UpdateCollisionDetectionMode();
	}

	private void UpdateCollisionDetectionMode()
	{
		if (itemState == ItemState.Ground)
		{
			rig.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		}
		else
		{
			rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}
	}

	public virtual void Interact(Character interactor)
	{
		if (interactor.player.HasEmptySlot(itemID))
		{
			base.gameObject.SetActive(value: false);
			view.RPC("RequestPickup", RpcTarget.MasterClient, interactor.GetComponent<PhotonView>());
			Debug.Log("Picking up " + base.gameObject.name);
			if (TryGetComponent<ItemBackpackVisuals>(out var component))
			{
				component.RemoveVisuals();
			}
			GlobalEvents.TriggerItemRequested(this, interactor);
		}
	}

	[PunRPC]
	public void DenyPickupRPC()
	{
		base.gameObject.SetActive(value: true);
		SetKinematicNetworked(value: false, base.transform.position, base.transform.rotation);
	}

	[PunRPC]
	public void RequestPickup(PhotonView characterView)
	{
		Character component = characterView.GetComponent<Character>();
		ItemSlot slot;
		bool flag = component.player.AddItem(itemID, data, out slot);
		if (itemState != ItemState.InBackpack)
		{
			if (flag)
			{
				component.refs.view.RPC("OnPickupAccepted", component.player.photonView.Owner, slot.itemSlotID);
				PhotonNetwork.Destroy(view);
			}
			else
			{
				view.RPC("DenyPickupRPC", component.player.photonView.Owner);
			}
		}
		else
		{
			if (!this.backpackReference.IsSome)
			{
				return;
			}
			if (flag)
			{
				var (b, backpackReference) = this.backpackReference.Value;
				backpackReference.GetData().itemSlots[b].EmptyOut();
				if (backpackReference.type == BackpackReference.BackpackType.Item)
				{
					backpackReference.view.RPC("SetItemInstanceDataRPC", RpcTarget.Others, backpackReference.GetItemInstanceData());
				}
				else
				{
					Character component2 = backpackReference.view.GetComponent<Character>();
					ItemSlot[] itemSlots = component2.player.itemSlots;
					BackpackSlot backpackSlot = component2.player.backpackSlot;
					byte[] array = IBinarySerializable.ToManagedArray(new InventorySyncData(itemSlots, backpackSlot, component2.player.tempFullSlot));
					component2.player.photonView.RPC("SyncInventoryRPC", RpcTarget.Others, array, false);
				}
				component.refs.view.RPC("OnPickupAccepted", component.player.photonView.Owner, slot.itemSlotID);
				backpackReference.GetVisuals().RefreshVisuals();
			}
			else
			{
				view.RPC("DenyPickupRPC", component.player.photonView.Owner);
			}
		}
	}

	public Vector3 Center()
	{
		if (!mainRenderer.UnityObjectExists())
		{
			return base.transform.position;
		}
		return mainRenderer.bounds.center;
	}

	public Transform GetTransform()
	{
		return base.transform;
	}

	public virtual string GetInteractionText()
	{
		return "pick up";
	}

	public string GetName()
	{
		return UIData.itemName;
	}

	public virtual bool IsInteractible(Character interactor)
	{
		if (itemState == ItemState.Held)
		{
			return false;
		}
		if (itemState == ItemState.InBackpack)
		{
			return false;
		}
		return true;
	}

	internal void Move(Vector3 position, Quaternion rotation)
	{
		base.transform.position = position;
		base.transform.rotation = rotation;
		rig.position = position;
		rig.rotation = rotation;
		rig.linearVelocity *= 0f;
		rig.angularVelocity *= 0f;
	}

	private void SetColliders(bool enabled, bool isTrigger, bool excludeLayer = false)
	{
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = enabled;
			colliders[i].isTrigger = isTrigger;
		}
		if (excludeLayer)
		{
			rig.excludeLayers = 1 << LayerMask.NameToLayer("Default");
		}
		else
		{
			rig.excludeLayers = 0;
		}
	}

	internal void SetState(ItemState setState, Character character = null)
	{
		Debug.Log($"Setting Item State: {setState}");
		itemState = setState;
		OnStateChange?.Invoke(setState);
		switch (setState)
		{
		case ItemState.InBackpack:
			holderCharacter = null;
			rig.useGravity = false;
			rig.isKinematic = true;
			rig.interpolation = RigidbodyInterpolation.None;
			SetColliders(enabled: true, isTrigger: true);
			base.transform.localScale = Vector3.one * 0.5f;
			break;
		case ItemState.Ground:
			holderCharacter = null;
			rig.useGravity = true;
			rig.isKinematic = false;
			rig.interpolation = RigidbodyInterpolation.Interpolate;
			centerOfMass = rig.centerOfMass;
			if (this is Backpack)
			{
				wearerCharacter = null;
			}
			SetColliders(enabled: true, isTrigger: false);
			base.transform.localScale = Vector3.one;
			break;
		case ItemState.Held:
			holderCharacter = character;
			rig.useGravity = false;
			rig.isKinematic = false;
			rig.interpolation = RigidbodyInterpolation.Interpolate;
			if (this is Backpack)
			{
				wearerCharacter = null;
			}
			if (character != null && PhotonNetwork.IsMasterClient)
			{
				base.photonView.TransferOwnership(character.GetComponent<PhotonView>().Owner);
			}
			SetColliders(enabled: true, isTrigger: false, excludeLayer: true);
			base.transform.localScale = Vector3.one;
			break;
		}
	}

	private void HideRenderers()
	{
		GetComponentsInChildren<MeshRenderer>().ForEach(delegate(MeshRenderer meshRenderer)
		{
			meshRenderer.enabled = false;
		});
	}

	public virtual bool CanUsePrimary()
	{
		if (!overrideUsability.IsNone)
		{
			return overrideUsability.Value;
		}
		OptionableIntItemData optionableIntItemData = GetData<OptionableIntItemData>(DataEntryKey.ItemUses);
		if (!optionableIntItemData.HasData)
		{
			return true;
		}
		if (optionableIntItemData.Value != -1)
		{
			return optionableIntItemData.Value > 0;
		}
		return true;
	}

	public virtual bool CanUseSecondary()
	{
		bool flag = true;
		OptionableIntItemData optionableIntItemData = GetData<OptionableIntItemData>(DataEntryKey.ItemUses);
		if (optionableIntItemData.HasData)
		{
			flag = optionableIntItemData.Value == -1 || optionableIntItemData.Value > 0;
		}
		if (!flag)
		{
			return false;
		}
		if (canUseOnFriend)
		{
			if (Interaction.instance.hasValidTargetCharacter)
			{
				return true;
			}
		}
		else if (UIData.hasSecondInteract)
		{
			return true;
		}
		return false;
	}

	public void StartUsePrimary()
	{
		if (isUsingSecondary)
		{
			CancelUseSecondary();
		}
		isUsingPrimary = true;
		castProgress = 0f;
		finishedCast = false;
		if (OnPrimaryStarted != null)
		{
			OnPrimaryStarted();
		}
	}

	public void ContinueUsePrimary()
	{
		if (isUsingSecondary)
		{
			CancelUseSecondary();
		}
		if (!isUsingPrimary)
		{
			return;
		}
		if (usingTimePrimary > 0f)
		{
			castProgress += 1f / usingTimePrimary * Time.deltaTime;
			if (castProgress >= 1f)
			{
				if (OnPrimaryHeld != null)
				{
					OnPrimaryHeld();
				}
				if (!finishedCast)
				{
					FinishCastPrimary();
				}
			}
		}
		else
		{
			if (!finishedCast)
			{
				FinishCastPrimary();
			}
			if (OnPrimaryHeld != null)
			{
				OnPrimaryHeld();
			}
		}
	}

	protected virtual void FinishCastPrimary()
	{
		if ((bool)GetComponent<ItemUseFeedback>())
		{
			holderCharacter.refs.animator.SetBool(GetComponent<ItemUseFeedback>().useAnimation, value: false);
			if ((bool)GetComponent<ItemUseFeedback>().sfxUsed)
			{
				GetComponent<ItemUseFeedback>().sfxUsed.Play(base.transform.position);
			}
		}
		finishedCast = true;
		lastFinishedCast = Time.time;
		castProgress = 0f;
		if (OnPrimaryFinishedCast != null)
		{
			OnPrimaryFinishedCast();
		}
	}

	public void CancelUsePrimary()
	{
		isUsingPrimary = false;
		castProgress = 0f;
		finishedCast = false;
		if (OnPrimaryCancelled != null)
		{
			OnPrimaryCancelled();
		}
		if (Player.localPlayer == null)
		{
			Debug.LogError("Player.localPlayer is null, cannot play movement animation");
		}
		else if (Player.localPlayer.character == null)
		{
			Debug.LogError("Player.localPlayer.character is null, cannot play movement animation");
		}
		else if (Player.localPlayer.character.refs == null)
		{
			Debug.LogError("Player.localPlayer.character.refs is null, cannot play movement animation");
		}
		else if (Player.localPlayer.character.refs.animations == null)
		{
			Debug.LogError("Player.localPlayer.character.refs.animations is null, cannot play movement animation");
		}
		else
		{
			Player.localPlayer.character.refs.animations.PlaySpecificAnimation("Movement");
		}
	}

	public void ScrollButtonLeft()
	{
		if (OnScrollButtonLeft != null)
		{
			OnScrollButtonLeft();
		}
	}

	public void ScrollButtonRight()
	{
		if (OnScrollButtonRight != null)
		{
			OnScrollButtonRight();
		}
	}

	public void Scroll(float value)
	{
		if (OnScrolled != null)
		{
			OnScrolled(value);
		}
		if (InputHandler.GetCurrentUsedInputScheme() == InputScheme.KeyboardMouse && OnScrolledMouseOnly != null)
		{
			OnScrolledMouseOnly(value);
		}
	}

	public void StartUseSecondary()
	{
		if (!isUsingPrimary && !isUsingSecondary)
		{
			isUsingSecondary = true;
			castProgress = 0f;
			finishedCast = false;
			if ((bool)holderCharacter && canUseOnFriend && Interaction.instance.hasValidTargetCharacter)
			{
				base.photonView.RPC("SendFeedDataRPC", RpcTarget.All, holderCharacter.photonView.ViewID, Interaction.instance.bestCharacter.character.photonView.ViewID, (int)itemID, totalSecondaryUsingTime);
			}
			if (OnSecondaryStarted != null)
			{
				OnSecondaryStarted();
			}
		}
	}

	[PunRPC]
	internal void SendFeedDataRPC(int giverID, int recieverID, int itemID, float totalUsingTime)
	{
		GameUtils.instance.StartFeed(giverID, recieverID, (ushort)itemID, totalUsingTime);
	}

	[PunRPC]
	internal void RemoveFeedDataRPC(int giverID)
	{
		GameUtils.instance.EndFeed(giverID);
	}

	public void ContinueUseSecondary()
	{
		if (isUsingPrimary || !isUsingSecondary)
		{
			return;
		}
		if (usingTimePrimary > 0f)
		{
			castProgress += 1f / totalSecondaryUsingTime * Time.deltaTime;
			if (castProgress >= 1f)
			{
				if (OnSecondaryHeld != null)
				{
					OnSecondaryHeld();
				}
				if (!finishedCast)
				{
					FinishCastSecondary();
				}
			}
		}
		else if (OnSecondaryHeld != null)
		{
			OnSecondaryHeld();
		}
	}

	public void FinishCastSecondary()
	{
		finishedCast = true;
		lastFinishedCast = Time.time;
		castProgress = 0f;
		if (canUseOnFriend && Interaction.instance.hasValidTargetCharacter)
		{
			if ((bool)holderCharacter)
			{
				holderCharacter.data.lastConsumedItem = Time.time;
				base.photonView.RPC("RemoveFeedDataRPC", RpcTarget.All, holderCharacter.photonView.ViewID);
			}
			Interaction.instance.bestCharacter.character.FeedItem(this);
			base.photonView.RPC("RemoveFeedDataRPC", RpcTarget.All, (int)itemID);
		}
		else if (OnSecondaryFinishedCast != null)
		{
			OnSecondaryFinishedCast();
		}
	}

	public void CancelUseSecondary()
	{
		isUsingSecondary = false;
		castProgress = 0f;
		finishedCast = false;
		if (OnSecondaryCancelled != null)
		{
			OnSecondaryCancelled();
		}
		Player.localPlayer.character.refs.animations.PlaySpecificAnimation("Movement");
		if ((bool)lastHolderCharacter)
		{
			base.photonView.RPC("RemoveFeedDataRPC", RpcTarget.All, lastHolderCharacter.photonView.ViewID);
		}
	}

	public IEnumerator ConsumeDelayed(bool ignoreActions = false)
	{
		if (!ignoreActions && OnConsumed != null)
		{
			OnConsumed();
		}
		yield return null;
		base.photonView.RPC("Consume", RpcTarget.All);
	}

	[PunRPC]
	public void Consume()
	{
		if (holderCharacter != null)
		{
			_ = holderCharacter.gameObject.name;
		}
		if ((bool)holderCharacter && holderCharacter.data.currentItem == this)
		{
			Optionable<byte> currentSelectedSlot = holderCharacter.refs.items.currentSelectedSlot;
			holderCharacter.refs.animator.SetBool("Consumed Item", value: true);
			GlobalEvents.TriggerItemConsumed(this, holderCharacter);
			if (holderCharacter.IsLocal)
			{
				if (currentSelectedSlot.IsSome)
				{
					holderCharacter.player.EmptySlot(currentSelectedSlot);
					holderCharacter.refs.items.EquipSlot(currentSelectedSlot);
				}
				else
				{
					Debug.LogError("No Item Selected locally but still consuming?? THIS IS BAD. CALL ZORRO");
				}
			}
			holderCharacter.data.lastConsumedItem = Time.time;
		}
		base.gameObject.SetActive(value: false);
	}

	public virtual void OnStash()
	{
		onStashAction?.Invoke();
		CancelUsePrimary();
		CancelUseSecondary();
	}

	[ContextMenu("Add Default Food Scripts")]
	public void AddDefaultFoodScripts()
	{
		usingTimePrimary = 1.2f;
		Action_PlayAnimation action_PlayAnimation = base.gameObject.AddComponent<Action_PlayAnimation>();
		action_PlayAnimation.OnPressed = true;
		action_PlayAnimation.animationName = "PlayerEat";
		Action_ModifyStatus action_ModifyStatus = base.gameObject.AddComponent<Action_ModifyStatus>();
		action_ModifyStatus.OnCastFinished = true;
		action_ModifyStatus.statusType = CharacterAfflictions.STATUSTYPE.Hunger;
		action_ModifyStatus.changeAmount = -0.1f;
		base.gameObject.AddComponent<Action_Consume>().OnCastFinished = true;
	}

	public void HoverEnter()
	{
		mpb.SetFloat(PROPERTY_INTERACTABLE, 1f);
		GetComponentInChildren<MeshRenderer>().SetPropertyBlock(mpb);
	}

	public void HoverExit()
	{
		mpb.SetFloat(PROPERTY_INTERACTABLE, 0f);
		GetComponentInChildren<MeshRenderer>().SetPropertyBlock(mpb);
	}

	public void SetKinematicNetworked(bool value)
	{
		base.photonView.RPC("SetKinematicRPC", RpcTarget.AllBuffered, value, base.transform.position, base.transform.rotation);
	}

	public void SetKinematicNetworked(bool value, Vector3 position, Quaternion rotation)
	{
		base.photonView.RPC("SetKinematicRPC", RpcTarget.AllBuffered, value, position, rotation);
	}

	[PunRPC]
	public void SetKinematicRPC(bool value, Vector3 position, Quaternion rotation)
	{
		rig.isKinematic = value;
		rig.position = position;
		rig.rotation = rotation;
	}

	public bool HasData(DataEntryKey key)
	{
		if (data == null)
		{
			return false;
		}
		return data.HasData(key);
	}

	public T GetData<T>(DataEntryKey key, Func<T> createDefault) where T : DataEntryValue, new()
	{
		if (data == null)
		{
			data = new ItemInstanceData(Guid.NewGuid());
			ItemInstanceDataHandler.AddInstanceData(data);
		}
		if (data.TryGetDataEntry<T>(key, out var value))
		{
			return value;
		}
		if (createDefault != null)
		{
			return data.RegisterEntry(key, createDefault());
		}
		return data.RegisterNewEntry<T>(key);
	}

	public T GetData<T>(DataEntryKey key) where T : DataEntryValue, new()
	{
		return GetData<T>(key, null);
	}

	internal void ForceSyncForFrames()
	{
		if (physicsSyncer != null)
		{
			physicsSyncer.ForceSyncForFrames();
		}
	}

	[PunRPC]
	public void SetItemInstanceDataRPC(ItemInstanceData instanceData)
	{
		data = instanceData;
		if (data != null)
		{
			OnInstanceDataRecieved();
			ItemComponent[] array = itemComponents;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].OnInstanceDataSet();
			}
		}
	}

	public virtual void OnInstanceDataRecieved()
	{
	}

	public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);
		if (PhotonNetwork.IsMasterClient)
		{
			ForceSyncForFrames();
			ItemState itemState = this.itemState;
			if ((itemState == ItemState.Ground || itemState == ItemState.Held || itemState == ItemState.InBackpack) && data != null)
			{
				view.RPC("SetItemInstanceDataRPC", newPlayer, data);
			}
			if (this.itemState == ItemState.InBackpack)
			{
				var (b, backpackReference) = this.backpackReference.Value;
				view.RPC("PutInBackpackRPC", newPlayer, b, backpackReference);
			}
			if (rig.isKinematic)
			{
				view.RPC("SetKinematicRPC", newPlayer, rig.isKinematic, rig.position, rig.rotation);
			}
		}
	}

	[PunRPC]
	public void PutInBackpackRPC(byte slotID, BackpackReference backpackReference)
	{
		Transform[] backpackSlots = backpackReference.GetVisuals().backpackSlots;
		this.backpackReference = Optionable<(byte, BackpackReference)>.Some((slotID, backpackReference));
		backpackSlotTransform = backpackSlots[slotID];
		SetState(ItemState.InBackpack);
		backpackReference.GetVisuals().SetSpawnedBackpackItem(slotID, this);
		if (backpackReference.IsOnMyBack())
		{
			HideRenderers();
		}
	}

	[PunRPC]
	public void SetCookedAmountRPC(int amount)
	{
		GetData<IntItemData>(DataEntryKey.CookedAmount).Value = amount;
		cooking.UpdateCookedBehavior();
	}

	public void SetUseRemainingPercentage(float percentage)
	{
		GetData<FloatItemData>(DataEntryKey.UseRemainingPercentage).Value = Mathf.Clamp01(percentage);
	}

	public void WasActive()
	{
		if (!inActiveList)
		{
			ALL_ACTIVE_ITEMS.Add(this);
		}
		inActiveList = true;
		timeSinceWasActive = 0f;
	}

	private void UpdateEntryInActiveList()
	{
		if (inActiveList)
		{
			timeSinceWasActive += Time.deltaTime;
			if (timeSinceWasActive > 30f)
			{
				RemoveFromActiveList();
			}
		}
	}

	private void RemoveFromActiveList()
	{
		if (inActiveList)
		{
			ALL_ACTIVE_ITEMS.Remove(this);
			inActiveList = false;
		}
	}

	private void OnDestroy()
	{
		RemoveFromActiveList();
	}

	public bool TryGetFeeder(out Character feeder)
	{
		if (trueHolderCharacter != null && trueHolderCharacter != holderCharacter)
		{
			feeder = trueHolderCharacter;
			return true;
		}
		feeder = null;
		return false;
	}

	public bool IsValidToSpawn()
	{
		LootData component = GetComponent<LootData>();
		if ((bool)component)
		{
			return component.IsValidToSpawn();
		}
		return true;
	}
}
