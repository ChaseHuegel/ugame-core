using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

[CreateAssetMenu(fileName = "New Item", menuName = "Swordfish/Audio/SoundElement")]
public class SoundElement : ScriptableObject
{
    [SerializeField] private AudioClip[] clips;

    public AudioClip GetClip()
    {
        return clips[
            clips.Length > 1 ? Random.Range(0, clips.Length) : 0
        ];
    }
}

}