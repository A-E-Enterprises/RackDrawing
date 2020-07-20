using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DrawingControl
{
	public static class Utils
	{
		//=============================================================================
		public static string GetPropertyName<TProperty>(Expression<Func<TProperty>> projection)
		{
			var memberExpression = (MemberExpression)projection.Body;
			return memberExpression.Member.Name;
		}

		//=============================================================================
		public static bool ConvertToDouble(string str, out double result)
		{
			result = 0.0;
			if (string.IsNullOrEmpty(str))
				return false;

			string strDouble = str.Replace(",", ".");
			try
			{
				result = Convert.ToDouble(strDouble, CultureInfo.InvariantCulture);
				return true;
			}
			catch { }

			return false;
		}

		//=============================================================================
		public static double _sDefPrec = 0.000001;
		public static bool FNE(double a, double b)
		{
			return !FEQ(a, b);
		}
		public static bool FEQ(double a, double b)
		{
			return (a - _sDefPrec <= b) && (b <= a + _sDefPrec);
		}
		public static bool FGT(double a, double b)
		{
			return a - _sDefPrec > b;
		}
		public static bool FGE(double a, double b)
		{
			return a >= b - _sDefPrec;
		}
		public static bool FLT(double a, double b)
		{
			return !FGE(a, b);
		}
		public static bool FLE(double a, double b)
		{
			return !FGT(a, b);
		}

		//=============================================================================
		public static Point GetWholePoint(Point pnt)
		{
			Point result = pnt;
			result.X = Convert.ToInt32(Math.Truncate(result.X));
			result.Y = Convert.ToInt32(Math.Truncate(result.Y));

			return result;
		}

		//=============================================================================
		public static int GetWholeNumber(double rNumber)
		{
			return Convert.ToInt32(Math.Truncate(rNumber));
		}

		//=============================================================================
		public static Point CheckBorders(Point pnt, double min_x, double min_y, double max_x, double max_y, double margin_X, double margin_Y)
		{
			Point result = pnt;
			if (Utils.FLT(result.X, min_x + margin_X))
				result.X = min_x + margin_X;
			if (Utils.FLT(result.Y, min_y + margin_Y))
				result.Y = min_y + margin_Y;
			if (Utils.FGT(result.X, max_x - margin_X))
				result.X = max_x - margin_X;
			if (Utils.FGT(result.Y, max_y - margin_Y))
				result.Y = max_y - margin_Y;

			return result;
		}

		//=============================================================================
		public static int CheckWholeNumber(double rNumber, double minValue, double maxValue)
		{
			double rResult = rNumber;

			if (Utils.FLT(rResult, minValue))
				rResult = minValue;
			if (!double.IsInfinity(maxValue) && Utils.FGT(rResult, maxValue))
				rResult = maxValue;

			return GetWholeNumber(rResult);
		}
		public static UInt32 Check_UInt32_Number(UInt32 number, int minValue, int maxValue)
		{
			UInt32 result = number;

			if (minValue >= 0 && result < minValue)
				result = (UInt32)minValue;
			if (maxValue >= 0 && result > maxValue)
				result = (UInt32)maxValue;

			return result;
		}

		//=============================================================================
		public static int GetWholeNumberByStep(double rNumber, double step)
		{
			double rResult = rNumber;

			// 
			rResult = rResult / step;
			rResult = Utils.GetWholeNumber(rResult);
			rResult = rResult * step;

			return Utils.GetWholeNumber(rResult);
		}
		public static UInt32 Get_UInt32_NumberByStep(UInt32 number, UInt32 step)
		{
			try
			{
				double rResult = number;

				// 
				rResult = rResult / step;
				rResult = Utils.GetWholeNumber(rResult);
				rResult = rResult * step;

				return Convert.ToUInt32(rResult);
			}
			catch { }

			return 0;
		}

		//=============================================================================
		/// <summary>
		/// Use IClonable.Clone() instead DeepClone.
		/// DeepClone should be used only for seralize\deserialize drawing to the file.
		/// It has very low performance for using in runtime operations.
		/// </summary>
		public static T DeepClone<T>(T obj)
		{
			using (var ms = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(ms, obj);
				ms.Position = 0;

				return (T)formatter.Deserialize(ms);
			}
		}

		//=============================================================================
		/// <summary>
		/// Finds a parent of a given item on the visual tree.
		/// </summary>
		/// <typeparam name="T">The type of the queried item.</typeparam>
		/// <param name="child">A direct or indirect child of the
		/// queried item.</param>
		/// <returns>The first parent item that matches the submitted
		/// type parameter. If not matching item can be found, a null
		/// reference is being returned.</returns>
		public static T TryFindParent<T>(this DependencyObject child)
			where T : DependencyObject
		{
			//get parent item
			DependencyObject parentObject = GetParentObject(child);

			//we've reached the end of the tree
			if (parentObject == null) return null;

			//check if the parent matches the type we're looking for
			T parent = parentObject as T;
			if (parent != null)
			{
				return parent;
			}
			else
			{
				//use recursion to proceed with next level
				return TryFindParent<T>(parentObject);
			}
		}

		//=============================================================================
		/// <summary>
		/// This method is an alternative to WPF's
		/// <see cref="VisualTreeHelper.GetParent"/> method, which also
		/// supports content elements. Keep in mind that for content element,
		/// this method falls back to the logical tree of the element!
		/// </summary>
		/// <param name="child">The item to be processed.</param>
		/// <returns>The submitted item's parent, if available. Otherwise
		/// null.</returns>
		public static DependencyObject GetParentObject(this DependencyObject child)
		{
			if (child == null) return null;

			//handle content elements separately
			ContentElement contentElement = child as ContentElement;
			if (contentElement != null)
			{
				DependencyObject parent = ContentOperations.GetParent(contentElement);
				if (parent != null) return parent;

				FrameworkContentElement fce = contentElement as FrameworkContentElement;
				return fce != null ? fce.Parent : null;
			}

			//also try searching for parent in framework elements (such as DockPanel, etc)
			FrameworkElement frameworkElement = child as FrameworkElement;
			if (frameworkElement != null)
			{
				DependencyObject parent = frameworkElement.Parent;
				if (parent != null) return parent;
			}

			//if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
			return VisualTreeHelper.GetParent(child);
		}

		//=============================================================================
		/// <summary>
		/// AngleBetween - the angle between 2 vectors
		/// </summary>
		/// <returns>
		/// Returns the the angle in degrees between vector1 and vector2
		/// </returns>
		/// <param name="vector1"> The first Vector </param>
		/// <param name="vector2"> The second Vector </param>
		public static double AngleBetween(Vector vector1, Vector vector2)
		{
			double sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
			double cos = vector1.X * vector2.X + vector1.Y * vector2.Y;

			return Math.Atan2(sin, cos) * (180 / Math.PI);
		}

		//=============================================================================
		public static double ConvertToRadians(double angle)
		{
			return (Math.PI / 180) * angle;
		}

		//=============================================================================
		public static double CalcScale(double imageAvailableWidth, double imageAvailableHeight, double drawingWidth, double drawingHeight)
		{
			double scaleX = imageAvailableWidth / drawingWidth;
			double scaleY = imageAvailableHeight / drawingHeight;

			if (scaleX < scaleY)
				return scaleX;

			return scaleY;
		}

		//=============================================================================
		// Returns unique index for pallet configuration in palletConfigColl.
		public static int GetUniquePalletConfigurationIndex(ObservableCollection<PalletConfiguration> palletConfigColl)
		{
			if (palletConfigColl == null)
				return -1;

			if (palletConfigColl.Count == 0)
				return 1;

			for(int index=1; index < 500; ++index)
			{
				PalletConfiguration palletConfig = palletConfigColl.FirstOrDefault(c => c != null && c.UniqueIndex == index);
				if (palletConfig == null)
					return index;
			}

			return 1;
		}

		//=============================================================================
		public enum eAdjustedSide
		{
			eNotAdjusted = 0,
			eLeft,
			eTop,
			eRight,
			eBot
		};
		/// <summary>
		/// Returns side at geometry1 at which it is adjusted to geometry2.
		/// </summary>
		public static eAdjustedSide GetAdjustedSide(BaseRectangleGeometry geometry1, BaseRectangleGeometry geometry2)
		{
			if (geometry1 == null || geometry2 == null)
				return eAdjustedSide.eNotAdjusted;

			//
			Point topLeftPnt_01 = geometry1.TopLeft_GlobalPoint;
			Point botRightPnt_01 = geometry1.BottomRight_GlobalPoint;
			//
			Point topLeftPnt_02 = geometry2.TopLeft_GlobalPoint;
			Point botRightPnt_02 = geometry2.BottomRight_GlobalPoint;

			// check left
			if (Utils.FEQ(topLeftPnt_01.X, botRightPnt_02.X)
				&& ((Utils.FGE(botRightPnt_02.Y, topLeftPnt_01.Y) && Utils.FLE(botRightPnt_02.Y, botRightPnt_01.Y))
					|| (Utils.FGE(topLeftPnt_02.Y, topLeftPnt_01.Y) && Utils.FLE(topLeftPnt_02.Y, botRightPnt_01.Y))
					|| (Utils.FGE(topLeftPnt_01.Y, topLeftPnt_02.Y) && Utils.FLE(topLeftPnt_01.Y, botRightPnt_02.Y))
					|| (Utils.FGE(botRightPnt_01.Y, topLeftPnt_02.Y) && Utils.FLE(botRightPnt_01.Y, botRightPnt_02.Y))))
				return eAdjustedSide.eLeft;
			// check right
			else if (Utils.FEQ(botRightPnt_01.X, topLeftPnt_02.X)
				&& ((Utils.FGE(botRightPnt_02.Y, topLeftPnt_01.Y) && Utils.FLE(botRightPnt_02.Y, botRightPnt_01.Y))
					|| (Utils.FGE(topLeftPnt_02.Y, topLeftPnt_01.Y) && Utils.FLE(topLeftPnt_02.Y, botRightPnt_01.Y))
					|| (Utils.FGE(topLeftPnt_01.Y, topLeftPnt_02.Y) && Utils.FLE(topLeftPnt_01.Y, botRightPnt_02.Y))
					|| (Utils.FGE(botRightPnt_01.Y, topLeftPnt_02.Y) && Utils.FLE(botRightPnt_01.Y, botRightPnt_02.Y))))
				return eAdjustedSide.eRight;
			// check top
			else if (Utils.FEQ(topLeftPnt_01.Y, botRightPnt_02.Y)
				&& ((Utils.FGE(topLeftPnt_02.X, topLeftPnt_01.X) && Utils.FLE(topLeftPnt_02.X, botRightPnt_01.X))
					|| (Utils.FGE(botRightPnt_02.X, topLeftPnt_01.X) && Utils.FLE(botRightPnt_02.X, botRightPnt_01.X))
					|| (Utils.FGE(topLeftPnt_01.X, topLeftPnt_02.X) && Utils.FLE(topLeftPnt_01.X, botRightPnt_02.X))
					|| (Utils.FGE(botRightPnt_01.X, topLeftPnt_02.X) && Utils.FLE(botRightPnt_01.X, botRightPnt_02.X))))
				return eAdjustedSide.eTop;
			// check bot
			else if (Utils.FEQ(botRightPnt_01.Y, topLeftPnt_02.Y)
				&& ((Utils.FGE(topLeftPnt_02.X, topLeftPnt_01.X) && Utils.FLE(topLeftPnt_02.X, botRightPnt_01.X))
					|| (Utils.FGE(botRightPnt_02.X, topLeftPnt_01.X) && Utils.FLE(botRightPnt_02.X, botRightPnt_01.X))
					|| (Utils.FGE(topLeftPnt_01.X, topLeftPnt_02.X) && Utils.FLE(topLeftPnt_01.X, botRightPnt_02.X))
					|| (Utils.FGE(botRightPnt_01.X, topLeftPnt_02.X) && Utils.FLE(botRightPnt_01.X, botRightPnt_02.X))))
				return eAdjustedSide.eTop;

			return eAdjustedSide.eNotAdjusted;
		}
	}
}
