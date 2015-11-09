#region NameSpace
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#endregion
/*! \brief Icons Class implement the spin feature.
 	*         
 	* For spining feature, this class generates the static icons
 	* and icon squre background. 
 	* This calss also handle Icons spining and tweening algorithm.
	*/
public class Icons : MonoBehaviour {

	#region Variables
	//! Singleton
	private static Icons instance;
	
	//! Constuct
	private Icons() {}
	
	//! Instance
	public static Icons Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(Icons)) as Icons;
			
			return instance;
		}
		
	}

	
	private const float SPIN_SPEED		= -2400;
	private const float SPIN_GAPTIME  	= 0.5349f;
	//!Spining speed must be a vector2, the y value takes from SPIN_SPEED.
	private Vector2 	m_Speed;

	//! static icons to spin. (4x5 = 20 icons)
	public OTSprite[] 	m_Icons ; 
	//! square background attached to icon.
	private OTSprite[]  m_BgSquares;
	//! Spin time for a reel.
	private float[]    	m_SpinTime;
	//! timer for spinning.
	private float			m_SpinElapsedTime;
	//Location for symbols at first row. 
	private Vector2[] 	m_LastRowPos; 
	//Location for symbols at last row.
	private Vector2[] 	m_FirstRowPos; 

	//! store icons pos and frame index when entring editor.
	public List<Pair<Vector2,int>> ORIGIN_ICONS ;
	//!origin icons infor. win index for each game. when exit from editor, should reset to origin. 
	public List<Pair<short, short>> ORIGIN_HEADTAIL;
	//!store target head/tail data for main game.
	public List<Pair<short,short>> TARGET_HEADTAIL;
	//!store head /tail data for free game editor
	public List<List<Pair<short,short>> >  FG_DATA_CONTAINER; 
	//! track which element in reel_strip.xml is the last one at spinning time.
	private int[]         m_tempIndexTail;  
	//! track which element in reel_strip.xml is the first one at spinning time.
	private int[]         m_tempIndexHead;

	//! track total rows travelling.
	private int m_traveller;
	//! track scatter icon column.
	private int m_curCol; 

	//! Checking if the tweening of spinning end.
	private bool[] isTweenEnd;
	private int[] m_TargetHeadIndex;
	
	//! offset size, from icon image data.
	private Vector2 m_offset ;


	/// <summary>
	/// Initialization, which occurs before all Start function.
	/// </summary>
	void Awake()
	{
		m_SpinElapsedTime = 0f;
		FG_DATA_CONTAINER = new List<List<Pair<short, short>>> ();
		isTweenEnd = new bool[5];
		ResetTweenStates ();
		TARGET_HEADTAIL = new List<Pair<short,short>> ();
		TARGET_HEADTAIL.Capacity = 5;
		
	}

	/// <summary>
	///  Use this for initialization
	/// </summary>
	void Start () {
		m_LastRowPos  = new Vector2[GameVariables.Instance.NUM_OF_COLS];
		m_FirstRowPos 	= new Vector2[GameVariables.Instance.NUM_OF_COLS];
		m_SpinTime    	= new float[GameVariables.Instance.NUM_OF_COLS];
		m_Speed 	  		= Vector2.zero;
		m_Speed.y 		= Time.deltaTime * SPIN_SPEED;
		m_offset 			= new Vector2 (0f, -154f);
		m_SpinTime[0] 	= 1f;
		m_traveller 		= 0;
		m_curCol 			= 1;
	}

	#endregion

	#region Internal Helper
	
	/// <summary>
	/// functor for sorting a specific list, compare with the pair's first element y value.
	/// </summary>
	/// <param name="s1"> pair 1 </param>
	/// <param name="s2"> pair 2</param>
	private static int comparePosY(Pair<Vector2,int> s1, Pair<Vector2,int> s2)
	{
		if (s1.First.y > s2.First.y)
			return -1;
		else
			return 1;
	}

	/// <summary>
	/// Reset tweening status.
	/// </summary>
	private void ResetTweenStates()
	{
		for (int i = 0; i< 5; ++i)
			isTweenEnd [i] = false;
	}

	/// <summary>
	/// Reset Spin datas at the end.
	/// </summary>
	private void ResetSpin()
	{
		AudioManager.Instance.StopSound("ReelSpin");
		AudioManager.Instance.StopSound("Scatter1"); 
		AudioManager.Instance.StopSound("Scatter2"); 
		AudioManager.Instance.StopSound("Scatter3"); 
		GameVariables.Instance.EDITOR_FLAG = GameVariables.EDIT_STATES.NG_SPIN_END;
		GameVariables.Instance.IS_SPIN = false;
		m_SpinElapsedTime = 0;
		m_traveller = 0;
		m_curCol = 1;
		ResetTweenStates();
		
		// when quit from editor and finish first spin which follow edited target, need to change editor state.
		if(GameVariables.Instance.EDITOR_FLAG == GameVariables.EDIT_STATES.NG_WAIT_TO_SPIN)
			GameVariables.Instance.EDITOR_FLAG = GameVariables.EDIT_STATES.NOT_IN_EDITOR; 
		
		GameVariables.Instance.IS_INCRESED = false;
	}

	/// <summary>
	/// Generate static icons and background squares. Load reel strip, set head / tail.
	/// </summary>
	private void InitIcons()
	{
		for (int i = 0; i < m_SpinTime.Length; ++i) 
		{	
			if(i!=0)
				m_SpinTime[i] = m_SpinTime[i-1] + SPIN_GAPTIME;
		}

		string reelName ="";
		string IconsName ="";
		if(GameVariables.Instance.IsThreeXFiveGame())
		{
			reelName = "Xml/reel_strips";
			IconsName = "Icons_Altas";
		}
		else
		{
			reelName = "Xml/reel_strips_4x5";
			IconsName = "Icons_Altas_4x5";
		}
		
		FileManager.Instance.LoadReelStrips(reelName);
		GameVariables.Instance.SetCurrentStrip (true);
		GenerateIconSprites (IconsName); 
		ResetTempHeadTail ();
		GenerateSpinResult (out m_TargetHeadIndex, false, 0);
		GenerateBgSquares ();

	}

	
	/// <summary>
	/// Generate 20 square background which are attaced to 20 static symbols.
	/// </summary>
	private void GenerateBgSquares()
	{
		m_BgSquares = new OTSprite[GameVariables.Instance.NUM_OF_COLS * GameVariables.Instance.NUM_OF_ROWS]; 
		for(int i = 0; i < GameVariables.Instance.NUM_OF_COLS; ++i) 
		{
			
			for(int k = 0; k < GameVariables.Instance.NUM_OF_ROWS;  ++k)
			{
				m_BgSquares[k + i * GameVariables.Instance.NUM_OF_ROWS ] = 
					GameVariables.Instance.GenarateOTSpriet("BackgroudSquares",	"Icon_Bg_Altas", "Bg_square",
					                                        (Vector2)GameVariables.Instance.DRUM_LOCATION[i]+ (m_offset * k), 
					                                        907, 0 , 1, 1);
			}
		}
	}

	#endregion

	#region Global Helper
	/// <summary>
	/// Generate 20 static icons.
	/// </summary>
	/// <param name="_altasName"> altas data of sprite sheet for OT sprite object </param>
	public void GenerateIconSprites(string _altasName)
	{
		if(!GameVariables.Instance.IsThreeXFiveGame())
		{
			m_offset = new Vector2 (0f, -142);
		}
		else
		{
			m_offset = new  Vector2 (0f, -154);
		}
		m_Icons = new OTSprite[GameVariables.Instance.NUM_OF_COLS * GameVariables.Instance.NUM_OF_ROWS]; 
		
		for(int i = 0; i < GameVariables.Instance.NUM_OF_COLS; ++i) 
		{
			
			for(int k = 0; k < GameVariables.Instance.NUM_OF_ROWS;  ++k)
			{
				m_Icons[k + i * GameVariables.Instance.NUM_OF_ROWS ] = 
					GameVariables.Instance.GenarateOTSpriet("MainGameIcons",	_altasName, "sprite",
					                                        (Vector2)GameVariables.Instance.DRUM_LOCATION[i]+ (m_offset * k), 
					                                        905,GameVariables.Instance.REEL_STRIPS_CURRENT[i][k], 1, 1);
			}
			
			m_LastRowPos[i]  = m_Icons[ (i+1) * GameVariables.Instance.NUM_OF_ROWS - 1].position;// + offset;
			m_FirstRowPos[i] = m_Icons[i * GameVariables.Instance.NUM_OF_ROWS].position;
		}
		
		m_tempIndexTail = new int[GameVariables.Instance.NUM_OF_COLS];
		m_tempIndexHead= new int[GameVariables.Instance.NUM_OF_COLS];
	}
	
	/// <summary>
	/// Genarate scatters in reel 2, 3 and 4.
	/// </summary>
	public void  SpinToFG()
	{
		InputManager.Instance.IsForceFG = false;
		int j = 0;
		List<List<int>> scatters = new List<List<int>>(); 

		//! For HOUST game, the scattters appear on reel 1 and 5.
		for (int i = 0; i < GameVariables.Instance.NUM_OF_COLS;  ++i) 
		{
			List<int> s = new List<int>();
			for(int k = 0; k < GameVariables.Instance.REEL_STRIPS_NG[i].Length; ++k)
			{
				int t = GameVariables.Instance.REEL_STRIPS_NG[i][k];
				if(t == GameVariables.Instance.SCATTER_INDEX)
				{
					s.Add(k);
				}
			}
			scatters.Add(s);
		}

		for(int k = 0; k < GameVariables.Instance.NUM_OF_COLS; ++k)
		{
			if(scatters[k].Count > 0)
			{
				int r = Random.Range(0, scatters[k].Count);
				RandomScatters(ref m_TargetHeadIndex [k], scatters[k][r], k);
			}
		}
		
	
		//RandomScatters(ref m_TargetHeadIndex [0], scatters[0][r], 0);
		//r = Random.Range(0, scatters[1].Count);
		//RandomScatters(ref m_TargetHeadIndex [4], scatters[1][r], 4);
	
	}


	private void RandomScatters(ref int _target, int _s, int _i)
	{
		_target = _s - 2 - Random.Range (0, 3);
		_target  = (_target < 0)? _target += GameVariables.Instance.REEL_STRIPS_NG [_i].Length : _target ;
	}


	/// <summary>
	/// Initizlize Static icons, head/tail and free game data.
	/// </summary>
	public void Initialize()
	{
		InitIcons ();
		StoreHeadTails_FG();
		for( int i = 0; i < GameVariables.Instance.NUM_OF_COLS; ++i)
		{
			Pair<short,short> fgdata = new Pair<short,short>(Icons.Instance.FG_DATA_CONTAINER[0][i].First, Icons.Instance.FG_DATA_CONTAINER[0][i].Second);
			Icons.Instance.TARGET_HEADTAIL.Add(fgdata);
		}
	}
	
	/// <summary>
	/// Reset head/tail which used in spin function. 
	/// Icons to spin in this order: head -> symbol 1 -> s2 -> s3 -> s4 -> tail.
	/// 5 head indices can show us how the whole screen looks like.
	/// </summary>
	public void ResetTempHeadTail()
	{
		//the symbol 1's index is starting from 0.
		for(int i = 0; i < GameVariables.Instance.NUM_OF_COLS; ++i) 
		{
			m_tempIndexTail[i] = GameVariables.Instance.NUM_OF_ROWS;  // sicne we have 4 icons already(0,1.2.3), so the tail should start from 4.
			m_tempIndexHead[i] = GameVariables.Instance.REEL_STRIPS_CURRENT[i].Length-1;
		}
	}
	#endregion
	
	#region Editor Icons
	/// <summary>
	/// Load the screen before entrying editor.
	/// </summary>
	public void ResetIcons()
	{
		for (int i = 0; i< m_Icons.Length; ++i) 
		{
			m_Icons[i].position  = ORIGIN_ICONS[i].First;
			m_Icons[i].frameIndex = ORIGIN_ICONS[i].Second;
		}
		for (int i = 0; i < GameVariables.Instance.NUM_OF_COLS; ++i)
		{
			m_tempIndexHead[i] = ORIGIN_HEADTAIL[i].First;
			m_tempIndexTail[i]  = ORIGIN_HEADTAIL[i].Second;
		}
	}

	/// <summary>
	/// Destroy icons when reel strips changed.
	/// Becasue it will regenarate static icons from different reel strip. 
	/// </summary>
	public void DestroyIconSprites()
	{
		foreach (OTSprite os in m_Icons)
			Destroy (os.gameObject, 0);
		
	}
	
	/// <summary>
	/// Store head/tail indices for editor.
	/// </summary>
	/// <param name="_hT"> head/tail container to modified. </param>
	public void StoreHeadTail(out List<Pair<short,short>> _hT)
	{
		_hT = new List<Pair<short,short>> ();
		for (int i = 0; i < GameVariables.Instance.NUM_OF_COLS; ++i) 
		{ 
			_hT.Add( new Pair<short, short>((short)m_tempIndexHead[i], (short)m_tempIndexTail[i]) );
		}
	}

	/// <summary>
	/// Store Origin Icons.
	/// </summary>
	/// <param name="_icons"> origin icons container to modified. </param>
	public void StoreIcons(out List<Pair<Vector2,int>> _icons)
	{
		_icons = new List<Pair<Vector2,int>> ();
		for (int i = 0; i< m_Icons.Length; ++i) 
		{
			_icons.Add( new Pair<Vector2, int>( m_Icons[i].position,  m_Icons[i].frameIndex ) );
		}
	}

	/// <summary>
	/// Store Free game heaed/tail.
	/// </summary>
	public void StoreHeadTails_FG()
	{
		FG_DATA_CONTAINER.Clear ();
		for (short i = 0; i <   20; ++i) 
		{
			List< Pair<short, short>> _hts = new List< Pair<short, short>> ();
			//		List< Pair<short, short>> _taget_hts = new List< Pair<short, short>> ();
			for(short j = 0; j < GameVariables.Instance.NUM_OF_COLS; ++j)
			{
				// head / tail index added t o container. 
				_hts.Add (new Pair<short, short>((short)(GameVariables.Instance.REEL_STRIPS_FG[j].Length - 1),  (short)(GameVariables.Instance.NUM_OF_ROWS)) );
				
			}
			FG_DATA_CONTAINER.Add (_hts);
		}
	}

	/// <summary>
	/// Over-write free game editor container.
	/// </summary>
	/// <param name="_gameID"> game ID for free game. </param>
	public void SaveFGEditorContainer(int _gameID)
	{
		List< Pair<short, short>> _hts = new List< Pair<short, short>> ();
		for(short j = 0; j < GameVariables.Instance.NUM_OF_COLS; ++j)
		{
			_hts.Add(new Pair<short, short>( (short)m_tempIndexHead[j], (short)m_tempIndexTail[j]) );
		}
		FG_DATA_CONTAINER[_gameID] = _hts;
		for(int u = 0; u <  5; ++u)
			Debug.Log("gameID: " + _gameID + "  Head: " + FG_DATA_CONTAINER[_gameID][u].First + "tail :  " + FG_DATA_CONTAINER[_gameID][u].Second);
	}

	/// <summary>
	/// Load free game data by it's ID number.
	/// </summary>
	/// <param name="_gameID"> game ID for free game. </param>
	public void LoadEGEditorData(int _gameID)
	{
		for(int i = 0; i < GameVariables.Instance.NUM_OF_COLS; ++i) 
		{
			m_tempIndexHead[i] = FG_DATA_CONTAINER[_gameID][i].First;  // sicne we have 4 icons already(0,1.2.3), so the tail should start from 4.
			
			m_tempIndexTail[i]  = FG_DATA_CONTAINER[_gameID][i].Second;
			//	Debug.Log(new Vector2(m_tempIndexTail[i],m_tempIndexHead[i]) );
		}
	}

	/// <summary>
	/// Initiate(re-render) icons when changing free game. 
	/// </summary>
	/// <param name="_gameID"> game ID for free game. </param>
	public void IconsInitiate_FG(int _gameID)
	{
		for (short i = 0; i < GameVariables.Instance.NUM_OF_COLS; ++i) 
		{
			//!!! improtant to sort sprites!!!
			SortIconSprites(i);
			for(short k = 0; k < GameVariables.Instance.NUM_OF_ROWS; ++k)
			{
				int _tIndex = i* GameVariables.Instance.NUM_OF_ROWS  + k;
				int resultIndex = 0;
				int numOfIndex = GameVariables.Instance.REEL_STRIPS_CURRENT[i].Length;
				//if(GameVariables.Instance.EDITOR_FLAG != GameVariables.EDIT_STATES.NG_WAIT_TO_SPIN)
				{
					m_tempIndexHead[i] = FG_DATA_CONTAINER[_gameID][i].First;  // from random generator function.
					m_tempIndexTail[i] = FG_DATA_CONTAINER[_gameID][i].Second;
				}
				resultIndex = m_tempIndexHead[i] + (k + 1);
				resultIndex = (resultIndex < numOfIndex)? resultIndex : (resultIndex % numOfIndex);
				m_Icons[_tIndex].frameIndex = GameVariables.Instance.REEL_STRIPS_CURRENT[i][resultIndex];
			}
		}
	}
	#endregion
	
	#region Reels Spin
	/// <summary>
	/// Spining function for main game and NON-WUXIA free game.
	/// </summary>
	/// <param name="_isFreeGame"> If current game in free game mode. </param>
	///  <param name="_gameID"> Free game ID. </param>
	
	public void SpinReels( bool _isFreeGame,short _gameID = 0)
	{
		AudioManager.Instance.PlaySound("ReelSpin");


		m_SpinElapsedTime += Time.deltaTime;


		for(short i = 0; i < GameVariables.Instance.NUM_OF_COLS; ++i) 
		{
			// sort icon sprites base on y value.
			SortIconSprites (i);
			// if time out, do tweening of current drum (Not editted game)
			if( m_SpinElapsedTime > m_SpinTime[i] )
			{
				//Tweening the stopped column
				if( isTweenEnd[i] == false)  
				{
					if(GameVariables.Instance.IS_FREEGAME && FgEditor.Instance.IS_FG_EDITED[_gameID])
					{				
						TARGET_HEADTAIL[i].First = FG_DATA_CONTAINER[_gameID][i].First;
						TARGET_HEADTAIL[i].Second = FG_DATA_CONTAINER[_gameID][i].Second;
					}
					TweeningIcons(i, ref m_curCol, FgEditor.Instance.IS_FG_EDITED[_gameID]);
				}
				// skip static reel's spinning loop below.
				continue;
			}
			//spin current 'i'th reel.
			SpinOneReel(i);
			SortIconSprites(i);
		}
		// the last drum finishes spin, reset all properties
		if(isTweenEnd[GameVariables.Instance.NUM_OF_COLS-1] == true)
		{
			ResetSpin();
			// generate result for next game.
		}
	}


	public void GernerateResult()
	{
		GenerateSpinResult (out m_TargetHeadIndex, false, 0);
		if(InputManager.Instance.IsForceFG)
		{
			SpinToFG();
		}
	}

	/// <summary>
	/// spin single reel in editor mode.
	/// </summary>
	/// <param name="_moveUp"> in editorm, curret reel is moving up or down. </param>
	///  <param name="_curCol">  Current Reel number. </param>
	 public void SpinSingleReel(bool _moveUp, short _curCol)
	{
		// sort icon sprites base on y value.
		SortIconSprites (_curCol);

		//Spinning loop
		for(int k = 0; k < GameVariables.Instance.NUM_OF_ROWS; ++k)
		{
			int tInedx = _curCol * GameVariables.Instance.NUM_OF_ROWS  + k;
			//update position of icon
			if(_moveUp)
				m_Icons[tInedx].position -= new Vector2(0, m_offset.y );
			else
				m_Icons[tInedx].position += new Vector2(0, m_offset.y );
		
			//update sprite of icon
			if(m_Icons[tInedx].position.y < m_LastRowPos[_curCol].y - 1)
			{
				Vector2 dif = new Vector2(0,m_LastRowPos[_curCol].y + m_offset.y - m_Icons[tInedx].position.y);
				m_Icons[tInedx].position = m_FirstRowPos[_curCol];
				m_Icons[tInedx].position -= dif;
	
				m_Icons[tInedx].frameIndex = GameVariables.Instance.REEL_STRIPS_CURRENT[_curCol][m_tempIndexHead[_curCol]];
				UpdateTailHeadIndex(_curCol, true);

			}
			else if(m_Icons[tInedx].position.y > m_FirstRowPos[_curCol].y + 1 )
			{
				Vector2 dif = new Vector2(0,m_FirstRowPos[_curCol].y - m_offset.y - m_Icons[tInedx].position.y);
				m_Icons[tInedx].position = m_LastRowPos[_curCol];
				m_Icons[tInedx].position -= dif;
				m_Icons[tInedx].frameIndex = GameVariables.Instance.REEL_STRIPS_CURRENT[_curCol][m_tempIndexTail[_curCol]];
				//Bugggsssssssss!!!!! here array index is out of range, because of tempIndexTail
				UpdateTailHeadIndex(_curCol, false);

				//Debug.Log("Press Up Head Pos:"  + m_tempIndexHead[_curCol] +"->" + GameVariables.Instance.REEL_STRIPS_NG[_curCol][m_tempIndexHead[_curCol]]);
				//Debug.Log("Press Up Tail Pos:"  + m_tempIndexTail[_curCol] +"->" + GameVariables.Instance.REEL_STRIPS_NG[_curCol][m_tempIndexTail[_curCol]]);

			}		
		}
		SortIconSprites (_curCol);
	}

	/// <summary>
	/// Track Head/Tain when spinning.
	/// </summary>
	/// <param name="_i"> current reel number.</param>
	///  <param name="_isHeadMove"> if the reel move upwards.</param>
	private void UpdateTailHeadIndex(int _i, bool _isHeadMove)
	{
		if (_isHeadMove) 
		{
			--m_tempIndexHead [_i];  // store the last index ( size -1) of current reel. 
			if (m_tempIndexHead [_i] == -1)
			{
				m_tempIndexHead [_i] = GameVariables.Instance.REEL_STRIPS_CURRENT[_i].Length - 1;
			}

			--m_tempIndexTail [_i];
			if (m_tempIndexTail [_i] == -1)
			{
				m_tempIndexTail [_i] = GameVariables.Instance.REEL_STRIPS_CURRENT [_i].Length - 1;
			}
			
		}
		else
		{
			++m_tempIndexTail[_i];
			++m_tempIndexHead[_i]; 

			if(m_tempIndexTail[_i] == GameVariables.Instance.REEL_STRIPS_CURRENT [_i].Length )
				m_tempIndexTail[_i] =  0;
				 // store the last index ( size -1) of current reel. 
			if(m_tempIndexHead[_i] == GameVariables.Instance.REEL_STRIPS_CURRENT [_i].Length )
				m_tempIndexHead[_i] =  0;

		}


	}

	/// <summary>
	/// Sort the static iocn frames base on y value.
	/// </summary>
	/// <param name="_i"> current reel number. </param>
	private void SortIconSprites(short _i)
	{
		//Sort m_icons after every change by Y value..
		List<Pair<Vector2,int>> sp = new List<Pair<Vector2,int>>();
		for(int k = 0; k < GameVariables.Instance.NUM_OF_ROWS ; ++k)
		{
			int tInedx2 =  _i * GameVariables.Instance.NUM_OF_ROWS  + k;
			sp.Add( new Pair<Vector2,int>(m_Icons[tInedx2].position,m_Icons[tInedx2].frameIndex));
		}
		sp.Sort(comparePosY);
		//cur_Pos.Sort(comparePosY);
		for(int k = 0; k < GameVariables.Instance.NUM_OF_ROWS ; ++k)
		{
			int tInedx2 =  _i * GameVariables.Instance.NUM_OF_ROWS  + k;
			m_Icons[tInedx2].position   = sp[k].First;	
			m_Icons[tInedx2].frameIndex = sp[k].Second;
		}
		// end of sort
	}



	/// <summary>
	/// Tweening icons to proper location when near stopping.
	/// </summary>
	/// <param name="i"> Current reel number. </param>
	/// <param name="_curCol"> counting scatter column(reel). </param>
	private void TweeningIcons( short i, ref int _curCol, bool isFgEdited)
	{
		//tweening
		bool _hasScatter = false;



		for(int k = 0; k < GameVariables.Instance.NUM_OF_ROWS; ++k)
		{
			//if we go to 2nd col, the 1st col will no
			//need to tweening anymore.
			// variable 'a' is varibale used to tween icons of current 'i'th column only/
		
			if( (m_traveller / GameVariables.Instance.NUM_OF_ROWS ) == i)
			{
				int tInedx2 = i* GameVariables.Instance.NUM_OF_ROWS  + k;
				//hard core the final output before tweening.
				int numOfIndex = GameVariables.Instance.REEL_STRIPS_CURRENT[i].Length;  //FG version also
				int resultIndex = 0;

				if( (GameVariables.Instance.EDITOR_FLAG != GameVariables.EDIT_STATES.NG_WAIT_TO_SPIN
				  	 && WUXIA_SM.Instance.CURRENT_STATE == WUXIA_SM.STATE.GS_REEL_SPIN_NG) ||
				   (WUXIA_SM.Instance.CURRENT_STATE == WUXIA_SM.STATE.GS_REEL_SPIN_FG && !isFgEdited))
				{
					m_tempIndexHead[i] = m_TargetHeadIndex[i];  // from random generator function.
					m_tempIndexTail[i] = m_TargetHeadIndex[i] + GameVariables.Instance.NUM_OF_ROWS + 1;
					if(m_tempIndexTail[i] > numOfIndex) 
						m_tempIndexTail[i] = m_tempIndexTail[i]  % numOfIndex;
				}
				else if(GameVariables.Instance.EDITOR_FLAG == GameVariables.EDIT_STATES.NG_WAIT_TO_SPIN
				        || (WUXIA_SM.Instance.CURRENT_STATE == WUXIA_SM.STATE.GS_REEL_SPIN_FG && isFgEdited) )
				{
					// for wuxia game, in free game, non-win spin will load specific rell strip to guranteen non-win outpu.
					// win spin will go throuh the arlgorithm to decide the output symbols.

					// for other projects.
					m_tempIndexHead[i] = Icons.instance.TARGET_HEADTAIL[i].First;  // from editor
					m_tempIndexTail[i] = Icons.instance.TARGET_HEADTAIL[i].Second;
				}


				resultIndex = m_tempIndexHead[i] + (k + 1);
				resultIndex = (resultIndex < numOfIndex)? resultIndex : (resultIndex % numOfIndex);
				m_Icons[tInedx2].frameIndex = GameVariables.Instance.REEL_STRIPS_CURRENT[i][resultIndex];
			

				// Play Stop Sound Clip.
				// No need to test row 0.(not invisable.)
				if(m_Icons[tInedx2].frameIndex  == GameVariables.Instance.SCATTER_INDEX  && _curCol == i && k != 0) 
				{
					_hasScatter = true;
					_curCol++;
				}
			
				if(_hasScatter && k == GameVariables.Instance.NUM_OF_ROWS - 1 )
				switch(i)
				{
					case 1:
					AudioManager.Instance.PlaySound("Scatter1"); break;
					case 2:
					AudioManager.Instance.PlaySound("Scatter2"); break;
					case 3:
					AudioManager.Instance.PlaySound("Scatter3"); break;
				}
				else if(!_hasScatter && k == GameVariables.Instance.NUM_OF_ROWS - 1)
				{						     //only play once for each reel.  ^^^^^^^^^^
					AudioManager.Instance.PlaySound("ReelStop",0.1f);
					AudioManager.Instance.StopSound("ReelStop");
				}
				// End of Play Stop Sound Clip

				// Make sure all reels has some bounce back strength.
				float adjust = ( m_FirstRowPos[i].y - m_Icons[tInedx2].position.y);// - k * m_Icons[tInedx2].size.y; 
				adjust = adjust % m_Icons[tInedx2].size.y;
				Vector2 offset = new Vector2(0, adjust);
				m_Icons[tInedx2].position += offset ;
				offset = new Vector2(0, 150);
				m_Icons[tInedx2].position  -= offset;

				Vector3  tw =  new Vector3(offset.x, offset.y, 0);
				++m_traveller;
				//Debug.Log("TWEEN COL :"+i);
				new OTTween(m_Icons[tInedx2].transform, 0.5f,OTEasing.QuintOut)
									.TweenAdd("position",tw)
									.OnFinish(delegate(OTTween tween)
						      		{
										if( (m_traveller / GameVariables.Instance.NUM_OF_ROWS) == i + 1)
										isTweenEnd[i] = true;
									});
			}
		}  // end of tweening
	}
	
	/// <summary>
	/// help function for SpinReels.
	/// </summary>
	/// <param name="i"> reel number. </param>
	private void SpinOneReel(short i)
	{
		//Spinning loop
		for(int k = 0; k < GameVariables.Instance.NUM_OF_ROWS; ++k)
		{
			int tInedx = i* GameVariables.Instance.NUM_OF_ROWS  + k;
			//update position of icon
			m_Icons[tInedx].position += m_Speed;
		
			//update sprite of icon
			if(m_Icons[tInedx].position.y < m_LastRowPos[i].y + m_offset.y)
			{
				Vector2 dif = new Vector2(0,m_LastRowPos[i].y + m_offset.y - m_Icons[tInedx].position.y);
				m_Icons[tInedx].position = m_FirstRowPos[i];
				m_Icons[tInedx].position -= dif;
			
				m_Icons[tInedx].frameIndex = GameVariables.Instance.REEL_STRIPS_CURRENT[i][m_tempIndexHead[i]];

				UpdateTailHeadIndex(i, true);
			}


		}
	}

	/// <summary>
	/// help function for Generate random output after end of evry spin.
	/// </summary>
	/// <param name="_headIndex"> Tareget heads to modify. </param>
	///  <param name="_isFreeGame"> If curret gamWe in free mode.</param>
	//  <param name="_gameID"> Current free game nnumber </param>
	private void GenerateSpinResult(out int[] _headIndex, bool _isFreeGame, short _gameID)
	{
		_headIndex = new int[GameVariables.Instance.NUM_OF_COLS];
	 	
	 	for(int i = 0; i < GameVariables.Instance.NUM_OF_COLS; ++i)
	 	{
			if(!_isFreeGame)
			{
	 			_headIndex[i] = Random.Range(0, GameVariables.Instance.REEL_STRIPS_NG[i].Length -1);
			}
			else
			{
				//_headIndex[i] = FG_DATA_CONTAINER[_gameID][i].First;
				_headIndex[i] = Random.Range(0, GameVariables.Instance.REEL_STRIPS_FG[i].Length -1);
			}
	 	}
	}

	public void SetFirstFreeGameResult()
	{
		GenerateSpinResult(out m_TargetHeadIndex, true, 0);
	}
	#endregion

}
