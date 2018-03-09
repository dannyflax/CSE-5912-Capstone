﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spotlight : MonoBehaviour {
    private GameObject planeChild;
    private bool generated;
    private Plane plane;


    public enum ShadowApplicatorType { Hazard, Physics2D };

    public ShadowApplicatorType applicatorType;

    public Material shadowMaterial;

    void Start()
    {
        planeChild = new GameObject(this.gameObject.name + " plane collider");
        planeChild.transform.parent = transform;
        generated = false;
        plane = new Plane(MainObjectContainer.Instance.ShadowPlane.transform.up, MainObjectContainer.Instance.ShadowPlane.transform.position);
    }

    private void CreateCollider()
    {
        ShadowPolygonHelper.MakeSpotlightCollider(this.gameObject.GetComponent<Light>(), plane, planeChild);
        Rigidbody2D rb2d = planeChild.AddComponent<Rigidbody2D>();
        rb2d.isKinematic = true;
        SpotlightCollider colliderScript = planeChild.AddComponent<SpotlightCollider>();
        colliderScript.applicatorType = this.applicatorType;
        colliderScript.player2D = MainObjectContainer.Instance.Player2D;
        colliderScript.shadowMaterial = this.shadowMaterial;
    }

    private void ActivateCollider()
    {
        if (!generated)
        {
            CreateCollider();
            generated = true;
        }
        planeChild.SetActive(true);
    }

    private void DeactivateCollider()
    {
        planeChild.SetActive(false);
    }

    public void SwitchTo2D(Cancellable cancellable)
    {
        cancellable.PerformCancellable(ActivateCollider, DeactivateCollider);
    }

    public void SwitchTo3D(Cancellable cancellable)
    {
        cancellable.PerformCancellable(DeactivateCollider, ActivateCollider);
    }
}
