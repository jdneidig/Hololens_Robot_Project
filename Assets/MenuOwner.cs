﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOwner : MonoBehaviour {
	public GameObject menuPrefab;
	public GameObject menu { get; protected set; }
	public bool isMenuOpen {
		get {
			return menu != null;
		}
		set {
			if (value) {
				if (isMenuOpen)
					Destroy(menu);
				menu = Instantiate(menuPrefab);
				menu.GetComponent<ObjectMenu>().Bind(gameObject);
				BuildMenu();
			}
			else if (isMenuOpen) {
				Destroy(menu);
				menu = null;
				OnMenuClose();
			}
		}
	}

	private void Start() {
		menu = null;
	}

	protected virtual void BuildMenu() {
		isMenuOpen = false;
	}

	protected virtual void OnMenuClose() { }
}
