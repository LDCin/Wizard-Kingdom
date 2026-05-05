using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Utils;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _isGameOver = 1f;

    private Animator _animator;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    // private void Update()
    // {
    //     Fall();
    // }
    public void Fall()
    {
        transform.Translate(Vector3.down * _moveSpeed * Time.deltaTime * _isGameOver);
    }

    private void FixedUpdate()
    {
        Fall();
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag(GameConfig.GROUND_TAG))
        {
            _isGameOver = 0;
            _animator.SetBool("Victory", true);
            Debug.Log("Game Over!");
        }
    }
}
