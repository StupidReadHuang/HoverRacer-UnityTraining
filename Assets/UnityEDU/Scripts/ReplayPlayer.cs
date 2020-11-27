using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReplayPlayer : MonoBehaviour, IInput
{
    //Keeps track of the current frame number of the simulation.
    private int frameIndex;
    //Keeps track of our position in the replay. Since some
    //frames are discarded, these are not neccessarially the
    //same thing.
    private int replayIndex;

    //Properties of an IInput interface.
    public bool IsBreaking { get; private set; }
    public float Rudder { get; private set; }
    public float Thruster { get; private set; }

    //The loaded replay
    private Replay replay;

    private void Start()
    {
        frameIndex = replayIndex = 0;

        //Load the replay if it exists. Disable the replay otherwise.
        if (!File.Exists(Replay.replayPath))
        {
            transform.root.gameObject.SetActive(false);
        }
        else
        { 
            replay = Replay.Deserialize();
        }
    }

    private void FixedUpdate()
    {
        //Every fixed update frame, advance through the 
        //replay until we've found the correct frame
        for (int i = replayIndex; i < replay.recording.Count; i++)
        {
            if (frameIndex >= replay.recording[i].index)
            {
                replayIndex = i;
            }
            else
            {
                break;
            }
        }

        //Apply that frame's input to our current input.
        Thruster = replay.recording[replayIndex].thruster;
        Rudder = replay.recording[replayIndex].rudder;
        IsBreaking = replay.recording[replayIndex].isBreaking;

        frameIndex++;
    }
}
