"""
Declarative Movie Trivia Quiz Agent

Loads a movie trivia quiz agent from a YAML definition and uses it to ask
and grade a trivia question.

For authentication, run `az login` command in terminal or set up service principal
credentials via environment variables:
    AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET
"""

import asyncio
import sys
from pathlib import Path

from agent_framework.declarative import AgentFactory
from foundry_client_factory import load_config

AGENT_DEFINITION_FILENAME = "movie-trivia-agent.yaml"


def load_agent_definition(agent_definition_path: str) -> str:
    """Load the YAML definition from the script directory or current working directory."""
    candidates = (
        [Path(agent_definition_path)]
        if Path(agent_definition_path).is_absolute()
        else [
            Path(__file__).parent / agent_definition_path,
            Path.cwd() / agent_definition_path,
        ]
    )

    for candidate in candidates:
        if candidate.is_file():
            return candidate.read_text()

    raise FileNotFoundError(
        f"Could not find the agent definition file '{agent_definition_path}'."
    )


async def ask_question(agent) -> str:
    """Ask the agent for a single movie trivia question."""
    response = await agent.run(
        "Ask exactly one movie trivia question. Do not reveal the answer until I respond."
    )
    question = response.text.strip()
    if not question:
        raise RuntimeError("The agent did not return a trivia question.")
    return question


async def grade_answer(agent, question: str, answer: str) -> str:
    """Ask the agent to evaluate the user's answer against the trivia question."""
    prompt = (
        f"The trivia question you asked was:\n{question}\n\n"
        f"My answer is:\n{answer}\n\n"
        "Tell me whether my answer is correct.\n"
        'If it is correct, start with "Correct:" and give a short explanation.\n'
        'If it is incorrect, start with "Incorrect:", then provide the correct answer and a short explanation.'
    )
    response = await agent.run(prompt)
    evaluation = response.text.strip()
    if not evaluation:
        raise RuntimeError("The agent did not return an evaluation.")
    return evaluation


async def main() -> None:
    agent_definition_path = sys.argv[1] if len(sys.argv) > 1 else AGENT_DEFINITION_FILENAME
    agent_definition = load_agent_definition(agent_definition_path)

    config = load_config()

    try:
        factory = AgentFactory(
            client_kwargs={
                "project_endpoint": config.endpoint,
                "credential": config.credential,
            }
        )

        async with factory.create_agent_from_yaml(agent_definition) as agent:
            print("Movie Trivia Quiz Agent")
            print(f"Definition: {Path(agent_definition_path).name}")
            print()

            question = await ask_question(agent)
            print("Question:")
            print(question)
            print()

            answer = input("Your answer: ").strip()

            if not answer:
                print()
                print("No answer provided. Exiting.")
                return

            evaluation = await grade_answer(agent, question, answer)

            print()
            print("Result:")
            print(evaluation)
    finally:
        await config.credential.close()


if __name__ == "__main__":
    asyncio.run(main())
