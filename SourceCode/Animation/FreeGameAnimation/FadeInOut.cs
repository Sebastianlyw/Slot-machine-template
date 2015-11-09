using UnityEngine;
using System.Collections;


// <summary>
/// <para>Version: 2.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Implementation of alpha fade in/out for 2d sprite object.
/// </summary>
public class FadeInOut : MonoBehaviour {
	
	//!sprite renderer component of attached object.
	SpriteRenderer my_sprite;
	
	
	/// <summary> 
	/// Use this for Initialization.
	/// </summary>
	void Awake()
	{
		my_sprite = GetComponent<SpriteRenderer>() as SpriteRenderer;
	}
	/// <summary> 
	/// Fade in object with a delay time _t.
	/// </summary>
	/// <param name="_t"> Fade In Time </param>
	/// <param name="_dt"> Delay time</param>
	public void FadeIn(float _alphaTo, float _t, float _dt)
	{
		LeanTween.value (gameObject, setSpriteAlpha, 0f, _alphaTo, _t).setDelay (_dt);
	}
	/// <summary> 
	/// Fade out object with a delay time _t.
	/// </summary>
	/// <param name="_t"> Fade Out Time </param>
	/// <param name="_dt"> Delay time</param>
	public void FadeOut(float _t, float _dt)
	{
		LeanTween.value (gameObject, setSpriteAlpha, 0.69f, 0f, _t).setDelay (_dt);
	}
	/// <summary> 
	/// Call back function which will be called per update frame.
	/// </summary>
	/// <param name="_alpha"> the updated alpha value from leantween funciton. </param>
	private void setSpriteAlpha( float _alpha )
	{
		if(my_sprite)
			my_sprite.color = new Color(1f,1f,1f,_alpha);
	}
	
}
