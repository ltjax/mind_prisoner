using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAudio : MonoBehaviour {

    [Range(0, 1)]
    public float Volume;
    public AudioClip[] StepCycle;
    private GameObject mySpeaker;
    private List<AudioSource> Sources;
    private int currentStep = 0;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(StepCycle.Length > 0);
        mySpeaker = SpawnSpeaker();
    }

    private void PlayClip(AudioClip c) {
        Debug.Assert(mySpeaker != null);
        // Initialize source list if necessary
        if(Sources == null) {
            Sources = new List<AudioSource>();
        }

        // Search for a free audio source
        AudioSource src = null;
        for(int i = 0; i < Sources.Count; i++) {
            if(!Sources[i].isPlaying) {
                src = Sources[i];
                break;
            }
        }

        // If none found, add a new one
        if(src == null) {
            src = mySpeaker.AddComponent<AudioSource>();
            src.bypassEffects = true;
            src.playOnAwake = false;
            src.loop = false;
            src.dopplerLevel = 0;
            src.spatialBlend = 0;
            Sources.Add(src);
        }

        // Play the clip
        src.volume = Volume;
        src.clip = c;
        src.Play();
    }

    public void PlayStep() {
        PlayClip(StepCycle[currentStep]);
        currentStep = (currentStep + 1) % StepCycle.Length;
    }

    // Spawns empty subordinate object for playing sounds
    private GameObject SpawnSpeaker() {
        GameObject result = new GameObject();
        result.name = "Speaker";
        result.transform.parent = transform;
        result.transform.localPosition = Vector3.zero;
        result.transform.localRotation = Quaternion.identity;
        return result;
    }
}
