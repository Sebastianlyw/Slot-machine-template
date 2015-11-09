#region NameSpace
using UnityEngine;
using System.Collections;
#endregion


/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// Combination test(Free Game) editor.
/// 
/// </summary>
public class FgEditor : SlotMachineEditor {
	#region Variables

	//! Singleton
	private static FgEditor instance;
	
	//!Constuct
	private FgEditor() {}
	
	//! Instance
	public static FgEditor Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(FgEditor)) as FgEditor;
			
			return instance;
		}
		
	}
	//! Singleton end


	private const short NUM_FGTOEDITE = 20;
	private int m_GameID ; // 0 - 19
	private int m_Col;
	private bool m_IsNewGame;
	public bool[] IS_FG_EDITED;
	#endregion

	/// <summary> 
	/// Use this for local variable initialization
	/// </summary>
	void Awake()
	{
		m_GameID = 0;
		m_Col = GameVariables.Instance.NUM_OF_COLS;
		m_IsNewGame = false;  // true if turn to a new game.
		IS_FG_EDITED = new bool[NUM_FGTOEDITE];
		ResetFg ();

		//even not entring editor, we still need to initialize container.
	}

	private void ResetFg()
	{
		for (int i = 0; i < NUM_FGTOEDITE; ++i)
			IS_FG_EDITED [i] = false;
	}
	/// <summary> 
	/// Initialize Datas used in free game mode.
	/// </summary>
	public void InitFreeGameEditor()
	{
		//update reel strips
		GameVariables.Instance.SetCurrentStrip (false);

		InitEditor ();

		Icons.Instance.DestroyIconSprites();
		//generate ioncs to display from different reel strips.
		string iconName = "";
		if(GameVariables.Instance.IsThreeXFiveGame())
		{
			iconName = "Icons_Altas";
		}
		else
		{
			iconName = "Icons_Altas_4x5";
		}
		Icons.Instance.GenerateIconSprites(iconName);

		Icons.Instance.ResetTempHeadTail ();

		//store default (zeor position) head/tail info to container.
		Icons.Instance.StoreHeadTails_FG();

		ResetFg ();
	}

	/// <summary> 
	/// Update editor status, should be called per frame if game is in Free game editor state.
	/// </summary>
	public void UpdateFGEditor()
	{
		Debug.Log ("gameID: " + m_GameID);
		Debug.Log ("Current COlumn: " + m_CurrentColumn);
		// m_currentColumn for free game editor is from 0 - 99. for main game is only 0 - 4

		ControlUpdate ();

		if (m_IsNewGame) 
		{
			//save old game data before going to next new game.
			Icons.Instance.SaveFGEditorContainer (m_GameID );
			IS_FG_EDITED[m_GameID] = true;

			//make sure update m_GameId after overwirting data in container for previous game.
			m_GameID = m_CurrentColumn / m_Col;

			//load new head/tail for  new game.
			Icons.Instance.LoadEGEditorData(m_GameID);
			//render new icons for new game.!!!
			Icons.Instance.IconsInitiate_FG(m_GameID);

			m_IsNewGame = false;
		}

		if (Input.GetKeyDown (KeyCode.J)) 
		{
			//store the current  game before ending editiing.
			Icons.Instance.SaveFGEditorContainer (m_GameID );
			IS_FG_EDITED[m_GameID] = true;
			DestoryMaker();

			//set state to exit editor
			GameVariables.Instance.EDITOR_FLAG  = GameVariables.EDIT_STATES.FG_EDIT_END;
		
			//Reset head/tail to normal game reel strip version!!!
			GameVariables.Instance.SetCurrentStrip (true);
			Icons.Instance.ResetTempHeadTail( );

			TextAndDigitDisp.Instance.SetMessageEditor (" ");
			ResetBG();

			Icons.Instance.ResetIcons ();
		}

	}

	/// <summary> 
	/// Update static icons showing on screen base on user's input.
	/// </summary>
	private void ControlUpdate()
	{
		// input update start
		if (Input.GetKeyDown (KeyCode.LeftArrow) )
		{
			if( (m_ReelMaker.transform.position.x > LEFT_EDGE) ) 
			{
				m_ReelMaker.transform.position -= new Vector3(m_Xoffset , 0, 0);
				--m_CurrentColumn;
			} 
			else if(m_ReelMaker.transform.position.x < LEFT_EDGE && m_GameID  > 0)
			{
				m_ReelMaker.transform.position += new Vector3( (m_Col -1) *  m_Xoffset , 0, 0);
				--m_CurrentColumn;
				m_IsNewGame = true;
			}
		}
		else if (Input.GetKeyDown (KeyCode.RightArrow) )
		{
			if( (m_ReelMaker.transform.position.x < RIGHT_EDGE ) )
			{
				++m_CurrentColumn;	
				m_ReelMaker.transform.position += new Vector3(m_Xoffset , 0, 0);
			}
			else if((m_ReelMaker.transform.position.x >RIGHT_EDGE) && m_GameID < 19)
			{
				++m_CurrentColumn;	
				m_ReelMaker.transform.position -= new Vector3( (m_Col-1) *  m_Xoffset , 0, 0);
				m_IsNewGame = true;
			}
		}
		
		if (Input.GetKeyDown (KeyCode.UpArrow)) 
		{
			Icons.Instance.SpinSingleReel (true, (short) (m_CurrentColumn % m_Col));
			//Casting ^^^^^  to (0,4)
		}
		else if (Input.GetKeyDown (KeyCode.DownArrow)) 
		{
			Icons.Instance.SpinSingleReel (false, (short) (m_CurrentColumn % m_Col));
		}
		// Input update end
		
	}






}
