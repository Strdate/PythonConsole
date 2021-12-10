using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SkylinesPythonShared.API
{
	[Serializable]
	[Doc("Abstract bezier structure")]
	public class Bezier
    {
		[Doc("Creates new bezier from 4 control points")]
		public Bezier(Vector _a, Vector _b, Vector _c, Vector _d)
		{
			this.a = _a;
			this.b = _b;
			this.c = _c;
			this.d = _d;
		}

		private Bezier() { }

		[Doc("Returns point on bezier (t is number from 0 to 1)")]
		public Vector position(float t)
		{
			float num = 1f - t;
			float num2 = t * t * t;
			float num3 = 3f * t * t * num;
			float num4 = 3f * t * num * num;
			float num5 = num * num * num;
			return new Vector(this.a.x * num5 + this.b.x * num4 + this.c.x * num3 + this.d.x * num2, this.a.y * num5 + this.b.y * num4 + this.c.y * num3 + this.d.y * num2, this.a.z * num5 + this.b.z * num4 + this.c.z * num3 + this.d.z * num2);
		}

		[Doc("Returns tangent vector to the bezier (t is number from 0 to 1)")]
		public Vector tangent(float t)
		{
			float num = t * t;
			float num2 = 3f * num;
			float num3 = 6f * t - 9f * num;
			float num4 = 3f - 12f * t + 9f * num;
			float num5 = 6f * t - 3f - 3f * num;
			return new Vector(this.a.x * num5 + this.b.x * num4 + this.c.x * num3 + this.d.x * num2, this.a.y * num5 + this.b.y * num4 + this.c.y * num3 + this.d.y * num2, this.a.z * num5 + this.b.z * num4 + this.c.z * num3 + this.d.z * num2);
		}

		[Doc("Returns normal vector to the bezier. Y coord is undefined")]
		public Vector flat_normal(float t)
        {
			return tangent(t).flat_rotate(Vector.pi / 2);
        }

		[XmlIgnore]
		[ToStringIgnore]
		[Doc("Returns inverted bezier")]
		public Bezier inverted => new Bezier(this.d, this.c, this.b, this.a);

		[Doc("Point A (start)")]
		public Vector a { get; private set; }

		[Doc("Point B")]
		public Vector b { get; private set; }

		[Doc("Point C")]
		public Vector c { get; private set; }

		[Doc("Point D (end)")]
		public Vector d { get; private set; }

		public override string ToString()
		{
			return Util.RuntimeToString(this);
		}
	}
}
