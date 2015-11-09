
using UnityEngine;
using System.Collections;


/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Implementation of Changing Game defination at run time.
/// Not used in WUXIA
/// </summary>
public class LoadHUD : MonoBehaviour {

	public GameObject m_Prefab3x5;
	public GameObject m_Prefab4x5;


	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start()
	{
		string fileName = " ";
		string reelstrip = " ";
		if(GameVariables.Instance.GAME_DEFINATION == GameVariables.GAME_DEFINE.THREE_X_FIVE)
		{
			GameVariables.Instance.NUM_OF_ROWS = 4;
			fileName = "Icons_Altas";
			reelstrip = "Xml/reel_strips";
			m_Prefab4x5.SetActive(false);
			m_Prefab3x5.SetActive(true);
			
		}
		else
		{
			GameVariables.Instance.NUM_OF_ROWS = 5;
			fileName = "Icons_Altas_4x5" ;
			reelstrip = "Xml/reel_strips_4x5";
			m_Prefab3x5.SetActive(false);
			m_Prefab4x5.SetActive(true);
		}

		FileManager.Instance.LoadLinesDefinition(GameVariables.Instance.GAME_DEFINATION);
		FileManager.Instance.LoadReelStrips(reelstrip);
		Icons.Instance.ResetTempHeadTail ();

	}
	
	
	


	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update () {

		//toggle game defination
		if (Input.GetKeyDown (KeyCode.Equals) && WUXIA_SM.Instance.CURRENT_STATE == WUXIA_SM.STATE.GS_IDLE) 
		{
			Debug.Log (GameVariables.Instance.IS_TOGGLEMODE);
			//! toggle flag always true for 3x5 game, and false for 4x5 game.
			GameVariables.Instance.IS_TOGGLEMODE = !GameVariables.Instance.IS_TOGGLEMODE ;

			GameVariables.Instance.GAME_DEFINATION  
					=  (GameVariables.Instance.GAME_DEFINATION == GameVariables.GAME_DEFINE.THREE_X_FIVE) ?
				    	GameVariables.GAME_DEFINE.FOUR_X_FIVE : GameVariables.GAME_DEFINE.THREE_X_FIVE;

			string fileName = " ";
			string reelstrip = " ";
			if(GameVariables.Instance.GAME_DEFINATION == GameVariables.GAME_DEFINE.THREE_X_FIVE)
			{
				GameVariables.Instance.NUM_OF_ROWS = 4;
				fileName = "Icons_Altas";
				reelstrip = "Xml/reel_strips";
				m_Prefab4x5.SetActive(false);
				m_Prefab3x5.SetActive(true);
				LineButtons.Instance.GenerateLineButtons();
				InputManager.Instance.GenerateButtons();
			}
			else
			{
				GameVariables.Instance.NUM_OF_ROWS = 5;
				fileName = "Icons_Altas_4x5" ;
				reelstrip = "Xml/reel_strips_4x5";
				m_Prefab3x5.SetActive(false);
				m_Prefab4x5.SetActive(true);
				InputManager.Instance.GenerateMaxLineLabels();
				LineButtons.Instance.DestoryLineButtons();
			}
			//load specific win line xml file
		
			FileManager.Instance.LoadLinesDefinition(GameVariables.Instance.GAME_DEFINATION);
			FileManager.Instance.LoadReelStrips(reelstrip);
			Icons.Instance.DestroyIconSprites();
			// should use different icon set, fasle for main game / true for free game.
			GameVariables.Instance.ChangeLayoutSetting ();
			Icons.Instance.GenerateIconSprites(fileName);
			Icons.Instance.ResetTempHeadTail ();
		}
	
	}
	

	
}
