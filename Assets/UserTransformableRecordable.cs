﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UserTransformableRecordable : UserTransformable {
	protected override void BuildMenu() {
		if (!isMenuOpen) return;
		if (UserTransformManager.instance.isRecording) {
			if (UserTransformManager.instance.recorder.target == this) {
				// we are recording
				menu.GetComponent<ObjectMenu>().AddButton(
					"Take Snapshot",
					delegate () {
						UserTransformManager.instance.recorder.TakeSnapshot();
						isMenuOpen = false;
					}
				);
				menu.GetComponent<ObjectMenu>().AddButton(
					"Stop Recording",
					delegate () {
						UserTransformManager.instance.StopRecording();
						isMenuOpen = false;
					}
				);
				menu.GetComponent<ObjectMenu>().AddButton(
					"Save Recording",
					delegate () {
						UserTransformManager.instance.SaveRecording();
						isMenuOpen = false;
					}
				);
			}
			else {
				// someone else is recording
				menu.GetComponent<ObjectMenu>().AddButton(
					"Switch Recording To This",
					delegate () {
						UserTransformManager.instance.StopRecording();
						UserTransformManager.instance.StartRecording(this as UserTransformableRecordable);
						isMenuOpen = false;
					}
				);
			}
		}
		else {
			// nobody is recording
			menu.GetComponent<ObjectMenu>().AddButton(
				"Start Recording",
				delegate () {
					UserTransformManager.instance.StartRecording(this);
					isMenuOpen = false;
					isMenuOpen = true;
				}
			);

			menu.GetComponent<ObjectMenu>().AddButton(
				"Load Recording",
				delegate () {
					UserTransformManager.instance.StartRecording(this);
					UserTransformManager.instance.LoadRecording(gameObject);
					isMenuOpen = false;
				}
			);
		}
	}
}