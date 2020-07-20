using AppInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DrawingControl
{
	/// <summary>
	/// DrawingDocument can have only one WarehouseSheet.
	/// WarehouseSheet can contain only special geometry rectangles, which are bound to
	/// other sheets from the document and display bound sheet graphics.
	/// </summary>
	[Serializable]
	public class WarehouseSheet : DrawingSheet, ISerializable
	{
		public WarehouseSheet(DrawingDocument doc)
			: base(doc)
		{
			m_Length = 100000;
			m_Width = 70000;
			Name = "Warehouse";

			_InitRoofList();
		}
		public WarehouseSheet(WarehouseSheet whSheet)
			: base(whSheet)
		{
			if(whSheet != null)
			{
				// Clone roofs list
				if(whSheet.RoofsList != null)
				{
					foreach(Roof roof in whSheet.RoofsList)
					{
						if (roof == null)
							continue;

						Roof roofClone = roof.Clone() as Roof;
						if (roofClone == null)
							continue;

						this.RoofsList.Add(roofClone);
					}
				}
			}
		}

		#region Properties

		//=============================================================================
		// List with available roofs.
		private List<Roof> m_RoofsList = new List<Roof>();
		public List<Roof> RoofsList { get { return m_RoofsList; } }
		//=============================================================================
		// Selected roof from RoofsList. It is applied only to WarehouseSheet. DrawingSheet cant have a roof.
		public Roof SelectedRoof { get { return m_RoofsList.FirstOrDefault(r => r != null && r.IsSelected); } }

		#endregion

		#region Functions

		//=============================================================================
		public void CreateSheetGeometry(DrawingSheet sheet)
		{
			Cancel();

			if (DrawingDocument._sDrawing == null)
				return;

			SheetGeometry sg = new SheetGeometry(this, sheet);
			_AfterCreateNewGeom(sg);
		}

		//=============================================================================
		public override IClonable Clone()
		{
			return new WarehouseSheet(this);
		}

		#endregion

		#region Protected functions

		//=============================================================================
		// Initialize roofs list.
		private void _InitRoofList()
		{
			if (m_RoofsList == null)
				m_RoofsList = new List<Roof>();
			m_RoofsList.Clear();

			FlatRoof flatRoof = new FlatRoof();
			flatRoof.IsSelected = true;
			m_RoofsList.Add(flatRoof);
			m_RoofsList.Add(new GableRoof());
			m_RoofsList.Add(new ShedRoof());
		}

		#endregion

		#region Serialization

		//=============================================================================
		// 1.0 
		// 1.1 Add roofs list.
		protected static string _sWarehouseSheet_strMajor = "WarehouseSheet_MAJOR";
		protected static int _sWarehouseSheet_MAJOR = 1;
		protected static string _sWarehouseSheet_strMinor = "WarehouseSheet_MINOR";
		protected static int _sWarehouseSheet_MINOR = 1;
		//=============================================================================
		public WarehouseSheet(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			//
			int iMajor = (int)info.GetValue(_sWarehouseSheet_strMajor, typeof(int));
			int iMinor = (int)info.GetValue(_sWarehouseSheet_strMinor, typeof(int));
			if (iMajor > _sWarehouseSheet_MAJOR)
				++DrawingDocument._sNewVersion_StreamRead;
			else if (iMajor == _sWarehouseSheet_MAJOR && iMinor > _sWarehouseSheet_MINOR)
				++DrawingDocument._sNewVersion_StreamRead;

			if (iMajor <= _sWarehouseSheet_MAJOR)
			{
				try
				{
					// 1.1
					//
					if (iMajor >= 1 && iMinor >= 1)
					{
						m_RoofsList = (List<Roof>)info.GetValue("RoofsList", typeof(List<Roof>));
						if (m_RoofsList == null || m_RoofsList.Count == 0)
							_InitRoofList();
					}
					else
						_InitRoofList();
				}
				catch
				{
					++DrawingDocument._sStreamReadException;
				}
			}
		}
		//=============================================================================
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			//
			info.AddValue(_sWarehouseSheet_strMajor, _sWarehouseSheet_MAJOR);
			info.AddValue(_sWarehouseSheet_strMinor, _sWarehouseSheet_MINOR);

			// 1.1
			info.AddValue("RoofsList", m_RoofsList);
		}
		//=============================================================================
		public override void OnDeserialization(object sender)
		{
			base.OnDeserialization(sender);
		}

		#endregion
	}
}
