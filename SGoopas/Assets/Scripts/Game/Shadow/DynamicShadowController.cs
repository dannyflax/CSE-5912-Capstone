﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicShadowController : ShadowController {

    private Rigidbody rb;
    private Vector3 linearVelocity;
    private Vector3 angularVelocity;

	public override void ConfigureWithLightParams(Light shadowLight, GameObject shadowPlane) { 
		// Wait until we get the light params from above to construct the shadow.
		base.ConfigureWithLightParams (shadowLight, shadowPlane);
		rb = gameObject.GetComponent<Rigidbody>();
		linearVelocity = new Vector3();
		angularVelocity = new Vector3();
	}

    public override void ConstructShadow()
    {
        base.ConstructShadow();

        linearVelocity = rb.velocity;
        rb.velocity = new Vector3();
        angularVelocity = rb.angularVelocity;
        rb.angularVelocity = new Vector3();

        rb.isKinematic = true;

        shadowCaster.CreateShadow();
    }

    public override void DeconstructShadow()
    {
        base.DeconstructShadow();

        rb.isKinematic = false;

        rb.velocity = linearVelocity;
        rb.angularVelocity = angularVelocity;

        shadowCaster.DestroyShadow();
    }

}
