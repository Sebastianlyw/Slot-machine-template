#region NameSpace
using UnityEngine;
using System.Collections;

#endregion

/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Implementation of Input Behaviour.
/// </summary>
public class InputManager : MonoBehaviour {

	#region Variables
	//! Singleton
	private static InputManager instance;
	
	//! Constuct
	private InputManager() {}
	
	//!Instance
	public static InputManager Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(InputManager)) as InputManager;
			
			return instance;
		}
		
	}
	/************ end of Singleton**************/


	//! ToDo: Should move all these play lines crap to somewhere else!
	public const long CREDIT_INSERT = 500;
	
	OTSprite m_spriteButtonBetLine;
	OTSprite m_spriteButtonBetLineLabel;
	OTSprite m_spriteButtonPlayLine;
	OTSprite m_spriteButtonPlayLineLabel;
	OTTextSprite m_textButtonBetLine;
	OTTextSprite m_textButtonPlayLine;

	OTAnimatingSprite m_MaxLineLeft;
	OTAnimatingSprite m_MaxLineRight;
	
	private int m_iCurBetLineOption = 0;
	private int m_iCurPlayLineOption;

	public bool IsAlwaysWin = false;
	public bool IsAlwaysRetr = false;
	public int IsCountingFGwin = 1;
	public bool IsForceFG = false;

	private bool m_isSpinPressed;

	#endregion



	//! for hot key testing
	void Update()
	{
	
	

	}
	/// <summary>
	///Check if drop coin button pressed.
	/// </summary>
	void CheckCreditInput() {
		
		if (Input.GetKeyDown(KeyCode.C) || TextAndDigitDisp.Instance.CheckCreditPressed())
		{
			AudioManager.Instance.PlaySound("DropCoin");
			AudioManager.Instance.StopSound("DropCoin");
			GameVariables.Instance.SetCurrentCredit(GameVariables.Instance.GetCurrentCredit() + CREDIT_INSERT);
			TextAndDigitDisp.Instance.SetCreditAmount(GameVariables.Instance.GetCurrentCredit());
		}
	}
	
	/// <summary>
	///	Use this for initialization of bet/ win / credits labels.
	/// </summary>
	void Start () {

		m_iCurPlayLineOption = 4;	
		m_isSpinPressed = false;
		if (GameVariables.Instance.GAME_DEFINATION != GameVariables.GAME_DEFINE.FOUR_X_FIVE) 
		{
			GenerateButtons();						
		}
		else
		{
			GenerateMaxLineLabels();
		}
	}

	private void ResetAnim(OTObject owner)
	{
		owner.GetComponent<OTAnimatingSprite> ().frameIndex = 4;
	}

	public void GenerateMaxLineLabels()
	{
		m_MaxLineLeft = GameObject.Find("MaxLineLeftAni_Spr").GetComponent<OTAnimatingSprite>();
		m_MaxLineRight = GameObject.Find("MaxLineRightAni_Spr").GetComponent<OTAnimatingSprite>();
		m_MaxLineRight.frameIndex = 4;
		m_MaxLineLeft.frameIndex = 4;
		m_MaxLineLeft.onAnimationFinish = ResetAnim;
		m_MaxLineRight.onAnimationFinish = ResetAnim;
	}

	public void GenerateButtons()
	{
		m_spriteButtonBetLine = GameObject.Find ("Button_BetBar_Sprite").GetComponent<OTSprite> ();
		m_spriteButtonBetLineLabel = GameObject.Find ("Button_BetLabel_Sprite").GetComponent<OTSprite> ();
		m_spriteButtonPlayLine = GameObject.Find ("Button_PlayLineBar_Sprite").GetComponent<OTSprite> ();
		m_spriteButtonPlayLineLabel = GameObject.Find ("Button_PlayLineLabel_Sprite").GetComponent<OTSprite> ();
		m_textButtonBetLine = GameObject.Find ("Button_BetText_Sprite").GetComponent<OTTextSprite> ();
		m_textButtonPlayLine = GameObject.Find ("Button_PlaylineText_Sprite").GetComponent<OTTextSprite> ();
		
		m_iCurBetLineOption = 0;
		m_iCurPlayLineOption = 4;	
		
		float fScaleFactor = 1.0f;
		m_spriteButtonBetLine.size = m_spriteButtonBetLine.size * fScaleFactor;
		m_spriteButtonBetLineLabel.size = m_spriteButtonBetLineLabel.size * fScaleFactor;
		m_spriteButtonPlayLine.size = m_spriteButtonPlayLine.size * fScaleFactor;
		m_spriteButtonPlayLineLabel.size = m_spriteButtonPlayLineLabel.size * fScaleFactor;
		m_textButtonBetLine.size = m_textButtonBetLine.size * fScaleFactor;
		m_textButtonPlayLine.size = m_textButtonPlayLine.size * fScaleFactor;
		
		
		UpdateOptionsAndText ();
	}
	/// <summary>
	///	Update bet / line data.
	/// </summary>
	public void BetAndLineUpdate()
	{
		CheckCreditInput ();
	//	if (GameVariables.Instance.GAME_DEFINATION == GameVariables.GAME_DEFINE.THREE_X_FIVE) 
		CheckBetAndPlayLineUpdate ();

	}

	
	/// <summary>
	///	Update the bet per line and playline setting.
	/// And check if input is valid.
	/// </summary>
	public bool IsValidInput()
	{
		BetAndLineUpdate();
		return (  (GameVariables.Instance.GetCurrentCredit () >= GameVariables.Instance.GetTotalBetCredit ()) 
		        	 && IsSpinInput());
	}

	/// <summary>
	///	Check if input for next spin is valid.
	/// </summary>
	public bool IsSpinInput()
	{
		if (Input.GetKeyDown (KeyCode.Alpha6)) 
		{
			IsForceFG = true;
		}

		return 		(Input.GetKeyDown (KeyCode.J) 		||
						Input.GetKeyDown (KeyCode.Alpha6) ||
		          		m_isSpinPressed);
		        	
	}

	/// <summary>
	///	Input Update when game is in Idle state. it will go to spin or editor. 
	/// </summary>
	public void InputUpdate()
	{

		if (IsValidInput ())
		{
			GameVariables.Instance.IS_SPIN = true;
		}

		if (Input.GetKeyDown (KeyCode.E)) 
		{
			GameVariables.Instance.EDITOR_FLAG  = GameVariables.EDIT_STATES.NG_IDLE;
		}

	}
	public void CheckCountingStart()
	{
		if( IsCountingFGwin == 1)
			IsCountingFGwin = 2;
//		if (Input.GetKeyDown (KeyCode.Alpha7) || Input.GetKeyDown (KeyCode.J) && WinAmountAnim.Instance.CURRENT_WIN!= 0)
//		{
//			IsCountingFGwin = 2;
//		}

	}

	/// <summary>
	///	Check the play line options and update to label 
	/// </summary>
	public void UpdateOptionsAndText()
	{
		for (int i=0; i<GameVariables.Instance.BET_OPTIONS.Length; i++)
		{
			if (GameVariables.Instance.GetCreditPerLine() == GameVariables.Instance.BET_OPTIONS[i])
			{
				m_iCurBetLineOption = i;
			}
		}		
		
		for (int i=0; i<GameVariables.Instance.PLAYLINE_OPTIONS.Length; i++)
		{
			if (GameVariables.Instance.GetPlayLine() == GameVariables.Instance.PLAYLINE_OPTIONS[i])
			{
				m_iCurPlayLineOption = 4;
			}
		}
		
		m_textButtonBetLine.text = GameVariables.Instance.BET_OPTIONS[m_iCurBetLineOption].ToString();
		m_textButtonPlayLine.text = GameVariables.Instance.PLAYLINE_OPTIONS[m_iCurPlayLineOption].ToString();		

		GameVariables.Instance.BET_PER_LINE = GameVariables.Instance.BET_OPTIONS [m_iCurBetLineOption];
		GameVariables.Instance.SetPlayLine(GameVariables.Instance.PLAYLINE_OPTIONS [m_iCurPlayLineOption]);

		                                      
	}

	//! Check input for changing play line and bet per line.
	private void GameInputs()
	{
		
		if (Input.GetKeyDown (KeyCode.F10))
		{
			if(m_iCurBetLineOption < 4)
				++m_iCurBetLineOption;
		}
		else if (Input.GetKeyDown (KeyCode.F9))
		{
			if(m_iCurBetLineOption >0)
				--m_iCurBetLineOption;
		}
		
		
		if (Input.GetKeyDown (KeyCode.F1))
		{
			m_iCurPlayLineOption = 0;
			m_isSpinPressed = true;
		}
		else if (Input.GetKeyDown (KeyCode.F2))
		{
			m_iCurPlayLineOption = 1;
			m_isSpinPressed = true;
		}
		else if (Input.GetKeyDown (KeyCode.F3))
		{
			m_iCurPlayLineOption = 2;
			m_isSpinPressed = true;
		}
		else if (Input.GetKeyDown (KeyCode.F4))
		{
			m_iCurPlayLineOption = 3;
			m_isSpinPressed = true;
		}
		else if (Input.GetKeyDown (KeyCode.F5))
		{
			m_isSpinPressed = true;

			if(m_iCurPlayLineOption != 4)
			{
				if(!GameVariables.Instance.IsThreeXFiveGame())
				{
					m_MaxLineLeft.Play();
					m_MaxLineRight.Play();
				}
				m_iCurPlayLineOption = 4;
			}
		}
		else
		{
			m_isSpinPressed = false;
		}


		if(!GameVariables.Instance.IsThreeXFiveGame())
		{
			if (m_iCurPlayLineOption < 4) 
			{
				m_MaxLineLeft.alpha = 0f;
				m_MaxLineRight.alpha = 0f;
			}
			else
			{
				m_MaxLineLeft.alpha = 1f;
				m_MaxLineRight.alpha = 1f;
			}
		}
//		if (Input.GetKeyDown (KeyCode.Alpha8))
//		{
//			if(m_iCurPlayLineOption < 4)
//				++m_iCurPlayLineOption;
//		}
//		else if (Input.GetKeyDown (KeyCode.Alpha9))
//		{
//			if(m_iCurPlayLineOption > 0)
//				--m_iCurPlayLineOption;
//		}

	}
	/// <summary>
	///	Check if user has press some input for bet and playline update.
	/// </summary>
	public void CheckBetAndPlayLineUpdate()
	{
		GameInputs ();

	
		GameVariables.Instance.SetCreditPerLine(GameVariables.Instance.BET_OPTIONS[m_iCurBetLineOption]);
	
		GameVariables.Instance.SetPlayLine(GameVariables.Instance.PLAYLINE_OPTIONS[m_iCurPlayLineOption]);

		GameVariables.Instance.SetTotalBetCredit (GameVariables.Instance.BET_OPTIONS [m_iCurBetLineOption]
																	 * GameVariables.Instance.PLAYLINE_OPTIONS [m_iCurPlayLineOption]);


		GameVariables.Instance.BET_PER_LINE = GameVariables.Instance.BET_OPTIONS [m_iCurBetLineOption];
		GameVariables.Instance.SetPlayLine(GameVariables.Instance.PLAYLINE_OPTIONS [m_iCurPlayLineOption]);

		if (GameVariables.Instance.GAME_DEFINATION == GameVariables.GAME_DEFINE.THREE_X_FIVE) 
			ThreeXFiveLayout ();
		else
			FourXFiveLayout ();

	}


	private void FourXFiveLayout()
	{
		//! update digital on button
		GameObject.Find ("MaxLineDigit_Left_Spr").GetComponent<OTTextSprite> ().text = (GameVariables.Instance.PLAYLINE_OPTIONS [m_iCurPlayLineOption]).ToString();
		GameObject.Find ("MaxLineDigit_Right_Spr").GetComponent<OTTextSprite> ().text = (GameVariables.Instance.PLAYLINE_OPTIONS [m_iCurPlayLineOption]).ToString ();

	}

	// play lines and bet per line buttons.
	private void ThreeXFiveLayout()
	{

			//update playline options for mouse input
			if (OT.Clicked (m_spriteButtonPlayLine)) 
			{
				if (m_iCurPlayLineOption < (GameVariables.Instance.PLAYLINE_OPTIONS.Length - 1)) {
					m_iCurPlayLineOption++;
				} else {
					m_iCurPlayLineOption = 0;
				}
			}

			//update bet options for mouse input
			if (OT.Clicked(m_spriteButtonBetLine))
			{
				if (m_iCurBetLineOption < (GameVariables.Instance.BET_OPTIONS.Length - 1))
				{
					m_iCurBetLineOption++;
				}
				else
				{
					m_iCurBetLineOption = 0;
				}
				//bet button label
			}

			m_textButtonBetLine.text = GameVariables.Instance.BET_OPTIONS[m_iCurBetLineOption].ToString();
			m_textButtonPlayLine.text = GameVariables.Instance.PLAYLINE_OPTIONS[m_iCurPlayLineOption].ToString();
			//play line button label.

				// Update Line Button display
				for(int i = 0; i < GameVariables.NUM_OF_LINES; i++)
			{
				if (i < GameVariables.Instance.PLAYLINE_OPTIONS[m_iCurPlayLineOption])
				{
					LineButtons.Instance.SetLineButtonColorAlpha(i, 1f);
				}
				else
				{
					LineButtons.Instance.SetLineButtonColorAlpha(i, 0f);
				}
			}			

	}
	/// <summary>
	///	Update total bet for game
	/// </summary>
	public void PrintBetAmout()
	{
		//update total bet for game
		TextAndDigitDisp.Instance.SetBetAmount (GameVariables.Instance.BET_OPTIONS [m_iCurBetLineOption]
		                                        * GameVariables.Instance.PLAYLINE_OPTIONS [m_iCurPlayLineOption]);

	}







}
