using AppInterfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DrawingControl
{
	public class DrawingDocument_State : IClonable
	{
		public DrawingDocument_State(
			List<DrawingSheet> sheets,
			int iCurrSheetIndex,
			List<ColumnSizeIndex> _columnSizes,
			Dictionary<int, RackSizeIndex> _rackSizes,
			eBracingType _rackBracingType,
			RackAccessories _rackAccessories,
			//
			string customerName,
			string customerContactNo,
			string customerEMail,
			string customerENQ,
			string customerAddress,
			string customerSite,
			//
			List<PalletConfiguration> palletConfigList,
			List<MHEConfiguration> mheConfigsList,
			//
			bool showAdvancedProperties,
			//
			ePalletType racksPalletsType,
			double racksPalletsOverhangValue,
			//
			string strCurrency,
			double rate,
			double discount
			)
		{
			// Clone sheets
			Sheets = new List<DrawingSheet>();
			if(sheets != null)
			{
				foreach(DrawingSheet sheet in sheets)
				{
					if (sheet == null)
						continue;

					DrawingSheet sheetClone = sheet.Clone() as DrawingSheet;
					if (sheetClone == null)
						continue;

					this.Sheets.Add(sheetClone);
				}
			}
			//
			CurrentSheetIndex = iCurrSheetIndex;
			// Clone column sizes
			ColumnSizes = new List<ColumnSizeIndex>();
			if(_columnSizes != null)
			{
				foreach(ColumnSizeIndex columnSize in _columnSizes)
				{
					if (columnSize == null)
						continue;

					ColumnSizeIndex columnSizeClone = columnSize.Clone() as ColumnSizeIndex;
					if (columnSizeClone == null)
						continue;

					this.ColumnSizes.Add(columnSizeClone);
				}
			}
			// Clone rack size
			RackSizes = new Dictionary<int, RackSizeIndex>();
			if(_rackSizes != null)
			{
				foreach(int rackSizeKey in _rackSizes.Keys)
				{
					RackSizeIndex rackSize = _rackSizes[rackSizeKey];
					if (rackSize == null)
						continue;

					RackSizeIndex rackSizeClone = rackSize.Clone() as RackSizeIndex;
					if (rackSizeClone == null)
						continue;

					this.RackSizes[rackSizeKey] = rackSizeClone;
				}
			}

			//
			Rack_BracingType = _rackBracingType;
			Rack_Accessories = null;
			if(_rackAccessories != null)
				Rack_Accessories = _rackAccessories.Clone() as RackAccessories;

			//
			CustomerName = customerName;
			if (CustomerName == null)
				CustomerName = string.Empty;

			CustomerContactNo = customerContactNo;
			if (CustomerContactNo == null)
				CustomerContactNo = string.Empty;

			CustomerEMail = customerEMail;
			if (CustomerEMail == null)
				CustomerEMail = string.Empty;

			CustomerENQ = customerENQ;
			if (CustomerENQ == null)
				CustomerENQ = string.Empty;

			CustomerAddress = customerAddress;
			if (CustomerAddress == null)
				CustomerAddress = string.Empty;

			CustomerSite = customerSite;
			if (CustomerSite == null)
				CustomerSite = string.Empty;

			// Clone pallet configurations
			PalletConfigList = new List<PalletConfiguration>();
			if(palletConfigList != null)
			{
				foreach (PalletConfiguration palletConfig in palletConfigList)
				{
					if (palletConfig == null)
						continue;

					PalletConfiguration palletConfigClone = palletConfig.Clone() as PalletConfiguration;
					if (palletConfigClone == null)
						continue;

					this.PalletConfigList.Add(palletConfigClone);
				}
			}
			// Clone MHE configurations
			MHEConfigsList = new List<MHEConfiguration>();
			if(mheConfigsList != null)
			{
				foreach(MHEConfiguration mheConfig in mheConfigsList)
				{
					if (mheConfig == null)
						continue;

					MHEConfiguration mheConfigClone = mheConfig.Clone() as MHEConfiguration;
					if (mheConfigClone == null)
						continue;

					this.MHEConfigsList.Add(mheConfigClone);
				}
			}

			//
			ShowAdvancedProperties = showAdvancedProperties;

			//
			RacksPalletType = racksPalletsType;
			RacksPalletsOverhangValue = racksPalletsOverhangValue;

			//
			Currency = strCurrency;
			Rate = rate;
			Discount = discount;
		}
		public DrawingDocument_State(DrawingDocument_State state)
		{
			if(state != null)
			{
				// Clone sheets
				Sheets = new List<DrawingSheet>();
				if (state.Sheets != null)
				{
					foreach (DrawingSheet sheet in state.Sheets)
					{
						if (sheet == null)
							continue;

						DrawingSheet sheetClone = sheet.Clone() as DrawingSheet;
						if (sheetClone == null)
							continue;

						this.Sheets.Add(sheetClone);
					}
				}
				//
				CurrentSheetIndex = state.CurrentSheetIndex;
				// Clone column sizes
				ColumnSizes = new List<ColumnSizeIndex>();
				if (state.ColumnSizes != null)
				{
					foreach (ColumnSizeIndex columnSize in state.ColumnSizes)
					{
						if (columnSize == null)
							continue;

						ColumnSizeIndex columnSizeClone = columnSize.Clone() as ColumnSizeIndex;
						if (columnSizeClone == null)
							continue;

						this.ColumnSizes.Add(columnSizeClone);
					}
				}
				// Clone rack size
				RackSizes = new Dictionary<int, RackSizeIndex>();
				if (state.RackSizes != null)
				{
					foreach (int rackSizeKey in state.RackSizes.Keys)
					{
						RackSizeIndex rackSize = state.RackSizes[rackSizeKey];
						if (rackSize == null)
							continue;

						RackSizeIndex rackSizeClone = rackSize.Clone() as RackSizeIndex;
						if (rackSizeClone == null)
							continue;

						this.RackSizes[rackSizeKey] = rackSizeClone;
					}
				}

				//
				Rack_BracingType = state.Rack_BracingType;
				Rack_Accessories = null;
				if (state.Rack_Accessories != null)
					Rack_Accessories = state.Rack_Accessories.Clone() as RackAccessories;

				//
				CustomerName = state.CustomerName;
				if (CustomerName == null)
					CustomerName = string.Empty;

				CustomerContactNo = state.CustomerContactNo;
				if (CustomerContactNo == null)
					CustomerContactNo = string.Empty;

				CustomerEMail = state.CustomerEMail;
				if (CustomerEMail == null)
					CustomerEMail = string.Empty;

				CustomerENQ = state.CustomerENQ;
				if (CustomerENQ == null)
					CustomerENQ = string.Empty;

				CustomerAddress = state.CustomerAddress;
				if (CustomerAddress == null)
					CustomerAddress = string.Empty;

				CustomerSite = state.CustomerSite;
				if (CustomerSite == null)
					CustomerSite = string.Empty;

				// Clone pallet configurations
				PalletConfigList = new List<PalletConfiguration>();
				if (state.PalletConfigList != null)
				{
					foreach (PalletConfiguration palletConfig in state.PalletConfigList)
					{
						if (palletConfig == null)
							continue;

						PalletConfiguration palletConfigClone = palletConfig.Clone() as PalletConfiguration;
						if (palletConfigClone == null)
							continue;

						this.PalletConfigList.Add(palletConfigClone);
					}
				}
				// Clone MHE configurations
				MHEConfigsList = new List<MHEConfiguration>();
				if (state.MHEConfigsList != null)
				{
					foreach (MHEConfiguration mheConfig in state.MHEConfigsList)
					{
						if (mheConfig == null)
							continue;

						MHEConfiguration mheConfigClone = mheConfig.Clone() as MHEConfiguration;
						if (mheConfigClone == null)
							continue;

						this.MHEConfigsList.Add(mheConfigClone);
					}
				}

				//
				ShowAdvancedProperties = state.ShowAdvancedProperties;

				//
				RacksPalletType = state.RacksPalletType;
				RacksPalletsOverhangValue = state.RacksPalletsOverhangValue;

				//
				Currency = state.Currency;
				Rate = state.Rate;
				Discount = state.Discount;
			}
		}

		#region Properties

		//=============================================================================
		public List<DrawingSheet> Sheets { get; private set; }
		//=============================================================================
		public int CurrentSheetIndex { get; private set; }
		//=============================================================================
		public List<ColumnSizeIndex> ColumnSizes { get; private set; }
		//=============================================================================
		public Dictionary<int, RackSizeIndex> RackSizes { get; private set; }
		//=============================================================================
		public eBracingType Rack_BracingType { get; private set; }
		//=============================================================================
		public RackAccessories Rack_Accessories { get; private set; }
		//=============================================================================
		public string CustomerName { get; protected set; }
		public string CustomerContactNo { get; protected set; }
		public string CustomerEMail { get; protected set; }
		public string CustomerENQ { get; protected set; }
		public string CustomerAddress { get; protected set; }
		public string CustomerSite { get; protected set; }
		//=============================================================================
		public List<PalletConfiguration> PalletConfigList { get; protected set; }
		//=============================================================================
		public List<MHEConfiguration> MHEConfigsList { get; protected set; }
		//=============================================================================
		public bool ShowAdvancedProperties { get; protected set; }
		//=============================================================================
		public ePalletType RacksPalletType { get; protected set; }
		//=============================================================================
		public double RacksPalletsOverhangValue { get; protected set; }
		//=============================================================================
		// Dont copy it, because it is not changed
		//public List<RackColumn> RacksColumnsList { get; protected set; }
		//public List<RackBeam> RacksBeamsList { get; protected set; }
		//=============================================================================
		public string Currency { get; protected set; }
		//=============================================================================
		public double Rate { get; protected set; }
		//=============================================================================
		public double Discount { get; protected set; }

		#endregion

		//=============================================================================
		public virtual IClonable Clone()
		{
			return new DrawingDocument_State(this);
		}
	}
}
