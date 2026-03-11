using ACadSharp.Entities;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Extensions
{
	public static class PolylineExtensions
	{
		/// <summary>
		/// Explodes the polyline into a collection of entities formed by <see cref="Line"/> and <see cref="Arc"/>.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Entity> Explode(this IPolyline polyline)
		{
			//Generic explode method for Polyline2D and LwPolyline
			List<Entity> entities = new List<Entity>();

			for (int i = 0; i < polyline.Vertices.Count(); i++)
			{
				IVertex curr = polyline.Vertices.ElementAt(i);
				IVertex next = polyline.Vertices.ElementAtOrDefault(i + 1);

				if (next == null && polyline.IsClosed)
				{
					next = polyline.Vertices.First();
				}
				else if (next == null)
				{
					break;
				}

				Entity e = null;
				if (isStraightSegment(curr, next))
				{
					//Is a line
					e = new Line
					{
						StartPoint = curr.Location.Convert<XYZ>(),
						EndPoint = next.Location.Convert<XYZ>(),
						Normal = polyline.Normal,
						Thickness = polyline.Thickness,
					};
				}
				else
				{
					XY p1 = curr.Location.Convert<XY>();
					XY p2 = next.Location.Convert<XY>();

					if (tryCreateArcSegment(polyline, p1, p2, curr.Bulge, out Arc arc))
					{
						e = arc;
					}
					else
					{
						e = new Line
						{
							StartPoint = curr.Location.Convert<XYZ>(),
							EndPoint = next.Location.Convert<XYZ>(),
							Normal = polyline.Normal,
							Thickness = polyline.Thickness,
						};
					}
				}

				e.MatchProperties(polyline);

				entities.Add(e);
			}

			return entities;
		}

		/// <summary>
		/// Retrieves the points of the specified polyline as a sequence of the specified vector type.
		/// </summary>
		/// <typeparam name="T">The type of vector to return for each point. Must implement <see cref="IVector"/> and have a parameterless
		/// constructor.</typeparam>
		/// <param name="polyline">The polyline from which to retrieve the points. Cannot be <see langword="null"/>.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> containing the points of the polyline, converted to the specified vector type.</returns>
		public static IEnumerable<T> GetPoints<T>(this IPolyline polyline)
			where T : IVector, new()
		{
			return polyline.Vertices.Select(v => v.Location.Convert<T>());
		}

		/// <summary>
		/// Generates a collection of points representing the vertices of the specified polyline,  including interpolated
		/// points for arcs based on the given precision.
		/// </summary>
		/// <remarks>This method processes the vertices of the polyline and generates a sequence of points.  For
		/// straight segments, the start and end points are included. For arc segments, additional  points are interpolated
		/// based on the specified <paramref name="precision"/>. If the polyline  is closed, the method ensures continuity by
		/// connecting the last vertex to the first.</remarks>
		/// <typeparam name="T">The type of the points to return. Must implement <see cref="IVector"/> and have a parameterless constructor.</typeparam>
		/// <param name="polyline">The polyline from which to extract points. The polyline may contain straight segments and arcs.</param>
		/// <param name="precision">The number of points to generate for each arc segment. Must be equal to or greater than 2.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> containing the points of the polyline, including interpolated points for arcs.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="precision"/> is less than 2.</exception>
		public static IEnumerable<T> GetPoints<T>(this IPolyline polyline, int precision)
			where T : IVector, new()
		{
			if (precision < 2)
			{
				throw new ArgumentOutOfRangeException(nameof(precision), precision, "The arc precision must be equal or greater than two.");
			}

			var points = new List<T>();
			for (int i = 0; i < polyline.Vertices.Count(); i++)
			{
				IVertex curr = polyline.Vertices.ElementAt(i);
				IVertex next = polyline.Vertices.ElementAtOrDefault(i + 1);

				if (next == null && polyline.IsClosed)
				{
					next = polyline.Vertices.First();
				}
				else if (next == null)
				{
					break;
				}

				if (isStraightSegment(curr, next))
				{
					if (i == 0)
					{
						points.Add(curr.Location.Convert<T>());
					}

					points.Add(next.Location.Convert<T>());
				}
				else
				{
					XY p1 = curr.Location.Convert<XY>();
					XY p2 = next.Location.Convert<XY>();

					if (!tryCreateArcSegment(polyline, p1, p2, curr.Bulge, out Arc arc))
					{
						if (i == 0)
						{
							points.Add(curr.Location.Convert<T>());
						}

						points.Add(next.Location.Convert<T>());
						continue;
					}

					IEnumerable<T> lst = arc.PolygonalVertexes(precision).Select(p => p.Convert<T>());

					var f = lst.First().Round(8);
					var l = lst.Last().Round(8);

					if (f.Equals(curr.Location.Convert<T>().Round(8)))
					{
						points.AddRange(lst.Skip(1));
					}
					else if (l.Equals(curr.Location.Convert<T>().Round(8)))
					{
						lst = lst.Reverse();
						points.AddRange(lst.Skip(1));
					}
				}
			}

			return points;
		}

		private static bool isStraightSegment(IVertex curr, IVertex next)
		{
			if (curr == null || next == null)
			{
				return true;
			}

			if (Math.Abs(curr.Bulge) <= 1e-12)
			{
				return true;
			}

			XY p1 = curr.Location.Convert<XY>();
			XY p2 = next.Location.Convert<XY>();
			return !isFinite(curr.Bulge) || p1.DistanceFrom(p2) <= 1e-9;
		}

		private static bool tryCreateArcSegment(IPolyline polyline, XY p1, XY p2, double bulge, out Arc arc)
		{
			arc = null;
			try
			{
				if (!isFinite(bulge) || p1.DistanceFrom(p2) <= 1e-9 || Math.Abs(bulge) <= 1e-12)
				{
					return false;
				}

				arc = Arc.CreateFromBulge(p1, p2, bulge);
				if (arc == null || !isFinite(arc.Radius) || arc.Radius <= 1e-12)
				{
					arc = null;
					return false;
				}

				arc.Center = new XYZ(arc.Center.X, arc.Center.Y, polyline.Elevation);
				arc.Normal = polyline.Normal;
				arc.Thickness = polyline.Thickness;
				return true;
			}
			catch
			{
				arc = null;
				return false;
			}
		}

		private static bool isFinite(double value)
		{
			return !double.IsNaN(value) && !double.IsInfinity(value);
		}
	}
}
