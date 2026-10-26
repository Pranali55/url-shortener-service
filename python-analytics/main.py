from fastapi import FastAPI
from routers import analytics

app = FastAPI(title="URL Shortener Analytics Service")
app.include_router(analytics.router)

@app.get("/health")
def health():
    return {"status": "ok", "service": "analytics"}
