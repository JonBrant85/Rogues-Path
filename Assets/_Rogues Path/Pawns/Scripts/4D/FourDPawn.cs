using System;
using System.Collections;
using System.Collections.Generic;
using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using UnityEngine;

public class FourDPawn : MonoBehaviour {
    [SerializeField] private Character4D character;
    [SerializeField] private AnimationManager animationManager;

    private float xDirection, yDirection;

    private void Start() {
        character.SetDirection(Vector2.down);
        animationManager.SetState(CharacterState.Idle);
    }

    private void Update() {
        
        // Get camera position but keep the billboard's original height
        Vector3 targetPosition = Camera.main!.transform.position;
        targetPosition.y = transform.position.y; 

        // Face the target position on the horizontal plane
        transform.LookAt(targetPosition);
        
        xDirection = Input.GetAxis("Horizontal");
        yDirection = Input.GetAxis("Vertical");
        character.SetDirection(xDirection < 0 ? Vector2.right : Vector2.left);
        character.SetDirection(yDirection < 0 ? Vector2.up : Vector2.down);
        animationManager.SetState((xDirection, yDirection) == (0, 0) ? CharacterState.Idle : CharacterState.Walk);

        (float x, float y) direction = (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        switch (direction) {
            default:
                animationManager.SetState(CharacterState.Idle);
                break;
            case (< 0, 0):
                character.SetDirection(Vector2.left);
                animationManager.SetState(CharacterState.Walk);
                break;
            case (> 0, 0):
                character.SetDirection(Vector2.right);
                animationManager.SetState(CharacterState.Walk);
                break;
            case (0, <0):
                character.SetDirection(Vector2.down);
                animationManager.SetState(CharacterState.Walk);
                break;
            case (0, >0):
                character.SetDirection(Vector2.up);
                animationManager.SetState(CharacterState.Walk);
                break;
        }
    }
}