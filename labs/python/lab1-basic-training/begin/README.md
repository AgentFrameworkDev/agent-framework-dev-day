# Lab 0 - Hello World

This lab is to make sure you can access Microsoft Foundry resources.

STEP 1:

Look on the whiteboard for a password. Set that password in the code! Then AFTER YOU'VE done the python initialization steps (../README.md) simply run `python main.py`. If you see some output from the Agent specified in Program.cs then you are succeeding.

STEP 2:

Review the output. What do you notice? How does it compare to other runs? To .NET runs?

![Model comparison table](model-comparison-table.png)

Run your version multiple times.

Consider comparing:

1. Speed - the **SECS** column is elapsed seconds running the model
2. Input token use - the **IN** column is how many tokens taken up by the input prompt
3. Output token use - the **OUT** column
4. Reasoning token use - the **(REAS)** column
5. Does the quality of the response vary? - the **REPONSE** column show Agent's output; why aren't all of the responses identical?

Did you notice there are TWO prompts? Feel free to modify the prompts to see what happens.

Resources:

* [OpenAI's Tokenizer]<https://platform.openai.com/tokenizer>
* Ask your favorite AI "what is a Reasoning model and why should I care?"
