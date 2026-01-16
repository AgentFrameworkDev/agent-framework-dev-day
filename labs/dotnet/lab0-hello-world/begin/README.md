# Lab 0 - Hello World

This lab is to make sure you can access Microsoft Foundry resources.

STEP 1:

Look on the whiteboard for a password. Set that password in the code! Then run `dotnet run`. If you see some output from the Agent specified in Program.cs then you are succeeding.

STEP 2:

Review the output. What do you notice? How does it compare to other runs? To Python runs?

![Model comparison table](model-comparison-table.png)

Run your version multiple times.

Consider comparing:

1. Speed - the **SECS** column is elapsed seconds running the model
2. Input token use - the **IN** column is how many tokens taken up by the input prompt
3. Output token use - the **OUT** column
4. Reasoning token use - the **(REAS)** column
5. Does the quality of the response vary? - the **REPONSE** column show Agent's output; why aren't all of the responses identical? Compare semantics with [Fun with Vectors](https://funwithvectors.com/) app.

Resources:

* [OpenAI's Tokenizer]<https://platform.openai.com/tokenizer>
* Consider comparing the MEANING "semantics" with [Fun with vectors](https://funwithvectors.com/) app.
* Ask your favorite AI "what is a Reasoning model and why should I care?"
