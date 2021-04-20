using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

public enum GameState
{
    NONE,
    MENU,
    PAUSED,
    PLAYING,
    SPECTATING
}

public class GameMaster : Singleton<GameMaster>
{
    [SerializeField] private GameState state = GameState.PLAYING;
    public static GameState GetState() { return Instance.state; }
    public static void SetState(GameState state) { Instance.state = state; }

    [Header("References")]
    public Player player;
    public World world;

    [Header("Data")]
    public ItemDatabase itemDatabase;
    public AudioDatabase audioDatabase;
    public GameSettings gameSettings;

    [Header("Prefabs")]
    public GameObject droppedItemPrefab;

    public Timer masterTick = new Timer(0.1f);

    public static ItemDatabase GetItemDatabase() { return Instance.itemDatabase; }
    public static AudioDatabase GetAudioDatabase() { return Instance.audioDatabase; }
    public static GameSettings GetSettings() { return Instance.gameSettings; }

    public static World GetWorld() { return Instance.world; }

    public static Player GetPlayer() { return Instance.player; }
    public static void SetPlayer(Player player) { Instance.player = player; }

    public static bool MasterTicked() { return Instance.masterTick.IsDone(); }

    public static DroppedItem DropItem(Vector3 position, Item item, int amount = 1)
    {
        if (item == null) return null;
        return DropItem(position, new ItemStack(item, amount));
    }
    public static DroppedItem DropItem(Vector3 position, ItemStack stack)
    {
        if (stack == null) return null;

        GameObject obj = Instantiate(Instance.droppedItemPrefab, position, Quaternion.identity);
        DroppedItem drop = obj.GetComponent<DroppedItem>();

        drop.SetStack(stack);

        return drop;
    }

    public static DroppedItem DropItemNaturally(Vector3 position, Item item, int amount = 1) { return DropItemNaturally(position, new ItemStack(item, amount)); }
    public static DroppedItem DropItemNaturally(Vector3 position, ItemStack stack)
    {
        Vector3 offset = new Vector3(
                UnityEngine.Random.value * 3f - 1.5f,
                UnityEngine.Random.value * 2f + 1f,
                UnityEngine.Random.value * 3f - 1.5f
                );

        return DropItem(position + offset, stack);
    }

    public void Update()
    {
        masterTick.Tick();
    }
}