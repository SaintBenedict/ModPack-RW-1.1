﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;
using RimWorld;
using Verse.Sound;

namespace rjw.MainTab
{
	[StaticConstructorOnStartup]
	public static class WhoreCheckbox
    {
        public static readonly Texture2D WhoreCheckboxOnTex = ContentFinder<Texture2D>.Get("UI/Commands/Service_on");
        public static readonly Texture2D WhoreCheckboxOffTex = ContentFinder<Texture2D>.Get("UI/Commands/Service_off");
        public static readonly Texture2D WhoreCheckboxDisabledTex = ContentFinder<Texture2D>.Get("UI/Commands/Service_Refuse");

		private static bool checkboxPainting;
		private static bool checkboxPaintingState;

		public static void Checkbox(Vector2 topLeft, ref bool checkOn, float size = 24f, bool disabled = false, Texture2D texChecked = null, Texture2D texUnchecked = null, Texture2D texDisabled = null)
		{
			WhoreCheckbox.Checkbox(topLeft.x, topLeft.y, ref checkOn, size, disabled, texChecked, texUnchecked);
		}

		public static void Checkbox(float x, float y, ref bool checkOn, float size = 24f, bool disabled = false, Texture2D texChecked = null, Texture2D texUnchecked = null, Texture2D texDisabled = null)
		{
			Rect rect = new Rect(x, y, size, size);
			WhoreCheckbox.CheckboxDraw(x, y, checkOn, disabled, size, texChecked, texUnchecked,texDisabled);
			if (!disabled)
			{
				MouseoverSounds.DoRegion(rect);
				bool flag = false;
				Widgets.DraggableResult draggableResult = Widgets.ButtonInvisibleDraggable(rect, false);
				if (draggableResult == Widgets.DraggableResult.Pressed)
				{
					checkOn = !checkOn;
					flag = true;
				}
				else if (draggableResult == Widgets.DraggableResult.Dragged)
				{
					checkOn = !checkOn;
					flag = true;
					WhoreCheckbox.checkboxPainting = true;
					WhoreCheckbox.checkboxPaintingState = checkOn;
				}
				if (Mouse.IsOver(rect) && WhoreCheckbox.checkboxPainting && Input.GetMouseButton(0) && checkOn != WhoreCheckbox.checkboxPaintingState)
				{
					checkOn = WhoreCheckbox.checkboxPaintingState;
					flag = true;
				}
				if (flag)
				{
					if (checkOn)
					{
						SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
					}
					else
					{
						SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
					}
				}
			}
		}

		private static void CheckboxDraw(float x, float y, bool active, bool disabled, float size = 24f, Texture2D texChecked = null, Texture2D texUnchecked = null, Texture2D texDisabled = null)
		{
			Texture2D image;
			if (disabled)
			{
				image = ((!(texDisabled != null)) ? WhoreCheckbox.WhoreCheckboxDisabledTex : texDisabled);
			} 
			else if (active)
			{
				image = ((!(texChecked != null)) ? WhoreCheckbox.WhoreCheckboxOnTex : texChecked);
			}
			else
			{
				image = ((!(texUnchecked != null)) ? WhoreCheckbox.WhoreCheckboxOffTex : texUnchecked);
			}
			Rect position = new Rect(x, y, size, size);
			GUI.DrawTexture(position, image);
		}

	}
}
