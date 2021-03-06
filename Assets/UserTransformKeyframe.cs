﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace Assets {
	public class UserTransformKeyframe {
		public Vector3 localPosition;
		public Quaternion localRotation;
		public List<UserTransformAction> actions;

		public UserTransformKeyframe() {
			localPosition = new Vector3();
			localRotation = Quaternion.identity;
			actions = new List<UserTransformAction>();
		}

		public UserTransformKeyframe(ref BinaryReader reader) {
			actions = new List<UserTransformAction>();
			GetFromBinary(ref reader);
		}

		public UserTransformKeyframe(Vector3 position, Quaternion rotation) {
			localPosition = position;
			localRotation = rotation;
			actions = new List<UserTransformAction>();
		}

		public UserTransformKeyframe(Transform transform) {
			actions = new List<UserTransformAction>();
			GetFromTransform(transform);
		}

		public async Task Execute(Transform frameOfReference) {
			RobotInterface.MoveToPoseCommand command = new RobotInterface.MoveToPoseCommand(localPosition, localRotation);
			command.velocity = 1.5f;
			RobotInterface.instance.QueueMove(command);
			await ExecuteActions(frameOfReference);
		}

		async Task ExecuteActions(Transform frameOfReference) {
			foreach(UserTransformAction action in actions) {
				await action.Execute(this, frameOfReference);
			}
		}

		public void GetFromTransform(Transform transform) {
			localPosition = transform.localPosition;
			localRotation = transform.localRotation;
		}

		public void PutToTransform(ref Transform transform) {
			transform.localPosition = localPosition;
			transform.localRotation = localRotation;
		}

		public void GetFromBinary(ref BinaryReader reader) {
			localPosition.x = reader.ReadSingle();
			localPosition.y = reader.ReadSingle();
			localPosition.z = reader.ReadSingle();

			localRotation.x = reader.ReadSingle();
			localRotation.y = reader.ReadSingle();
			localRotation.z = reader.ReadSingle();
			localRotation.w = reader.ReadSingle();

			uint nActions = reader.ReadUInt32();
			for(uint i = 0; i < nActions; i++) {
				actions.Add(UserTransformAction.FromBinary(ref reader));
			}
		}

		public void PutToBinary(ref BinaryWriter writer) {
			writer.Write((float)localPosition.x);
			writer.Write((float)localPosition.y);
			writer.Write((float)localPosition.z);

			writer.Write((float)localRotation.x);
			writer.Write((float)localRotation.y);
			writer.Write((float)localRotation.z);
			writer.Write((float)localRotation.w);

			writer.Write((UInt32)actions.Count);
			for(int i = 0; i < actions.Count; i++) {
				actions[i].PutToBinary(ref writer);
			}
		}

		public UserTransformableGhost CreateGhost(UserTransformableRecordable target) {
			GameObject ghostObject = GameObject.Instantiate(target, target.transform.parent).gameObject;
			ghostObject.transform.localPosition = localPosition;
			ghostObject.transform.localRotation = localRotation;

			OutputText.instance.text = OutputText.instance.text + localPosition.ToString("F3") + localRotation.ToString("F3") + "\n";

			ghostObject.GetComponent<UserTransformableRecordable>().enabled = false;

			if (ghostObject.GetComponent<UserTransformableGhost>() == null) ghostObject.AddComponent<UserTransformableGhost>();
			OutputText.instance.text = OutputText.instance.text + "point1\n";
			UserTransformableGhost ghost = ghostObject.GetComponent<UserTransformableGhost>();
			ghost.enabled = true;
			OutputText.instance.text = OutputText.instance.text + "point2\n";
			ghost.GhostifyRenderer();
			ghost.BindKeyframe(this);

			return ghost;
		}
	}
}
