using System;
using Framework.Util;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public interface ICollisionEmitter
{
	event EventHandler<Collider2DParam> OnEnter;

	event EventHandler<Collider2DParam> OnStay;

	event EventHandler<Collider2DParam> OnExit;

	void OnTriggerEnter2DNotify(Collider2D c);

	void OnTriggerStay2DNotify(Collider2D c);

	void OnTriggerExit2DNotify(Collider2D c);
}
