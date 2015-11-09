using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*! \brief Handle global variables for game.
 *         
 * Declaration and implmentation of most of global variables.
 * 
 */
public class GameVariables : MonoBehaviour {
	
	#region Vriables
	//! Singleton
	private static GameVariables instance;
	
	//! Constuct
	private GameVariables() {}
	
	//! Instance
	public static GameVariables Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(GameVariables)) as GameVariables;
			
			return instance;
		}
		
	}

	public const int 		NUM_OF_LINES = 50;
	public const float 	ICON_SIZE = 154;
	public const float   SHOW_WIN_TIME = 0.8f;
	
	//! enum to store different project code.
	public enum GAMECODE
	{
		TEMPLATE,
		WUXIA,
		GRACEFUL,
		SHEEP,
		END
	}

	//!Current game project code.
	public GAMECODE PROJ_CODE = GAMECODE.TEMPLATE;
	
	//! substitute symbol frame index. 
	public int SUBSTITUE_INDEX;

	//! scatter symbol frame index. 
	public int SCATTER_INDEX;

	//! boolean varaible for checking if WUXIA free game win.
	public bool IS_FG_WIN_WX;

	//! boolean varaible for checking if WUXIA free game retrigger.
	public bool IS_RETRIGGER_WX;


	public int[][] REEL_STRIPS_NG; 
	public int[][] REEL_STRIPS_FG;
	//! reeel strip used in spining.
	public int[][] REEL_STRIPS_CURRENT;

	//! store win line icon indices. (0-19 for 3x5 game)
	//! load from xml file. for win conditon checking
	public List<int[]> winLineData; 

	//! store indcies of scatters/
	public List<int> SCATTERINDICES_WIN ;

	//! check if changing to another project.
	public bool   IS_TOGGLEMODE;

	//! check if still spining.
	public bool   IS_SPIN;

	//! check if win amount increment done.
	public bool   IS_INCRESED;
	
	//! store probability of hiting each array set.
	public int[,]  PROB_CASESELECT;

	//! store probability of winning the free game for different play lines.
	public int[] 	PROB_WIN_FG;
	
	public bool   IS_WIN;
	public bool 	IS_FREEGAME;
	public int	  	BET_PER_LINE;

	public int[]  PLAYLINE_OPTIONS;
	public int[]  BET_OPTIONS;

	public  int FG_SCATTERS = 2;

	//! Two different game defination. with different layout and reel strip.
	public enum GAME_DEFINE
	{
		THREE_X_FIVE,
		FOUR_X_FIVE
	}
	public GAME_DEFINE GAME_DEFINATION ;

	//! number fo icons appears per drum.
	public int NUM_OF_ROWS ;    
	//! number of reels.
	public int NUM_OF_COLS  =5;	

	//! states for combination test editor.
	public enum EDIT_STATES
	{
		NOT_IN_EDITOR = 0,
		NG_IDLE = 1,
		NG_WAIT_TO_SPIN,
		NG_SPIN_END = 3,
		FG_IDLE,
		FG_WAIT_TO_SPIN,
		FG_EDIT_END
	}
	public EDIT_STATES  EDITOR_FLAG;

	//! Current total credits player owns.
	private long m_Credits = 0;
	public long GetCurrentCredit()		 	{ return m_Credits; 		}
	public void SetCurrentCredit(long _c) 	{ m_Credits = _c; 			}

	//! Total bets for next spin.
	private long m_TotalBetCredit = 1;
	public long GetTotalBetCredit()		 	{  return m_TotalBetCredit; }
	public void SetTotalBetCredit (int _tb)	{  m_TotalBetCredit = _tb;	}

	//! How much bets cost per line
	private int m_CreditPerLine = 1;
	public int GetCreditPerLine() 			{  return m_CreditPerLine;	} 
	public void SetCreditPerLine (int _c)	{  m_CreditPerLine = _c; 	}

	//! Total play lines
	private int m_PlayLines = 1;
	public int GetPlayLine()				{ return m_PlayLines;		}
	public void SetPlayLine (int _l)		{  m_PlayLines = _l;		}
	
	//! Main Game Odds table. 
	public int[,] ODDSTABLE_NG;

	//! Free Game Odds table.
	public int[,] ODDSTABLE_FG;
	
	//! First icon postion for each drum
	public ArrayList DRUM_LOCATION = new ArrayList();

	public string BG_NAME ="";

	#endregion
	
	#region Global Helper
	/// <summary>
	/// Genarate a new OT-Sprite object with arguments taken.
	/// </summary>
	/// <param name="_parentName"> name of parent object.</param>
	/// <param name="_altasName"> name of altas data object.</param>
	/// <param name="_name"> name of OT-Sprite object to genarate.</param>
	/// <param name="_pos"> Position of object.</param>
	/// <param name="_depth"> Depth(z value of position) of object.</param>
	/// <param name="_frameIndex"> frameIndex in the related sprite sheet.</param>
	/// <param name="_alpha"> alpha value.</param>
	/// <param name="_collisionDepth"> collision depth value.</param>
	/// <returns> return a <see cref="OTSprite"/> Object. </returns>
	public OTSprite GenarateOTSpriet(string _parentName, string _altasName, string _name ="unNamedSprite",
	                                 Vector2 _pos = new Vector2(), int _depth = 0, 
	                                 int _frameIndex = 0, float _aphla = 1, int _collisionDepth = 1) 
	{
		OTSprite rtnSprite;
		rtnSprite 				  = OT.CreateObject (OTObjectType.Sprite).GetComponent<OTSprite>();
		rtnSprite.spriteContainer = GameObject.Find(_altasName).GetComponent<OTSpriteAtlasCocos2D>();
		
		rtnSprite.name 			  = _name;
		rtnSprite.position 		  = _pos;
		rtnSprite.size 			  = GameObject.Find(_altasName).GetComponent<OTSpriteAtlasCocos2D>().atlasData[_frameIndex].size;		
		rtnSprite.depth 		  = _depth;
		rtnSprite.frameIndex 	  = _frameIndex;	
		rtnSprite.alpha 		  = _aphla;
		rtnSprite.collisionDepth  = _collisionDepth;
		rtnSprite.transform.parent = GameObject.Find (_parentName).transform;

		
		return rtnSprite;
	}
	
	/// <summary>
	/// Update current reel strip - 'REEL_STRIPS_CURRENT' contianer with either REEL_STRIPS_NG or REEL_STRIPS_FG.
	/// This function do a deep copy.
	/// </summary>
	/// <param name="_isNG"> boolean to show if the main game reel strip should be set.</param>
	public void SetCurrentStrip(bool _isNG)
	{
		//set current reel strip to normal game.
		if (_isNG) 
		{
			REEL_STRIPS_CURRENT = new int[NUM_OF_COLS][];
			for(int i = 0; i < NUM_OF_COLS; ++i)
			{
				REEL_STRIPS_CURRENT[i] = new int[REEL_STRIPS_NG[i].Length];
				for( int k = 0 ; k < REEL_STRIPS_NG[i].Length; ++k)
				{
					REEL_STRIPS_CURRENT[i] [k] = REEL_STRIPS_NG[i][k];
				}
			}
		}
		else
		{
			REEL_STRIPS_CURRENT = new int[NUM_OF_COLS][];
			for(int i = 0; i < NUM_OF_COLS; ++i)
			{
				REEL_STRIPS_CURRENT[i] = new int[REEL_STRIPS_FG[i].Length];
				for( int k = 0 ; k < REEL_STRIPS_FG[i].Length; ++k)
				{
					REEL_STRIPS_CURRENT[i] [k] = REEL_STRIPS_FG[i][k];
				}
			}
		}
	}

	
	/// <summary>
	/// return whether there are three scatters in screen.
	/// </summary>
	public bool IsThreeScatters()
	{
		WinManager.Instance.IsTriggerFG ();
		return (SCATTERINDICES_WIN.Count == GameVariables.instance.FG_SCATTERS);
	}

	public bool IsThreeXFiveGame()
	{
		return (GAME_DEFINATION == GAME_DEFINE.THREE_X_FIVE);
	}
	#endregion

	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start () {
		FileManager.Instance.Initialize ();
		Icons.Instance.Initialize ();
	}
	
	/// <summary>
	/// Initialization of variables.
	/// </summary>
	void Awake()
	{
		PROJ_CODE = GAMECODE.TEMPLATE;
			
		IS_RETRIGGER_WX		= false;
		IS_FG_WIN_WX 		    = false;
		IS_SPIN 						= false;
		IS_WIN 						= false;
		IS_INCRESED 				= false;
		IS_FREEGAME 				 = false;
		EDITOR_FLAG 				= EDIT_STATES.NOT_IN_EDITOR;
		IS_TOGGLEMODE			= false;
		BET_PER_LINE	 			= 1;
		
		
	//	if (PROJ_CODE == GAMECODE.WUXIA) 
		SCATTER_INDEX 			= 12;
		SUBSTITUE_INDEX 		= 13;
		
		PLAYLINE_OPTIONS 		= new int[5] {1, 10, 20, 35 ,50};
		BET_OPTIONS 				= new int[5] {1, 2, 5, 10 ,20};		
		
		GAME_DEFINATION = GAME_DEFINE.THREE_X_FIVE;
		if(GAME_DEFINATION == GAME_DEFINE.THREE_X_FIVE)
		{
			BG_NAME 		  = "Background_Sprite";
			NUM_OF_ROWS = 4;
		}
		else
		{
			BG_NAME 		  = "Background_Sprite_4x5";
			NUM_OF_ROWS = 5;
		}

		
		
		NUM_OF_COLS = 5;
		
		REEL_STRIPS_NG = new int[NUM_OF_COLS][ ];
		REEL_STRIPS_FG  = new int[NUM_OF_COLS][ ];
		REEL_STRIPS_CURRENT = new int[NUM_OF_COLS][ ];
		SCATTERINDICES_WIN = new List<int> ();
		winLineData = new List<int[]>(); 
		ODDSTABLE_NG = new int[20,10];  // 2o incons 
		ODDSTABLE_FG = new int[20,10];
		
		//store locations of first icon for each drum.
		SetReelLoaction ();
	}

	private void SetReelLoaction()
	{
		Vector2 loc_TopLeft = new Vector2 (199, -91f);
		float xOffset = 154f; 
		if (GAME_DEFINATION == GAME_DEFINE.THREE_X_FIVE) 
		{
			loc_TopLeft = new Vector2 (180f, -117f);
			xOffset = 166f;
		}
		
		DRUM_LOCATION = new ArrayList();
		for (int i = 0; i < NUM_OF_COLS; ++i) 
		{
			DRUM_LOCATION.Add(loc_TopLeft);
			loc_TopLeft.x +=  xOffset;
		}
	}

	public void ChangeLayoutSetting()
	{
		SetReelLoaction();
		if (GAME_DEFINATION == GAME_DEFINE.FOUR_X_FIVE)
			BG_NAME = "Background_Sprite_4x5";
		else
			BG_NAME = "Background_Sprite";
		
	}



}


