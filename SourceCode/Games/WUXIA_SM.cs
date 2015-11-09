#region Namespaces
using UnityEngine;
using System.Collections;
#endregion


/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// State Machine is a overview of whole game flow chart.
/// In WU XIA Game, the flow chart for free games is different
/// from other projecrts. Before spining there is a check win state
/// already. the WUXIA algorithm will run at that stage.             
/// </summary>
public class WUXIA_SM : MonoBehaviour {

	#region Variables

	/* Start of  Singleton */
	private static WUXIA_SM instance;  
	//! Constuct
	private WUXIA_SM() {}
	//! Instance
	public static WUXIA_SM Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(WUXIA_SM)) as WUXIA_SM;	
			return instance;
		}	
	}
	/*End of  Singleton */


	//! States for whole game.
	public enum STATE
	{
		GS_IDLE,
		GS_REEL_SPIN_NG,
		GS_CHECK_WIN,
		GS_REEL_SPIN_FG,
		GS_COUNTING_FG,  //! counting up the total credits with the win amount earned in free game. 
		GS_CHECK_WIN_FG, 		 /*!< speical state which is only for WUXIA game, occurs before each free game spin*/
		GS_WIN_ANIMATION,
		GS_TAKE_WIN,  			 /*!< reason need a state for take win is to show amount for a certain time. */
		GS_NG_EDITOR,
		GS_FG_EDITOR,
		GS_DELAY,
		GS_END
	}
	public STATE CURRENT_STATE 	= STATE.GS_IDLE;
	public STATE NEXT_STATE 	= STATE.GS_IDLE;
	private STATE m_TempNS;  //! store the incoming next state temporary.
	
	public bool isPasue = false;
	//! inner state for each main state of game.
	public enum INNER_STATE
	{
		IS_ENTER,
		IS_UPDATE,
		IS_EXIT
	}
	private INNER_STATE m_InnerSate = INNER_STATE.IS_ENTER;
	
	//! for take win state. decide how long win amout stay at screen after taking win.
	private float m_WinShowTime;
	
	//!Flag variable for DelayAndSetStateTO fucntion.
	private bool isExecuting = false;  

	

	/// <summary> 
	/// Initialize function which pre-excuate before all start functions.
	/// </summary>
	void Awake()
	{
		CURRENT_STATE 	= STATE.GS_IDLE;
		NEXT_STATE 	= STATE.GS_IDLE;
		m_TempNS = STATE.GS_IDLE;
		m_InnerSate = INNER_STATE.IS_ENTER;
		isExecuting = false; 
		m_WinShowTime = GameVariables.SHOW_WIN_TIME;
	}
	
	/// <summary> 
	/// Use this for initialization
	/// </summary>
	void Start () {
	}
	#endregion

	#region Helper_functions

	/// <summary>
	/// set the next State of game with delay time.
	/// Notice: all delay will cause update block of the state to excute more than one time,it may bring some unexpected bug, need to use delay carefully.
	/// </summary>
	/// <param name="_time"> Delay time </param>
	/// <param name="_state"> A <see cref="STATE"/> will be set to next state.</param>
	void DelayAndSetStateTO(float _time, STATE _state)
	{
		if(isExecuting == false)
			StartCoroutine(SetState(_time, _state) );
	}

	/// <summary> 
	/// IEnumerator for DelayAndSetStateTO fucntion.
	/// </summary>
	IEnumerator SetState(float _time, STATE _state){
		isExecuting = true;
		yield return new WaitForSeconds(_time);
		isExecuting = false;
		NEXT_STATE = _state;
	}
	

	//! _ns  : store really next state to go in delay time.
	void PauseGame(float _dt)
	{
		STATE _nS = NEXT_STATE;
		NEXT_STATE = STATE.GS_DELAY;
		if(isExecuting == false)
			StartCoroutine(GoDelayState(_dt, _nS) );
	}

	IEnumerator GoDelayState(float _dt, STATE _ns)
	{
		isExecuting = true;
		yield return new WaitForSeconds (_dt);
		isExecuting = false;
		NEXT_STATE = _ns;
	}


	/// <summary> 
	/// This is a help function to update game meters at begining of each spin.
	/// </summary>
	private void UpdateMeters()
	{
		GameVariables.Instance.SetCurrentCredit(GameVariables.Instance.GetCurrentCredit()  
		                                        - GameVariables.Instance.GetTotalBetCredit());
		TextAndDigitDisp.Instance.SetCreditAmount(GameVariables.Instance.GetCurrentCredit());
		GameVariables.Instance.IS_SPIN = true;
		
		//update(print out) total bet for new game before every new spin
		InputManager.Instance.PrintBetAmout();
		
		//reset win amount to zero at beginning of spin.
		TextAndDigitDisp.Instance.SetWinAmount(0);
	}



	#endregion

	#region State_Machine
	/// <summary> 
	/// Udpate State Machine once per frame.
	/// </summary>
	void Update () {

	if(isPasue)
			return;
		if (CURRENT_STATE != NEXT_STATE) 
		{
			Debug.Log (CURRENT_STATE + "->" + NEXT_STATE);
		//	Debug.Log (m_InnerSate);
			CURRENT_STATE = NEXT_STATE;
		}
		if(Input.GetKeyDown(KeyCode.Alpha0) )
			Debug.Log (CURRENT_STATE);



		// Main Game Body. 
		switch (CURRENT_STATE) 
		{

		case STATE.GS_IDLE:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER:   
				if(!GameVariables.Instance.IS_FREEGAME && FreeGame.Instance.IsGMTransition)
				{
					FreeGame.Instance.FreeGameEnd();
				}
				m_InnerSate = INNER_STATE.IS_UPDATE;
				break;
			
			case INNER_STATE.IS_UPDATE:

				//! Still play animation after last spin of feature game. 
				//! function inside will self check validation 
				if(FreeGame.Instance.IsExitFreeGame())
				{
					LineAnim.Instance.DrawPlayLines();
					IconAnim.Instance.PlayAnimation();
				}
				//Check Player Input when Idle state, 3 cases:
				//1. update bet information accordingly.
				//2. start next spin ( main or free game)
				//3. Entry the combination test(editor) mode. 
				InputManager.Instance.InputUpdate();
				if(FreeGame.Instance.IsGMTransition && !GameVariables.Instance.IS_FREEGAME  && InputManager.Instance.IsCountingFGwin != 3)
				{
					m_TempNS = STATE.GS_COUNTING_FG;
					m_InnerSate = INNER_STATE.IS_EXIT;
				}
				//Go to Main Game spin.
				else if(GameVariables.Instance.IS_SPIN  &&
				  		 !GameVariables.Instance.IS_FREEGAME)
				{		
					m_TempNS = STATE.GS_REEL_SPIN_NG;
					m_InnerSate = INNER_STATE.IS_EXIT;
				}
				// press "j" to start free games.
				else if(GameVariables.Instance.IS_FREEGAME )
				     // || FreeGame.Instance.FG_COUNTER > 0))
				{
					m_TempNS = STATE.GS_REEL_SPIN_FG;
					m_InnerSate = INNER_STATE.IS_EXIT;
				}
				else if (GameVariables.Instance.EDITOR_FLAG  == GameVariables.EDIT_STATES.NG_IDLE)
				{	
					m_TempNS = STATE.GS_NG_EDITOR;
					m_InnerSate = INNER_STATE.IS_EXIT;
				}
				break;
				
			case INNER_STATE.IS_EXIT:	
				NEXT_STATE = m_TempNS;
				if(NEXT_STATE == STATE.GS_COUNTING_FG)
					PauseGame(1.8f);

				m_InnerSate = INNER_STATE.IS_ENTER; 
				break;
			}
			break;

		case STATE.GS_COUNTING_FG:
			switch(m_InnerSate)
			{
				case INNER_STATE.IS_ENTER:   
				m_InnerSate = INNER_STATE.IS_UPDATE;
				InputManager.Instance.IsCountingFGwin = 1; // reset this flag value.
				AnimManager.Instance.IsEndCounFG_Win = false;

				break;
				case INNER_STATE.IS_UPDATE:

				InputManager.Instance.CheckCountingStart();

				if(FreeGame.Instance.IsExitFreeGame())
				{
					LineAnim.Instance.DrawPlayLines();
					IconAnim.Instance.PlayAnimation();
				}

				if(InputManager.Instance.IsCountingFGwin == 3)
				{
					m_TempNS = STATE.GS_IDLE;
					m_InnerSate =INNER_STATE.IS_EXIT;

				}
				else if(InputManager.Instance.IsCountingFGwin == 2)
				{
					AnimManager.Instance.CountUpFreeGameWin();
				}
				break;
			case INNER_STATE.IS_EXIT:	
				NEXT_STATE = m_TempNS;
				m_InnerSate = INNER_STATE.IS_ENTER; 
				break;
			}
			break;
			// Mian Game Spin. Random genarate output.
		case STATE.GS_REEL_SPIN_NG:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER:   
				// Reset varaibles about counting free gaming win behaviour.
				if(InputManager.Instance.IsCountingFGwin == 3)
				{
					FreeGame.Instance.IsGMTransition = false;
					InputManager.Instance.IsCountingFGwin = 1;
					AudioManager.Instance.StopSound("GameTransition");
				}

				// only update meters( amount from last spin) when new spinning happen.
				UpdateMeters();
				IconAnim.Instance.Reset();
				// Assign proper reel srtips(true -> Main game) to use. 
				GameVariables.Instance.SetCurrentStrip (true);

				// reset head/tail index to intitial postion( spining function will use them).
				Icons.Instance.ResetTempHeadTail();

				Icons.Instance.GernerateResult();

				m_InnerSate = INNER_STATE.IS_UPDATE; 
				break;

			case INNER_STATE.IS_UPDATE:
				if(!GameVariables.Instance.IS_SPIN)
				{	//if spin finshed , go to check win state.
					m_TempNS = STATE.GS_CHECK_WIN;
					m_InnerSate = INNER_STATE.IS_EXIT;
				} 
				else
				{  
					Icons.Instance.SpinReels(false); 
				}
				break;		
				
			case INNER_STATE.IS_EXIT:
				NEXT_STATE = m_TempNS;
				m_InnerSate = INNER_STATE.IS_ENTER; 
				break;
			}
			break;

			//Notice: For WUXIA GAME, CONFIRM WIN WHEN STATE BECOME GS_CHECK_WIN
		case STATE.GS_CHECK_WIN:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER:   
				m_InnerSate = INNER_STATE.IS_UPDATE; 
				break;
			case INNER_STATE.IS_UPDATE:
				// Go to Animation state if got win or trigger free games.
				if( WinManager.Instance.IsGameWin() || WinManager.Instance.IsTriggerFG () )
				{
					m_TempNS = STATE.GS_WIN_ANIMATION;
					m_InnerSate = INNER_STATE.IS_EXIT;
				}
				else
				{
					FreeGame.Instance.CheckFreeGame();
					m_TempNS = STATE.GS_IDLE;
					m_InnerSate = INNER_STATE.IS_EXIT;
				}
				
				break;		
				
			case INNER_STATE.IS_EXIT:
				NEXT_STATE = m_TempNS;
				m_InnerSate = INNER_STATE.IS_ENTER; 
				break;
			}
			break;

			// Come to this state means we have won the current game.
			// so beside the animtaion behaviour, we also need to handle some win/bet amount update.
		case STATE.GS_WIN_ANIMATION:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER:   
						
				//update total credit(store to varaibles) but not update meters at this stage.
				// The meters only updated at the starting of next spin.
				WinManager.Instance.ComputeTotalWin(GameVariables.Instance.BET_PER_LINE);
					if(!GameVariables.Instance.IS_FREEGAME && !GameVariables.Instance.IsThreeScatters())
					GameVariables.Instance.SetCurrentCredit  (GameVariables.Instance.GetCurrentCredit() + WinManager.Instance.TOTALWIN);


