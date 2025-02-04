﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Spotlight : MonoBehaviour
{
    private GameObject planeChild;
    private bool generated;
    private Plane plane;


    public enum ShadowApplicatorType { Hazard, Physics2D };

    public ShadowApplicatorType ApplicatorType;

    public Material ShadowMaterial;
    public Material IndicatorMaterial;

    void Start()
    {
        planeChild = new GameObject(this.gameObject.name + " plane collider");
        planeChild.transform.parent = transform;
        generated = false;
        plane = new Plane(MainObjectContainer.Instance.ShadowPlane.transform.up, MainObjectContainer.Instance.ShadowPlane.transform.position);
        if (!ShadowMaterial)
            CreateDefaultShadowMaterial();
        if (!IndicatorMaterial)
            CreateDefaultIndicatorMaterial();
    }

    private void CreateDefaultShadowMaterial()
    {
        ShadowMaterial = new Material(Shader.Find("Standard"))
        {
            color = gameObject.GetComponent<Light>().color
        };
    }

    private void CreateDefaultIndicatorMaterial()
    {
        IndicatorMaterial = new Material(Shader.Find("Unlit/Stripes"))
        {
            color = gameObject.GetComponent<Light>().color
        };
    }

    private void CreateCollider()
    {
        ShadowPolygonHelper.MakeSpotlightCollider(this.gameObject.GetComponent<Light>(), plane, planeChild);
        Rigidbody2D rb2d = planeChild.AddComponent<Rigidbody2D>();
        rb2d.isKinematic = true;
        SpotlightCollider colliderScript = planeChild.AddComponent<SpotlightCollider>();
        colliderScript.applicatorType = this.ApplicatorType;
        colliderScript.ShadowMaterial = this.ShadowMaterial;
        colliderScript.IndicatorMaterial = this.IndicatorMaterial;
        planeChild.tag = "Physics2D";
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

#if UNITY_EDITOR

[CustomEditor(typeof(Spotlight))]
public class SpotlightEditor : Editor
{
    override public void OnInspectorGUI()
    {
        var spotlight = target as Spotlight;

        spotlight.ApplicatorType = (Spotlight.ShadowApplicatorType) EditorGUILayout.EnumPopup("Spotlight Type", spotlight.ApplicatorType);

        using (EditorGUILayout.FadeGroupScope group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(spotlight.ApplicatorType != Spotlight.ShadowApplicatorType.Physics2D)))
        {
            if (group.visible == false)
            {
                EditorGUI.indentLevel++;
                bool allowSceneObjects = !EditorUtility.IsPersistent(target);
                GUIContent shadow = new GUIContent("2D Shadow Material", "The material for the movable, 2D object generated by the physics spotlight to use. May be left blank to use default.");
                spotlight.ShadowMaterial = (Material) EditorGUILayout.ObjectField(shadow, spotlight.ShadowMaterial, typeof(Material), allowSceneObjects);
                GUIContent indicator = new GUIContent("Indicator Material", "The material for the indicator of the old shadow position left behind by the physics spotlight. May be left blank to use default.");
                spotlight.IndicatorMaterial = (Material)EditorGUILayout.ObjectField(indicator, spotlight.IndicatorMaterial, typeof(Material), allowSceneObjects);
                EditorGUI.indentLevel--;
            }
        }
    }
}

#endif
