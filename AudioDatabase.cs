using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

[CreateAssetMenu(fileName = "New Database", menuName = "Swordfish/Database/Audio")]
public class AudioDatabase : ScriptableObject
{
    [SerializeField] private List<SoundElement> database = new List<SoundElement>();

    public SoundElement Get(string name)
    {
        foreach (SoundElement item in database)
        {
            if (item.name == name)
            {
                return item;
            }
        }

        return new SoundElement();
    }
}