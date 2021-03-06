﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using System.IO;

public class UserTransformManager : MonoBehaviour {
	public static UserTransformManager instance;

	public UserTransformRecordEnvironment recordEnvironment { get; private set; }
	public UserTransformLiveEnvironment liveEnvironment { get; private set; }
	public Arrow arrowPrefab;

	private UserTransformable _focusedObject = null;
	public UserTransformable focusedObject {
		get {
			return _focusedObject;
		}
		set {
			if(_focusedObject != null) {
				_focusedObject.OnLoseFocus();
			}
			_focusedObject = value;
			transformMode = TransformMode.none;
			if (_focusedObject != null) {
				_focusedObject.OnGainFocus();
			}
		}
	}

	public bool isRecording {
		get {
			return recordEnvironment != null;
		}
	}

	public bool isLive {
		get {
			return liveEnvironment != null;
		}
	}

	public enum TransformMode {
		none,
		translate,
		rotate,
		num_options
	}

	private TransformMode _transformMode;
	public TransformMode transformMode {
		get {
			return _transformMode;
		}
		set {
			switch(value) {
				case TransformMode.none:
					HoloInputManager.instance.ChangeCaptureType(HoloInputManager.GestureCaptureType.none);
					break;
				case TransformMode.translate:
				case TransformMode.rotate:
					HoloInputManager.instance.ChangeCaptureType(HoloInputManager.GestureCaptureType.manipulation);
					break;
			}
			_transformMode = value;
		}
	}

	void Start () {
		instance = this;
		focusedObject = null;
		recordEnvironment = null;
		liveEnvironment = null;
	}

	public void StartNewRecording(UserTransformableRecordable target) {
		recordEnvironment = new UserTransformRecordEnvironment(target);
	}

	public void LoadRecording(UserTransformableRecordable target, string filename) {
		recordEnvironment = new UserTransformRecordEnvironment(target, filename);
	}

	public void StartLiveSession(UserTransformableRecordable target) {
		liveEnvironment = new UserTransformLiveEnvironment(target);
		InvokeRepeating("UpdateLiveSession", 0.0f, 0.1f);
	}

	public void StopLiveSession() {
		CancelInvoke("UpdateLiveSession");
		liveEnvironment = null;
	}

	private void UpdateLiveSession() {
		liveEnvironment.UpdateRobot();
	}

	public void SaveRecording(string filename) {
		if(recordEnvironment != null) recordEnvironment.recording.Save(filename);
	}

	public void StopRecording() {
		if (recordEnvironment != null) {
			recordEnvironment.DeleteAllSnapshots();
			recordEnvironment = null;
		}
	}
}