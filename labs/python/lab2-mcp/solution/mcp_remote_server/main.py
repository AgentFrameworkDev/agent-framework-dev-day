"""
MCP Remote Server - REST API backend using FastAPI.

This is a pure REST API server (no MCP). The MCP Bridge calls this API.
"""
import json
from pathlib import Path
from fastapi import FastAPI, HTTPException, Request
from pydantic import BaseModel

app = FastAPI(title="MCP Remote Server - REST API")


def load_tickets() -> dict[str, dict]:
    """Load tickets from shared data/tickets.json file."""
    # Find the data folder (mcp/data/tickets.json - up 2 levels from python folder)
    data_file = Path(__file__).parent.parent.parent / "data" / "tickets.json"
    print(f"Looking for tickets at: {data_file}")
    if data_file.exists():
        with open(data_file, "r") as f:
            ticket_list = json.load(f)
            print(f"Loaded {len(ticket_list)} tickets")
            return {t["id"]: t for t in ticket_list}
    print(f"Warning: {data_file} not found")
    # Fallback to empty dict if file not found
    return {}


# In-memory ticket storage loaded from shared JSON file
tickets: dict[str, dict] = load_tickets()


class Ticket(BaseModel):
    id: str
    customerId: str | None = None
    customerName: str | None = None
    subject: str | None = None
    description: str
    status: str
    priority: str | None = None
    assignedTo: str | None = None


class TicketUpdate(BaseModel):
    status: str


@app.get("/")
async def root():
    return {"message": "MCP Remote Server - REST API", "endpoints": ["/api/tickets", "/api/tickets/{id}"]}


@app.get("/api/tickets")
async def get_all_tickets(request: Request, maxResults: int = 5) -> list[Ticket]:
    """Get all tickets."""
    print(f"Authorization header: {request.headers.get('authorization', 'None')}")
    all_tickets = [Ticket(**t) for t in tickets.values()]
    return all_tickets[:maxResults]


@app.get("/api/tickets/{ticket_id}")
async def get_ticket(ticket_id: str, request: Request) -> Ticket:
    """Get a specific ticket by ID."""
    print(f"Authorization header: {request.headers.get('authorization', 'None')}")
    if ticket_id not in tickets:
        raise HTTPException(status_code=404, detail=f"Ticket '{ticket_id}' not found")
    return Ticket(**tickets[ticket_id])


@app.put("/api/tickets/{ticket_id}")
async def update_ticket(ticket_id: str, update: TicketUpdate, request: Request) -> Ticket:
    """Update a ticket's status."""
    print(f"Authorization header: {request.headers.get('authorization', 'None')}")
    if ticket_id not in tickets:
        raise HTTPException(status_code=404, detail=f"Ticket '{ticket_id}' not found")
    
    tickets[ticket_id]["status"] = update.status
    return Ticket(**tickets[ticket_id])


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=5060)
