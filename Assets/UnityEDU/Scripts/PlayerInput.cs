//This script handles reading inputs from the player and passing it on to the vehicle. We 
//separate the input code from the behaviour code so that we can easily swap controls 
//schemes or even implement and AI "controller". Works together with the VehicleMovement script

using UnityEngine;

public class PlayerInput : MonoBehaviour, IInput
{
    public string verticalAxisName = "Vertical";        //The name of the thruster axis
    public string horizontalAxisName = "Horizontal";    //The name of the rudder axis
    public string brakingKey = "Brake";                 //The name of the brake button

    //We hide these in the inspector because we want 
    //them public but we don't want people trying to change them
    [HideInInspector]
    public float Thruster { get; private set; }			  //The current thruster value
	[HideInInspector]
    public float Rudder { get; private set; }             //The current rudder value
    [HideInInspector]
    public bool IsBreaking { get; private set; }          //The current brake value

    void Update()
	{
		//If the player presses the Escape key and this is a build (not the editor), exit the game
		if (Input.GetButtonDown("Cancel") && !Application.isEditor)
			Application.Quit();

		//If a GameManager exists and the game is not active...
		if (GameManager.instance != null && !GameManager.instance.IsActiveGame())
		{
			//...set all inputs to neutral values and exit this method
			Thruster = Rudder = 0f;
			IsBreaking = false;
			return;
		}

		//Get the values of the thruster, rudder, and brake from the input class
		Thruster = Input.GetAxis(verticalAxisName);
		Rudder = Input.GetAxis(horizontalAxisName);
		IsBreaking = Input.GetButton(brakingKey);
	}
}
