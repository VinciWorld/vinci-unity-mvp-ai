# Unity Autobattle Game: Merging Solana with Machine Learning

### Table of Contents:

- [Introduction](#introduction)
- [Workflow](#workflow)
- [Future Developments](#future-developments)
- [Getting Started](#getting-started)
- [Intellectual Property Notice](#ip-notice)
- [Conclusion](#conclusion)

---

### Architecture overview

![image](https://github.com/VinciWorld/vinci-unity-mvp-ai/assets/8352477/efcb99e5-4563-4081-a6b4-162bf6126169)



### Introduction <a name="introduction"></a>
Experience a unique autobattle game that combines the strengths of the Solana blockchain with remote machine learning training, creating an innovative gaming platform.

### Workflow <a name="workflow"></a>

- **Login**: Begin by accessing the game via the Phantom wallet.
- **Agent Setup**: Craft an agent geared for training.
- **Configure**: Define your agent's training environment and adjust parameters.
- **Training**: Unity establishes a link to the central node, forwarding the training assignment. Through a WebSocket connection, you receive real-time updates on training metrics from the train node.
- **Evaluation**: After the training concludes, assess the model within Unity.
- **NFT Creation**: Satisfied with the results? Convert your model into an NFT.
- **Competitions on Solana**: Once your model is an NFT, you can register for competitions on the Solana blockchain. Post competing, your scores are stored on the blockchain for transparency and security.

### Future Developments <a name="future-developments"></a>
Our commitment to enhancement remains unwavering:

- Feature to resume model training.
- Broader configuration options.
- Enhanced agent customization capabilities.
- Inclusion of more Unity-centric features.
- Refinements to improve the gameplay.


### Getting Started <a name="getting-started"></a>

#### Prerequisites <a name="prerequisites"></a>
- Make sure your browser supports WebGL.
- Ensure you have `git` installed for cloning the repository (if accessing it for the first time).
- A simple HTTP server (like `http-server` for Node.js) to serve the WebGL build.

#### Launching a Pre-existing WebGL Build

1. **Clone and Navigate**:
   - If you haven't already, clone the repository:  
     ```
     git clone https://github.com/VinciWorld/vinci-train-node.git
     ```
   - Navigate to the folder containing the WebGL build:  
     ```
     cd build/webgl
     ```

2. **Set Up a Simple HTTP Server**:
   - Using Node.js `http-server` (you can use other servers based on your preference):
     - Install it globally if you haven’t:  
       ```
       npm install -g http-server
       ```
     - Start the server in the directory containing the WebGL build:  
       ```
       http-server .
       ```

3. **Access the Build**:
   - The server will display URLs you can use to access the content. Typically it's:
     ```
     http://localhost:8080
     ```
   - Open your preferred WebGL-compatible browser and navigate to the URL.

4. **Interact with the WebGL Content**:
   - Once the build loads, you can interact with the WebGL application as intended.

5. **Termination**:
   - Once done, you can terminate the HTTP server by pressing `Ctrl + C` in the terminal.

To train the model, the following components need to be installed:

- **Central Node**: [vinci-central-node](https://github.com/VinciWorld/vinci-central-node)
- **Train Node**: [vinci-train-node](https://github.com/VinciWorld/vinci-train-node)


### Intellectual Property Notice <a name="ip-notice"></a>
We utilize third-party assets for certain game elements. Owing to Unity Asset Store licensing agreements, these cannot be hosted on our GitHub repository.

### Conclusion <a name="conclusion"></a>
The Unity Autobattle Game showcases the synergy of blockchain technology, particularly Solana, with machine learning in a gaming scenario. As we stride forward, we eagerly anticipate the evolution of this blend and the enriching experiences it promises.

---
