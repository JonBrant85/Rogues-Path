using System;
using System.Collections;
using System.Collections.Generic;
using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using UnityEngine;
using UnityEngine.Serialization;

public class FourDPawn : MonoBehaviour {
    [FormerlySerializedAs("character")]
    [SerializeField] public Character4D Character;
    [SerializeField] public AnimationManager animationManager;

    public float xDirection, yDirection;

    private void Start() {
        Character.SetDirection(Vector2.down);
        animationManager.SetState(CharacterState.Idle);
    }

    private void Update() {
        // Billboarding
        Vector3 targetPosition = Camera.main!.transform.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition);

        (float x, float y) direction = (xDirection, yDirection);

        switch (direction) {
            default:
                animationManager.SetState(CharacterState.Idle);
                break;
            case (< 0, 0):
                Character.SetDirection(Vector2.right);
                break;
            case (> 0, 0):
                Character.SetDirection(Vector2.left);
                break;
            case (0, <0):
                Character.SetDirection(Vector2.down);
                break;
            case (0, >0):
                Character.SetDirection(Vector2.up);
                break;
        }
    }
}