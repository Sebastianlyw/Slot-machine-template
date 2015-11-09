using UnityEngine;
using System.Collections;

public class JumpBounce : MonoBehaviour {

	public float offsetY = 10f;
	public float bouncingTime =0;
	private float originY;
	private float tragetY;
	private bool m_IsStarted;
	// Use this for initialization
	void Start () {
	
		originY = transform.position.y;
		offsetY = 50f;
		bouncingTime = 0.35f;
		m_IsStarted = false;
	}
	
	// Update is called once per frame
	void Update () {
		tragetY = originY + offsetY;

		Hashtable returnParam = new Hashtable();
		returnParam.Add("t", 0.35f);
		returnParam.Add("obj", gameObject);
		returnParam.Add("y", originY);


		if(!m_IsStarted || Input.GetKeyDown(KeyCode.B))
			LeanTween.moveY(gameObject, tragetY,bouncingTime).setEase(LeanTweenType.easeOutBounce ).setOnComplete(BounceBack).setOnCompleteParam(returnParam);

	}


	private void BounceBack(object hash)
	{
		Hashtable h = hash as Hashtable;
		float time = (float) h["t"];
		float oY = (float) h ["y"];
		GameObject go = (GameObject) h ["obj"];
		LeanTween.moveY(go, oY, time).setEase(LeanTweenType.easeOutBounce );
		m_IsStarted = true;
	}

	private void  RestJumpBounce()
	{
		m_IsStarted = false;
	}

}

