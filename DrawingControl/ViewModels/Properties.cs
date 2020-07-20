using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrawingControl
{
	/// <summary>
	/// Base view model for geometry property.
	/// 
	/// </summary>
	public abstract class Property_ViewModel : BaseViewModel
	{
		public Property_ViewModel(BaseRectangleGeometry owner, string sysName, string strGroup)
		{
			m_owner = owner;
			m_strSystemName = sysName;

			//
			IsNumeric = false;

			//
			m_strGroup = strGroup;
		}

		public Property_ViewModel(BaseRectangleGeometry owner, string sysName, string localName, string strGroup)
			: this(owner, sysName, strGroup)
		{
			m_strLocalName = localName;
		}

		public Property_ViewModel(BaseRectangleGeometry owner, string sysName, string localName, bool isReadOnly, string strGroup)
			: this(owner, sysName, localName, strGroup)
		{
			m_IsReadOnly = isReadOnly;
		}

		//=============================================================================
		protected BaseRectangleGeometry m_owner = null;

		//=============================================================================
		private string m_strSystemName = string.Empty;
		public string SystemName
		{
			get { return m_strSystemName; }
		}

		//=============================================================================
		private string m_strGroup = string.Empty;
		public string Group { get { return m_strGroup; } }

		//=============================================================================
		public bool IsNumeric { get; set; }

		//=============================================================================
		protected string m_strLocalName = string.Empty;
		public virtual string Name
		{
			get
			{
				if (!string.IsNullOrEmpty(m_strLocalName))
					return m_strLocalName;

				return SystemName;
			}
		}

		//=============================================================================
		public virtual object Value { get; set; }

		//=============================================================================
		public virtual List<string> StandardValues
		{
			get { return null; }
		}

		//=============================================================================
		protected bool m_IsReadOnly = false;
		public virtual bool IsReadOnly
		{
			get { return m_IsReadOnly; }
			set
			{
				if (m_IsReadOnly != value)
				{
					m_IsReadOnly = value;
					NotifyPropertyChanged(() => IsReadOnly);
				}
			}
		}

		//=============================================================================
		public void Update_Value()
		{
			NotifyPropertyChanged(() => Value);
		}
	}

	public class GeometryProperty : Property_ViewModel
	{
		public GeometryProperty(BaseRectangleGeometry owner, string strPropSystemName, bool bIsNumeric, string strGroup)
			: base(owner, strPropSystemName, strGroup)
		{
			IsNumeric = bIsNumeric;
		}

		public GeometryProperty(BaseRectangleGeometry owner, string strPropSystemName, string strPropLocName, bool bIsNumeric, string strGroup)
			: base(owner, strPropSystemName, strPropLocName, strGroup)
		{
			IsNumeric = bIsNumeric;
		}

		public GeometryProperty(BaseRectangleGeometry owner, string strPropSystemName, string strPropLocName, bool isReadOnly, bool bIsNumeric, string strGroup)
			: base(owner, strPropSystemName, strPropLocName, isReadOnly, strGroup)
		{
			IsNumeric = bIsNumeric;
		}

		//=============================================================================
		public override object Value
		{
			get
			{
				if (m_owner != null)
					return m_owner.GetPropertyValue(SystemName);

				return null;
			}
			set
			{
				bool bRes = false;
				string strError = string.Empty;
				if (m_owner != null)
					bRes = m_owner.SetPropertyValue(SystemName, value, true, true, true, out strError);

				NotifyPropertyChanged(() => Value);

				// Sheet.OnPropertyChanged() was called inside SetPropertyValue()
				//
				//// notify Sheet for update all geometry
				//if (m_owner != null && m_owner.Sheet != null)
				//	m_owner.Sheet.OnPropertyChanged(m_owner, SystemName, bRes, strError);
			}
		}
	}

	public class GeometryType_Property : Property_ViewModel
	{
		public GeometryType_Property(BaseRectangleGeometry owner)
			: base(owner, "GeometryType", "Geometry Type", "Geometry")
		{
			m_IsReadOnly = true;

			//
			if (owner != null)
			{
				try
				{
					string strType = owner.GetType().ToString();

					// regex and cut only filename
					string strPattern = @"[^\.]+$";
					Regex regex = new Regex(strPattern);
					Match match = regex.Match(strType);

					m_strLocalName = match.Value;
				}
				catch { }
			}
		}

		//=============================================================================
		public override object Value
		{
			get
			{
				if (m_owner != null)
					return m_owner.Text;

				return string.Empty;
			}
			set
			{
				NotifyPropertyChanged(() => Value);
			}
		}
	}
}