//				Debug.Log("Current Win :  " + GameVariables.Instance.GetCurrentCredit()); 
				m_InnerSate = INNER_STATE.IS_UPDATE; 
				break;
				
			case INNER_STATE.IS_UPDATE:
			
				if(GameVariables.Instance.EDITOR_FLAG == GameVariables.EDIT_STATES.NG_IDLE)
				{
					m_TempNS = STATE.GS_NG_EDITOR;
					m_InnerSate = INNER_STATE.IS_EXIT;
				}
				//Animation will not end(in Main Game)untill some skip input detected. 
				// In free game, either skip input or finishing win amount increasment will trigger Animation ending.
				// Then go to take win state. 
				if(AnimManager.Instance.m_IsEnd)
				{
					m_TempNS = STATE.GS_TAKE_WIN;
					m_InnerSate = INNER_STATE.IS_EXIT;
				}
				else
				{

					AnimManager.Instance.UpdateAnimation();
				}
				break;		
				
			case INNER_STATE.IS_EXIT:
				NEXT_STATE = m_TempNS;
				//Reset animation related variables.
				AnimManager.Instance.m_IsEnd = false;
				LineAnim.Instance.ResetIconsAlpha();
				IconAnim.Instance.HideAnimIcons();
				GameVariables.Instance.IS_INCRESED = false;

				AnimManager.Instance.IsPlayBigWin = false;
				GoldCoinEmitter.Instance.StopGoldCoinAnimation();

				m_InnerSate = INNER_STATE.IS_ENTER; 
				break;
			}
			break;
			//Animation state is over, there are some win amount update in meter.
			// this state will show that win amount for seconds and decide which type of spin to go for next game.
		case STATE.GS_TAKE_WIN:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER:   
				m_InnerSate = INNER_STATE.IS_UPDATE; 

				//! Move from free game spin enter state.
				//! if retrigger , it is a win also, so it must come to take win state.
				//! And we need to update free game left counter before checkFreegame function called.
				if (WinManager.Instance.IsTriggerFG())
				{
					FreeGame.Instance.FG_LEFT += FreeGame.Instance.NUM_OF_FGS;
					TextAndDigitDisp.Instance.IS_COUNTEND = false; 
					Icons.Instance.SetFirstFreeGameResult();
					FreeGame.Instance.CheckFreeGame();
				}
				else
				{
					FreeGame.Instance.CheckFreeGame();	
				}

			
			
				break;
			case INNER_STATE.IS_UPDATE:
			


				// Pause game for showing win amount 
				if ( (m_WinShowTime -= Time.deltaTime) < 0 )
				{	
					m_InnerSate = INNER_STATE.IS_EXIT;
					//if game switch from free game to main game and player doesn't skip the last free game animation.
					//The game will become idle state and waiting for input.
					if(FreeGame.Instance.IsGMTransition)
						m_TempNS = STATE.GS_IDLE;
					else if(GameVariables.Instance.IS_FREEGAME )
						m_TempNS = STATE.GS_REEL_SPIN_FG;
					else if(!GameVariables.Instance.IS_FREEGAME )// && FreeGame.Instance.IsGMTransition ) 
						m_TempNS = STATE.GS_REEL_SPIN_NG;
				}
				break;		
				
			case INNER_STATE.IS_EXIT:
				NEXT_STATE = m_TempNS;
				//reset win amount show time.
				m_WinShowTime = GameVariables.SHOW_WIN_TIME;
				m_InnerSate = INNER_STATE.IS_ENTER; 

				//only case: exit from free game and free game got win.
				if(!GameVariables.Instance.IS_FREEGAME && NEXT_STATE == STATE.GS_IDLE)
				{
					PauseGame(1.5f);
				}
				break;
			}
			break;

			//Only in idle state, editor input detected will entry this main game editor.
		case STATE.GS_NG_EDITOR:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER:   
				NgEditor.Instance.NgInitEditor();
				m_InnerSate = INNER_STATE.IS_UPDATE; 
				
				break;
			case INNER_STATE.IS_UPDATE:
				
				NgEditor.Instance.ControlUpdate();
				//From main game editor , will go to either main game or free game editor.
				//But for WuXia, no more free game editor. since it's free game use different flowchat from others. 
				if(GameVariables.Instance.EDITOR_FLAG == GameVariables.EDIT_STATES.NG_WAIT_TO_SPIN)
				{
					m_TempNS = STATE.GS_IDLE;
					m_InnerSate = INNER_STATE.IS_EXIT; 
				}
				if(GameVariables.Instance.EDITOR_FLAG == GameVariables.EDIT_STATES.FG_IDLE)
				{
					m_TempNS = STATE.GS_FG_EDITOR;
					m_InnerSate = INNER_STATE.IS_EXIT;
				}
			
				break;		
				
			case INNER_STATE.IS_EXIT:
				NEXT_STATE = m_TempNS;
				m_InnerSate = INNER_STATE.IS_ENTER; 
				break;
			}
			break;

		case STATE.GS_FG_EDITOR:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER:  
				//re-render different layout base on different reel strip.
				FgEditor.Instance.InitFreeGameEditor();
				m_InnerSate = INNER_STATE.IS_UPDATE; 
				break;
			case INNER_STATE.IS_UPDATE:
				FgEditor.Instance.UpdateFGEditor();
				if(GameVariables.Instance.EDITOR_FLAG  == GameVariables.EDIT_STATES.FG_EDIT_END)
				{
					m_TempNS = STATE.GS_IDLE;
					m_InnerSate = INNER_STATE.IS_EXIT;
				}
				break;		
				
			case INNER_STATE.IS_EXIT:
				NEXT_STATE = m_TempNS;
				m_InnerSate = INNER_STATE.IS_ENTER; 

				//it will spin first editer main game after exiting from free game editor.
				GameVariables.Instance.EDITOR_FLAG = GameVariables.EDIT_STATES.NG_WAIT_TO_SPIN;
				break;
			}
			break;

		case STATE.GS_REEL_SPIN_FG:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER: 
				IconAnim.Instance.Reset();
				GameVariables.Instance.IS_SPIN = true;
				//change reel strips to free game's.
				GameVariables.Instance.SetCurrentStrip (false);
				// reset head/tail index to intitial postion.
				Icons.Instance.ResetTempHeadTail();
				m_InnerSate = INNER_STATE.IS_UPDATE; 

				//! shoud check if free game is edited.
				Icons.Instance.GernerateResult();


				break;

			case INNER_STATE.IS_UPDATE:

				TextAndDigitDisp.Instance.CountingUpFGS();
				//				Debug.Log(TextAndDigitDisp.Instance.IS_COUNTEND);
				
				if(!TextAndDigitDisp.Instance.IS_COUNTEND)
					return;

				TextAndDigitDisp.Instance.ShowLeftGames();
				// spin fnish
				if(!GameVariables.Instance.IS_SPIN)  
				{

					m_TempNS = STATE.GS_CHECK_WIN;
					m_InnerSate = INNER_STATE.IS_EXIT;
				} 
				else
				{

				//	Debug.Log(FreeGame.Instance.FG_ID);
					Icons.Instance.SpinReels(true, (short)(FreeGame.Instance.FG_ID)); 
				}
				break;		
				
			case INNER_STATE.IS_EXIT:
				NEXT_STATE = m_TempNS;
				if(NEXT_STATE == STATE.GS_IDLE)
					PauseGame(1.5f);
				else
					PauseGame(0.5f);

				m_InnerSate = INNER_STATE.IS_ENTER; 
				break;
			}
			break;
		}// end of State machine

	}// end of update

	#endregion




}


