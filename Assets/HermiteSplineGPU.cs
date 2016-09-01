﻿using UnityEngine;
using System.Collections;
using System.Linq;

public class HermiteSplineGPU : MonoBehaviour
{
    public Renderer Target;

    public AnimationCurve LargeCurve;
    public AnimationCurve SmallCurve;

    void Start()
    {
        var propertyBlock = new MaterialPropertyBlock();

        SetupShaderAnimationCurve("_LargeCurve", propertyBlock, LargeCurve);
        SetupShaderAnimationCurve("_SmallCurve", propertyBlock, SmallCurve);

		Target.SetPropertyBlock(propertyBlock);
    }

    private void SetupShaderAnimationCurve(string curveName, MaterialPropertyBlock propertyBlock, AnimationCurve curve)
    {
        var keys = curve.keys;
        var points = keys.Select(_ => _.value).ToArray();
        var tangents = keys.Select(_ => new Vector4(_.inTangent, _.outTangent)).ToArray();
		var knotVector = keys.Select(_ => _.time).ToArray();

		propertyBlock.SetFloatArray(curveName + "Points", points);
		propertyBlock.SetVectorArray(curveName + "Tangents", tangents);
		propertyBlock.SetFloatArray(curveName + "KnotVector", knotVector);
        propertyBlock.SetFloat(curveName + "PointCount", points.Length);
    }
}
