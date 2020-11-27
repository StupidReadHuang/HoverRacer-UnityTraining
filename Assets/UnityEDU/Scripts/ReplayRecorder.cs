using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

//Require a PlayerInput component
[RequireComponent(typeof(PlayerInput))]
public class ReplayRecorder : MonoBehaviour
{
    private PlayerInput input;
    private InputState lastFrame;
    private Replay replay = new Replay();

    public bool recording = true;

    public static int frameIndex = 0;

    private void Start()
    {
        //Initialize everything

        //Get the input component.
        input = GetComponent<PlayerInput>();
        //Set our last frame to something it would never be, so we always save the first frame.
        lastFrame = new InputState(float.NaN, float.NaN, true);
        //Subscribe to the race finished event.
        GameManager.instance.RaceFinished += OnRaceFinished;


        Debug.Log("Replay will save in: " + Replay.replayPath);
    }

    private void FixedUpdate()
    {
        if (recording)
        {
            //Get a new frame, and add it to the replay, if it's different from the last frame.
            var thisFrame = new InputState(input.Thruster, input.Rudder, input.IsBreaking);
            if (thisFrame != lastFrame)
            {
                replay.recording.Add(thisFrame);
            }
            //Set lastFrame
            lastFrame = thisFrame;
            frameIndex++;
        }
    }

    private void OnRaceFinished(float elapsedTime)
    {
        //The race has ended. Determine if we should save this replay.
        replay.elapsedTime = elapsedTime;
        recording = false;
        bool doSave = false;
        if (!File.Exists(Replay.replayPath))
        {
            //Always save if there's no replay already.
            doSave = true;
        }
        else
        {
            //Get the old replay's time and compare.
            //If this replay is faster, save over it.
            var oldReplay = Replay.Deserialize();
            if (elapsedTime < oldReplay.elapsedTime || oldReplay.elapsedTime <= 0)
            {
                doSave = true;
            }
            else
            {
                Debug.Log(string.Format("Replay won't save: new time is {0} and old time is {1}", elapsedTime, oldReplay.elapsedTime));
            }
        }

        if (doSave)
        {
            SaveReplay(elapsedTime);
        }
    }

    private void SaveReplay(float elapsedTime)
    {
        //Save the replay.
        try
        {
            //Create the target directory if it doesn't exist.
            if (!Directory.Exists(Path.GetDirectoryName(Replay.replayPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Replay.replayPath));
            }

            //Delete any existing replay.
            if (File.Exists(Replay.replayPath))
            {
                File.Delete(Replay.replayPath);
            }

            //Save the serialized replay.
            File.WriteAllText(Replay.replayPath, replay.Serialize());
            Debug.Log("Replay saved at: " + Replay.replayPath);
        }
        catch (Exception ex)
        {

            Debug.Log("There was an exception saving the replay.");
            Debug.LogException(ex);
        }
    }

    
}

/// <summary>
/// Replay Object. Contains a list of input states, as well as the total time of the replay.
/// </summary>
[System.Serializable]
public class Replay
{
    /// <summary>
    /// Finish time of the replay.
    /// </summary>
    public float elapsedTime = float.PositiveInfinity;
    
    /// <summary>
    /// The list of inputs that comprise the replay.
    /// </summary>
    public List<InputState> recording = new List<InputState>();

    /// <summary>
    /// The default location of the replay.
    /// </summary>
    public static readonly string replayPath = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Assets") ,"best.Replay");

    /// <summary>
    /// Create a string representing the replay object.
    /// </summary>
    /// <returns>The replay object as a string.</returns>
    public string Serialize()
    {
        return JsonUtility.ToJson(this, true);
    }

    /// <summary>
    /// Read a replay file from the default location.
    /// </summary>
    /// <returns>The saved replay.</returns>
    public static Replay Deserialize()
    {
        string replayContent = "";
        try
        {
            if (!Directory.Exists(Path.GetDirectoryName(replayPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(replayPath));
            }

            if (File.Exists(replayPath))
            {
                replayContent = File.ReadAllText(replayPath);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("There was an exception reading the replay.");
            Debug.LogException(ex);
        }
        return Deserialize(replayContent);
    }

    /// <summary>
    /// Gets a replay object deserialized from the parameter.
    /// </summary>
    /// <param name="replayContent">The serialized replay object.</param>
    /// <returns>The deserialized replay.</returns>
    public static Replay Deserialize(string replayContent)
    {
        try
        {
            return JsonUtility.FromJson<Replay>(replayContent) ?? Default();
        }
        catch (Exception ex)
        {
            Debug.Log("There was an exception deserializing the replay.");
            Debug.LogException(ex);
        }

        return Default();
    }

    /// <summary>
    /// Get a default replay object that never ends, but doesn't have any inputs.
    /// </summary>
    /// <returns>The default replay.</returns>
    public static Replay Default()
    {
        return new Replay()
        {
            elapsedTime = float.PositiveInfinity,
        };
    }
}

/// <summary>
/// An input state object. Contains input axes as well as an index.
/// </summary>
[System.Serializable]
public struct InputState
{
    public float thruster;
    public float rudder;
    public bool isBreaking;
    public int index;
    
    /// <summary>
    /// Construct an input state from a set of inputs.
    /// </summary>
    /// <param name="thruster">The current value of the Thuster axis.</param>
    /// <param name="rudder">The current value of the Rudder axis.</param>
    /// <param name="isBreaking">The current value of the Brake button.</param>
    public InputState(float thruster, float rudder, bool isBreaking)
    {
        this.thruster = thruster;
        this.rudder = rudder;
        this.isBreaking = isBreaking;
        index = ReplayRecorder.frameIndex;
    }

    #region equality overrides
    //Override equals to only consider the inputs.
    public static bool operator == (InputState a, InputState b)
    {
        return a.thruster == b.thruster && a.rudder == b.rudder && a.isBreaking == b.isBreaking;
    }

    public static bool operator != (InputState a, InputState b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj is InputState)
        {
            return this == (InputState)obj;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    #endregion
}
