# Lab 0 - Hello World

This lab is to make sure you can access Microsoft Foundry resources.

STEP 1:

Look on the whiteboard for a password. Set that password in the code! Then run `dotnet run`. If you see some output from the Agent specified in Program.cs then you are succeeding.

STEP 2:

Review the output. What do you notice?

Consider comparing:

1. Speed - the **SECS** column is elapsed seconds running the model
2. Input token use - the **IN** column is how many tokens taken up by the input prompt
3. Output token use - the **OUT** column
4. Inference token use - the **(INF)** column
5. Does the quality of the response vary? - the **REPONSE** column show Agent's output

Did you notice there are TWO prompts? Feel free to modify the prompts to see what happens.

Resources:

* [OpenAI's Tokenizer]<https://platform.openai.com/tokenizer>
* Ask your favorite AI "what is an Inference model and why should I care?"
