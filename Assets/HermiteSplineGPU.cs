using UnityEngine;
using System;
using System.Collections;
using System.Linq;

[Serializable]
public class NormalizedAnimationCurve
{
	[SerializeField]
	private AnimationCurve _curve;

	[SerializeField]
	private float _amplitude;

	[SerializeField]
	private float _duration;

	public Keyframe[] Keys { get { return _curve.keys; }}

	public float Evaluate(float t)
	{
		return _curve.Evaluate(t / _duration) * _amplitude;
	}

	public float GetAmplitude()
	{
		return _amplitude;
	}

	public float GetDuration()
	{
		return _duration;
	}
}

public class HermiteSplineGPU : MonoBehaviour
{
    public Renderer Target;

    public NormalizedAnimationCurve LargeCurve;
    public NormalizedAnimationCurve SmallCurve;

	void Start()
    {
        var propertyBlock = new MaterialPropertyBlock();

        SetupShaderAnimationCurve("_LargeCurve", propertyBlock, LargeCurve);
        SetupShaderAnimationCurve("_SmallCurve", propertyBlock, SmallCurve);

		Target.SetPropertyBlock(propertyBlock);
    }

    private void SetupShaderAnimationCurve(string curveName, MaterialPropertyBlock propertyBlock, NormalizedAnimationCurve curve)
    {
        var keys = curve.Keys;
        var points = keys.Select(_ => _.value).ToArray();
        var tangents = keys.Select(_ => new Vector4(_.inTangent, _.outTangent)).ToArray();
		var knotVector = keys.Select(_ => _.time).ToArray();

		propertyBlock.SetFloatArray(curveName + "Points", points);
		propertyBlock.SetVectorArray(curveName + "Tangents", tangents);
		propertyBlock.SetFloatArray(curveName + "KnotVector", knotVector);
        propertyBlock.SetFloat(curveName + "PointCount", points.Length);
	    propertyBlock.SetFloat(curveName + "Amplitude", curve.GetAmplitude());
	    propertyBlock.SetFloat(curveName + "Duration", curve.GetDuration());
    }
}
