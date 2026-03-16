"""
MCP Remote Server - REST API backend using FastAPI.

This is a pure REST API server (no MCP). The MCP Bridge calls this API.

NOTE: This file is pre-completed for the lab exercises.
It provides the REST API backend that the MCP Bridge wraps.
"""
import json
from pathlib import Path
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

app = FastAPI(title="MCP Remote Server - REST API")


def load_tickets() -> dict[str, dict]:
    """Load tickets from shared data/tickets.json file."""
    data_file = Path(__file__).parent.parent.parent / "data" / "tickets.json"
    if data_file.exists():
        with open(data_file, "r") as f:
            ticket_list = json.load(f)
            return {t["id"]: t for t in ticket_list}
    # Fallback if file not found
    return {
        "TICKET-001": {"id": "TICKET-001", "customerId": "C001", "customerName": "Alice Johnson", "subject": "Login issue", "description": "Cannot login to the system", "status": "Open", "priority": "High", "assignedTo": None},
        "TICKET-002": {"id": "TICKET-002", "customerId": "C002", "customerName": "Bob Smith", "subject": "Performance problem", "description": "System is running slowly", "status": "In Progress", "priority": "Medium", "assignedTo": "support-team"},
        "TICKET-003": {"id": "TICKET-003", "customerId": "C003", "customerName": "Carol White", "subject": "Data sync error", "description": "Data not syncing properly", "status": "Open", "priority": "High", "assignedTo": None},
    }


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
async def get_all_tickets(maxResults: int = 5) -> list[Ticket]:
    """Get all tickets with optional limit."""
    return [Ticket(**t) for t in tickets.values()][:maxResults]


@app.get("/api/tickets/{ticket_id}")
async def get_ticket(ticket_id: str) -> Ticket:
    """Get a specific ticket by ID."""
    if ticket_id not in tickets:
        raise HTTPException(status_code=404, detail=f"Ticket '{ticket_id}' not found")
    return Ticket(**tickets[ticket_id])


@app.put("/api/tickets/{ticket_id}")
async def update_ticket(ticket_id: str, update: TicketUpdate) -> Ticket:
    """Update a ticket's status."""
    if ticket_id not in tickets:
        raise HTTPException(status_code=404, detail=f"Ticket '{ticket_id}' not found")
    tickets[ticket_id]["status"] = update.status
    return Ticket(**tickets[ticket_id])


if __name__ == "__main__":
    import uvicorn
    print("Starting REST API Server on http://localhost:5060")
    print("Endpoints: GET /api/tickets, GET /api/tickets/{id}, PUT /api/tickets/{id}")
    uvicorn.run(app, host="0.0.0.0", port=5060)
