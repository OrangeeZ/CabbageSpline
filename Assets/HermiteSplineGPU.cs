using UnityEngine;
using System.Collections;
using System.Linq;

public class HermiteSplineGPU : MonoBehaviour
{
    public Renderer Target;

    public AnimationCurve AnimationCurve;

    void Start()
    {
        var propertyBlock = new MaterialPropertyBlock();

        var points = AnimationCurve.keys.Select(_ => _.value).ToArray();
        var tangents = AnimationCurve.keys.Select(_ => new Vector4(_.inTangent, _.outTangent)).ToArray();
		var knotVector = AnimationCurve.keys.Select(_ => _.time).ToArray();

		propertyBlock.SetFloatArray("points", points);
		propertyBlock.SetVectorArray("tangents", tangents);
		propertyBlock.SetFloatArray("knotVector", knotVector);

		Target.SetPropertyBlock(propertyBlock);
    }
}
