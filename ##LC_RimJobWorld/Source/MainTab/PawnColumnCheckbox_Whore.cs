﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;
using RimWorld;
using Verse.Sound;

namespace rjw.MainTab
{
    public abstract class PawnColumnCheckbox_Whore : PawnColumnWorker
    {
		public const int HorizontalPadding = 2;

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (!this.HasCheckbox(pawn))
			{
				return;
			}
			int num = (int)((rect.width - 24f) / 2f);
			int num2 = Mathf.Max(3, 0);
			Vector2 vector = new Vector2(rect.x + (float)num, rect.y + (float)num2);
			Rect rect2 = new Rect(vector.x, vector.y, 24f, 24f);
			if (Find.TickManager.TicksGame % 60 == 0)
			{
				pawn.UpdatePermissions();
				//Log.Message("GetDisabled UpdateCanDesignateService for " + xxx.get_pawnname(pawn));
				//Log.Message("UpdateCanDesignateService " + pawn.UpdateCanDesignateService());
				//Log.Message("CanDesignateService " + pawn.CanDesignateService());
				//Log.Message("GetDisabled " + GetDisabled(pawn));
			}
			bool disabled = this.GetDisabled(pawn);
			bool value;
			if (disabled) 
			{
				value = false;
			}
			else 
			{
				value = this.GetValue(pawn);
			}
			
			bool flag = value;
			Vector2 topLeft = vector;
			WhoreCheckbox.Checkbox(topLeft, ref value, 24f, disabled, WhoreCheckbox.WhoreCheckboxOnTex, WhoreCheckbox.WhoreCheckboxOffTex, WhoreCheckbox.WhoreCheckboxDisabledTex);
			if (Mouse.IsOver(rect2))
			{
				string tip = this.GetTip(pawn);
				if (!tip.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect2, tip);
				}
			}
			if (value != flag)
			{
				this.SetValue(pawn, value);
			}
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 28);
		}

		public override int GetMaxWidth(PawnTable table)
		{
			return Mathf.Min(base.GetMaxWidth(table), this.GetMinWidth(table));
		}

		public override int GetMinCellHeight(Pawn pawn)
		{
			return Mathf.Max(base.GetMinCellHeight(pawn), 24);
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return this.GetValueToCompare(a).CompareTo(this.GetValueToCompare(b));
		}

		private int GetValueToCompare(Pawn pawn)
		{
			if (!this.HasCheckbox(pawn))
			{
				return 0;
			}
			if (!this.GetValue(pawn))
			{
				return 1;
			}
			return 2;
		}

		protected virtual string GetTip(Pawn pawn)
		{
			return null;
		}

		protected virtual bool HasCheckbox(Pawn pawn)
		{
			return true;
		}

		protected abstract bool GetValue(Pawn pawn);

		protected abstract void SetValue(Pawn pawn, bool value);

		protected abstract bool GetDisabled(Pawn pawn);

		protected override void HeaderClicked(Rect headerRect, PawnTable table)
		{
			base.HeaderClicked(headerRect, table);
			if (Event.current.shift)
			{
				List<Pawn> pawnsListForReading = table.PawnsListForReading;
				for (int i = 0; i < pawnsListForReading.Count; i++)
				{
					if (this.HasCheckbox(pawnsListForReading[i]))
					{
						if (Event.current.button == 0)
						{
							if (!this.GetValue(pawnsListForReading[i]))
							{
								this.SetValue(pawnsListForReading[i], true);
							}
						}
						else if (Event.current.button == 1 && this.GetValue(pawnsListForReading[i]))
						{
							this.SetValue(pawnsListForReading[i], false);
						}
					}
				}
				if (Event.current.button == 0)
				{
					SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
				}
				else if (Event.current.button == 1)
				{
					SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
				}
			}
		}

		protected override string GetHeaderTip(PawnTable table)
		{
			return base.GetHeaderTip(table) + "\n" + "CheckboxShiftClickTip".Translate();
		}
	}
}

