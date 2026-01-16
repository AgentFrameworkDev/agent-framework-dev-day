# Lab 1 - Model Safety Comparison

This lab explores how AI safety guardrails affect model responses.

STEP 1:

Run `python main.py` and try the default prompt (just press Enter).
Observe how the two models respond differently to the same input.

STEP 2:

Try your own prompts! Compare responses between:

- **gpt-4.1-mini-safety** - Model with safety guardrails enabled
- **gpt-4.1-mini-no-safety** - Model without safety guardrails

Things to notice:

1. Does the safety model refuse certain requests?
2. How does response time differ?
3. Are token counts different between models?
4. Try edge cases - what triggers safety guardrails?

Resources:

- [Microsoft Foundry Guardrails](https://learn.microsoft.com/en-us/azure/ai-foundry/guardrails/guardrails-overview?view=foundry)
