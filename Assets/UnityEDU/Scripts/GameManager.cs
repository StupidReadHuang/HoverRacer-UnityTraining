//This script manages the timing and flow of the game. It is also responsible for telling
//the UI when and how to update

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
	//The game manager holds a public static reference to itself. This is often referred to
	//as being a "singleton" and allows it to be access from all other objects in the scene.
	//This should be used carefully and is generally reserved for "manager" type objects
	public static GameManager instance;		

	[Header("Race Settings")]
	public int numberOfLaps = 3;			//The number of laps to complete
	public VehicleMovement vehicleMovement;	//A reference to the ship's VehicleMovement script

	[Header("UI References")]
	public ShipUI shipUI;					//A reference to the ship's ShipUI script
	public LapTimeUI lapTimeUI;				//A reference to the LapTimeUI script in the scene
	public GameObject gameOverUI;			//A reference to the UI objects that appears when the game is complete

	[Header("Music Manager")]
	public GameObject musicManager;			//A reference to the Music Manager game object

	float[] lapTimes;						//An array containing the player's lap times
	int currentLap = 0;						//The current lap the player is on
	bool isGameOver;						//A flag to determine if the game is over
	bool raceHasBegun;						//A flag to determine if the race has begun

    public event Action<float> RaceFinished;//An event that can be subscribed to that will trigger when the race is finished.

	void Awake()
	{
		//If the variable instance has not be initialized, set it equal to this
		//GameManager script...
		if (instance == null)
			instance = this;
		//...Otherwise, if there already is a GameManager and it isn't this, destroy this
		//(there can only be one GameManager)
		else if (instance != this)
			Destroy(gameObject);


	}

	void OnEnable()
	{
		//When the GameManager is enabled, we start a coroutine to handle the setup of
		//the game. It is done this way to allow our intro cutscene to work. By slightly
		//delaying the start of the race, we give the cutscene time to take control and 
		//play out
		StartCoroutine(Init());
	}

	IEnumerator Init()
	{
		//Update the lap number on the ship
		UpdateUI_LapNumber();

		//Wait a little while to let everything initialize
		yield return new WaitForSeconds(.1f);

		//Initialize the lapTimes array and set that the race has begun
		lapTimes = new float[numberOfLaps];
		raceHasBegun = true;
	}

	void Update()
	{
		//Update the speed on the ship UI
		UpdateUI_Speed ();

		//If we have an active game...
		if (IsActiveGame())
		{
			//...calculate the time for the lap and update the UI
			lapTimes[currentLap] += Time.deltaTime;
			UpdateUI_LapTime();
		}
	}

	//Called by the FinishLine script
	public void PlayerCompletedLap()
	{
		//If the game is already over exit this method 
		if (isGameOver)
			return;

		//Incremement the current lap
		currentLap++;

		//Play lap complete sound
		musicManager.GetComponent<MusicMixer>().CompleteLap();

		//Update the lap number UI on the ship
		UpdateUI_LapNumber ();



		//If the player has completed the required amount of laps...
		if (currentLap >= numberOfLaps)
		{
			//...the game is now over...
			isGameOver = true;

			//Play game complete sound...
			musicManager.GetComponent<MusicMixer>().CompleteGame();

			//...update the laptime UI...
			UpdateUI_FinalTime();

			//...and show the Game Over UI
			gameOverUI.SetActive(true);
		}
	}

	void UpdateUI_LapTime()
	{
		//If we have a LapTimeUI reference, update it
		if (lapTimeUI != null)
			lapTimeUI.SetLapTime(currentLap, lapTimes[currentLap]);
	}

	void UpdateUI_FinalTime()
	{
		//If we have a LapTimeUI reference... 
		if (lapTimeUI != null)
		{
			float total = 0f;

			//...loop through all of the lapTimes and total up an amount...
			for (int i = 0; i < lapTimes.Length; i++)
				total += lapTimes[i];

			//... and update the final race time
			lapTimeUI.SetFinalTime(total);

            //... and notify any subscribers that the race ended.
            if (RaceFinished != null)
            {
                RaceFinished.Invoke(total);
            }
		}
	}

	void UpdateUI_LapNumber()
	{
		//If we have a ShipUI reference, update it
		if (shipUI != null) 
			shipUI.SetLapDisplay (currentLap + 1, numberOfLaps);
	}

	void UpdateUI_Speed()
	{
		//If we have a VehicleMovement and ShipUI reference, update it
		if (vehicleMovement != null && shipUI != null) 
			shipUI.SetSpeedDisplay (Mathf.Abs(vehicleMovement.speed));
	}

	public bool IsActiveGame()
	{
		//If the race has begun and the game is not over, we have an active game
		return raceHasBegun && !isGameOver;
	}

	public void RestartClicked()
	{
		//When button clicked wait to reload scene to allow for button sound to play then call Restart function
		Invoke ("Restart", 1f);
	}

	void Restart()
	{
		//Restart the scene by loading the scene that is currently loaded
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}


}
