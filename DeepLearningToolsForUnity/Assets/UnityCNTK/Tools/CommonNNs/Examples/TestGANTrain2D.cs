using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityCNTK;
using CNTK;

public class TestGANTrain2D : MonoBehaviour {



    protected GAN gan;
    protected TrainerGAN trainerGan;
    public DataPlane2D dataPlane;
    public float lrGenerator = 0.001f;
    public float lrDiscriminator = 0.001f;
    public bool training = false;


    public int trainedEpisodes = 0;

    // Use this for initialization
    void Start () {
  