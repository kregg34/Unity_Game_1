﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    private Rigidbody rigidBody;
    private AudioSource audioSource;
    [SerializeField] float rcsThrust = 110f;
    [SerializeField] int thrust = 1200;
    [SerializeField] float levelLoadDelay = 3f;
    [SerializeField] AudioClip thrustAudio;
    [SerializeField] AudioClip deathAudio;
    [SerializeField] AudioClip levelCompleteAudio;

    [SerializeField] ParticleSystem thrustParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem deathParticles;

    enum State { Alive, Dying, Transcending};
    State state = State.Alive;

    private bool godMode = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //disables player movement when dying or transcending
        if(state == State.Alive)
        {
            RespondToThurstInput();
            RespondToRotateInput();
        }

        if (Debug.isDebugBuild)
        {
            CheckForDevKeys();
        }
    }

    private void CheckForDevKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextScene();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            godMode = !godMode;
        }
    }

    private void RespondToRotateInput()
    {
        float angVelocity = Time.deltaTime * rcsThrust;

        if (Input.GetKey(KeyCode.A))
        {
            rigidBody.angularVelocity = Vector3.zero;
            transform.Rotate(Vector3.forward * angVelocity);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rigidBody.angularVelocity = Vector3.zero;
            transform.Rotate(-Vector3.forward * angVelocity);
        }
    }

    private void RespondToThurstInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            applyThrust();
        }
        else
        {
            audioSource.Stop();

            if (thrustParticles.isPlaying)
            {
                thrustParticles.Stop();
            }
        }
    }

    private void applyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * thrust * Time.deltaTime);

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(thrustAudio);
        }

        if (!thrustParticles.isPlaying)
        {
            thrustParticles.Play();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(state != State.Alive || godMode) //ignore collisions when dying or transcending
        {
            return;
        }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        audioSource.Stop();
        audioSource.PlayOneShot(deathAudio);
        deathParticles.Play();
        thrustParticles.Stop();
        Invoke("LoadFirstLevel", levelLoadDelay);
    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;
        audioSource.Stop();
        audioSource.PlayOneShot(levelCompleteAudio);
        successParticles.Play();
        Invoke("LoadNextScene", levelLoadDelay);
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextScene()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        int numScenes = SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene((index + 1) % numScenes);
    }
}
