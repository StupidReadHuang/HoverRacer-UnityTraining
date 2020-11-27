using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayLoader : MonoBehaviour
{
    public string replaySceneName;

#if UNITY_2018_4_OR_NEWER
    private PhysicsScene replayPhysicsScene;

    private void Start()
    {
        //Load the scene containing the replay
        LoadSceneParameters param = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
        SceneManager.LoadScene(replaySceneName, param);

        //Get the replay scene's physics scene.
        Scene replayScene = SceneManager.GetSceneByName(replaySceneName);
        replayPhysicsScene = replayScene.GetPhysicsScene();
    }

    private void FixedUpdate()
    {
        //Simulate replay scene on FixedUpdate.
        replayPhysicsScene.Simulate(Time.fixedDeltaTime);

    }
#endif
}
