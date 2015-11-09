#region NameSpace
using UnityEngine;
using System.Collections;
#endregion


/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Implementation of Layout/logic of 50 line buttons.
/// </summary>
public class LineButtons : MonoBehaviour {


	#region Variables
	/*
	* Singleton Declaration
	*/
	// Singleton
	private static LineButtons instance;
	
	// Constuct
	private LineButtons() {}
	
	// Instance
	public static LineButtons Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(LineButtons)) as LineButtons;
			
			return instance;
		}
		
	}
	/*
	* End of Singleton Declaration
	*/


	private OTSprite[] m_SpriteLineButtons_Gray ;
	private OTSprite[] m_SpriteLineButtons_Color;
	private OTSprite[] m_SpriteLineButtons_Win;
	private Vector2[]  m_posLineButtons; 		 


	private Vector2 m_posStartL =  new Vector2(-64.0f + 48.0f, -150.0f);
	private Vector2 m_posStartR =  new Vector2(1024.0f + 23.6f, -150.0f);	

	private int[] m_iLineButtonOrder =  new int [50]
	{
		4,22,18,10, 2,12,20, 6, 9,24,16,14, 1,15,17,25, 8, 7,21,13, 3,11,19,23, 5,
		31,38,42,47,50,26,49,41,32,35,44,37,28,29,36,45,34,33,40,48,27,46,43,39,30
	};
	#endregion



	/// <summary>
	/// Set the location of 50 linbe buttons.
	/// </summary>
	void LoadLineButtonsPosition()
	{
		int iMidPoint = m_iLineButtonOrder.Length / 2;
		int iRemainder = m_iLineButtonOrder.Length % 2;
		int i = 0;
		
		float fOffsetX = 18.6f;
//		float fOffsetY = 12.5f;
		
		if (iRemainder != 0)
		{
			return;
		}
		
		/// Declare & initialize variable
		float [] fLeftPosX = new float [25] 
		{ 0.0f, 53.0f, 0.0f, 53.0f, 0.0f, 53.0f, 0.0f, 53.0f, 0.0f, 53.0f, 0.0f, 53.0f, 0.0f,
			53.0f, 0.0f, 53.0f, 0.0f, 53.0f, 0.0f, 53.0f, 0.0f, 53.0f, 0.0f, 53.0f, 0.0f};
		
		for (i=0; i<iMidPoint; i++)
		{
			int iLineNum = m_iLineButtonOrder[i];
			
			m_posLineButtons[iLineNum-1] = m_posStartL;
			m_posLineButtons[iLineNum-1].x += fLeftPosX[i] + fOffsetX;
			m_posLineButtons[iLineNum-1].y -= ((i *90/4));
		}
		
		for (i=0; i<iMidPoint; i++)
		{
			int iLineNum = m_iLineButtonOrder[i+iMidPoint];
			
			m_posLineButtons[iLineNum-1] = m_posStartR;
			m_posLineButtons[iLineNum-1].x -= (fLeftPosX[24 - i] + fOffsetX);
			m_posLineButtons[iLineNum-1].y -= ((i *90/4));		
		}
	}

	/// <summary>
	/// Set Button alpha.
	/// </summary>
	public void SetLineButtonColorAlpha(int iIndex, float fAlpha)
	{
		m_SpriteLineButtons_Color[iIndex].alpha = fAlpha;
	}

	/// <summary>
	/// Set top button alpha to 1, rest to 0.
	/// </summary>
	public void SetLineButtonColorSize(int iIndex, Vector2 size, bool isTop)
	{
		//Line button animation only exist in 3x5 game.
		if (!GameVariables.Instance.IsThreeXFiveGame ())
			return;

		if (isTop)
		{
			m_SpriteLineButtons_Win[iIndex].alpha = 1f;
		}
		else
		{
			m_SpriteLineButtons_Win[iIndex].alpha = 0f;
		}
	}

	/// <summary>
	/// Disable buttons. 
	/// </summary>
	public void StopAnimation()
	{
		if(GameVariables.Instance.IsThreeXFiveGame())
		for (int i = 0; i <GameVariables.NUM_OF_LINES; i++)
		{
			m_SpriteLineButtons_Win[i].alpha = 0f;
		}
	}


	/// <summary>
	/// Update is called once per frame. Disable line buttons if game is 4x5.
	/// </summary>
	void Update () {

		if(GameVariables.Instance.IS_TOGGLEMODE )
		{

			GameVariables.Instance.IS_TOGGLEMODE = !GameVariables.Instance.IS_TOGGLEMODE ;


			foreach (OTSprite ots in m_SpriteLineButtons_Gray)
				ots.alpha = (GameVariables.Instance.GAME_DEFINATION == GameVariables.GAME_DEFINE.FOUR_X_FIVE)? 0 : 1;


			foreach (OTSprite ots in m_SpriteLineButtons_Color)
				ots.alpha = (GameVariables.Instance.GAME_DEFINATION == GameVariables.GAME_DEFINE.FOUR_X_FIVE)? 0 : 1;

			foreach (OTSprite ots in m_SpriteLineButtons_Win)
				ots.alpha = (GameVariables.Instance.GAME_DEFINATION == GameVariables.GAME_DEFINE.FOUR_X_FIVE)? 0 : 1;
		}
	
	}
	
	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start()
	{
		if (GameVariables.Instance.GAME_DEFINATION == GameVariables.GAME_DEFINE.FOUR_X_FIVE)
					return;
		GenerateLineButtons ();
	}

	public void DestoryLineButtons()
	{
		if (m_SpriteLineButtons_Color != null)
			foreach (OTSprite ots in m_SpriteLineButtons_Color)
				Destroy (ots.gameObject);

		if (m_SpriteLineButtons_Gray != null)
			foreach (OTSprite ots in m_SpriteLineButtons_Gray)
				Destroy (ots.gameObject);

		if (m_SpriteLineButtons_Win != null)
			foreach (OTSprite ots in m_SpriteLineButtons_Win)
				Destroy (ots.gameObject);
		
	}
	public void GenerateLineButtons()
	{
		int num = GameVariables.NUM_OF_LINES;
		m_SpriteLineButtons_Gray  = new OTSprite[num];
		m_SpriteLineButtons_Color = new OTSprite[num];
		m_SpriteLineButtons_Win   = new OTSprite[num];
		m_posLineButtons 		  = new Vector2[num]; 		 
		
		
		LoadLineButtonsPosition();
		for (int i = 0; i <GameVariables.NUM_OF_LINES ; i++)
		{			
			string strName = "Spr_LineButton_" + i;
			m_SpriteLineButtons_Gray[i] =
				GameVariables.Instance.GenarateOTSpriet("LineButtons", "LineButtons_Altas",
				                                        strName, m_posLineButtons[i],
				                                        490,0,1,1);
			
			strName = "Spr_LineButton_Seleted_" + i;
			m_SpriteLineButtons_Color[i] =
				GameVariables.Instance.GenarateOTSpriet("LineButtons", "LineButtons_Altas",
				                                        strName, m_posLineButtons[i],
				                                        480,0,0,1);
			strName = "Spr_LineButton_Win_" + i;
			m_SpriteLineButtons_Win[i] =
				GameVariables.Instance.GenarateOTSpriet("LineButtons","LineButtons_Altas",
				                                        strName, m_posLineButtons[i],
				                                        470,0,0,1);
			
			for (int k = 0; k <GameVariables.NUM_OF_LINES; k++)
			{	
				if (i == (m_iLineButtonOrder[k] - 1))
				{
					m_SpriteLineButtons_Gray[i].frameIndex = m_iLineButtonOrder[k] - 1;
					m_SpriteLineButtons_Color[i].frameIndex = 50 + m_iLineButtonOrder[k] - 1;
					m_SpriteLineButtons_Win[i].frameIndex = 150 + m_iLineButtonOrder[k] - 1;
					break;
				}
			}	
			
		}// end of initializing of 50 lines buttons
	}
	





}
