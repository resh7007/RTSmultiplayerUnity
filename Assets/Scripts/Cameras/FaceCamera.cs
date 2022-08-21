using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class FaceCamera : MonoBehaviour
{
   private Transform mainCameraTransform;

   private void Start()
   {
      mainCameraTransform = Camera.main.transform;
   }

   private void LateUpdate()
   {
      transform.LookAt(transform.position+mainCameraTransform.rotation *Vector3.forward, mainCameraTransform.transform.rotation *Vector3.up);
      
   }
}
