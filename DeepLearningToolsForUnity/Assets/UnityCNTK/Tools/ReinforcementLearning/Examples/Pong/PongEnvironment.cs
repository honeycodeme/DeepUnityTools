using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityCNTK;
using CNTK;
using UnityCNTK.ReinforcementLearning;


public class PongEnvironment : MonoBehaviour,IRLEnvironment {

    public ControlSource playerLeftC