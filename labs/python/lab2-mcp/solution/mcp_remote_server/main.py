"""
MCP Remote Server - REST API backend using FastAPI.

This is a pure REST API server (no MCP). The MCP Bridge calls this API.
"""
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

app = FastAPI(title="MCP Remote Server - REST API")

# In-memory ticket storage
tickets: dict[str, dict] = {
    "TICKET-001": {"id": "TICKET-001", "title": "Login issue", "status": "Open", "description": "Cannot login to the system"},
    "TICKET-002": {"id": "TICKET-002", "title": "Performance problem", "status": "In Progress", "description": "System is running slowly"},
    "TICKET-003": {"id": "TICKET-003", "title": "Data sync error", "status": "Open", "description": "Data not syncing properly"},
}


class Ticket(BaseModel):
    id: str
    title: str
    status: str
    description: str


class TicketUpdate(BaseModel):
    status: str


@app.get("/")
async def root():
    return {"message": "MCP Remote Server - REST API", "endpoints": ["/api/tickets", "/api/tickets/{id}"]}


@app.get("/api/tickets")
async def get_all_tickets() -> list[Ticket]:
    """Get all tickets."""
    return [Ticket(**t) for t in tickets.values()]


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
    uvicorn.run(app, host="0.0.0.0", port=5060)
