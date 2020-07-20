using DrawingControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RackDrawingApp
{
	public class RackAccessories_ViewModel : BaseViewModel
	{
		public RackAccessories_ViewModel(RackAccessories accessories)
		{
			m_Accessories = accessories;
		}

		//=============================================================================
		private RackAccessories m_Accessories = null;
		public RackAccessories Accessories { get { return m_Accessories; } }

		//=============================================================================
		public bool UprightGuard
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.UprightGuard;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.UprightGuard = value;

				NotifyPropertyChanged(() => UprightGuard);
			}
		}
		//=============================================================================
		public bool RowGuard
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.RowGuard;

				return false;
			}
			set
			{
				if (m_Accessories != null)
				{
					m_Accessories.RowGuard = value;

					// disable IsHeavyDutyEnabled, if RowGuard is disabled
					if (!m_Accessories.RowGuard)
						m_Accessories.IsHeavyDutyEnabled = false;
				}

				NotifyPropertyChanged(() => IsHeavyDutyEnabled);
				NotifyPropertyChanged(() => RowGuard);
			}
		}
		//=============================================================================
		public bool IsHeavyDutyEnabled
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.IsHeavyDutyEnabled;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.IsHeavyDutyEnabled = value;

				NotifyPropertyChanged(() => IsHeavyDutyEnabled);
			}
		}
		//=============================================================================
		public bool Signages
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.Signages;

				return false;
			}
			set
			{
				if (m_Accessories != null)
				{
					m_Accessories.Signages = value;

					if(m_Accessories.Signages)
					{
						m_Accessories.IsSafetyPrecautionsEnabled = true;
						m_Accessories.IsSafeWorkingLoadsEnabled = true;
					}
					else
					{
						m_Accessories.IsSafetyPrecautionsEnabled = false;
						m_Accessories.IsSafeWorkingLoadsEnabled = false;
						m_Accessories.IsMenaEnabled = false;
					}
				}

				NotifyPropertyChanged(() => Signages);
				NotifyPropertyChanged(() => IsMenaEnabled);
				NotifyPropertyChanged(() => IsSafetyPrecautionsEnabled);
				NotifyPropertyChanged(() => IsSafeWorkingLoadsEnabled);
			}
		}

		//=============================================================================
		public bool IsMeshCladdingEnabled
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.IsMeshCladdingEnabled;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.IsMeshCladdingEnabled = value;

				NotifyPropertyChanged(() => IsMeshCladdingEnabled);
			}
		}

		//=============================================================================
		public bool IsMenaEnabled
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.IsMenaEnabled;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.IsMenaEnabled = value;
				_UpdateSignanes();

				NotifyPropertyChanged(() => Signages);
				NotifyPropertyChanged(() => IsMenaEnabled);
				NotifyPropertyChanged(() => IsSafetyPrecautionsEnabled);
				NotifyPropertyChanged(() => IsSafeWorkingLoadsEnabled);
			}
		}

		//=============================================================================
		public bool IsSafetyPrecautionsEnabled
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.IsSafetyPrecautionsEnabled;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.IsSafetyPrecautionsEnabled = value;
				_UpdateSignanes();

				NotifyPropertyChanged(() => Signages);
				NotifyPropertyChanged(() => IsMenaEnabled);
				NotifyPropertyChanged(() => IsSafetyPrecautionsEnabled);
				NotifyPropertyChanged(() => IsSafeWorkingLoadsEnabled);
			}
		}
		//=============================================================================
		public int SafetyPrecautionsQuantity
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.SafetyPrecautionsQuantity;

				return 0;
			}
			set
			{
				if (m_Accessories != null && value >= 1)
					m_Accessories.SafetyPrecautionsQuantity = value;

				NotifyPropertyChanged(() => SafetyPrecautionsQuantity);
			}
		}

		//=============================================================================
		public bool IsSafeWorkingLoadsEnabled
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.IsSafeWorkingLoadsEnabled;

				return false;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.IsSafeWorkingLoadsEnabled = value;
				_UpdateSignanes();

				NotifyPropertyChanged(() => Signages);
				NotifyPropertyChanged(() => IsMenaEnabled);
				NotifyPropertyChanged(() => IsSafetyPrecautionsEnabled);
				NotifyPropertyChanged(() => IsSafeWorkingLoadsEnabled);
			}
		}
		//=============================================================================
		public int SafeWorkingLoadsQuantity
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.SafeWorkingLoadsQuantity;

				return 0;
			}
			set
			{
				if (m_Accessories != null && value >= 1)
					m_Accessories.SafeWorkingLoadsQuantity = value;

				NotifyPropertyChanged(() => SafeWorkingLoadsQuantity);
			}
		}

		//=============================================================================
		public double MeshHeight
		{
			get
			{
				if (m_Accessories != null)
					return m_Accessories.MeshHeight;

				return 0.0;
			}
			set
			{
				if (m_Accessories != null)
					m_Accessories.MeshHeight = value;

				NotifyPropertyChanged(() => MeshHeight);
			}
		}


		//=============================================================================
		private void _UpdateSignanes()
		{
			if (m_Accessories == null)
				return;

			if (!m_Accessories.IsSafetyPrecautionsEnabled && !m_Accessories.IsSafeWorkingLoadsEnabled && !m_Accessories.IsMenaEnabled && m_Accessories.Signages)
				m_Accessories.Signages = false;
		}
	}
}
