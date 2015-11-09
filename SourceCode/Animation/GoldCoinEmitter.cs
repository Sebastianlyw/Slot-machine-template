using UnityEngine;
using System.Collections;

public class GoldCoinEmitter : MonoBehaviour {
	
	/*
	* Singleton Declaration
	*/
	// Singleton
	private static GoldCoinEmitter instance;
	
	// Constuct
	private GoldCoinEmitter() {}
	
	// Instance
	public static GoldCoinEmitter Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(GoldCoinEmitter)) as GoldCoinEmitter;
			
			return instance;
		}
		
	}
	/*
	* End of Singleton Declaration
	*/	
	
	public enum STATE
	{
		DISP_NONE,
		ANIMATE_GOLD_COIN
	};
	
	OTAnimatingSprite m_aniSpriteGoldCoin01;
	OTAnimatingSprite m_aniSpriteGoldCoin02;
	OTAnimatingSprite m_aniSpriteGoldCoin03;
	OTAnimatingSprite m_aniSpriteGoldCoin04;
	OTSpriteAtlasCocos2D m_spriteContainerGoldCoin;
	
	Vector2[] m_sizeOfGoldCoin = new Vector2[30];
	Vector2[] m_posGoldCoin = new Vector2[30];
	
	float m_fElapsedTime;
	
	STATE m_state = STATE.DISP_NONE;
	
	void Awake () {
		
		// Init Gold coin size
		m_aniSpriteGoldCoin01 = GameObject.Find ("GoldCoin_01").GetComponent<OTAnimatingSprite>();
		m_aniSpriteGoldCoin02 = GameObject.Find ("GoldCoin_02").GetComponent<OTAnimatingSprite>();
		m_aniSpriteGoldCoin03 = GameObject.Find ("GoldCoin_03").GetComponent<OTAnimatingSprite>();
		m_aniSpriteGoldCoin04 = GameObject.Find ("GoldCoin_04").GetComponent<OTAnimatingSprite>();
		
		int l = 0;
		for (int i=0; i<13; i++)
		{
			string strAtlasName = "";
			
			if (i < 9)
			{
				strAtlasName = "GoldCoin_Altas_0" + (i + 1);
			}
			else
			{
				strAtlasName = "GoldCoin_Altas_" + (i + 1);
			}
			
			int j = GameObject.Find (strAtlasName).GetComponent<OTSpriteAtlasCocos2D>().atlasData.Length;
			
			for (int k=0; k<j; k++)
			{
				m_sizeOfGoldCoin[l] = GameObject.Find (strAtlasName).GetComponent<OTSpriteAtlasCocos2D>().atlasData[k].size;
				l++;
			}
		}
		
		
		// Init Gold coin position
		float[] XposCoin = new float[30]
		{ 370.0f,369.0f,353.0f,343.0f,342.0f,347.0f,330.0f,325.0f,315.0f,269.0f,
			274.0f,266.0f,277.0f,198.0f,230.0f,160.0f,184.0f,222.0f,183.0f,124.0f,
			120.0f,166.0f,122.0f,87.0f,51.0f,56.0f,153.0f,149.0f,99.0f,267.0f };
		
		float[] YposCoin = new float[30]
		{ 0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,0.0f,
			0.0f,37.0f,51.0f,27.0f,62.0f,46.0f,146.0f,167.0f,179.0f,218.0f,336.0f,
			345.0f,438.0f,476.0f,459.0f,491.0f };		
		
		for(int i=0; i<30; i++)
		{
			m_posGoldCoin[i] = new Vector2(0.0f, (YposCoin[i] * -1.0f) + (-140.0f));
		}
		
		
		// Hide the animation
		m_aniSpriteGoldCoin01.alpha = 0f;
		m_aniSpriteGoldCoin02.alpha = 0f;
		m_aniSpriteGoldCoin03.alpha = 0f;
		m_aniSpriteGoldCoin04.alpha = 0f;	
		
		// Get Sprite Container
		m_spriteContainerGoldCoin = GameObject.Find ("GoldCoin_Altas_01").GetComponent<OTSpriteAtlasCocos2D>();
		
		// Init elapsed time
		m_fElapsedTime = 0f;
		
		// Init State
		m_state = STATE.DISP_NONE;		
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		switch(m_state)
		{
		case STATE.DISP_NONE:
			break;
		case STATE.ANIMATE_GOLD_COIN:
			UpdateGoldCoinAnimation();
			break;
		default:
			break;
		}		
	}
	private bool m_IsStart = false;
	public void StartGoldCoinAnimation() {

		m_IsStart = true;
		m_state = STATE.ANIMATE_GOLD_COIN;
		
		m_aniSpriteGoldCoin01.looping = true;
		m_aniSpriteGoldCoin02.looping = true;
		m_aniSpriteGoldCoin03.looping = true;
		m_aniSpriteGoldCoin04.looping = true;
		
		m_fElapsedTime = 0f;
		
		m_aniSpriteGoldCoin01.Play();
		m_aniSpriteGoldCoin02.Play(0, 0.48f);
		m_aniSpriteGoldCoin03.Play(0, 0.76f);
		m_aniSpriteGoldCoin04.Play(0, 1.32f);		
	}
	
	void UpdateGoldCoinAnimation() {

		if(!m_IsStart)
			return;

		m_fElapsedTime += Time.deltaTime;
		
		// Display the first frame when the delay complete
		if (m_aniSpriteGoldCoin01.alpha != 1f && m_fElapsedTime >= 0f)
		{
			m_aniSpriteGoldCoin01.alpha = 1f;
		}
		
		if (m_aniSpriteGoldCoin02.alpha != 1f && m_fElapsedTime >= 0.48f)
		{
			m_aniSpriteGoldCoin02.alpha = 1f;
		}		
		
		if (m_aniSpriteGoldCoin03.alpha != 1f && m_fElapsedTime >= 0.76f)
		{
			m_aniSpriteGoldCoin03.alpha = 1f;
		}		
		
		if (m_aniSpriteGoldCoin04.alpha != 1f && m_fElapsedTime >= 1.32f)
		{
			m_aniSpriteGoldCoin04.alpha = 1f;
		}	
		
		m_aniSpriteGoldCoin01.size = m_sizeOfGoldCoin[int.Parse(m_aniSpriteGoldCoin01.frameName)];
		m_aniSpriteGoldCoin02.size = m_sizeOfGoldCoin[int.Parse(m_aniSpriteGoldCoin02.frameName)];
		m_aniSpriteGoldCoin03.size = m_sizeOfGoldCoin[int.Parse(m_aniSpriteGoldCoin03.frameName)];
		m_aniSpriteGoldCoin04.size = m_sizeOfGoldCoin[int.Parse(m_aniSpriteGoldCoin04.frameName)];
		
		m_aniSpriteGoldCoin01.position = m_posGoldCoin[int.Parse(m_aniSpriteGoldCoin01.frameName)];
		m_aniSpriteGoldCoin02.position = m_posGoldCoin[int.Parse(m_aniSpriteGoldCoin02.frameName)];
		m_aniSpriteGoldCoin03.position = m_posGoldCoin[int.Parse(m_aniSpriteGoldCoin03.frameName)];
		m_aniSpriteGoldCoin04.position = m_posGoldCoin[int.Parse(m_aniSpriteGoldCoin04.frameName)];		
	}
	
	public void StopGoldCoinAnimation() {
		if(!m_IsStart)
			return;
		m_IsStart = false;
		m_state = STATE.DISP_NONE;
		
		m_aniSpriteGoldCoin01.looping = false;
		m_aniSpriteGoldCoin02.looping = false;
		m_aniSpriteGoldCoin03.looping = false;
		m_aniSpriteGoldCoin04.looping = false;
		
		m_aniSpriteGoldCoin01.alpha = 0f;
		m_aniSpriteGoldCoin02.alpha = 0f;
		m_aniSpriteGoldCoin03.alpha = 0f;
		m_aniSpriteGoldCoin04.alpha = 0f;		
		
		m_aniSpriteGoldCoin01.Stop();
		m_aniSpriteGoldCoin02.Stop();
		m_aniSpriteGoldCoin03.Stop();
		m_aniSpriteGoldCoin04.Stop();	
		
		m_aniSpriteGoldCoin01.spriteContainer = m_spriteContainerGoldCoin;
		m_aniSpriteGoldCoin02.spriteContainer = m_spriteContainerGoldCoin;
		m_aniSpriteGoldCoin03.spriteContainer = m_spriteContainerGoldCoin;
		m_aniSpriteGoldCoin04.spriteContainer = m_spriteContainerGoldCoin;
		
		m_aniSpriteGoldCoin01.frameIndex = 0;
		m_aniSpriteGoldCoin02.frameIndex = 0;
		m_aniSpriteGoldCoin03.frameIndex = 0;
		m_aniSpriteGoldCoin04.frameIndex = 0;	


	}
	
	public STATE GetState() {
		
		return m_state;
	}	
}
