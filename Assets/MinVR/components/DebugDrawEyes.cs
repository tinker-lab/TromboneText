using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinVR
{

	public class DebugDrawEyes : MonoBehaviour
	{

        public float sphereScale = 0.05f;

        GameObject leftObj;
        GameObject rightObj;

        // Start is called before the first frame update
        void Start() {
            leftObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftObj.transform.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
            leftObj.GetComponent<Renderer>().material.color = Color.red;

            rightObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightObj.transform.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
            rightObj.GetComponent<Renderer>().material.color = Color.green;
        }

        // Update is called once per frame
        void Update()
		{
            var screens = FindObjectsOfType<TrackedProjectionScreen>();
            for (int i=0; i<screens.Length; i++) {
                Plane p = new Plane(screens[i].GetTopLeftCorner(), screens[i].GetTopRightCorner(), screens[i].GetBottomRightCorner());

                Vector3 lp = p.ClosestPointOnPlane(screens[i].GetLeftEyePosition());
                leftObj.transform.position = lp;

                Vector3 rp = p.ClosestPointOnPlane(screens[i].GetRightEyePosition());
                rightObj.transform.position = rp;
            }
		}
	}

}