
# Posterior Policy Optimization(PPO)
## Overview
Original paper of PPO: https://arxiv.org/pdf/1707.06347.pdf

The implementation is partially inspired by Unity ML Agent's python implementation. https://github.com/Unity-Technologies/ml-agents

## Algorithm 1 PPO, Actor-Critic Style
### Input and Output
Input for PPO neural network is the states.

The output of a PPO actor neural network is a distribution of policy's actions. Example: if action is discrete, output can be probabilities of each action; if action is continuous, output can be the mean variance of a normal distribution.

The action is sampled based on the output distribution.

The output of a PPO critic neural network is the value of each state.

### Training Data
The training data is from PPO agent playing the game. These data are collected at each step:
* state
* action taken sample from the output distribution of output from actor network
* the probability of the action taken based on the output distribution of output from actor network
* output value from critic network
* reward obtained after the action is taken.
After end of each game, other data are calculated based on the recorded data:
* advantage: ![advantage equation](https://github.com/honeycodeme/DeepUnityTools/blob/master/Docs/Images/advantage-equation.png)
* target value = advantage + recorded value
### Loss Function:
L = L_policy + L_value + L_entropy
* L_value = (target value - value from NN)^2
* entropy: For normal distribution: ![entropy for normal distribution](https://wikimedia.org/api/rest_v1/media/math/render/svg/5c47c048d3fbf311a0b8af942f44f02908bec393)
* L_policy: Refer equations 7 and 6 on: https://arxiv.org/pdf/1707.06347.pdf . Note that the old probability is the recorded probability. The new probability if the current probability of the recorded action(this differs from Unity's implementation).
### pseudo code
```python
for iteration=1, 2, . . . do:
  for actor=1, 2, . . . , N do:
    Run policy in environment for T timesteps or until the end of game
    Compute advantage estimates A_1, . . . , A_T
  Optimize loss: L, with K epochs and minibatch size M ≤ NT and update the policy.
```

## Usage:
1. Define environment and implement the interface IRLEnvironment.
2. Create neural network for PPO.

```csharp
var network = new PPONetworkContinuousSimple(stateSize, actionSize, numOflayers, hiddenSize, DeviceDescriptor.GPUDevice(0),0.01f);
```

 - PPONetworkContinuousSimple is a simple dense neural network implementation. For custom network, inherit the abstract class ```public abstract class PPONetwork```.

3. Create a `PPOModel` using the neural network. `PPOModel` will add variables needed for training on top of network. It also provides helper functions for evaluation etc.
```csharp
var model = new PPOModel(network);
```

4. Create the PPO trainer. The PPO `TrainerPPOSimple` is a helper class to train the PPO network. It helps step the environment and record the reward,states etc, and calculate the discounted advantage after each episode. 
```csharp
trainer = new TrainerPPOSimple(model, LearnerDefs.AdamLearner(learningRate), numberOfActor, bufferSize, maxStepHorizon);
```
- The `buffersize` is the capacity of buffer that records data used in trainer. It should be at least maxPossibleStepOfEachGame*numOfGamesToRunBeforeEachTrain. 
- The `maxStepHorizon` is the max steps of each game before calculate the discounted advantage. One game is to end when this is reached.

5. Run the steps every loop until game agent is winning. 
```csharp
        trainer.Step(environment);//step environment using trainer.
        bool reset = trainer.Record(environment);//record information after the step, and return whether should reset environment.
        //reset if yes
        if (reset)
        {
            environment.Reset();
            episodesThisTrain++;
            // If certain number of episodes' data is collected, start train
            // Ideally only one episode of run is needed and instead multiply agent will run parallel to get more data.
            if (episodesThisTrain >= episodeToRunForEachTrain)
            {    
                //train with all collected data.
                //the trainer will randomize data's order and train using all of them with a minibatch size
                //and this process will be done for iterationForEachTrain times.
                trainer.TrainAllData(minibatch, iterationForEachTrain);
                print("Training Loss:" + trainer.LastLoss); //print loss
                //clear data
                trainer.ClearData();
                episodesThisTrain = 0;
            }
        }
```