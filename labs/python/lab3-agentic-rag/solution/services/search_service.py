"""
Azure AI Search service for IT support tickets.
"""
import json
from typing import Any
from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from azure.search.documents.models import VectorizedQuery
from azure.identity import get_bearer_token_provider
from agent_framework.azure import AzureOpenAIChatClient
from openai import AzureOpenAI

from config import AzureConfig


class SearchService:
    """Service for searching IT support tickets in Azure AI Search."""
    
    def __init__(self, config: AzureConfig, chat_client: AzureOpenAIChatClient):
        """
        Initialize the search service.
        
        Args:
            config: Azure configuration with search endpoint and index name
            chat_client: Chat client for generating embeddings
        """
        self.config = config
        self.chat_client = chat_client

        # Create a token provider that returns a fresh bearer token on each call
        token_provider = get_bearer_token_provider(
            config.credential,
            "https://cognitiveservices.azure.com/.default",
        )

        self.openai_client = AzureOpenAI(
            azure_ad_token_provider=token_provider,
            api_version=config.openai_api_version
        )
        self.search_client = SearchClient(
            endpoint=config.search_endpoint,
            index_name=config.search_index_name,
            credential=AzureKeyCredential(config.search_api_key),
        )
    
    def get_embedding(self, text: str) -> list[float]:
        """
        Generate embedding vector for the given text.
        
        Args:
            text: Text to embed
            
        Returns:
            List of floats representing the embedding vector
        """
        try:
            response = self.openai_client.embeddings.create(
                input=text,
                model=self.config.embedding_model
            )
            return response.data[0].embedding
        except Exception as e:
            raise RuntimeError(f"Failed to generate embedding: {e}") from e
    
    def search_tickets(
        self,
        query: str,
        top_k: int = 5,
        select_fields: list[str] | None = None,
        include_semantic_search: bool = True
    ) -> list[dict[str, Any]]:
        """
        Perform hybrid search on IT support tickets.
        
        Combines keyword search with vector search for optimal results.
        
        Args:
            query: User's search query
            top_k: Number of top results to return
            select_fields: Specific fields to return (default: all common fields)
            
        Returns:
            List of ticket dictionaries with search results
        """
        if select_fields is None:
            select_fields = [
                "Id", "Subject", "Body", "Answer", "Business_Type",
                "Type", "Queue", "Priority", "Tags"
            ]
        
        query_vector = None
        if include_semantic_search:
            # Get embedding for the query
            query_vector = self.get_embedding(query)
        
            # Create vector query for semantic search
            vector_query = VectorizedQuery(
                vector=query_vector,
                fields="BodyEmbeddings,AnswerEmbeddings",
            )
        
            # Perform hybrid search (keyword + vector)
            results = self.search_client.search(
                search_text=query,
                vector_queries=[vector_query],
                select=select_fields,
                top=top_k
            )
        else:
            # Perform keyword-only search
            results = self.search_client.search(
                search_text=query,
                select=select_fields,
                top=top_k
            )

        # Format results
        formatted_results = []
        for doc in results:
            result_obj = {
                "id": doc.get("Id", ""),
                "subject": doc.get("Subject", ""),
                "body": doc.get("Body", ""),
                "answer": doc.get("Answer", ""),
                "type": doc.get("Type", ""),
                "department": doc.get("Queue", ""),
                "priority": doc.get("Priority", ""),
                "business_type": doc.get("Business_Type", ""),
                "tags": doc.get("Tags", [])
            }
            formatted_results.append(result_obj)
        
        return formatted_results
    
    def search_tickets_with_filter(
        self,
        query: str,
        odata_filter: str | None = None,
        top_k: int = 5,
        select_fields: list[str] | None = None
    ) -> list[dict[str, Any]]:
        """
        Perform hybrid search on IT support tickets with optional OData filter.
        
        Combines keyword search with vector search and applies filters for precise results.
        
        Args:
            query: User's search query
            odata_filter: OData filter expression (e.g., "Type eq 'Incident' and Priority eq 'low'")
            top_k: Number of top results to return
            select_fields: Specific fields to return (default: all common fields)
            
        Returns:
            List of ticket dictionaries with search results
        """
        if select_fields is None:
            select_fields = [
                "Id", "Subject", "Body", "Answer", "Business_Type",
                "Type", "Queue", "Priority", "Tags"
            ]
        
        # Get embedding for the query
        query_vector = self.get_embedding(query)
        
        # Create vector query for semantic search
        vector_query = VectorizedQuery(
            vector=query_vector,
            fields="BodyEmbeddings,AnswerEmbeddings",
        )
        
        # Perform hybrid search (keyword + vector) with filter
        results = self.search_client.search(
            search_text=query,
            vector_queries=[vector_query],
            filter=odata_filter,
            select=select_fields,
            top=top_k
        )
        
        # Format results
        formatted_results = []
        for doc in results:
            result_obj = {
                "id": doc.get("Id", ""),
                "subject": doc.get("Subject", ""),
                "body": doc.get("Body", ""),
                "answer": doc.get("Answer", ""),
                "type": doc.get("Type", ""),
                "department": doc.get("Queue", ""),
                "priority": doc.get("Priority", ""),
                "business_type": doc.get("Business_Type", ""),
                "tags": doc.get("Tags", [])
            }
            formatted_results.append(result_obj)
        
        return formatted_results
    
    def search_tickets_json(self, query: str, top_k: int = 5) -> str:
        """
        Perform search and return results as JSON string.
        
        Args:
            query: User's search query
            top_k: Number of top results to return
            
        Returns:
            JSON string of search results
        """
        results = self.search_tickets(query, top_k)
        return json.dumps(results, indent=2, ensure_ascii=False)
