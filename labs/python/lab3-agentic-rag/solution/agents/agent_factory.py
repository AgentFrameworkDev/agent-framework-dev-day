"""
Agent factory for creating all specialized agents.

This module imports individual agent creation functions and provides
a unified AgentFactory class for easy agent instantiation.
"""
from agent_framework import Agent
from agent_framework.openai import OpenAIChatClient

from agents import (
    classifier_agent,
    yes_no_agent,
    semantic_search_agent,
    count_agent,
    difference_agent,
    intersection_agent,
    multi_hop_agent,
    comparative_agent,
)
from services import SearchService


class AgentFactory:
    """Factory for creating specialized agents."""
    
    def __init__(self, chat_client: OpenAIChatClient, search_service: SearchService):
        """
        Initialize the agent factory.
        
        Args:
            chat_client: Azure OpenAI chat client
            search_service: Search service for ticket queries
        """
        self.chat_client = chat_client
        self.search_service = search_service
    
    def create_all_agents(self) -> dict[str, Agent]:
        """
        Create all agents needed for the system.
        
        Returns:
            Dictionary mapping agent names to Agent instances
        """
        return {
            "classifier": classifier_agent.create_classifier_agent(self.chat_client),
            "semantic_search": semantic_search_agent.create_semantic_search_agent(self.chat_client, self.search_service),
            "yes_no": yes_no_agent.create_yes_no_agent(self.chat_client, self.search_service),
            "count": count_agent.create_count_agent(self.chat_client, self.search_service),
            "difference": difference_agent.create_difference_agent(self.chat_client, self.search_service),
            "intersection": intersection_agent.create_intersection_agent(self.chat_client, self.search_service),
            "multi_hop": multi_hop_agent.create_multi_hop_agent(self.chat_client, self.search_service),
            "comparative": comparative_agent.create_comparative_agent(self.chat_client, self.search_service),
        }
