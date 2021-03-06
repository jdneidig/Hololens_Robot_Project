﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;

public class IpConfigurator : MonoBehaviour {
	public static IpConfigurator instance = null;
	IpConfigButton activeButton = null;
	GameObject numpadPrefab = null;
	GameObject numpad = null;

	private void Awake() {
		instance = this;
	}

	void Start() {
		numpadPrefab = Resources.Load<GameObject>("Numpad");
		gameObject.transform.Find("IPConfirm").GetComponent<MenuButton>().onClick = delegate () { TryConnect(); };
		gameObject.transform.Find("IPCancel").GetComponent<MenuButton>().onClick = delegate () { Close(); };
		Close();
		Open();
	}

	public void Open() {
		gameObject.SetActive(true);
		LoadDefaults();

		Vector3 offset = Camera.main.transform.forward;
		offset.y = 0;
		offset = offset.normalized * 0.5f;

		gameObject.transform.position = Camera.main.transform.position + offset;
		gameObject.transform.rotation = Quaternion.LookRotation(offset, new Vector3(0, 1, 0));

		Vector3 position;
		Quaternion rotation;

		try {
			if (RobotInterface.instance.GetFetchedRealPose(out position, out rotation)) {
				OutputText.instance.text = OutputText.instance.text + "\n" + position.ToString() + " " + rotation.ToString();
			}
		} catch (Exception e) {
			OutputText.instance.text = OutputText.instance.text + "\n" + e.Message + "\n" + e.StackTrace;
		}
	}

	public void Close() {
		ReleaseNumpad();
		gameObject.SetActive(false);
	}

	public void SaveDefaults() {
		try {
			FileStream file = File.Create(Path.Combine(Application.persistentDataPath, "default_ip_settings.bin"));

			BinaryWriter writer = new BinaryWriter(file);
			for(uint i = 0; i < 4; i++) writer.Write(byte.Parse(GetIpString(i)));
			writer.Write(ushort.Parse(GetSocketString()));

			writer.Flush();
			file.Flush();

			writer.Dispose();
			file.Dispose();
		}
		catch (Exception e) {
			OutputText.instance.text = e.Message + "\n" + e.StackTrace;
		}
	}

	public void LoadDefaults() {
		try {
			string path = Path.Combine(Application.persistentDataPath, "default_ip_settings.bin");
			if (!File.Exists(path)) return;
			FileStream file = File.OpenRead(path);

			BinaryReader reader = new BinaryReader(file);

			for (uint i = 0; i < 4; i++) SetIpString(i, reader.ReadByte().ToString());
			SetSocketString(reader.ReadUInt16().ToString());

			reader.Dispose();
			file.Dispose();
		}
		catch (Exception e) {
			OutputText.instance.text = e.Message + "\n" + e.StackTrace;
		}
	}

	public void RequestNumpad(IpConfigButton button) {
		if(numpad == null) {
			numpad = Instantiate(numpadPrefab, button.transform.position + new Vector3(0f, -0.04f, -0.03f), button.transform.rotation);
		} else {
			numpad.transform.SetPositionAndRotation(button.transform.position + new Vector3(0f, -0.04f, -0.03f), button.transform.rotation);
		}
		activeButton = button;
		numpad.GetComponent<Numpad>().onKeyPress = button.OnKeyPress;
	}

	public void ReleaseNumpad() {
		if (numpad != null) Destroy(numpad);
		numpad = null;
		activeButton = null;
	}

	public string GetIpString() {
		string[] byteStrings = { GetIpString(0), GetIpString(1), GetIpString(2), GetIpString(3) };
		return string.Join(".", byteStrings);
	}

	public string GetIpString(uint n) {
		return gameObject.transform.Find("IPConfigByte" + (n + 1).ToString()).GetComponent<IpConfigButton>().text;
	}

	public void SetIpString(uint n, string ipString) {
		gameObject.transform.Find("IPConfigByte" +(n + 1).ToString()).GetComponent<IpConfigButton>().text = ipString;
	}

	public string GetSocketString() {
		return gameObject.transform.Find("IPConfigSocket").GetComponent<IpConfigButton>().text;
	}

	public void SetSocketString(string socketString) {
		gameObject.transform.Find("IPConfigSocket").GetComponent<IpConfigButton>().text = socketString;
	}

	public async Task TryConnect() {
		RobotInterface.instance.onConnectSuccess = onConnectSuccess;
		RobotInterface.instance.onConnectFailure = onConnectFailure;

		ReleaseNumpad();

		try {
			GameObject alert = NoButtonAlert.Create("Trying to connect to " + GetIpString() + ":" + GetSocketString() + " ...");
			await RobotInterface.instance.StartConnection(GetIpString(), GetSocketString());
			Destroy(alert);
		} catch(Exception e) {
			OutputText.instance.text = e.Message + "\n" + e.StackTrace;
		}
	}

	public void onConnectSuccess() { 
		try {
			OneButtonAlert.Create("Connected successfully!");
			SaveDefaults();
			Close();
			RobotInterface.instance.MoveNow(new RobotInterface.MoveJointsCommand(new float[] { 0, -Mathf.PI / 2.0f, Mathf.PI / 2.0f, -Mathf.PI / 2.0f, 0, 0 }));
			RobotInterface.instance.FetchRealPose();
		} catch (Exception e) {
			OutputText.instance.text = OutputText.instance.text + "\n" + e.Message + "\n" + e.StackTrace;
		}
	}

	public void onConnectFailure(string errorMessage) {
		OneButtonAlert.Create("Failed to connect. Error message:\n" + errorMessage);
	}
}
