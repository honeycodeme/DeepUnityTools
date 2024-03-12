
# AI and Deep learning tools for Unity using CNTK

## Note
This project was developed for Aalto University's Computational Intelligence in Games course material. 

The development is stopped now, because we decided to use Tensorflowsharp with Unity MLAgent instead of CNTK for multiplatform support. The new project will be in public soon. [Here](https://github.com/tcmxx/UnityTensorflowKeras).

## Content 
This rep contains some useful deep learning related tools implemented primarily using CNTK C# library.
Current contents:
- Helper functions to build/train neural network layers. (https://docs.microsoft.com/en-us/cognitive-toolkit/)
  - Layers definitions
  - Simple neural network
  - cGAN
- Universal Style Transfer(https://arxiv.org/pdf/1705.08086.pdf)
- Reinforcement Learning
  - Proximal Policy Optimization(PPO)(https://arxiv.org/pdf/1707.06347.pdf)
  - Deep Q-Learning(DQL)(https://arxiv.org/abs/1312.5602)
  
## Platform and Installation
Currently it only works on Windows. If you need to use GPU for NN, you also need a proper Nvidia graphic card.
Installation