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
        CreateGAN();

    }
	
	// Update is called once per frame
	void Update () {
        if (training)
        {
            TrainOnce(10);
        }
    }



    public void CreateGAN()
    {
        gan = new GAN(2, 0, 2, 10, 5, 10, 5, DeviceDescriptor.GPUDevice(0));
        trainerGan = new TrainerGAN(gan, LearnerDefs.AdamLearner(lrGenerator), LearnerDefs.AdamLearner(lrDiscriminator),DeviceDescriptor.GPUDevice(0));
        trainerGan.usePredictionInTraining = true;
    }


    public void LoadTrainingData(