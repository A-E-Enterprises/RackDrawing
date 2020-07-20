using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DrawingControl
{
	// Side on the geometry where it has contact with another geometry.
	public enum eGeometryContactSide : int
	{
		eWithoutContactSide = 1,
		eLeft = 2,
		eRight = 3,
		eTop = 4,
		eBot = 5
	}

	public enum eTieBeamPlacement
	{
		eNone = 0,
		eFirst_Last_EveryThirdFrame = 1,
		eFirst_Last_EveryAlternate = 2,
		eEveryFrame = 3
	}

	public class TieBeamCalculationInfo
	{
		public TieBeamCalculationInfo()
		{
			this.OppositeRacksDict = new Dictionary<Rack, eTieBeamFrame>();
		}

		/// <summary>
		/// Racks between which need to place tie beam.
		/// Key - rack
		/// Value - where tie beam should be placed
		/// </summary>
		public Dictionary<Rack, eTieBeamFrame> OppositeRacksDict { get; private set; }
	}

	public abstract class TieBeamGroup
	{
		public abstract Point TopLeftPnt { get; }
		public abstract Point TopRightPnt { get; }
		public abstract Point BotLeftPnt { get; }
		public abstract Point BotRightPnt { get; }
		// #74 - Tie Beam Trick.
		// Returns the biggest beam span.
		public abstract double BeamLength { get; }
		public double Depth
		{
			get
			{
				double depth = 0.0;

				if (this.IsHorizontal)
					depth = BotLeftPnt.Y - TopLeftPnt.Y;
				else
					depth = TopRightPnt.X - TopLeftPnt.X;

				return depth;
			}
		}

		public eTieBeamPlacement TieBeamPlacement { get; set; }

		public bool IsHorizontal { get; protected set; }

		// eGeometryContactSide - is side on the rack, which contacts with aisle space
		public abstract Dictionary<Rack, eTieBeamFrame> GetTieBeamRacks(eGeometryContactSide aisleSpaceContactSide);

		/// <summary>
		/// Updates Rack.RequiredTieBeamFrames property for the racks in this group
		/// </summary>
		public abstract void UpdateRacksTieBeamRequirements();
	}

	public class TieBeam_SingleGroup : TieBeamGroup
	{
		public TieBeam_SingleGroup(List<Rack> racksGroup)
		{
			this.RacksGroup = new List<Rack>();
			if(racksGroup != null)
				this.RacksGroup.AddRange(racksGroup);

			if(this.RacksGroup != null && this.RacksGroup.Count > 0)
			{
				Rack firstRack = this.RacksGroup[0];
				if(firstRack != null)
				{
					m_TopLeftPnt = firstRack.TopLeft_GlobalPoint;
					m_BotLeftPnt = firstRack.BottomLeft_GlobalPoint;

					this.IsHorizontal = firstRack.IsHorizontal;
				}

				Rack lastRack = this.RacksGroup[this.RacksGroup.Count - 1];
				if(lastRack != null)
				{
					m_TopRightPnt = lastRack.TopRight_GlobalPoint;
					m_BotRightPnt = lastRack.BottomRight_GlobalPoint;
				}
			}
		}

		public List<Rack> RacksGroup { get; private set; }
		//
		public override double BeamLength
		{
			get
			{
				double beamLength = 0.0;

				// #74 - Tie Beam Trick.
				// Returns the biggest beam span.
				if (this.RacksGroup != null)
				{
					foreach (Rack rack in RacksGroup)
					{
						if (rack == null)
							continue;

						if(Utils.FLT(beamLength, rack.BeamLength))
							beamLength = rack.BeamLength;
					}
				}

				return beamLength;
			}
		}
		//
		private Point m_TopLeftPnt = new Point();
		public override Point TopLeftPnt { get { return m_TopLeftPnt; } }
		//
		private Point m_TopRightPnt = new Point();
		public override Point TopRightPnt { get { return m_TopRightPnt; } }
		//
		private Point m_BotLeftPnt = new Point();
		public override Point BotLeftPnt { get { return m_BotLeftPnt; } }
		//
		private Point m_BotRightPnt = new Point();
		public override Point BotRightPnt { get { return m_BotRightPnt; } }

		public override Dictionary<Rack, eTieBeamFrame> GetTieBeamRacks(eGeometryContactSide aisleSpaceContactSide)
		{
			Dictionary<Rack, eTieBeamFrame> result = new Dictionary<Rack, eTieBeamFrame>();

			if (eGeometryContactSide.eWithoutContactSide == aisleSpaceContactSide)
				return result;
			if (this.RacksGroup == null)
				return result;

			result = TieBeamUtils.GetTieBeamRacks(this.RacksGroup, this.TieBeamPlacement);

			return result;
		}

		/// <summary>
		/// Updates Rack.RequiredTieBeamFrames property for the racks in this group
		/// </summary>
		public override void UpdateRacksTieBeamRequirements()
		{
			if (eTieBeamPlacement.eNone == this.TieBeamPlacement)
				return;

			// Get all racks which requires tie beam
			Dictionary<Rack, eTieBeamFrame> result = TieBeamUtils.GetTieBeamRacks(this.RacksGroup, this.TieBeamPlacement);
			// Set their RequiredTieBeamFrames property
			foreach(Rack rack in result.Keys)
			{
				if (rack == null)
					continue;

				eTieBeamFrame requiredTieBeamFrame = result[rack];
				rack.RequiredTieBeamFrames = requiredTieBeamFrame;
			}
		}
	}

	public class TieBeam_BTBGroup : TieBeamGroup
	{
		public TieBeam_BTBGroup(List<Rack> racksGroup_01, List<Rack> racksGroup_02, List<Rack> tieBeamTrickGroup)
		{
			this.RacksGroup_01 = new List<Rack>();
			if (racksGroup_01 != null)
				this.RacksGroup_01.AddRange(racksGroup_01);

			this.RacksGroup_02 = new List<Rack>();
			if (racksGroup_02 != null)
				this.RacksGroup_02.AddRange(racksGroup_02);

			// For this case
			//       |   M  |
			// |   M  |    |   M  |
			//
			// Place all racks in RacksGroup_01, otherwise 
			// public override Dictionary<Rack, eTieBeamFrame> GetTieBeamRacks(eGeometryContactSide aisleSpaceContactSide)
			// makes incorrect calculations.
			if (this.RacksGroup_01.Count == 0 && this.RacksGroup_02.Count > 0)
			{
				this.RacksGroup_01.AddRange(this.RacksGroup_02);
				this.RacksGroup_02.Clear();
			}

			this.TieBeamTrickGroup = new List<Rack>();
			if(tieBeamTrickGroup != null)
			{
				this.TieBeamTrickGroup.AddRange(tieBeamTrickGroup);
				this.TieBeamTrickGroup.RemoveAll(rack => rack != null && (this.RacksGroup_01.Contains(rack) && this.RacksGroup_02.Contains(rack)));
			}

			Rack firstRack_01 = null;
			Rack lastRack_01 = null;
			if (RacksGroup_01 != null && RacksGroup_01.Count > 0)
			{
				firstRack_01 = RacksGroup_01[0];
				lastRack_01 = RacksGroup_01[RacksGroup_01.Count - 1];
			}

			Rack firstRack_02 = null;
			Rack lastRack_02 = null;
			if (RacksGroup_02 != null && RacksGroup_02.Count > 0)
			{
				firstRack_02 = RacksGroup_02[0];
				lastRack_02 = RacksGroup_02[RacksGroup_02.Count - 1];
			}

			if (firstRack_01 != null && firstRack_02 != null && lastRack_01 != null && lastRack_02 != null)
			{
				this.IsHorizontal = firstRack_01.IsHorizontal;

				if (firstRack_01.IsHorizontal)
				{
					if (Utils.FLT(firstRack_01.TopLeft_GlobalPoint.Y, firstRack_02.TopLeft_GlobalPoint.Y))
					{
						m_IsRackGroup01PlacedTopLeft = true;

						m_TopLeftPnt = firstRack_01.TopLeft_GlobalPoint;
						if (Utils.FGT(m_TopLeftPnt.X, firstRack_02.TopLeft_GlobalPoint.X))
							m_TopLeftPnt.X = firstRack_02.TopLeft_GlobalPoint.X;

						m_BotLeftPnt = firstRack_02.BottomLeft_GlobalPoint;
						if (Utils.FGT(m_BotLeftPnt.X, firstRack_01.TopLeft_GlobalPoint.X))
							m_BotLeftPnt.X = firstRack_01.TopLeft_GlobalPoint.X;

						m_TopRightPnt = lastRack_01.TopRight_GlobalPoint;
						if (Utils.FLT(m_TopRightPnt.X, lastRack_02.TopRight_GlobalPoint.X))
							m_TopRightPnt.X = lastRack_02.TopRight_GlobalPoint.X;

						m_BotRightPnt = lastRack_02.BottomRight_GlobalPoint;
						if (Utils.FLT(m_BotRightPnt.X, lastRack_01.TopRight_GlobalPoint.X))
							m_BotRightPnt.X = lastRack_01.TopRight_GlobalPoint.X;
					}
					else
					{
						m_IsRackGroup01PlacedTopLeft = false;

						m_TopLeftPnt = firstRack_02.TopLeft_GlobalPoint;
						if (Utils.FGT(m_TopLeftPnt.X, firstRack_01.TopLeft_GlobalPoint.X))
							m_TopLeftPnt.X = firstRack_01.TopLeft_GlobalPoint.X;

						m_BotLeftPnt = firstRack_01.BottomLeft_GlobalPoint;
						if (Utils.FGT(m_BotLeftPnt.X, firstRack_02.TopLeft_GlobalPoint.X))
							m_BotLeftPnt.X = firstRack_02.TopLeft_GlobalPoint.X;

						m_TopRightPnt = lastRack_02.TopRight_GlobalPoint;
						if (Utils.FLT(m_TopRightPnt.X, lastRack_01.TopRight_GlobalPoint.X))
							m_TopRightPnt.X = lastRack_01.TopRight_GlobalPoint.X;

						m_BotRightPnt = lastRack_01.BottomRight_GlobalPoint;
						if (Utils.FLT(m_BotRightPnt.X, lastRack_02.TopRight_GlobalPoint.X))
							m_BotRightPnt.X = lastRack_02.TopRight_GlobalPoint.X;
					}
				}
				else
				{
					if (Utils.FLE(firstRack_01.TopLeft_GlobalPoint.X, firstRack_02.TopLeft_GlobalPoint.X))
					{
						m_IsRackGroup01PlacedTopLeft = true;

						m_TopLeftPnt = firstRack_01.TopLeft_GlobalPoint;
						if (Utils.FGT(m_TopLeftPnt.Y, firstRack_02.TopLeft_GlobalPoint.Y))
							m_TopLeftPnt.Y = firstRack_02.TopLeft_GlobalPoint.Y;

						m_TopRightPnt = firstRack_02.TopRight_GlobalPoint;
						if (Utils.FGT(m_BotLeftPnt.Y, firstRack_01.TopLeft_GlobalPoint.Y))
							m_BotLeftPnt.Y = firstRack_01.TopLeft_GlobalPoint.Y;

						m_BotLeftPnt = lastRack_01.BottomLeft_GlobalPoint;
						if (Utils.FLT(m_BotLeftPnt.Y, lastRack_02.BottomLeft_GlobalPoint.Y))
							m_BotLeftPnt.Y = lastRack_02.BottomLeft_GlobalPoint.Y;

						m_BotRightPnt = lastRack_02.BottomRight_GlobalPoint;
						if (Utils.FLT(m_BotRightPnt.Y, lastRack_01.BottomLeft_GlobalPoint.Y))
							m_BotRightPnt.Y = lastRack_01.BottomLeft_GlobalPoint.Y;
					}
					else
					{
						m_IsRackGroup01PlacedTopLeft = false;

						m_TopLeftPnt = firstRack_02.TopLeft_GlobalPoint;
						if (Utils.FGT(m_TopLeftPnt.Y, firstRack_01.TopLeft_GlobalPoint.Y))
							m_TopLeftPnt.Y = firstRack_01.TopLeft_GlobalPoint.Y;

						m_TopRightPnt = firstRack_01.TopRight_GlobalPoint;
						if (Utils.FGT(m_BotLeftPnt.Y, firstRack_02.TopLeft_GlobalPoint.Y))
							m_BotLeftPnt.Y = firstRack_02.TopLeft_GlobalPoint.Y;

						m_BotLeftPnt = lastRack_02.BottomLeft_GlobalPoint;
						if (Utils.FLT(m_BotLeftPnt.Y, lastRack_01.BottomLeft_GlobalPoint.Y))
							m_BotLeftPnt.Y = lastRack_01.BottomLeft_GlobalPoint.Y;

						m_BotRightPnt = lastRack_01.BottomRight_GlobalPoint;
						if (Utils.FLT(m_BotRightPnt.Y, lastRack_02.BottomLeft_GlobalPoint.Y))
							m_BotRightPnt.Y = lastRack_02.BottomLeft_GlobalPoint.Y;
					}
				}
			}
			else
			{
				// Check this case:
				//       |   M  |
				// |   M  |    |   M  |
				//
				// racksGroup_01.Count == 0 || racksGroup_02.Count == 0
				// but tieBeamTrickGroup.Count > 0
				//
				Rack firstRack = null;
				if (firstRack_01 != null)
					firstRack = firstRack_01;
				else if (firstRack_02 != null)
					firstRack = firstRack_02;

				Rack lastRack = null;
				if (lastRack_01 != null)
					lastRack = lastRack_01;
				else if (lastRack_02 != null)
					lastRack = lastRack_02;

				if(firstRack != null && lastRack != null && TieBeamTrickGroup.Count > 0)
				{
					this.IsHorizontal = firstRack.IsHorizontal;

					if(this.IsHorizontal)
					{
						// Take X-position from firstRack and lastRack.
						// Include TieBeamTrickGroup in calc Y-position.

						m_TopLeftPnt = firstRack.TopLeft_GlobalPoint;
						m_BotLeftPnt = firstRack.BottomLeft_GlobalPoint;
						m_TopRightPnt = lastRack.TopRight_GlobalPoint;
						m_BotRightPnt = lastRack.BottomRight_GlobalPoint;

						foreach (Rack tieBeamTrickRack in this.TieBeamTrickGroup)
						{
							if (tieBeamTrickRack == null)
								continue;

							if (Utils.FGT(m_TopLeftPnt.Y, tieBeamTrickRack.TopLeft_GlobalPoint.Y))
							{
								m_TopLeftPnt.Y = tieBeamTrickRack.TopLeft_GlobalPoint.Y;
								m_TopRightPnt.Y = tieBeamTrickRack.TopRight_GlobalPoint.Y;

								m_IsRackGroup01PlacedTopLeft = false;
							}

							if (Utils.FLT(m_BotLeftPnt.Y, tieBeamTrickRack.BottomLeft_GlobalPoint.Y))
							{
								m_BotLeftPnt.Y = tieBeamTrickRack.BottomLeft_GlobalPoint.Y;
								m_BotRightPnt.Y = tieBeamTrickRack.BottomRight_GlobalPoint.Y;

								m_IsRackGroup01PlacedTopLeft = true;
							}
						}
					}
					else
					{
						// Take Y-position from firstRack and lastRack.
						// Include TieBeamTrickGroup in calc X-position.

						m_TopLeftPnt = firstRack.TopLeft_GlobalPoint;
						m_TopRightPnt = firstRack.TopRight_GlobalPoint;
						m_BotLeftPnt = lastRack.BottomLeft_GlobalPoint;
						m_BotRightPnt = lastRack.BottomRight_GlobalPoint;

						foreach (Rack tieBeamTrickRack in this.TieBeamTrickGroup)
						{
							if (tieBeamTrickRack == null)
								continue;

							if (Utils.FGT(m_TopLeftPnt.X, tieBeamTrickRack.TopLeft_GlobalPoint.X))
							{
								m_TopLeftPnt.X = tieBeamTrickRack.TopLeft_GlobalPoint.X;
								m_BotLeftPnt.X = tieBeamTrickRack.BottomLeft_GlobalPoint.X;

								m_IsRackGroup01PlacedTopLeft = false;
							}

							if (Utils.FLT(m_TopRightPnt.X, tieBeamTrickRack.TopRight_GlobalPoint.X))
							{
								m_TopRightPnt.X = tieBeamTrickRack.TopRight_GlobalPoint.X;
								m_BotRightPnt.X = tieBeamTrickRack.BottomRight_GlobalPoint.X;

								m_IsRackGroup01PlacedTopLeft = true;
							}
						}
					}
				}
			}
		}

		//
		public List<Rack> RacksGroup_01 { get; private set; }
		public List<Rack> RacksGroup_02 { get; private set; }
		/// <summary>
		/// List with racks for which tie beam trick is applied.
		/// Contains only unique racks, which are not included in RacksGroup_01 and RacksGroup_02.
		/// </summary>
		public List<Rack> TieBeamTrickGroup { get; private set; }
		/// <summary>
		/// If true then RacksGroup_01 are placed top left relative to RacksGroup_02.
		/// </summary>
		private bool m_IsRackGroup01PlacedTopLeft = false;
		//
		public override double BeamLength
		{
			get
			{
				double beamLength = 0.0;

				// #74 - Tie Beam Trick.
				// Returns the biggest beam span.
				if (this.RacksGroup_01 != null)
				{
					foreach (Rack rack in RacksGroup_01)
					{
						if (rack == null)
							continue;

						if (Utils.FLT(beamLength, rack.BeamLength))
							beamLength = rack.BeamLength;
					}
				}
				//
				if (this.RacksGroup_02 != null)
				{
					foreach (Rack rack in RacksGroup_02)
					{
						if (rack == null)
							continue;

						if (Utils.FLT(beamLength, rack.BeamLength))
							beamLength = rack.BeamLength;
					}
				}

				return beamLength;
			}
		}
		//
		private Point m_TopLeftPnt = new Point();
		public override Point TopLeftPnt { get { return m_TopLeftPnt; } }
		//
		private Point m_TopRightPnt = new Point();
		public override Point TopRightPnt { get { return m_TopRightPnt; } }
		//
		private Point m_BotLeftPnt = new Point();
		public override Point BotLeftPnt { get { return m_BotLeftPnt; } }
		//
		private Point m_BotRightPnt = new Point();
		public override Point BotRightPnt { get { return m_BotRightPnt; } }

		public override Dictionary<Rack, eTieBeamFrame> GetTieBeamRacks(eGeometryContactSide aisleSpaceContactSide)
		{
			Dictionary<Rack, eTieBeamFrame> result = new Dictionary<Rack, eTieBeamFrame>();

			if (eGeometryContactSide.eWithoutContactSide == aisleSpaceContactSide)
				return result;

			List<Rack> racksGroup = null;
			if(this.IsHorizontal)
			{
				if(eGeometryContactSide.eBot == aisleSpaceContactSide)
				{
					if (m_IsRackGroup01PlacedTopLeft)
						racksGroup = this.RacksGroup_02;
					else
						racksGroup = this.RacksGroup_01;
				}
				else if(eGeometryContactSide.eTop == aisleSpaceContactSide)
				{
					if (m_IsRackGroup01PlacedTopLeft)
						racksGroup = this.RacksGroup_01;
					else
						racksGroup = this.RacksGroup_02;
				}
			}
			else
			{
				if (eGeometryContactSide.eLeft == aisleSpaceContactSide)
				{
					if (m_IsRackGroup01PlacedTopLeft)
						racksGroup = this.RacksGroup_01;
					else
						racksGroup = this.RacksGroup_02;
				}
				else if (eGeometryContactSide.eRight == aisleSpaceContactSide)
				{
					if (m_IsRackGroup01PlacedTopLeft)
						racksGroup = this.RacksGroup_02;
					else
						racksGroup = this.RacksGroup_01;
				}
			}

			if (racksGroup == null)
				return result;

			// Check this case:
			//       |   M  |
			// |   M  |    |   M  |
			//
			// racksGroup.Count = 0 and need to calculate frames for TieBeamTrickGroup
			if(racksGroup.Count == 0 && TieBeamTrickGroup.Count >= 2)
			{
				result.Add(TieBeamTrickGroup[0], eTieBeamFrame.eEndFrame);
				result.Add(TieBeamTrickGroup[TieBeamTrickGroup.Count - 1], eTieBeamFrame.eStartFrame);
				return result;
			}

			result = TieBeamUtils.GetTieBeamRacks(racksGroup, this.TieBeamPlacement);

			return result;
		}

		/// <summary>
		/// Updates Rack.RequiredTieBeamFrames property for the racks in this group
		/// </summary>
		public override void UpdateRacksTieBeamRequirements()
		{
			if (eTieBeamPlacement.eNone == this.TieBeamPlacement)
				return;

			// Get all racks which requires tie beam
			Dictionary<Rack, eTieBeamFrame> result = new Dictionary<Rack, eTieBeamFrame>();
			//
			Dictionary<Rack, eTieBeamFrame> topDict = this.GetTieBeamRacks(eGeometryContactSide.eTop);
			if(topDict != null)
			{
				foreach(Rack rackKey in topDict.Keys)
				{
					if (rackKey == null)
						continue;

					eTieBeamFrame frame = topDict[rackKey];
					if (frame == eTieBeamFrame.eNone)
						continue;

					if (!result.ContainsKey(rackKey))
						result[rackKey] = eTieBeamFrame.eNone;
					result[rackKey] |= frame;
				}
			}
			//
			Dictionary<Rack, eTieBeamFrame> botDict = this.GetTieBeamRacks(eGeometryContactSide.eBot);
			if (botDict != null)
			{
				foreach (Rack rackKey in botDict.Keys)
				{
					if (rackKey == null)
						continue;

					eTieBeamFrame frame = botDict[rackKey];
					if (frame == eTieBeamFrame.eNone)
						continue;

					if (!result.ContainsKey(rackKey))
						result[rackKey] = eTieBeamFrame.eNone;
					result[rackKey] |= frame;
				}
			}
			//
			Dictionary<Rack, eTieBeamFrame> leftDict = this.GetTieBeamRacks(eGeometryContactSide.eLeft);
			if (leftDict != null)
			{
				foreach (Rack rackKey in leftDict.Keys)
				{
					if (rackKey == null)
						continue;

					eTieBeamFrame frame = leftDict[rackKey];
					if (frame == eTieBeamFrame.eNone)
						continue;

					if (!result.ContainsKey(rackKey))
						result[rackKey] = eTieBeamFrame.eNone;
					result[rackKey] |= frame;
				}
			}
			//
			Dictionary<Rack, eTieBeamFrame> rightDict = this.GetTieBeamRacks(eGeometryContactSide.eRight);
			if (rightDict != null)
			{
				foreach (Rack rackKey in rightDict.Keys)
				{
					if (rackKey == null)
						continue;

					eTieBeamFrame frame = rightDict[rackKey];
					if (frame == eTieBeamFrame.eNone)
						continue;

					if (!result.ContainsKey(rackKey))
						result[rackKey] = eTieBeamFrame.eNone;
					result[rackKey] |= frame;
				}
			}

			// Set their RequiredTieBeamFrames property
			foreach (Rack rack in result.Keys)
			{
				if (rack == null)
					continue;

				eTieBeamFrame requiredTieBeamFrame = result[rack];
				rack.RequiredTieBeamFrames = requiredTieBeamFrame;
			}
		}
	}

	/// <summary>
	/// TieBeamGroupsInfo.Calculate() method depends on the racks group sort.
	/// </summary>
	internal class RacksGroupsComparer : IComparer<List<Rack>>
	{
		/// <summary>
		/// -1 means x less than y
		///  0 means x equals y
		///  1 means x greater than y
		/// 
		/// Order:
		/// 1. Horizontal racks groups
		/// 2. Vertical racks groups
		/// 
		/// If racks have same rotation(horizontal\vertical) then:
		/// 1. If racks are horizontal then rack with less TopLeftPoint.X should be placed first.
		/// 2. If racks are vertical then rack with less TopLeftPoint.Y should be placed first.
		/// 
		/// If racks have same top left point position then compare racks count in the group.
		/// Group with bigger racks count should be placed first.
		/// </summary>
		public int Compare(List<Rack> x, List<Rack> y)
		{
			if (x != null && y != null)
			{
				if (x.Count == 0 && y.Count == 0)
					return 0;
				else if (x.Count == 0)
					return 1;
				else if (y.Count == 0)
					return -1;

				Rack firstRack_X = x[0];
				Rack firstRack_Y = y[0];

				if(firstRack_X != null && firstRack_Y != null)
				{
					// Compare rotation
					if(firstRack_X.IsHorizontal != firstRack_Y.IsHorizontal)
					{
						if (firstRack_X.IsHorizontal)
							return -1;
						else
							return 1;
					}

					// Compare top left point position.
					Point topLeftPoint_X = firstRack_X.TopLeft_GlobalPoint;
					Point topLeftPoint_Y = firstRack_Y.TopLeft_GlobalPoint;

					if(firstRack_X.IsHorizontal)
					{
						if (Utils.FLT(topLeftPoint_X.X, topLeftPoint_Y.X))
							return -1;
						else if (Utils.FLT(topLeftPoint_Y.X, topLeftPoint_X.X))
							return 1;
					}
					else
					{
						if (Utils.FLT(topLeftPoint_X.Y, topLeftPoint_Y.Y))
							return -1;
						else if (Utils.FLT(topLeftPoint_Y.Y, topLeftPoint_X.Y))
							return 1;
					}

					// Compare group count
					int groupCount_X = x.Count;
					int groupCount_Y = y.Count;

					if (groupCount_X > groupCount_Y)
						return -1;
					else if (groupCount_X < groupCount_Y)
						return 1;

					return 0;
				}
				else if (firstRack_X == null)
					return 1;
				else if (firstRack_Y == null)
					return -1;
			}
			else if (x == null)
				return 1;
			else if (y == null)
				return -1;

			return 0;
		}
	}

	/// <summary>
	/// Racks grouped for tie beam calculation.
	/// </summary>
	public class TieBeamGroupsInfo
	{
		public TieBeamGroupsInfo(List<List<Rack>> racksGroups, ePalletType racksPalletType, double racksBTBDistance)
		{
			this.Calculate(racksGroups, racksPalletType, racksBTBDistance);
		}

		#region Properties

		/// <summary>
		/// Set with racks already inlcuded in BTB groups
		/// </summary>
		private HashSet<Rack> m_BTBRacksSet = new HashSet<Rack>();

		/// <summary>
		/// List of single row racks groups.
		/// </summary>
		public List<TieBeam_SingleGroup> m_SingleGroupsList = new List<TieBeam_SingleGroup>();
		/// <summary>
		/// List of double rows(BTB) racks groups.
		/// </summary>
		public List<TieBeam_BTBGroup> m_BTBGroupsList = new List<TieBeam_BTBGroup>();

		public List<TieBeamGroup> m_TieBeamsGroupsList = new List<TieBeamGroup>();

		#endregion

		#region Methods

		/// <summary>
		/// Groups rack in BTB and single group.
		/// </summary>
		public void Calculate(List<List<Rack>> racksGroups, ePalletType racksPalletType, double racksBTBDistance)
		{
			m_BTBRacksSet.Clear();
			m_SingleGroupsList.Clear();
			m_BTBGroupsList.Clear();

			if (racksGroups == null || racksGroups.Count == 0)
				return;

			// This method requires racks group sort before calculate.
			RacksGroupsComparer racksGroupsComparer = new RacksGroupsComparer();
			racksGroups.Sort(racksGroupsComparer);

			foreach (List<Rack> group_01 in racksGroups)
			{
				this.Calculate(group_01, null, racksGroups, racksPalletType, racksBTBDistance);
			}

			// Calculate frames which requires tie beams.
			CalcTieBeamPlacement();
		}
		private void Calculate(
			List<Rack> group_01,
			Rack group_01_StartRack,
			List<List<Rack>> racksGroups,
			ePalletType racksPalletType,
			double racksBTBDistance,
			//
			List<Rack> oldSingleGroup = null,
			List<Rack> oldBTBGroup = null,
			List<Rack> oldTieBeamTrickList = null
			)
		{
			if (group_01 == null)
				return;

			if (group_01.Count == 0)
				return;

			if (racksGroups == null || racksGroups.Count == 0)
				return;

			// Racks group from current group_01.
			List<Rack> singleGroup = new List<Rack>();
			if (oldSingleGroup != null)
				singleGroup.AddRange(oldSingleGroup);
			// BTB racks for singleGroup.
			List<Rack> btbGroup = new List<Rack>();
			if (oldBTBGroup != null)
				btbGroup.AddRange(oldBTBGroup);
			// List with racks for which tie beam trick is applied.
			List<Rack> tieBeamTrickList = new List<Rack>();
			if (oldTieBeamTrickList != null)
				tieBeamTrickList.AddRange(oldTieBeamTrickList);
			//
			Rack lastAddedBTBRack = null;
			List<Rack> lastAddedBTBRackGroup = null;

			int index_01 = 0;
			if (group_01_StartRack != null && group_01.Contains(group_01_StartRack))
				index_01 = group_01.IndexOf(group_01_StartRack);
			for (; index_01 < group_01.Count; ++index_01)
			{
				Rack rack_01 = group_01[index_01];

				if (rack_01 == null || rack_01.Column == null)
				{
					AddGroups(ref singleGroup, ref btbGroup, ref tieBeamTrickList);
					continue;
				}

				if (m_BTBRacksSet.Contains(rack_01))
				{
					AddGroups(ref singleGroup, ref btbGroup, ref tieBeamTrickList);
					continue;
				}

				double mLength_01 = rack_01.Length;
				if (!rack_01.IsFirstInRowColumn)
					mLength_01 += rack_01.DiffBetween_M_and_A;

				// try to find btb rack to this
				bool bBTBRackIsFound = false;
				foreach (List<Rack> group_02 in racksGroups)
				{
					if (group_02 == null)
						continue;
					if (group_02.Count == 0)
						continue;
					if (group_01 == group_02)
						continue;

					// racks should have the same rotation
					if (group_02[0] == null)
						continue;
					if (group_02[0].IsHorizontal != rack_01.IsHorizontal)
						continue;

					// #74 - Tie Beam Trick.
					// Racks in btbGroup and tieBeamTrickList should have the same depth.
					Rack btbRackExample = null;
					if (btbGroup.Count > 0)
						btbRackExample = btbGroup[0];
					else if (tieBeamTrickList.Count > 0)
						btbRackExample = tieBeamTrickList[0];
					if (btbRackExample != null && Utils.FNE(btbRackExample.Depth, group_02[0].Depth))
						continue;

					// Tie beam trick.
					// If rack_01 is A-rack and rack_02 is M-rack then
					// try to compare rack_01 right column with rack_02 left column.
					bool bTieBeamTrick = false;
					// Tie beam trick can be applied only when btbGroup contains any rack.
					// Otherwise, tie beam trick will be applied in this case:
					//                 ---------
					//                 |    M   |
					// -------------------------
					// |    M   |   A   |   A   |
					// -------------------------
					bool bTieBeamTrickCanBeApplied = btbGroup.Count > 0;

					for (int index_02 = 0; index_02 < group_02.Count; ++index_02)
					{
						Rack rack_02 = group_02[index_02];
						if (rack_02 == null || rack_02.Column == null)
							break;

						if (m_BTBRacksSet.Contains(rack_02))
							continue;

						// Racks group are BTB if they stays face to face and distance between them is equal to BTB distance.
						// Racks are BTB if their columns stay face-to-face.
						//
						// Step 1.
						// Check right column(for horizontal racks) and bot columns(for vertical racks).
						// Columns stays face to face if their center is aligned.
						if (rack_01.IsHorizontal)
						{
							// compare right side columns
							Point rack_01_RightColumnCenter = rack_01.TopRight_GlobalPoint;
							rack_01_RightColumnCenter.X -= rack_01.Column.Length / 2;
							Point rack_02_RightColumnCenter = rack_02.TopRight_GlobalPoint;
							rack_02_RightColumnCenter.X -= rack_02.Column.Length / 2;
							if (Utils.FNE(rack_01_RightColumnCenter.X, rack_02_RightColumnCenter.X))
							{
								// Tie beam trick.
								// If rack_01 is A-rack and rack_02 is M-rack then
								// try to compare rack_01 right column with rack_02 left column.
								bool bColumnsFramesStaysFaceToFace = false;
								// condition "(!rack_01.IsFirstInRowColumn || singleGroup.Count > 0)" is used for multiswitch case:
								// |   M  |  A  |  A  |    |   M  |
								// |   M  |    |   M  |  A  |    |   M  |
								//
								// condition "btbGroup.Count + tieBeamTrickList.Count > 0"
								//       |   M  |
								// |   M  |    |   M  |
								if (btbGroup.Count + tieBeamTrickList.Count > 0)
								{
									// Remove this condition "(!rack_01.IsFirstInRowColumn || singleGroup.Count > 0) &&".
									// Otherwise this case is not working:
									//       |   M  |    |   M  |
									// |   M  |    |   M  |  A  |
									//
									// Try to apply tie beam trick at the end frame of rack_01.
									if (/*(!rack_01.IsFirstInRowColumn || singleGroup.Count > 0) &&*/ rack_02.IsFirstInRowColumn && rack_02.Column != null)
									{
										Point rack_02_LeftColumnCenter = rack_02.TopLeft_GlobalPoint;
										rack_02_LeftColumnCenter.X += rack_02.Column.Length / 2;
										if (Utils.FEQ(rack_01_RightColumnCenter.X, rack_02_LeftColumnCenter.X))
										{
											bColumnsFramesStaysFaceToFace = true;
											bTieBeamTrick = true;
										}
									}
								}
								else
								{
									// Try to apply tie beam trick at the start frame of rack_01.
									if (rack_01.IsFirstInRowColumn && rack_02.Column != null)
									{
										Point rack_01_LeftColumnCenter = rack_01.TopLeft_GlobalPoint;
										rack_01_LeftColumnCenter.X += rack_01.Column.Length / 2;
										if (Utils.FEQ(rack_01_LeftColumnCenter.X, rack_02_RightColumnCenter.X))
										{
											bColumnsFramesStaysFaceToFace = true;
											bTieBeamTrick = true;
										}
									}
								}

								if (!bColumnsFramesStaysFaceToFace)
								{
									continue;
								}
							}
						}
						else
						{
							// compare bot side columns
							Point rack_01_BotColumnCenter = rack_01.BottomLeft_GlobalPoint;
							rack_01_BotColumnCenter.Y -= rack_01.Column.Length / 2;
							Point rack_02_BotColumnCenter = rack_02.BottomLeft_GlobalPoint;
							rack_02_BotColumnCenter.Y -= rack_02.Column.Length / 2;
							if (Utils.FNE(rack_01_BotColumnCenter.Y, rack_02_BotColumnCenter.Y))
							{
								// Tie beam trick.
								// If rack_01 is A-rack and rack_02 is M-rack then
								// try to compare rack_01 bot column with rack_02 top column.
								bool bColumnsFramesStaysFaceToFace = false;
								// condition "(!rack_01.IsFirstInRowColumn || singleGroup.Count > 0)" is used for multiswitch case:
								// |   M  |  A  |  A  |    |   M  |
								// |   M  |    |   M  |  A  |    |   M  |
								//
								// condition "btbGroup.Count + tieBeamTrickList.Count > 0"
								//       |   M  |
								// |   M  |    |   M  |
								if (btbGroup.Count + tieBeamTrickList.Count > 0)
								{
									// Remove this condition "(!rack_01.IsFirstInRowColumn || singleGroup.Count > 0) &&".
									// Otherwise this case is not working:
									//       |   M  |    |   M  |
									// |   M  |    |   M  |  A  |
									//
									// Try to apply tie beam trick at the end frame of rack_01.

									if (/*(!rack_01.IsFirstInRowColumn || singleGroup.Count > 0) &&*/ rack_02.IsFirstInRowColumn && rack_02.Column != null)
									{
										Point rack_02_TopColumnCenter = rack_02.TopLeft_GlobalPoint;
										rack_02_TopColumnCenter.Y += rack_02.Column.Length / 2;
										if (Utils.FEQ(rack_01_BotColumnCenter.Y, rack_02_TopColumnCenter.Y))
										{
											bColumnsFramesStaysFaceToFace = true;
											bTieBeamTrick = true;
										}
									}
								}
								else
								{
									// Try to apply tie beam trick at the start frame of rack_01.
									if (rack_01.IsFirstInRowColumn && rack_02.Column != null)
									{
										Point rack_01_TopColumnCenter = rack_01.TopLeft_GlobalPoint;
										rack_01_TopColumnCenter.Y += rack_01.Column.Length / 2;
										if (Utils.FEQ(rack_01_TopColumnCenter.Y, rack_02_BotColumnCenter.Y))
										{
											bColumnsFramesStaysFaceToFace = true;
											bTieBeamTrick = true;
										}
									}
								}

								if (!bColumnsFramesStaysFaceToFace)
								{
									continue;
								}
							}
						}

						// Step 2.
						// Racks should have the same M-length.
						if (!bTieBeamTrick)
						{
							double mLength_02 = rack_02.Length;
							if (!rack_02.IsFirstInRowColumn)
								mLength_02 += rack_02.DiffBetween_M_and_A;
							if (Utils.FNE(mLength_01, mLength_02))
							{
								if (btbGroup.Count > 0)
									AddGroups(ref singleGroup, ref btbGroup, ref tieBeamTrickList);
								continue;
							}
						}

						// Step 3.
						double distanceBetweenRacks = 0.0;
						if (rack_01.IsHorizontal)
						{
							if (Utils.FLT(rack_01.TopLeft_GlobalPoint.Y, rack_02.TopLeft_GlobalPoint.Y))
								distanceBetweenRacks = rack_02.TopLeft_GlobalPoint.Y - rack_01.BottomLeft_GlobalPoint.Y;
							else
								distanceBetweenRacks = rack_01.TopLeft_GlobalPoint.Y - rack_02.BottomLeft_GlobalPoint.Y;
						}
						else
						{
							if (Utils.FLT(rack_01.TopLeft_GlobalPoint.X, rack_02.TopLeft_GlobalPoint.X))
								distanceBetweenRacks = rack_02.TopLeft_GlobalPoint.X - rack_01.TopRight_GlobalPoint.X;
							else
								distanceBetweenRacks = rack_01.TopLeft_GlobalPoint.X - rack_02.TopRight_GlobalPoint.X;
						}

						// remove margin
						double distance = distanceBetweenRacks;
						if (ePalletType.eOverhang == racksPalletType)
						{
							if (rack_01.IsHorizontal)
								distance -= rack_01.MarginY + rack_02.MarginY;
							else
								distance -= rack_01.MarginX + rack_02.MarginX;
						}

						if (Utils.FLE(distance, racksBTBDistance))
						{
							// Groups have different count.
							// It looks like there is single racks group, so add them to singleGroup and
							// add btb racks.
							//if (singleGroup.Count != (btbGroup.Count + tieBeamTrickList.Count))
							if(singleGroup.Count > 0 && (btbGroup.Count + tieBeamTrickList.Count == 0))
							{
								AddGroups(ref singleGroup, ref btbGroup, ref tieBeamTrickList);
							}

							if(!singleGroup.Contains(rack_01))
								singleGroup.Add(rack_01);
							if(!m_BTBRacksSet.Contains(rack_01))
								m_BTBRacksSet.Add(rack_01);

							bBTBRackIsFound = true;
							lastAddedBTBRack = rack_02;
							lastAddedBTBRackGroup = group_02;

							if (bTieBeamTrick)
							{
								tieBeamTrickList.Add(rack_02);
								break;
							}

							if(!btbGroup.Contains(rack_02))
								btbGroup.Add(rack_02);
							if(!m_BTBRacksSet.Contains(rack_02))
								m_BTBRacksSet.Add(rack_02);

							break;
						}
					}

					// If tie beam trick is applied then take next rack_01 rack.
					//
					// condition "btbGroup.Count > 0"
					//       |   M  |
					// |   M  |    |   M  |
					if (bTieBeamTrick && bBTBRackIsFound && btbGroup.Count > 0)
						break;
					if (bBTBRackIsFound && btbGroup.Count > 0)
						break;
				}

				// If BTB-rack is not found, but we already have btb-groups then
				// need to complete this btb-group and start new single group with rack_01.
				if (!bBTBRackIsFound && (btbGroup.Count + tieBeamTrickList.Count > 0))
					AddGroups(ref singleGroup, ref btbGroup, ref tieBeamTrickList);

				if(!bBTBRackIsFound)
				{
					lastAddedBTBRack = null;
					lastAddedBTBRackGroup = null;
				}

				if (!singleGroup.Contains(rack_01))
					singleGroup.Add(rack_01);
			}

			// Try to check this case:
			//
			// |   M  |  A  |  A  |    |   M  |
			// |   M  |    |   M  |  A  |  A  |
			//
			// If racks in group_01 is ended, then switch to the last BTB rack and try to continue.
			if (lastAddedBTBRack != null && lastAddedBTBRackGroup != null && lastAddedBTBRackGroup.Contains(lastAddedBTBRack))
			{
				Rack newLoopRack = lastAddedBTBRack;
				if(btbGroup.Contains(newLoopRack))
				{
					// take next one rack
					int iCurrRackIndex = lastAddedBTBRackGroup.IndexOf(newLoopRack);
					if (iCurrRackIndex < (lastAddedBTBRackGroup.Count - 1))
						newLoopRack = lastAddedBTBRackGroup[iCurrRackIndex + 1];
					else
						newLoopRack = null;
				}

				if (newLoopRack != null)
				{
					m_BTBRacksSet.Remove(newLoopRack);
					this.Calculate(lastAddedBTBRackGroup, newLoopRack, racksGroups, racksPalletType, racksBTBDistance, btbGroup, singleGroup, tieBeamTrickList);
					return;
				}
			}

			AddGroups(ref singleGroup, ref btbGroup, ref tieBeamTrickList);
		}

		/// <summary>
		/// Add groups to m_SingleGroupsList and m_BTBGroupsList
		/// </summary>
		private void AddGroups(ref List<Rack> singleGroup, ref List<Rack> btbGroup, ref List<Rack> tieBeamTrickList)
		{
			if (singleGroup == null)
				return;
			if (singleGroup.Count == 0 && (btbGroup == null || btbGroup.Count == 0))
				return;

			int iTieBeamTrickCount = 0;
			if (tieBeamTrickList != null)
				iTieBeamTrickCount = tieBeamTrickList.Count;

			//if (btbGroup == null || (btbGroup.Count + iTieBeamTrickCount) != singleGroup.Count)
			if (btbGroup == null || btbGroup.Count == 0)
			{
				// make single group
				TieBeam_SingleGroup newSingleGroup = new TieBeam_SingleGroup(singleGroup);
				m_SingleGroupsList.Add(newSingleGroup);
			}
			else
			{
				// make btb group

				// Check this case:
				//       |   M  |
				// |   M  |    |   M  |
				if (singleGroup.Count + btbGroup.Count == 1)
				{
					if(tieBeamTrickList.Count == 2)
					{
						TieBeam_BTBGroup newBtbGroup = new TieBeam_BTBGroup(singleGroup, btbGroup, tieBeamTrickList);
						m_BTBGroupsList.Add(newBtbGroup);
					}
					else
					{
						// create single group
						if(singleGroup.Count > 0)
						{
							TieBeam_SingleGroup newSingleGroup = new TieBeam_SingleGroup(singleGroup);
							m_SingleGroupsList.Add(newSingleGroup);
						}
						if(btbGroup.Count > 0)
						{
							TieBeam_SingleGroup newSingleGroup = new TieBeam_SingleGroup(btbGroup);
							m_SingleGroupsList.Add(newSingleGroup);
						}
					}
				}
				else
				{
					TieBeam_BTBGroup newBtbGroup = new TieBeam_BTBGroup(singleGroup, btbGroup, tieBeamTrickList);
					m_BTBGroupsList.Add(newBtbGroup);
				}
			}

			singleGroup.Clear();
			if (btbGroup != null)
				btbGroup.Clear();
			if (tieBeamTrickList != null)
				tieBeamTrickList.Clear();
		}

		/// <summary>
		/// Calculate tie beam placement for groups.
		/// </summary>
		private void CalcTieBeamPlacement()
		{
			foreach (TieBeam_SingleGroup singleGroup in m_SingleGroupsList)
			{
				if (singleGroup == null)
					continue;

				eTieBeamPlacement placement = eTieBeamPlacement.eNone;

				double depth = singleGroup.Depth;
				double beamLength = singleGroup.BeamLength;
				double theBiggestLoadingHeight = 0.0;
				if (Utils.FGT(depth, 0.0) && Utils.FGT(beamLength, 0.0) && CalculateRackGroupParams(singleGroup.RacksGroup, out theBiggestLoadingHeight))
				{
					double hdRatio = theBiggestLoadingHeight / depth;
					placement = TieBeamUtils.GetTieBeamPlacement(beamLength, hdRatio);
				}

				singleGroup.TieBeamPlacement = placement;
				if (eTieBeamPlacement.eNone != singleGroup.TieBeamPlacement)
					m_TieBeamsGroupsList.Add(singleGroup);
				singleGroup.UpdateRacksTieBeamRequirements();
			}

			foreach (TieBeam_BTBGroup btbGroup in m_BTBGroupsList)
			{
				if (btbGroup == null)
					continue;

				eTieBeamPlacement placement = eTieBeamPlacement.eNone;

				double theBiggestLoadingHeight = 0.0;
				double firstGroupLoadingHeight = 0.0;
				if (CalculateRackGroupParams(btbGroup.RacksGroup_01, out firstGroupLoadingHeight))
					theBiggestLoadingHeight = firstGroupLoadingHeight;
				double secondGroupLoadingHeight = 0.0;
				if (CalculateRackGroupParams(btbGroup.RacksGroup_02, out secondGroupLoadingHeight) && Utils.FGT(secondGroupLoadingHeight, firstGroupLoadingHeight))
					theBiggestLoadingHeight = secondGroupLoadingHeight;

				double depth = btbGroup.Depth;
				double beamLength = btbGroup.BeamLength;
				if (Utils.FGT(depth, 0.0) && Utils.FGT(beamLength, 0.0))
				{
					double hdRatio = theBiggestLoadingHeight / depth;
					placement = TieBeamUtils.GetTieBeamPlacement(beamLength, hdRatio);
				}

				btbGroup.TieBeamPlacement = placement;
				if (eTieBeamPlacement.eNone != btbGroup.TieBeamPlacement)
					m_TieBeamsGroupsList.Add(btbGroup);
				btbGroup.UpdateRacksTieBeamRequirements();
			}
		}

		private bool CalculateRackGroupParams(List<Rack> racksGroup, out double theBiggestLoadingHeight)
		{
			theBiggestLoadingHeight = 0.0;

			if (racksGroup == null)
				return false;

			foreach (Rack rack in racksGroup)
			{
				if (rack == null)
					continue;

				if (Utils.FGT(rack.MaxLoadingHeight, theBiggestLoadingHeight))
					theBiggestLoadingHeight = rack.MaxLoadingHeight;
			}

			return true;
		}

		#endregion
	}

	public static class TieBeamUtils
	{
		public static List<TieBeamCalculationInfo> CalculateTieBeams(
			AisleSpace aisleSpace,
			eGeometryContactSide contactSide,
			Dictionary<Rack, eTieBeamFrame> tieBeamsRacksDict,
			List<Rack> oppositeRacksList
			)
		{
			List<TieBeamCalculationInfo> result = new List<TieBeamCalculationInfo>();

			if (aisleSpace == null)
				return result;
			if (eGeometryContactSide.eWithoutContactSide == contactSide)
				return result;
			if (tieBeamsRacksDict == null || tieBeamsRacksDict.Keys.Count == 0)
				return result;
			if (oppositeRacksList == null || oppositeRacksList.Count == 0)
				return result;

			// Go through rack which need tie beams and try to find an opposite rack
			foreach(Rack rack in tieBeamsRacksDict.Keys)
			{
				if (rack == null)
					continue;

				eTieBeamFrame frame = tieBeamsRacksDict[rack];
				if (eTieBeamFrame.eNone == frame)
					continue;

				if (rack.Column == null)
					continue;

				List<eTieBeamFrame> framesList = new List<eTieBeamFrame>();
				if (frame.HasFlag(eTieBeamFrame.eStartFrame))
					framesList.Add(eTieBeamFrame.eStartFrame);
				if (frame.HasFlag(eTieBeamFrame.eEndFrame))
					framesList.Add(eTieBeamFrame.eEndFrame);

				foreach (eTieBeamFrame tieBeamFrame in framesList)
				{
					if (eTieBeamFrame.eNone == tieBeamFrame)
						continue;

					// Check: aisle space should be placed between racks
					if (rack.IsHorizontal)
					{
						if (eTieBeamFrame.eStartFrame == tieBeamFrame && Utils.FGT(aisleSpace.TopLeft_GlobalPoint.X, rack.TopLeft_GlobalPoint.X))
							continue;
						if (eTieBeamFrame.eEndFrame == tieBeamFrame && Utils.FLT(aisleSpace.TopRight_GlobalPoint.X, rack.TopRight_GlobalPoint.X))
							continue;
					}
					else
					{
						if (eTieBeamFrame.eStartFrame == tieBeamFrame && Utils.FGT(aisleSpace.TopLeft_GlobalPoint.Y, rack.TopLeft_GlobalPoint.Y))
							continue;
						if (eTieBeamFrame.eEndFrame == tieBeamFrame && Utils.FLT(aisleSpace.BottomLeft_GlobalPoint.Y, rack.BottomLeft_GlobalPoint.Y))
							continue;
					}

					// Current rack frame center.
					// If rack is horizontal then it contains X frame center position.
					// If rack is vertical then it contains Y frame center position.
					double rackFrameCenter = 0.0;
					if(eTieBeamFrame.eStartFrame == tieBeamFrame)
					{
						if (rack.IsHorizontal)
							rackFrameCenter = rack.TopLeft_GlobalPoint.X;
						else
							rackFrameCenter = rack.TopLeft_GlobalPoint.Y;
						rackFrameCenter += rack.Column.Length / 2;
					}
					else if(eTieBeamFrame.eEndFrame == tieBeamFrame)
					{
						if (rack.IsHorizontal)
							rackFrameCenter = rack.BottomRight_GlobalPoint.X;
						else
							rackFrameCenter = rack.BottomRight_GlobalPoint.Y;
						rackFrameCenter -= rack.Column.Length / 2;
					}

					// Find an opposite rack.
					// Frame centers on opposite racks should be aligned.
					foreach (Rack oppositeRack in oppositeRacksList)
					{
						if (oppositeRack == null)
							continue;

						if (oppositeRack.Column == null)
							continue;

						List<eTieBeamFrame> oppositeRackFramesList = new List<eTieBeamFrame>();
						List<double> oppositeRackFrameCentersList = new List<double>();
						// If opposite rack is M then it has start column.
						// Also it has end column anyway. Look at rack advanced properties picture.
						if (oppositeRack.IsFirstInRowColumn)
						{
							// add start frame
							double oppositeRackStartFrameCenter = 0.0;
							if (oppositeRack.IsHorizontal)
								oppositeRackStartFrameCenter = oppositeRack.TopLeft_GlobalPoint.X;
							else
								oppositeRackStartFrameCenter = oppositeRack.TopLeft_GlobalPoint.Y;
							oppositeRackStartFrameCenter += oppositeRack.Column.Length / 2;

							oppositeRackFramesList.Add(eTieBeamFrame.eStartFrame);
							oppositeRackFrameCentersList.Add(oppositeRackStartFrameCenter);
						}
						// add end frame
						double oppositeRackEndFrameCenter = 0.0;
						if (oppositeRack.IsHorizontal)
							oppositeRackEndFrameCenter = oppositeRack.BottomRight_GlobalPoint.X;
						else
							oppositeRackEndFrameCenter = oppositeRack.BottomRight_GlobalPoint.Y;
						oppositeRackEndFrameCenter -= oppositeRack.Column.Length / 2;
						//
						oppositeRackFramesList.Add(eTieBeamFrame.eEndFrame);
						oppositeRackFrameCentersList.Add(oppositeRackEndFrameCenter);

						if (oppositeRackFramesList.Count != oppositeRackFrameCentersList.Count)
							continue;

						// go through opposite rack frames
						bool isOpFrameFound = false;
						for (int iOpRackFrameIndex = 0; iOpRackFrameIndex < oppositeRackFrameCentersList.Count; ++iOpRackFrameIndex)
						{
							double opRackFrameCenter = oppositeRackFrameCentersList[iOpRackFrameIndex];
							eTieBeamFrame opRackTieBeamFrame = oppositeRackFramesList[iOpRackFrameIndex];

							// compare frames centers
							if (Utils.FNE(rackFrameCenter, opRackFrameCenter))
								continue;

							// frames centers are aligned
							// tie beams can be added

							TieBeamCalculationInfo calcInfo = new TieBeamCalculationInfo();
							//
							if (!calcInfo.OppositeRacksDict.ContainsKey(rack))
								calcInfo.OppositeRacksDict[rack] = eTieBeamFrame.eNone;
							calcInfo.OppositeRacksDict[rack] |= tieBeamFrame;
							//
							if (!calcInfo.OppositeRacksDict.ContainsKey(oppositeRack))
								calcInfo.OppositeRacksDict[oppositeRack] = eTieBeamFrame.eNone;
							calcInfo.OppositeRacksDict[oppositeRack] |= opRackTieBeamFrame;
							//
							result.Add(calcInfo);

							isOpFrameFound = true;
							break;
						}

						// Dont check other opposite racks, take the next frame.
						if (isOpFrameFound)
							break;
					}
				}
			}


			return result;
		}

		//=============================================================================
		// Returns time beam placement depends on the beam span and HeightDepth ratio.
		public static eTieBeamPlacement GetTieBeamPlacement(double beamSpan, double hdRatio)
		{
			if (Utils.FLE(beamSpan, 0.0) || Utils.FLE(hdRatio, 0.0))
				return eTieBeamPlacement.eNone;

			if (Utils.FLE(hdRatio, 6.0))
				return eTieBeamPlacement.eNone;

			if (Utils.FGT(hdRatio, 6.0) && Utils.FLE(hdRatio, 10.0))
			{
				if (Utils.FLE(beamSpan, 1800.0))
					return eTieBeamPlacement.eFirst_Last_EveryThirdFrame;
				if (Utils.FGT(beamSpan, 1800.0) && Utils.FLE(beamSpan, 2700.0))
					return eTieBeamPlacement.eFirst_Last_EveryAlternate;
				return eTieBeamPlacement.eEveryFrame;
			}

			if (Utils.FGT(hdRatio, 10.0))
				return eTieBeamPlacement.eEveryFrame;

			return eTieBeamPlacement.eEveryFrame;
		}

		//=============================================================================
		// Calculates contact side between AisleSpace and length side of racks from headerRacksList.
		// eGeometryContactSide - is side on the rack, which contacts with aisle space
		public static bool CalculateContactSides(
			List<AisleSpace> aisleSpacesList,
			List<List<Rack>> racksGroupsList,
			List<TieBeamGroup> tieBeamsGroupsList,
			out Dictionary<AisleSpace, Dictionary<eGeometryContactSide, List<Rack>>> aisleSpaceContactsSidesDict,
			out Dictionary<AisleSpace, Dictionary<eGeometryContactSide, List<TieBeamGroup>>> tieBeamsContactsSidesDict
			)
		{
			aisleSpaceContactsSidesDict = new Dictionary<AisleSpace, Dictionary<eGeometryContactSide, List<Rack>>>();
			tieBeamsContactsSidesDict = new Dictionary<AisleSpace, Dictionary<eGeometryContactSide, List<TieBeamGroup>>>();

			if (aisleSpacesList == null || aisleSpacesList.Count == 0)
				return false;

			if (racksGroupsList == null || racksGroupsList.Count == 0)
				return false;

			if (tieBeamsGroupsList == null || tieBeamsGroupsList.Count == 0)
				return false;

			foreach (AisleSpace aisleSpaceGeom in aisleSpacesList)
			{
				if (aisleSpaceGeom == null)
					continue;

				//
				foreach (List<Rack> racksGroup in racksGroupsList)
				{
					if (racksGroup == null || racksGroup.Count == 0)
						continue;

					Rack firstRack = racksGroup[0];
					Rack lastRack = racksGroup[racksGroup.Count - 1];
					if (firstRack == null || lastRack == null)
						continue;

					eGeometryContactSide contactSide = eGeometryContactSide.eWithoutContactSide;
					if (firstRack.IsHorizontal)
					{
						// skip if they are not intersected in X-axis
						if (Utils.FLT(aisleSpaceGeom.TopRight_GlobalPoint.X, firstRack.TopLeft_GlobalPoint.X) || Utils.FGT(aisleSpaceGeom.TopLeft_GlobalPoint.X, lastRack.TopRight_GlobalPoint.X))
							continue;

						if (Utils.FEQ(firstRack.TopLeft_GlobalPoint.Y, aisleSpaceGeom.BottomLeft_GlobalPoint.Y))
						{
							contactSide = eGeometryContactSide.eTop;
						}
						else if (Utils.FEQ(firstRack.BottomLeft_GlobalPoint.Y, aisleSpaceGeom.TopLeft_GlobalPoint.Y))
						{
							contactSide = eGeometryContactSide.eBot;
						}
					}
					else
					{
						// skip if they are not intersected in Y-axis
						if (Utils.FLT(aisleSpaceGeom.BottomLeft_GlobalPoint.Y, firstRack.TopLeft_GlobalPoint.Y) || Utils.FGT(aisleSpaceGeom.TopLeft_GlobalPoint.Y, lastRack.BottomLeft_GlobalPoint.Y))
							continue;

						if (Utils.FEQ(firstRack.TopLeft_GlobalPoint.X, aisleSpaceGeom.TopRight_GlobalPoint.X))
						{
							contactSide = eGeometryContactSide.eLeft;
						}
						else if (Utils.FEQ(firstRack.TopRight_GlobalPoint.X, aisleSpaceGeom.TopLeft_GlobalPoint.X))
						{
							contactSide = eGeometryContactSide.eRight;
						}
					}

					if (eGeometryContactSide.eWithoutContactSide != contactSide)
					{
						if (!aisleSpaceContactsSidesDict.ContainsKey(aisleSpaceGeom))
							aisleSpaceContactsSidesDict[aisleSpaceGeom] = new Dictionary<eGeometryContactSide, List<Rack>>();
						if (!aisleSpaceContactsSidesDict[aisleSpaceGeom].ContainsKey(contactSide))
							aisleSpaceContactsSidesDict[aisleSpaceGeom][contactSide] = new List<Rack>();
						if (!aisleSpaceContactsSidesDict[aisleSpaceGeom][contactSide].Contains(firstRack))
							aisleSpaceContactsSidesDict[aisleSpaceGeom][contactSide].Add(firstRack);
					}
				}

				//
				foreach (TieBeamGroup group in tieBeamsGroupsList)
				{
					if (group == null)
						continue;

					eGeometryContactSide contactSide = eGeometryContactSide.eWithoutContactSide;
					if (group.IsHorizontal)
					{
						// skip if they are not intersected in X-axis
						if (Utils.FLT(aisleSpaceGeom.TopRight_GlobalPoint.X, group.TopLeftPnt.X) || Utils.FGT(aisleSpaceGeom.TopLeft_GlobalPoint.X, group.TopRightPnt.X))
							continue;

						if (Utils.FEQ(group.TopLeftPnt.Y, aisleSpaceGeom.BottomLeft_GlobalPoint.Y))
						{
							contactSide = eGeometryContactSide.eTop;
						}
						else if (Utils.FEQ(group.BotLeftPnt.Y, aisleSpaceGeom.TopLeft_GlobalPoint.Y))
						{
							contactSide = eGeometryContactSide.eBot;
						}
					}
					else
					{
						// skip if they are not intersected in Y-axis
						if (Utils.FLT(aisleSpaceGeom.BottomLeft_GlobalPoint.Y, group.TopLeftPnt.Y) || Utils.FGT(aisleSpaceGeom.TopLeft_GlobalPoint.Y, group.BotLeftPnt.Y))
							continue;

						if (Utils.FEQ(group.TopLeftPnt.X, aisleSpaceGeom.TopRight_GlobalPoint.X))
						{
							contactSide = eGeometryContactSide.eLeft;
						}
						else if (Utils.FEQ(group.TopRightPnt.X, aisleSpaceGeom.TopLeft_GlobalPoint.X))
						{
							contactSide = eGeometryContactSide.eRight;
						}
					}

					if (eGeometryContactSide.eWithoutContactSide != contactSide)
					{
						if (!tieBeamsContactsSidesDict.ContainsKey(aisleSpaceGeom))
							tieBeamsContactsSidesDict[aisleSpaceGeom] = new Dictionary<eGeometryContactSide, List<TieBeamGroup>>();
						if (!tieBeamsContactsSidesDict[aisleSpaceGeom].ContainsKey(contactSide))
							tieBeamsContactsSidesDict[aisleSpaceGeom][contactSide] = new List<TieBeamGroup>();
						if (!tieBeamsContactsSidesDict[aisleSpaceGeom][contactSide].Contains(group))
							tieBeamsContactsSidesDict[aisleSpaceGeom][contactSide].Add(group);
					}
				}
			}

			return true;
		}

		//=============================================================================
		/// <summary>
		/// Return list of rack from racksGroup, for which need to place tie beams.
		/// </summary>
		public static Dictionary<Rack, eTieBeamFrame> GetTieBeamRacks(List<Rack> racksGroup, eTieBeamPlacement tieBeamPlacement)
		{
			// key - rack
			// value - where tie beam should be placed
			Dictionary<Rack, eTieBeamFrame> result = new Dictionary<Rack, eTieBeamFrame>();
			if (eTieBeamPlacement.eNone == tieBeamPlacement)
				return result;
			if (racksGroup == null || racksGroup.Count == 0)
				return result;

			for (int i = 0; i < racksGroup.Count; ++i)
			{
				Rack rack = racksGroup[i];
				if (rack == null)
					break;

				// Need to add tie beam for the first and last rack for all eTieBeamPlacement.
				if (i == 0)
				{
					if (!result.ContainsKey(rack))
						result[rack] = 0;
					result[rack] |= eTieBeamFrame.eStartFrame;
				}
				// Add tie beams to every frame
				if (eTieBeamPlacement.eEveryFrame == tieBeamPlacement)
				{
					if (!result.ContainsKey(rack))
						result[rack] = 0;
					result[rack] |= eTieBeamFrame.eEndFrame;
				}
				// Add tie beam at the last frame
				else if (i == (racksGroup.Count - 1))
				{
					if (!result.ContainsKey(rack))
						result[rack] = 0;
					result[rack] |= eTieBeamFrame.eEndFrame;
				}
				//
				else
				{
					int iMultiplicity = 1;
					if (eTieBeamPlacement.eFirst_Last_EveryAlternate == tieBeamPlacement)
						iMultiplicity = 2;
					else if (eTieBeamPlacement.eFirst_Last_EveryThirdFrame == tieBeamPlacement)
						iMultiplicity = 3;

					if ((i + 1) % iMultiplicity == 0)
					{
						if (!result.ContainsKey(rack))
							result[rack] = 0;
						result[rack] |= eTieBeamFrame.eEndFrame;
					}
				}
			}

			// If rack is not first in the row(A-rack) and has eTieBeamFrame.eStartFrame
			// then need to find previous one rack and set eTieBeamFrame.eEndFrame to it.
			// A-rack cant have eTieBeamFrame.eStartFrame.
			bool bDictWasChanged = false;
			List<Rack> keysList = result.Keys.ToList();
			if (keysList != null)
			{
				foreach (Rack rack in keysList)
				{
					if (rack == null)
						continue;
					eTieBeamFrame frame = result[rack];

					if (!rack.IsFirstInRowColumn && frame.HasFlag(eTieBeamFrame.eStartFrame))
					{
						if (!bDictWasChanged)
							bDictWasChanged = true;

						// remove eTieBeamFrame.eStartFrame
						result[rack] &= ~eTieBeamFrame.eStartFrame;

						// try to find previous rack
						if (rack.Sheet == null)
							continue;

						List<Rack> currRackGroup = rack.Sheet.GetRackGroup(rack);
						if (currRackGroup == null || !currRackGroup.Contains(rack))
							continue;

						int iCurrRackIndex = currRackGroup.IndexOf(rack);
						if (iCurrRackIndex < 1)
							continue;

						Rack previousRack = currRackGroup[iCurrRackIndex - 1];
						if (previousRack == null)
							continue;

						// add tie beam to the end of previous rack
						if (!result.ContainsKey(previousRack))
							result[previousRack] = 0;
						result[previousRack] |= eTieBeamFrame.eEndFrame;
					}
				}
			}
			// remove racks without frames
			if(bDictWasChanged)
			{
				List<Rack> keysForDelete = new List<Rack>();
				//
				foreach(Rack rackKey in result.Keys)
				{
					if (rackKey == null)
						continue;

					if (result[rackKey] == eTieBeamFrame.eNone)
						keysForDelete.Add(rackKey);
				}
				//
				foreach(Rack rackKey in keysForDelete)
				{
					if (rackKey == null)
						continue;

					result.Remove(rackKey);
				}
			}

			return result;
		}

		//=============================================================================
		/// <summary>
		/// Creates tie beams between rack01 and rack02.
		/// If bAddAtStart = false then add TieBeam at the rack end,
		/// otherwise add at the end and start.
		/// </summary>
		public static TieBeam CreateTieBeam(DrawingSheet sheet, Rack rack01, Rack rack02, eTieBeamFrame firstRackFrame, eTieBeamFrame secondRackFrame)
		{
			if (sheet == null)
				return null;
			if (rack01 == null || rack02 == null)
				return null;

			if (rack01.IsHorizontal != rack02.IsHorizontal)
				return null;

			if (eTieBeamFrame.eNone == firstRackFrame || eTieBeamFrame.eNone == secondRackFrame)
				return null;

			if (rack01.IsHorizontal)
			{
				Rack topRack = rack01;
				Rack botRack = rack02;
				if (Utils.FLT(rack02.TopLeft_GlobalPoint.Y, rack01.TopLeft_GlobalPoint.Y))
				{
					topRack = rack02;
					botRack = rack01;
				}

				eTieBeamFrame topRackFrame = firstRackFrame;
				if (topRack == rack02)
					topRackFrame = secondRackFrame;
				bool bAddAtStart = true;
				if (eTieBeamFrame.eEndFrame == topRackFrame)
					bAddAtStart = false;

				if (bAddAtStart)
				{
					TieBeam tieBeamAtStart = new TieBeam(sheet);
					tieBeamAtStart.TopLeft_GlobalPoint = topRack.BottomLeft_GlobalPoint;
					tieBeamAtStart.Length_X = TieBeam.TIE_BEAM_DEPTH;
					tieBeamAtStart.Length_Y = botRack.TopLeft_GlobalPoint.Y - topRack.BottomLeft_GlobalPoint.Y;

					return tieBeamAtStart;
				}
				else
				{
					TieBeam tieBeamAtEnd = new TieBeam(sheet);
					tieBeamAtEnd.TopLeft_GlobalPoint = new Point(topRack.BottomRight_GlobalPoint.X - TieBeam.TIE_BEAM_DEPTH, topRack.BottomRight_GlobalPoint.Y);
					tieBeamAtEnd.Length_X = TieBeam.TIE_BEAM_DEPTH;
					tieBeamAtEnd.Length_Y = botRack.TopRight_GlobalPoint.Y - topRack.BottomRight_GlobalPoint.Y;

					return tieBeamAtEnd;
				}
			}
			else
			{
				Rack leftRack = rack01;
				Rack rightRack = rack02;
				if (Utils.FLT(rack02.TopLeft_GlobalPoint.X, rack01.TopLeft_GlobalPoint.X))
				{
					leftRack = rack02;
					rightRack = rack01;
				}

				eTieBeamFrame leftRackFrame = firstRackFrame;
				if (leftRack == rack02)
					leftRackFrame = secondRackFrame;
				bool bAddAtStart = true;
				if (eTieBeamFrame.eEndFrame == leftRackFrame)
					bAddAtStart = false;

				if (bAddAtStart)
				{
					TieBeam tieBeamAtStart = new TieBeam(sheet);
					tieBeamAtStart.TopLeft_GlobalPoint = leftRack.TopRight_GlobalPoint;
					tieBeamAtStart.Length_Y = TieBeam.TIE_BEAM_DEPTH;
					tieBeamAtStart.Length_X = rightRack.TopLeft_GlobalPoint.X - leftRack.TopRight_GlobalPoint.X;

					return tieBeamAtStart;
				}
				else
				{
					TieBeam tieBeamAtEnd = new TieBeam(sheet);
					tieBeamAtEnd.TopLeft_GlobalPoint = new Point(leftRack.BottomRight_GlobalPoint.X, leftRack.BottomRight_GlobalPoint.Y - TieBeam.TIE_BEAM_DEPTH);
					tieBeamAtEnd.Length_Y = TieBeam.TIE_BEAM_DEPTH;
					tieBeamAtEnd.Length_X = rightRack.BottomLeft_GlobalPoint.X - leftRack.BottomRight_GlobalPoint.X;

					return tieBeamAtEnd;
				}
			}

			return null;
		}
	}
}
