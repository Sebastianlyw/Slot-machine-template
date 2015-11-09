#region NameSpace
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#endregion

/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Handle Icon related animations.
/// </summary>
public class IconAnim : MonoBehaviour {

	#region Variables
	// Singleton
	private static IconAnim instance;
	
	// Constuct
	private IconAnim() {}
	
	// Instance
	public static IconAnim Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(IconAnim)) as IconAnim;
			
			return instance;
		}
		
	}
	/**********end of Singleton*******************/

	private OTAnimatingSprite[] m_AniSpr;

	private int m_StaticIndex;
	private const int 	SCATTERLOOPTIMES  = 2;
	private const int 	LINEANI_SPEED 		   = 100;  // the smaller value means the higher speed.
	private const int 	ICONBLINK_SPEED     = 50;  // the smaller value means the higher speed.

	

	public int CURRENT_WINLINE
	{
		get
		{
			return k;
		}
		set
		{
			k = value;
		}
	}


	public enum WUXIA_ANISTATE
	{
		IDLE,
		FIRST_CHECK,
		CHANGE_ICON,
		SECOND_CHECK,
		END
	}
	private WUXIA_ANISTATE m_AniStateWX 		= WUXIA_ANISTATE.IDLE;
	public WUXIA_ANISTATE WX_ANIM
	{
		get { return m_AniStateWX;}
		set { m_AniStateWX = value;}
	}

	public bool IsFinshScatter;
	private int m_FinishCounter;
	private int m_ScatterPlayLoop;
	private int m_ScatterCounter;
	private int m_SubPlayCounter;

	private int t = 0, k = 0, u =0,  loopTimes = 0;
	private bool isStartLines = false;
	public bool IS_START_LINEANI
	{
		get
		{
			return isStartLines;
		}
		set
		{
			isStartLines = value;
		}
	}
	
	// use hashset to store unique element.
	HashSet<int> blinkIcons;

	/* Variables for free game animation*/


	/* End of Variables for free game animation*/

	#endregion

	/// <summary>
	/// Generate a new OT- Animation Sprite object with arguments taken.
	/// </summary>
	/// <param name="_as"> A <see cref="OTAnimatingSprite"/> object will be generated. </param>
	/// <param name="_name"> name of generated object.</param>
	/// <param name="_atlas"> name of altas data object.</param>
	/// <param name="_dp"> Depth(z value of position) of object.</param>
	/// <param name="_onStart">if aniamtion plays on start.</param>
	/// <param name="_alpha"> alpha value.</param>
	private void GenarateAniSprite (out OTAnimatingSprite _as, string _name, string _atlas, int _dp, bool _onStart, float _alpha)
	{
		_as = new OTAnimatingSprite();
		_as = OT.CreateObject(OTObjectType.AnimatingSprite).GetComponent<OTAnimatingSprite>(); 
		_as.name = _name;
		_as.depth = _dp;
		_as._size =  GameObject.Find (_atlas).GetComponent<OTSpriteAtlasCocos2D>().atlasData[0].size;
		_as._playOnStart = _onStart;
		_as.alpha = _alpha;
		_as.transform.parent = GameObject.Find ("AnimatedIcons").transform;
		_as.looping = false;
		
	}

	
	/// <summary> 
	/// Hide Animated icons when some pause or skip case.
	/// </summary>
	public void HideAnimIcons()
	{

		if(TextAndDigitDisp.Instance.IS_COUNTEND || WinManager.Instance.WINICONINDICIES.Count == 0)
		{	
			for (int i = 0; i < GameVariables.Instance.NUM_OF_COLS * GameVariables.Instance.NUM_OF_ROWS ; ++i)
				Icons.Instance.m_Icons [i].alpha = 1;
			
		}
		
		DestroyAnimSprite();
		for (int i = 0; i < GameVariables.Instance.NUM_OF_COLS - 1; ++i)
			LineAnim.Instance.SPR_WINLINES  [i].alpha = 0f;
	}
	
	/// <summary> 
	/// Play icon animation.
	/// </summary>
	public void IconAnimation()
	{

		if(!GameVariables.Instance.IsThreeScatters())
			IsFinshScatter = true;
		
		if(!IsFinshScatter || IsIconAnimating())
			return;
		
		//Regenerate animating sprite every time to avoid non-synchronize issue. 
		if(IsIconDestroyed())
			GenerateAnimSprites();
		
		m_SubPlayCounter = 0;

		//m_IsPlay = true;
		//return if no winline besides scatters win, looping scatters animation.
		if (WinManager.Instance.WINICONINDICIES.Count == 0)
		{
			IsFinshScatter = false;
			return;
		}

		int[] winIndecis = WinManager.Instance.WINICONINDICIES[ k];
		
		for( int i = 0 ; i < winIndecis.Length; ++i)
		{
			m_StaticIndex = winIndecis[i];
			int _fi = Icons.Instance.m_Icons[ m_StaticIndex ].frameIndex;
			UpdateAniContainer(ref m_AniSpr[i], _fi, true, i);
		
			m_AniSpr[i].position = Icons.Instance.m_Icons[	m_StaticIndex ].position;
			Icons.Instance.m_Icons[	m_StaticIndex ].alpha = 0f;
			if(_fi == GameVariables.Instance.SUBSTITUE_INDEX)
				m_AniSpr[i].depth = 450;
			else
				m_AniSpr[i].depth = 460;
		}
	}

	/// <summary> 
	/// Play scatters animation.
	/// </summary>
	public void ScatterAnim()
	{
		if(IsFinshScatter || IsIconAnimating())
			return;
		
		if (IsIconDestroyed())
			GenerateAnimSprites();

		if(GameVariables.Instance.IsThreeScatters())
		{
			for(int p = 0; p < GameVariables.Instance.FG_SCATTERS; ++p)
			{
				int _index  = GameVariables.Instance.SCATTERINDICES_WIN[p];
				int _fi = Icons.Instance.m_Icons[	_index ].frameIndex;
				UpdateAniContainer(ref m_AniSpr[p], _fi, true, p) ;
			
				m_AniSpr[p].position = Icons.Instance.m_Icons[_index].position;
				Icons.Instance.m_Icons[_index ].alpha = 0f;
				m_AniSpr[p].onAnimationFinish = ScatterAnimFinsh;
				m_AniSpr[p].depth = 450;
			}
		}
	}

	/// <summary> 
	/// Assign proper container to animation sprite object
	/// </summary>
	/// <param name="_aniSp"> name of animation container. </param>
	/// <param name="_frameIndex"> frame index of symbol in static icons sprite sheet. </param>
	/// <param name="_i"> index of animation sprites </param>
	private void UpdateAniContainer(ref OTAnimatingSprite _aniSp, int _frameIndex, bool _isPlay, int _i)
	{
		string acName = (_frameIndex < 10)? "Icon0" + _frameIndex + "_ani_00_Altas" :  "Icon" + _frameIndex + "_ani_00_Altas";
		string aniName = (_frameIndex < 10)? "Icon0" + _frameIndex + "_Ani" :  "Icon" + _frameIndex + "_Ani";
		
		_aniSp.spriteContainer = GameObject.Find(acName).GetComponent<OTSpriteAtlasCocos2D>();
		_aniSp.animation = GameObject.Find(aniName).GetComponent<OTAnimation>();
		
		if(_isPlay)
		{
			_aniSp.Play();
			//	_aniSp.looping = true;
			m_AniSpr[_i].playOnStart = true;
			_aniSp.alpha = 1f;
			_aniSp.frameIndex = 0;
			foreach (OTAnimatingSprite os in m_AniSpr)
				os.frameIndex = 0;
		}

		_aniSp.numberOfPlays = 1;

		if(_frameIndex < GameVariables.Instance.SCATTER_INDEX)  
		{
			_aniSp.size = new Vector2 (180, 180);
			if(_frameIndex == 11) //! bell index
				_aniSp.size = new Vector2 (170, 170);
		}
		else // substitute and sactter animation icons
		{
			if(_frameIndex == GameVariables.Instance.SCATTER_INDEX)
			{
				_aniSp.size = new Vector2 (300, 300);
				_aniSp.numberOfPlays = 2;
			}
			else //subs
			{
				_aniSp.size =  new Vector2 (250, 250);
				_aniSp.alpha = 0.999f;
			}
		 
		}
		
		if (_frameIndex != GameVariables.Instance.SCATTER_INDEX)  // excecpt scatter.
			_aniSp.onAnimationFinish = AnimationFinsh;
		else  
			_aniSp.onAnimationFinish = null;
	}

	/// <summary> 
	/// Function Callback for OTAnimation
	/// </summary>
	private void AnimationFinsh(OTObject owner)
	{
		string	_subName = "Icon13_Ani";
		OTAnimatingSprite tempAsp = owner.GetComponent<OTAnimatingSprite> ();
		if (tempAsp.animation.name == _subName ) 
		{
			
			++m_SubPlayCounter;
			owner.GetComponent<OTAnimatingSprite> ().alpha = 0;
			//! if got more than one pearls, should take action after loop only once.
			if(m_SubPlayCounter == 2)
				return;
			
			ActtionAfterLoops ();
			
		}
		else
		{
			if(HasPearInWin())	//if(m_SubPlayCounter != 0)
				return;
			if (++m_FinishCounter % WinManager.Instance.WINICONINDICIES [k].Length != 0)
				return;
			
			m_FinishCounter = 0;
			//Debug.Log("Normal Icons");
			owner.GetComponent<OTAnimatingSprite> ().alpha = 0;
			ActtionAfterLoops ();
		}
		
		DestroyAnimSprite();
	}

	private bool HasPearInWin()
	{

		for(int i = 0; i < WinManager.Instance.WINICONINDICIES[ k ].Length; ++ i)
		{	
			if(m_AniSpr[i].animation)
			if(m_AniSpr[i].animation.name == "Icon13_Ani")
				return true;
		}
		return false;
	
	}

	private void ActtionAfterLoops()
	{

		int[] winIndecis = WinManager.Instance.WINICONINDICIES[ k ];

		//FreeGame.Instance.HideNormalPearls ();
		for (int i = 0; i < winIndecis.Length; ++i) {
		
				Icons.Instance.m_Icons [winIndecis [i]].alpha = 1f;
		}
		// win line index incresement.
		if (++k >= WinManager.Instance.WINICONINDICIES.Count) {
			IsFinshScatter = false;
			k = 0;
		}
		//reset lines blinking
		LineAnim.Instance.HideLines (k);
		LineAnim.Instance.BLINKTIMER = 0;
	}
	
	/// <summary> 
	/// Function Callback for scatter OTAnimation
	/// </summary>
	private void ScatterAnimFinsh(OTObject owner)
	{
		if (++m_ScatterCounter % 2 != 1)  // 2 scatters in this game.
			return;
		m_ScatterCounter = 0;
		//! check if got other normal symbols win, play those animations.
		//! and stop playing scatter animation. 
		if(WinManager.Instance.WINICONINDICIES.Count != 0)
		{
			Debug.Log("set scatter true play");
			IsFinshScatter = true;
		}
		
		
		
		for(int p = 0; p < GameVariables.Instance.FG_SCATTERS; ++p)
		{
			int _index  = GameVariables.Instance.SCATTERINDICES_WIN[p];
			Icons.Instance.m_Icons[_index ].alpha = 1f;
		}
		
		DestroyAnimSprite();
	}
	
	/// <summary> 
	/// Blink winning icons. 
	/// </summary>
	public void BlinkWinIcons()
	{

		//Blinking win icons twice.
		if( (++u)  % ICONBLINK_SPEED == 0)
		{
			++loopTimes;
		}
		if (loopTimes == 2)
		{
			LineAnim.Instance.BLINKTIMER  = 0;
			loopTimes = 0;
			t = k  = 0;
	//		Debug.Log(m_AniStateWX);

		
			isStartLines 		= true;
			for(int i = 0; i < GameVariables.Instance.NUM_OF_COLS  * GameVariables.Instance.NUM_OF_ROWS ; ++i)
				Icons.Instance.m_Icons[i].alpha = 1;

			return;
		}
		
		blinkIcons.Clear();
		//get all win icons to blink.
		
		for(int ii = 0; ii < 	WinManager.Instance.WINICONINDICIES.Count; ++ii)
		{
			for(int nn = 0; nn < 	WinManager.Instance.WINICONINDICIES[ii].Length; ++nn)
			{
				blinkIcons.Add(	WinManager.Instance.WINICONINDICIES[ii][nn]);
			}
		}
		int[] tarry = new int[blinkIcons.Count];
		blinkIcons.CopyTo(tarry);
		// blink win icons.
		for(int q = 0; q < blinkIcons.Count; ++q)
		{
			Icons.Instance.m_Icons[ tarry[q] ].alpha = ( (u) % (ICONBLINK_SPEED) > (ICONBLINK_SPEED /2) ) ? 1 : 0; 
		}
		
		if(GameVariables.Instance.IsThreeScatters())
		{

			AudioManager.Instance.PlaySound("FGHit");

			for(int p = 0; p < GameVariables.Instance.FG_SCATTERS; ++p)
				Icons.Instance.m_Icons[GameVariables.Instance.SCATTERINDICES_WIN[p]].alpha
					= ( (u) % (ICONBLINK_SPEED) > (ICONBLINK_SPEED /2) ) ? 1 : 0; 

		}

	}//end of icons blink.


	/// <summary> 
	/// General play animation function, including both scatter and normal icons.
	/// </summary>
	public void PlayAnimation()
	{
		//update number of scatters checking everytime before animating.
		WinManager.Instance.IsTriggerFG();

		if(GameVariables.Instance.IsThreeScatters())
		{
		
		
		//	Debug.Log("SCATTERS : "  + GameVariables.Instance.SCATTERINDICES_WIN.Count);
			if(IconAnim.Instance.IsFinshScatter == false)
			{
			
				AudioManager.Instance.StopSound("FGHit");
				//LineButtons.Instance.StopPlayLineNumber();
				IconAnim.Instance.ScatterAnim();
				return;
			}
			else
			{
				IconAnim.instance.IconAnimation();
			}
		}

		if(LineAnim.Instance.WINLINES.Count == 0)  // 3 scatter without other wining lines.
		{ 
			return;
		}
		
		//when lines blinking, play icon animation at same time. 
		if(!GameVariables.Instance.IsThreeScatters())
			IconAnim.Instance.IconAnimation();
	}

	
	/// <summary> 
	/// Rest icons status.
	/// </summary>
	public void Reset()
	{
		HideAnimIcons();
		LineButtons.Instance.StopAnimation ();
		k = t = u = loopTimes  = 0;
	}


	private void GenerateAnimSprites()
	{
		for(short i = 0; i <GameVariables.Instance.NUM_OF_COLS  ; ++i)
		{
			string animSpriteName = "Icon_AniSprite" + i;  
			GenarateAniSprite(out m_AniSpr[i], animSpriteName, "Icon00_ani_00_Altas" , 900, false, 1f);
			
			m_AniSpr[i].onAnimationFinish = AnimationFinsh;
			
		}
	}
	
	private void DestroyAnimSprite()
	{
		Debug.Log("Animation Destryed");
		GameObject par = GameObject.Find("AnimatedIcons");
		if(par)
			for(int i = 0; i < par.transform.childCount; ++i)
		{
			if(par.transform.GetChild(i))
				Destroy(par.transform.GetChild(i).gameObject);
		}
	}

	/// <summary>
	/// Determines whether this instance icons destroyed.
	/// </summary>
	/// <returns><c>true</c> if this instance is icon destroyed; otherwise, <c>false</c>.</returns>
	private bool IsIconDestroyed()
	{
		
		GameObject par = GameObject.Find("AnimatedIcons");
		if(par)
			if(par.transform.childCount == 0)
				return true;
		return false;
	}
	private bool IsIconAnimating()
	{
		foreach(OTAnimatingSprite oas in m_AniSpr)
			if(oas && oas.isPlaying)
				return true;
		
		return false;
		
	}



	/// <summary> 
	/// Initialization of local varaibles.
	/// </summary>
	void Awake()
	{
	
		m_StaticIndex 		= 0;
		m_FinishCounter 	= 0;
		m_ScatterPlayLoop = 0;
		IsFinshScatter 		= true;
		isStartLines 			= false;
		m_AniStateWX 		= WUXIA_ANISTATE.IDLE;
		
		int n = GameVariables.Instance.NUM_OF_COLS;// * (GameVariables.Instance.NUM_OF_ROWS - 1);
		m_AniSpr = new OTAnimatingSprite[n];
		

		
		blinkIcons =  new HashSet<int>();
	}


}
