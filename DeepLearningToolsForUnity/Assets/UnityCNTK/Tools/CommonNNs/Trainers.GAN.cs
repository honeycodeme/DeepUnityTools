using System.Collections;
using System.Collections.Generic;
using CNTK;
using System;
using System.Linq;
using UnityEngine;

namespace UnityCNTK
{
    public class TrainerGAN
    {
        protected Trainer trainerG;
        protected Trainer trainerD;

        protected List<Learner> learnersG;
       